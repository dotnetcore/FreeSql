using FreeSql.Internal;
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

            var insert = _orm.Insert<T1>()
                .AsTable(_tableRule).AsType(_table.Type)
                .WithConnection(_connection)
                .WithTransaction(_transaction)
                .NoneParameter(true) as Internal.CommonProvider.InsertProvider<T1>;
            insert._source = _source;
            var sql = insert.ToSql();
            if (sql.StartsWith("INSERT INTO ") == false) return null;
            _params = insert._params;
            return $"REPLACE INTO {sql.Substring("INSERT INTO ".Length)}";
        }
    }
}