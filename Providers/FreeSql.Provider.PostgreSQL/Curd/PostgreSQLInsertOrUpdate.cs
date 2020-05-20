using FreeSql.Internal;
using System.Linq;

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
            if (_source?.Any() != true) return null;

            var insert = _orm.Insert<T1>()
                .AsTable(_tableRule).AsType(_table.Type)
                .WithConnection(_connection)
                .WithTransaction(_transaction)
                .NoneParameter(true) as Internal.CommonProvider.InsertProvider<T1>;
            insert._source = _source;
            var ocdu = new OnConflictDoUpdate<T1>(insert);
            ocdu.IgnoreColumns(_table.Columns.Values.Where(a => a.Attribute.CanUpdate == false).Select(a => a.Attribute.Name).ToArray());
            if (_table.Columns.Values.Where(a => a.Attribute.IsPrimary == false && a.Attribute.CanUpdate == true).Any() == false)
                ocdu.DoNothing();
            var sql = ocdu.ToSql();
            _params = insert._params;
            return sql;
        }
    }
}