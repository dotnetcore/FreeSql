using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace FreeSql.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {

        /// <summary>
        /// 数据库表名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 指定数据库旧的表名，修改实体命名时，同时设置此参数为修改之前的值，CodeFirst才可以正确修改数据库表；否则将视为【创建新表】
        /// </summary>
        public string OldName { get; set; }

        internal bool? _DisableSyncStructure;
        /// <summary>
        /// 禁用 CodeFirst 同步结构迁移
        /// </summary>
        public bool DisableSyncStructure { get => _DisableSyncStructure ?? false; set => _DisableSyncStructure = value; }

        internal ConcurrentDictionary<string, ColumnAttribute> _columns { get; } = new ConcurrentDictionary<string, ColumnAttribute>(StringComparer.CurrentCultureIgnoreCase);
        internal ConcurrentDictionary<string, NavigateAttribute> _navigates { get; } = new ConcurrentDictionary<string, NavigateAttribute>(StringComparer.CurrentCultureIgnoreCase);
        internal ConcurrentDictionary<string, IndexAttribute> _indexs { get; } = new ConcurrentDictionary<string, IndexAttribute>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// 格式：属性名=开始时间(递增)<para></para>
        /// 按年分表：[Table(Name = "log_{yyyy}", AsTable = "create_time=2022-1-1(1 year)")]<para></para>
        /// 按月分表：[Table(Name = "log_{yyyyMM}", AsTable = "create_time=2022-5-1(1 month)")]<para></para>
        /// 按日分表：[Table(Name = "log_{yyyyMMdd}", AsTable = "create_time=2022-5-1(5 day)")]<para></para>
        /// 按时分表：[Table(Name = "log_{yyyyMMddHH}", AsTable = "create_time=2022-5-1(6 hour)")]<para></para>
        /// </summary>
        public string AsTable { get; set; }

        internal void ParseAsTable(TableInfo tb)
        {
            if (string.IsNullOrEmpty(AsTable) == false)
            {
                var atm = Regex.Match(AsTable, @"([\w_\d]+)\s*=\s*(\d\d\d\d)\s*\-\s*(\d\d?)\s*\-\s*(\d\d?)\s*( [\d:]+)?\((\d+)\s*(year|month|day|hour)\)", RegexOptions.IgnoreCase);
                if (atm.Success == false)
                    throw new Exception(CoreStrings.AsTable_PropertyName_FormatError(AsTable));

                tb.AsTableColumn = tb.Columns.TryGetValue(atm.Groups[1].Value, out var trycol) ? trycol :
                    tb.ColumnsByCs.TryGetValue(atm.Groups[1].Value, out trycol) ? trycol : throw new Exception(CoreStrings.NotFound_Table_Property_AsTable(atm.Groups[1].Value));
                if (tb.AsTableColumn.Attribute.MapType.NullableTypeOrThis() != typeof(DateTime))
                {
                    tb.AsTableColumn = null;
                    throw new Exception(CoreStrings.AsTable_PropertyName_NotDateTime(atm.Groups[1].Value));
                }
                var beginTime = $"{atm.Groups[2].Value}-{atm.Groups[3].Value}-{atm.Groups[4].Value}";
                var atm5 = atm.Groups[5].Value;
                if (string.IsNullOrWhiteSpace(atm5) == false)
                {
                    var hhmmss = atm5.Split(':').Select(a => int.TryParse(a.Trim(), out var tryint) ? tryint : 0).ToArray();
                    if (hhmmss.Length == 1) beginTime = $"{beginTime} {hhmmss[0]}:0:0";
                    else if (hhmmss.Length == 2) beginTime = $"{beginTime} {hhmmss[0]}:{hhmmss[1]}:0";
                    else if (hhmmss.Length == 3) beginTime = $"{beginTime} {hhmmss[0]}:{hhmmss[1]}:{hhmmss[2]}";
                }
                int.TryParse(atm.Groups[6].Value, out var atm6);
                string atm7 = atm.Groups[7].Value.ToLower();
                tb.AsTableImpl = new DateTimeAsTableImpl(Name, DateTime.Parse(beginTime), dt =>
                {
                    switch (atm7)
                    {
                        case "year": return dt.AddYears(atm6);
                        case "month": return dt.AddMonths(atm6);
                        case "day": return dt.AddDays(atm6);
                        case "hour": return dt.AddHours(atm6);
                    }
                    throw new NotImplementedException(CoreStrings.Functions_AsTable_NotImplemented(AsTable));
                });
            }
        }
    }

    public interface IAsTable
    {
        string[] AllTables { get; }
        string GetTableNameByColumnValue(object columnValue, bool autoExpand = false);
        string[] GetTableNamesByColumnValueRange(object columnValue1, object columnValue2);
        string[] GetTableNamesBySqlWhere(string sqlWhere, List<DbParameter> dbParams, SelectTableInfo tb, CommonUtils commonUtils);
    }
    class DateTimeAsTableImpl : IAsTable
    {
        readonly object _lock = new object();
        readonly List<string> _allTables = new List<string>();
        readonly List<DateTime> _allTablesTime = new List<DateTime>();
        readonly DateTime _beginTime;
        DateTime _lastTime;
        Func<DateTime, DateTime> _nextTimeFunc;
        string _tableName;
        Match _tableNameFormat;
        static Regex _regTableNameFormat = new Regex(@"\{([^\\}]+)\}");

        public DateTimeAsTableImpl(string tableName, DateTime beginTime, Func<DateTime, DateTime> nextTimeFunc)
        {
            if (nextTimeFunc == null) throw new ArgumentException(CoreStrings.Cannot_Be_NULL_Name("nextTimeFunc"));
            //beginTime = beginTime.Date; //日期部分作为开始
            _beginTime = beginTime;
            _nextTimeFunc = nextTimeFunc;
            _tableName = tableName;
            _tableNameFormat = _regTableNameFormat.Match(tableName);
            if (string.IsNullOrEmpty(_tableNameFormat.Groups[1].Value)) throw new ArgumentException(CoreStrings.TableName_Format_Error("yyyyMMdd"));
            ExpandTable(beginTime, DateTime.Now);
        }

        int GetTimestamp(DateTime dt) => (int)dt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        void ExpandTable(DateTime beginTime, DateTime endTime)
        {
            if (beginTime > endTime) endTime = _nextTimeFunc(beginTime);
            lock (_lock)
            {
                while (beginTime <= endTime)
                {
                    var dtstr = beginTime.ToString(_tableNameFormat.Groups[1].Value);
                    var name = _tableName.Replace(_tableNameFormat.Groups[0].Value, dtstr);
                    if (_allTables.Contains(name)) throw new ArgumentException(CoreStrings.Generated_Same_SubTable(_tableName));
                    _allTables.Insert(0, name);
                    _allTablesTime.Insert(0, beginTime);
                    _lastTime = beginTime;
                    beginTime = _nextTimeFunc(beginTime);
                }
            }
        }
        DateTime ParseColumnValue(object columnValue)
        {
            if (columnValue == null) throw new Exception(CoreStrings.SubTableFieldValue_IsNotNull);
            DateTime dt;
            if (columnValue is DateTime || columnValue is DateTime?)
                dt = (DateTime)columnValue;
            else if (columnValue is string)
            {
                if (DateTime.TryParse(string.Concat(columnValue), out dt) == false) throw new Exception(CoreStrings.SubTableFieldValue_NotConvertDateTime(columnValue));
            }
            else if (columnValue is int || columnValue is long)
            {
                dt = new DateTime(1970, 1, 1).AddSeconds((double)columnValue);
            }
            else throw new Exception(CoreStrings.SubTableFieldValue_NotConvertDateTime(columnValue));
            return dt;
        }

        public string GetTableNameByColumnValue(object columnValue, bool autoExpand = false)
        {
            var dt = ParseColumnValue(columnValue);
            if (dt < _beginTime) throw new Exception(CoreStrings.SubTableFieldValue_CannotLessThen(dt.ToString("yyyy-MM-dd HH:mm:ss"), _beginTime.ToString("yyyy-MM-dd HH:mm:ss")));
            var tmpTime = _nextTimeFunc(_lastTime);
            if (dt >= tmpTime && autoExpand)
            {
                // 自动建表
                ExpandTable(tmpTime, dt);
            }
            lock (_lock)
            {
                var allTablesCount = _allTablesTime.Count;
                for (var a = 0; a < allTablesCount; a++)
                    if (dt >= _allTablesTime[a])
                        return _allTables[a];
            }
            throw new Exception(CoreStrings.SubTableFieldValue_NotMatchTable(dt.ToString("yyyy-MM-dd HH:mm:ss")));
        }
        public string[] GetTableNamesByColumnValueRange(object columnValue1, object columnValue2)
        {
            var dt1 = ParseColumnValue(columnValue1);
            var dt2 = ParseColumnValue(columnValue2);
            if (dt1 > dt2) return new string[0];

            lock (_lock)
            {
                int dt1idx = 0, dt2idx = 0;
                var allTablesCount = _allTablesTime.Count;
                if (dt1 < _beginTime) dt1idx = allTablesCount - 1;
                else
                {
                    for (var a = allTablesCount - 2; a > -1; a--)
                    {
                        if (dt1 < _allTablesTime[a])
                        {
                            dt1idx = a + 1;
                            break;
                        }
                    }
                }
                if (dt2 > _allTablesTime.First()) dt2idx = 0;
                else
                {
                    for (var a = 0; a < allTablesCount; a++)
                    {
                        if (dt2 >= _allTablesTime[a])
                        {
                            dt2idx = a;
                            break;
                        }
                    }
                }
                if (dt2idx == -1) return new string[0];

                if (dt1idx == allTablesCount - 1 && dt2idx == 0) return _allTables.ToArray();
                var names = _allTables.GetRange(dt2idx, dt1idx - dt2idx + 1).ToArray();
                return names;
            }
        }

        static readonly ConcurrentDictionary<string, Regex[]> _dicRegSqlWhereDateTimes = new ConcurrentDictionary<string, Regex[]>();
        static Regex[] GetRegSqlWhereDateTimes(string columnName, string quoteParameterName)
        {
            return _dicRegSqlWhereDateTimes.GetOrAdd($"{columnName},{quoteParameterName}", cn =>
            {
                cn = columnName.Replace("[", "\\[").Replace("]", "\\]").Replace(".", "\\.").Replace("?", "\\?");
                var qpn = quoteParameterName.Replace("[", "\\[").Replace("]", "\\]").Replace(".", "\\.").Replace("?", "\\?");
                return new[]
                {
                    new Regex($@"(\s*)(datetime|cdate|to_date)(\s*)\(\s*({qpn}[\w_]+)\s*\)", RegexOptions.IgnoreCase),
                    new Regex($@"(\s*)(to_timestamp)(\s*)\(\s*({qpn}[\w_]+)\s*,\s*{qpn}[\w_]+\s*\)", RegexOptions.IgnoreCase),
                    new Regex($@"(\s*)(cast)(\s*)\(\s*({qpn}[^w_]+)\s+as\s+(datetime|timestamp)\s*\)", RegexOptions.IgnoreCase),
                    new Regex($@"({qpn}[^w_]+)(\s*)(::)(\s*)(datetime|timestamp)", RegexOptions.IgnoreCase),
                    new Regex($@"(\s*)(timestamp)(\s*)({qpn}[\w_]+)", RegexOptions.IgnoreCase), //firebird

                    new Regex($@"{cn}\s*between\s*'([^']+)'\s*and\s*'([^']+)'", RegexOptions.IgnoreCase), //预留暂时不用
                    new Regex($@"{cn}\s*between\s*{qpn}([\w_]+)\s*and\s*{qpn}([\w_]+)", RegexOptions.IgnoreCase),

                    new Regex($@"{cn}\s*(<|<=|>|>=)\s*'([^']+)'\s*and\s*{cn}\s*(<|<=|>|>=)\s*'([^']+)'", RegexOptions.IgnoreCase), //预留暂时不用
                    new Regex($@"{cn}\s*(<|<=|>|>=)\s*{qpn}([\w_]+)\s*and\s*{cn}\s*(<|<=|>|>=)\s*{qpn}([\w_]+)", RegexOptions.IgnoreCase),

                    new Regex($@"{cn}\s*(=|<|<=|>|>=)\s*'([^']+)'", RegexOptions.IgnoreCase), //预留暂时不用
                    new Regex($@"{cn}\s*(=|<|<=|>|>=)\s*{qpn}([\w_]+)", RegexOptions.IgnoreCase),
                };
            });
        }
        /// <summary>
        /// 可以匹配以下条件（支持参数化）：<para></para>
        /// `field` BETWEEN '2022-01-01 00:00:00' AND '2022-03-01 00:00:00'<para></para>
        /// `field` &gt; '2022-01-01 00:00:00' AND `field` &lt; '2022-03-01 00:00:00'<para></para>
        /// `field` &gt; '2022-01-01 00:00:00' AND `field` &lt;= '2022-03-01 00:00:00'<para></para>
        /// `field` &gt;= '2022-01-01 00:00:00' AND `field` &lt; '2022-03-01 00:00:00'<para></para>
        /// `field` &gt;= '2022-01-01 00:00:00' AND `field` &lt;= '2022-03-01 00:00:00'<para></para>
        /// `field` &gt; '2022-01-01 00:00:00'<para></para>
        /// `field` &gt;= '2022-01-01 00:00:00'<para></para>
        /// `field` &lt; '2022-01-01 00:00:00'<para></para>
        /// `field` &lt;= '2022-01-01 00:00:00'<para></para>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string[] GetTableNamesBySqlWhere(string sqlWhere, List<DbParameter> dbParams, SelectTableInfo tb, CommonUtils commonUtils)
        {
            if (string.IsNullOrWhiteSpace(sqlWhere)) return AllTables;
            var quoteParameterName = "";
            if (commonUtils._orm.Ado.DataType == DataType.ClickHouse) quoteParameterName = "@"; //特殊处理 Clickhouse 参数化
            else quoteParameterName = commonUtils.QuoteParamterName("");
            var quoteParameterNameCharArray = quoteParameterName.ToCharArray();
            var columnName = commonUtils.QuoteSqlName(tb.Table.AsTableColumn.Attribute.Name);

            var dictParams = new Dictionary<string, string>();
            var newSqlWhere = Utils.ReplaceSqlConstString(sqlWhere, dictParams, quoteParameterName);
            //var tsqlWhere = Utils.ParseSqlWhereLevel1(sqlWhere);

            var regs = GetRegSqlWhereDateTimes($"{(string.IsNullOrWhiteSpace(tb.Alias) ? "" : $"{tb.Alias}.")}{commonUtils.QuoteSqlName(tb.Table.AsTableColumn.Attribute.Name)}", quoteParameterName);
            for (var a = 0; a < 5; a++) newSqlWhere = regs[a].Replace(newSqlWhere, "$1$4");

            //var m = regs[5].Match(newSqlWhere);
            //if (m.Success) return GetTableNamesByColumnValueRange(m.Groups[1].Value, m.Groups[2].Value);
            //m = m = regs[7].Match(newSqlWhere);
            //if (m.Success) return LocalGetTables(m.Groups[1].Value, m.Groups[3].Value, ParseColumnValue(m.Groups[2].Value), ParseColumnValue(m.Groups[4].Value));
            //m = regs[9].Match(newSqlWhere);
            //if (m.Success) return LocalGetTables2(m.Groups[1].Value, ParseColumnValue(m.Groups[2].Value));

            var m = regs[6].Match(newSqlWhere);
            if (m.Success)
            {
                var val1 = LocalGetParamValue(m.Groups[1].Value);
                var val2 = LocalGetParamValue(m.Groups[2].Value);
                if (val1 == null || val2 == null) throw new Exception(CoreStrings.Failed_SubTable_FieldValue(sqlWhere));
                return GetTableNamesByColumnValueRange(val1, val2);
            }
            m = regs[8].Match(newSqlWhere);
            if (m.Success)
            {
                var val1 = LocalGetParamValue(m.Groups[2].Value);
                var val2 = LocalGetParamValue(m.Groups[4].Value);
                if (val1 == null || val2 == null) throw new Exception(CoreStrings.Failed_SubTable_FieldValue(sqlWhere));
                return LocalGetTables(m.Groups[1].Value, m.Groups[3].Value, ParseColumnValue(val1), ParseColumnValue(val2));
            }
            m = regs[10].Match(newSqlWhere);
            if (m.Success)
            {
                var val1 = LocalGetParamValue(m.Groups[2].Value);
                if (val1 == null) throw new Exception(CoreStrings.Failed_SubTable_FieldValue(sqlWhere));
                return LocalGetTables2(m.Groups[1].Value, ParseColumnValue(val1));
            }
            return AllTables;

            object LocalGetParamValue(string paramName)
            {
                if (dictParams.TryGetValue(quoteParameterName + paramName, out var trydictVal)) return trydictVal;
                return dbParams.Where(a => a.ParameterName.Trim(quoteParameterNameCharArray) == m.Groups[2].Value).FirstOrDefault()?.Value;
            }
            string[] LocalGetTables(string opt1, string opt2, DateTime val1, DateTime val2)
            {
                switch (opt1)
                {
                    case "<":
                    case "<=":
                        if (opt1 == "<") val1 = val1.AddSeconds(-1);
                        switch (opt2)
                        {
                            case "<":
                                val2 = val2.AddSeconds(-1);
                                return GetTableNamesByColumnValueRange(_beginTime, val1 > val2 ? val2 : val1);
                            case "<=":
                                return GetTableNamesByColumnValueRange(_beginTime, val1 > val2 ? val2 : val1);
                            case ">":
                                val2 = val2.AddSeconds(1);
                                return GetTableNamesByColumnValueRange(val2, val1);
                            case ">=":
                                return GetTableNamesByColumnValueRange(val2, val1);
                        }
                        break;
                    case ">":
                    case ">=":
                        if (opt1 == ">") val1 = val1.AddSeconds(1);
                        switch (opt2)
                        {
                            case "<":
                                val2 = val2.AddSeconds(-1);
                                return GetTableNamesByColumnValueRange(val1, val2);
                            case "<=":
                                return GetTableNamesByColumnValueRange(val1, val2);
                            case ">":
                                val2 = val2.AddSeconds(1);
                                return GetTableNamesByColumnValueRange(val1 > val2 ? val1 : val2, _lastTime);
                            case ">=":
                                return GetTableNamesByColumnValueRange(val1 > val2 ? val1 : val2, _lastTime);
                        }
                        break;
                }
                return AllTables;
            }
            string[] LocalGetTables2(string opt, DateTime val1)
            {
                switch (m.Groups[1].Value)
                {
                    case "=":
                        return GetTableNamesByColumnValueRange(val1, val1);
                    case "<":
                        val1 = val1.AddSeconds(-1);
                        return GetTableNamesByColumnValueRange(_beginTime, val1);
                    case "<=":
                        return GetTableNamesByColumnValueRange(_beginTime, val1);
                    case ">":
                        val1 = val1.AddSeconds(1);
                        return GetTableNamesByColumnValueRange(val1, _lastTime);
                    case ">=":
                        return GetTableNamesByColumnValueRange(val1, _lastTime);
                }
                return AllTables;
            }
        }

        public string[] AllTables
        {
            get
            {
                lock (_lock)
                {
                    return _allTables.ToArray();
                }
            }
        }
    }
}
