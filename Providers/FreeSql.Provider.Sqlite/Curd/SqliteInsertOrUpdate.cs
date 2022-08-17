using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace FreeSql.Sqlite.Curd
{

    class SqliteInsertOrUpdate<T1> : Internal.CommonProvider.InsertOrUpdateProvider<T1> where T1 : class
    {
        public SqliteInsertOrUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override IInsertOrUpdate<T1> SetSource(string sql, Expression<Func<T1, object>> tempPrimarys = null)
        {
            var list = _orm.Ado.CommandFluent(sql)
                .WithConnection(_connection)
                .WithTransaction(_transaction)
                .Query<T1>();
            return SetSource(list, tempPrimarys);
        }

        public override string ToSql()
        {
            if (_source?.Any() != true) return null;

            var sqls = new string[2];
            var dbParams = new List<DbParameter>();
            var ds = SplitSourceByIdentityValueIsNull(_source);
            if (ds.Item1.Any()) sqls[0] = string.Join("\r\n\r\n;\r\n\r\n", ds.Item1.Select(a => getInsertSql(a, false)));
            if (ds.Item2.Any()) sqls[1] = string.Join("\r\n\r\n;\r\n\r\n", ds.Item2.Select(a => getInsertSql(a, true)));
            _params = dbParams.ToArray();
            if (ds.Item2.Any() == false) return sqls[0];
            if (ds.Item1.Any() == false) return sqls[1];
            return string.Join("\r\n\r\n;\r\n\r\n", sqls);

            string getInsertSql(List<T1> data, bool flagInsert)
            {
                var insert = _orm.Insert<T1>()
                    .AsTable(_tableRule).AsType(_table.Type)
                    .WithConnection(_connection)
                    .WithTransaction(_transaction)
                    .NoneParameter(true) as Internal.CommonProvider.InsertProvider<T1>;
                insert._noneParameterFlag = flagInsert ? "c" : "cu";
                insert._source = data;
                insert._table = _table;

                string sql = "";
                if (IdentityColumn != null && flagInsert) sql = insert.ToSql();
                else
                {
                    insert.InsertIdentity();
                    if (_doNothing == false)
                    {
                        if (_updateIgnore.Any()) throw new Exception(CoreStrings.S_InsertOrUpdate_Unable_UpdateColumns);
                        sql = insert.ToSql();
                        if (sql?.StartsWith("INSERT INTO ") == true)
                            sql = $"REPLACE INTO {sql.Substring("INSERT INTO ".Length)}";
                    }
                    else
                    {
                        if (_tempPrimarys.Any() == false) throw new Exception(CoreStrings.Entity_Must_Primary_Key("fsql.InsertOrUpdate + IfExistsDoNothing + Sqlite ", _table.CsName));
                        sql = insert.ToSqlValuesOrSelectUnionAllExtension101(false, (rowd, idx, sb) =>
                            sb.Append(" \r\n WHERE NOT EXISTS(").Append(
                                _orm.Select<T1>()
                                .AsTable((_, __) => _tableRule?.Invoke(__)).AsType(_table.Type)
                                .DisableGlobalFilter()
                                .WhereDynamic(rowd)
                                .Limit(1).ToSql("1").Replace(" \r\n", " \r\n    ")).Append(")"));
                    }
                }
                if (string.IsNullOrEmpty(sql)) return null;
                if (insert._params?.Any() == true) dbParams.AddRange(insert._params);
                return sql;
            }
        }
    }
}