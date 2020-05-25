using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace FreeSql.Sqlite.Curd
{

    class SqliteInsertOrUpdate<T1> : Internal.CommonProvider.InsertOrUpdateProvider<T1> where T1 : class
    {
        public SqliteInsertOrUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override string ToSql()
        {
            if (_source?.Any() != true) return null;

            var sqls = new string[2];
            var dbParams = new List<DbParameter>();
            var ds = SplitSourceByIdentityValueIsNull(_source);
            if (ds.Item1.Any()) sqls[0] = getInsertSql(ds.Item1, false);
            if (ds.Item2.Any()) sqls[1] = getInsertSql(ds.Item2, true);
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
                insert._source = data;

                if (IdentityColumn != null && flagInsert == false) insert.InsertIdentity();

                var sql = insert.ToSql();
                if (string.IsNullOrEmpty(sql)) return null;
                if (insert._params?.Any() == true) dbParams.AddRange(insert._params);
                if (IdentityColumn != null && flagInsert) return sql;

                if (sql.StartsWith("INSERT INTO ") == false) return null;
                return $"REPLACE INTO {sql.Substring("INSERT INTO ".Length)}";
            }
        }
    }
}