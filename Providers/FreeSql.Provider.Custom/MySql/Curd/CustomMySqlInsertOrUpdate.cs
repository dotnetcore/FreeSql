using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Custom.MySql
{

    class CustomMySqlInsertOrUpdate<T1> : Internal.CommonProvider.InsertOrUpdateProvider<T1> where T1 : class
    {
        public CustomMySqlInsertOrUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override string ToSql()
        {
            var dbParams = new List<DbParameter>();
            if (_sourceSql != null)
            {
                var data = new List<T1>();
                data.Add((T1)_table.Type.CreateInstanceGetDefaultValue());
                var sql = getInsertSql(data, false, false);
                var sb = new StringBuilder();
                sb.Append(sql.Substring(0, sql.IndexOf(") VALUES")));
                sb.Append(") \r\n");
                WriteSourceSelectUnionAll(null, sb, null);
                if (_doNothing == false)
                    sb.Append(sql.Substring(sql.IndexOf("\r\nON DUPLICATE KEY UPDATE\r\n") + 2));
                else
                    throw new Exception("Not implemented! fsql.InsertOrUpdate + SetSource(sql) + IfExistsDoNothing + MySql");
                return sb.ToString();
            }
            if (_source?.Any() != true) return null;

            var sqls = new string[2];
            var ds = SplitSourceByIdentityValueIsNull(_source);
            if (ds.Item1.Any()) sqls[0] = string.Join("\r\n\r\n;\r\n\r\n", ds.Item1.Select(a => getInsertSql(a, false, true)));
            if (ds.Item2.Any()) sqls[1] = string.Join("\r\n\r\n;\r\n\r\n", ds.Item2.Select(a => getInsertSql(a, true, true)));
            _params = dbParams.ToArray();
            if (ds.Item2.Any() == false) return sqls[0];
            if (ds.Item1.Any() == false) return sqls[1];
            return string.Join("\r\n\r\n;\r\n\r\n", sqls);

            string getInsertSql(List<T1> data, bool flagInsert, bool noneParameter)
            {
                var insert = _orm.Insert<T1>()
                    .AsTable(_tableRule).AsType(_table.Type)
                    .WithConnection(_connection)
                    .WithTransaction(_transaction)
                    .NoneParameter(noneParameter) as Internal.CommonProvider.InsertProvider<T1>;
                insert._source = data;
                insert._table = _table;
                insert._noneParameterFlag = flagInsert ? "cuc" : "cu";

                string sql = "";
                if (IdentityColumn != null && flagInsert) sql = insert.ToSql();
                else
                {
                    insert.InsertIdentity();
                    if (_doNothing == false)
                    {
                        var cols = _table.Columns.Values.Where(a => _updateSetDict.ContainsKey(a.Attribute.Name) ||
                            _tempPrimarys.Contains(a) == false && a.Attribute.CanUpdate == true && a.Attribute.IsIdentity == false && _updateIgnore.ContainsKey(a.Attribute.Name) == false);
                        sql = new CustomMySqlOnDuplicateKeyUpdate<T1>(insert)
                            .UpdateColumns(cols.Select(a => a.Attribute.Name).ToArray())
                            .ToSql();
                        if (_updateSetDict.Any())
                        {
                            var findregex = new Regex("(t1|t2)." + _commonUtils.QuoteSqlName("test").Replace("test", "(\\w+)"));
                            foreach (var usd in _updateSetDict)
                            {
                                var field = _commonUtils.QuoteSqlName(usd.Key);
                                var findsql = $"{field} = VALUES({field})";
                                var usdval = findregex.Replace(usd.Value, m =>
                                {
                                    if (m.Groups[1].Value == "t1") return _commonUtils.QuoteSqlName(m.Groups[2].Value);
                                    return $"VALUES({_commonUtils.QuoteSqlName(m.Groups[2].Value)})";
                                });
                                sql = sql.Replace(findsql, $"{field} = {usdval}");
                            }
                        }
                    }
                    else
                    {
                        if (_tempPrimarys.Any() == false) throw new Exception(CoreStrings.Entity_Must_Primary_Key("fsql.InsertOrUpdate + IfExistsDoNothing + MySql ", _table.CsName));
                        sql = insert.ToSqlValuesOrSelectUnionAll();
                        if (sql?.StartsWith("INSERT INTO ") == true)
                            sql = $"INSERT IGNORE INTO {sql.Substring(12)}";
                        //sql = insert.ToSqlValuesOrSelectUnionAllExtension101(false, (rowd, idx, sb) =>
                        //{
                        //    sb.Append(" \r\n FROM dual WHERE NOT EXISTS(");
                        //    if (typeof(T1) == typeof(Dictionary<string, object>) && rowd is T1 dict)
                        //        sb.Append($"SELECT 1 FROM {_commonUtils.QuoteSqlName(_tableRule(null))} WHERE {_commonUtils.WhereItems<T1>(_tempPrimarys, "", new T1[] { dict })})");
                        //    else
                        //        sb.Append(
                        //            _orm.Select<T1>()
                        //            .AsTable((_, __) => _tableRule?.Invoke(__)).AsType(_table.Type)
                        //            .DisableGlobalFilter()
                        //            .WhereDynamic(rowd)
                        //            .Limit(1).ToSql("1").Replace(" \r\n", " \r\n    ")).Append(")");
                        //});
                    }
                }
                if (string.IsNullOrEmpty(sql)) return null;
                if (insert._params?.Any() == true) dbParams.AddRange(insert._params);
                return sql;
            }
        }
    }
}