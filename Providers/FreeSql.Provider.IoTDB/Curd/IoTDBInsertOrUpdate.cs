using System;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;

namespace FreeSql.IoTDB.Curd
{
    internal sealed class IoTDBInsertOrUpdate<T1> : InsertOrUpdateProvider<T1> where T1 : class
    {
        public IoTDBInsertOrUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression) { }

        public override string ToSql() => throw new NotSupportedException("IoTDB Tree does not support atomic INSERT ... ON CONFLICT/UPSERT. Use Insert for point writes.");
    }
}
