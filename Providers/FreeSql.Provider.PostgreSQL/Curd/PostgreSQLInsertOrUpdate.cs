using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.PostgreSQL.Curd
{

    class PostgreSQLInsertOrUpdate<T1> : Internal.CommonProvider.InsertOrUpdateProvider<T1> where T1 : class
    {
        public PostgreSQLInsertOrUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override string ToSql()
        {
            if ((_orm as IPostgreSQLProviderOptions)?.UseMergeInto == true) return ToSqlMergeInto();
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
                sb.Append(sql.Substring(sql.IndexOf("\r\nON CONFLICT(") + 2));
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
                    var ocdu = new OnConflictDoUpdate<T1>(insert.InsertIdentity());
                    ocdu._tempPrimarys = _tempPrimarys;
                    var cols = _table.Columns.Values.Where(a => _updateSetDict.ContainsKey(a.Attribute.Name) ||
                        _tempPrimarys.Contains(a) == false && a.Attribute.CanUpdate == true && a.Attribute.IsIdentity == false && _updateIgnore.ContainsKey(a.Attribute.Name) == false);
                    ocdu.UpdateColumns(cols.Select(a => a.Attribute.Name).ToArray());
                    if (_doNothing == true || cols.Any() == false)
                        ocdu.DoNothing();
                    sql = ocdu.ToSql();

                    if (_updateSetDict.Any())
                    {
                        var findregex = new Regex("(t1|t2)." + _commonUtils.QuoteSqlName("test").Replace("test", "(\\w+)"));
                        var tableName = _commonUtils.QuoteSqlName(TableRuleInvoke());
                        foreach (var usd in _updateSetDict)
                        {
                            var field = _commonUtils.QuoteSqlName(usd.Key);
                            var findsql = $"{field} = EXCLUDED.{field}";
                            var usdval = findregex.Replace(usd.Value, m =>
                            {
                                if (m.Groups[1].Value == "t1") return $"{tableName}.{_commonUtils.QuoteSqlName(m.Groups[2].Value)}";
                                return $"EXCLUDED.{_commonUtils.QuoteSqlName(m.Groups[2].Value)}";
                            });
                            sql = sql.Replace(findsql, $"{field} = {usdval}");
                        }
                    }
                }
                if (string.IsNullOrEmpty(sql)) return null;
                if (insert._params?.Any() == true) dbParams.AddRange(insert._params);
                return sql;
            }
        }

        public string ToSqlMergeInto()
        {
            var dbParams = new List<DbParameter>();
            if (_sourceSql != null) return getMergeSql(null);
            if (_source?.Any() != true) return null;

            var sqls = new string[2];
            var ds = SplitSourceByIdentityValueIsNull(_source);
            if (ds.Item1.Any()) sqls[0] = string.Join("\r\n\r\n;\r\n\r\n", ds.Item1.Select(a => getMergeSql(a)));
            if (ds.Item2.Any()) sqls[1] = string.Join("\r\n\r\n;\r\n\r\n", ds.Item2.Select(a => getInsertSql(a)));
            _params = dbParams.ToArray();
            if (ds.Item2.Any() == false) return sqls[0];
            if (ds.Item1.Any() == false) return sqls[1];
            return string.Join("\r\n\r\n;\r\n\r\n", sqls);

            string getMergeSql(List<T1> data)
            {
                if (_tempPrimarys.Any() == false) throw new Exception(CoreErrorStrings.InsertOrUpdate_Must_Primary_Key(_table.CsName));

                var tempPrimaryIsIdentity = _tempPrimarys.Any(b => b.Attribute.IsIdentity);
                var sb = new StringBuilder();
                if (IdentityColumn != null && tempPrimaryIsIdentity) sb.Append("SET IDENTITY_INSERT ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke())).Append(" ON;\r\n");
                sb.Append("MERGE INTO ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke())).Append(" t1 \r\nUSING (");
                WriteSourceSelectUnionAll(data, sb, dbParams);
                sb.Append(" ) t2 ON (").Append(string.Join(" AND ", _tempPrimarys.Select(a => $"t1.{_commonUtils.QuoteSqlName(a.Attribute.Name)} = t2.{_commonUtils.QuoteSqlName(a.Attribute.Name)}"))).Append(") \r\n");

                var cols = _table.Columns.Values.Where(a => _updateSetDict.ContainsKey(a.Attribute.Name) ||
                    _tempPrimarys.Contains(a) == false && a.Attribute.CanUpdate == true && a.Attribute.IsIdentity == false && _updateIgnore.ContainsKey(a.Attribute.Name) == false);
                if (_doNothing == false && cols.Any())
                    sb.Append("WHEN MATCHED THEN \r\n")
                        .Append("  update set ").Append(string.Join(", ", cols.Select(a =>
                        {
                            if (_updateSetDict.TryGetValue(a.Attribute.Name, out var valsql))
                                return $"{_commonUtils.QuoteSqlName(a.Attribute.Name)} = {valsql}";
                            return a.Attribute.IsVersion && a.Attribute.MapType != typeof(byte[]) ?
                                $"{_commonUtils.QuoteSqlName(a.Attribute.Name)} = t1.{_commonUtils.QuoteSqlName(a.Attribute.Name)} + 1" :
                                $"{_commonUtils.QuoteSqlName(a.Attribute.Name)} = t2.{_commonUtils.QuoteSqlName(a.Attribute.Name)}";
                        }))).Append(" \r\n");

                cols = _table.Columns.Values.Where(a => a.Attribute.CanInsert == true);
                if (tempPrimaryIsIdentity == false) cols = cols.Where(a => a.Attribute.IsIdentity == false || string.IsNullOrEmpty(a.DbInsertValue) == false);
                if (cols.Any())
                    sb.Append("WHEN NOT MATCHED THEN \r\n")
                        .Append("  insert (").Append(string.Join(", ", cols.Select(a => _commonUtils.QuoteSqlName(a.Attribute.Name)))).Append(") \r\n")
                        .Append("  values (").Append(string.Join(", ", cols.Select(a =>
                        {
                            //InsertValueSql = "seq.nextval"
                            if (tempPrimaryIsIdentity == false && a.Attribute.IsIdentity && string.IsNullOrEmpty(a.DbInsertValue) == false) return a.DbInsertValue;
                            return $"t2.{_commonUtils.QuoteSqlName(a.Attribute.Name)}";
                        }))).Append(");");

                if (IdentityColumn != null && tempPrimaryIsIdentity) sb.Append(";\r\nSET IDENTITY_INSERT ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke())).Append(" OFF;");

                return sb.ToString();
            }
            string getInsertSql(List<T1> data)
            {
                var insert = _orm.Insert<T1>()
                    .AsTable(_tableRule).AsType(_table.Type)
                    .WithConnection(_connection)
                    .WithTransaction(_transaction)
                    .NoneParameter(true) as Internal.CommonProvider.InsertProvider<T1>;
                insert._source = data;
                insert._table = _table;
                var sql = insert.ToSql();
                if (string.IsNullOrEmpty(sql)) return null;
                if (insert._params?.Any() == true) dbParams.AddRange(insert._params);
                return sql;
            }
        }
    }
}