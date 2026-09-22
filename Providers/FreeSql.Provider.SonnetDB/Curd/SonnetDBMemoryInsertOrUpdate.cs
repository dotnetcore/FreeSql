using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;

namespace FreeSql.SonnetDB.Curd
{
    /// <summary>
    /// Adapter for FreeSql's existing Select.WithMemory implementation.
    /// The base reader uses InsertOrUpdateProvider only to serialize rows as
    /// SELECT/UNION ALL; no SonnetDB upsert SQL is ever executed here.
    /// </summary>
    sealed class SonnetDBMemoryInsertOrUpdate : InsertOrUpdateProvider<object>
    {
        public SonnetDBMemoryInsertOrUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression) { }

        public override string ToSql() => throw new System.NotSupportedException(
            SonnetDBProvider<object>.UnsupportedUpsertMessage);
    }
}
