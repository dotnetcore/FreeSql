using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.SonnetDB.Curd;
using System;
using System.Data.Common;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using SonnetDB.Sql;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// SonnetDB 的 FreeSql 提供程序。
    /// 标记 <see cref="Provider.SonnetDB.Attributes.SonnetDBTableAttribute"/> 的实体使用
    /// 关系表语义；未标记的旧实体继续使用时序测量语义。
    /// </summary>
    public class SonnetDBProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) =>
            new SonnetDBSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsert<T1> CreateInsertProvider<T1>() =>
            new SonnetDBInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) =>
            new SonnetDBUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) =>
            new SonnetDBDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() =>
            new SonnetDBInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public SonnetDBProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            SonnetDBTypeHandlers.EnsureRegistered();
            SonnetDBVectorTypeHandlers.EnsureRegistered();
            SonnetDBJsonTypeHandlers.EnsureRegistered();
            this.InternalCommonUtils = new SonnetDBUtils(this);
            this.InternalCommonExpression = new SonnetDBExpression(this.InternalCommonUtils);
            this.Ado = new SonnetDBAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();
            ConfigureAopCompatibility();
            ConfigureDataReader();
            this.DbFirst = new SonnetDBDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new SonnetDBCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        void ConfigureAopCompatibility()
        {
            // SonnetDB JSON path indexes are provider metadata. Project them
            // into the old IndexInfo model only as ordinary indexes; the JSON
            // path itself remains private to SonnetDBCodeFirst.
            this.Aop.ConfigEntity += (_, args) =>
            {
                var attributes = args.EntityType
                    .GetCustomAttributes(typeof(Provider.SonnetDB.Attributes.SonnetDBJsonIndexAttribute), true)
                    .OfType<Provider.SonnetDB.Attributes.SonnetDBJsonIndexAttribute>();
                foreach (var attribute in attributes)
                {
                    if (args.ModifyIndexResult.Any(a => string.Equals(a.Name, attribute.Name, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    args.ModifyIndexResult.Add(new DataAnnotations.IndexAttribute(
                        attribute.Name, attribute.Fields, attribute.IsUnique));
                }
            };
            this.Aop.ConfigEntityProperty += (_, args) =>
            {
                var type = args.Property.PropertyType.NullableTypeOrThis();
                if (type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly) || type == typeof(TimeSpan))
                    args.ModifyResult.RereadSql = "{0}";
            };

            // SonnetDB 4.5 supports FOR UPDATE [NOWAIT] for a single relational
            // base table in an explicit transaction. Keep only the syntax that
            // the engine still rejects guarded at the provider boundary.
            this.Aop.CurdBefore += (_, args) =>
            {
                if (ContainsSkipLocked(args.Sql))
                    throw new NotSupportedException("SonnetDB 仅支持 FOR UPDATE [NOWAIT]，不支持 SKIP LOCKED。");
            };
            // The legacy ambient transaction helper reports commit failures only
            // through TraceAfter. Preserve SonnetDB's commit failure contract
            // without changing the shared transaction implementation.
            this.Aop.TraceAfter += (_, args) =>
            {
                if (args.Operation == "ThreadTransaction" &&
                    args.Remark == CoreErrorStrings.Commit && args.Exception != null)
                    ExceptionDispatchInfo.Capture(args.Exception).Throw();
            };
            this.Aop.ParseExpression += (_, args) =>
            {
                if (SonnetDBExpression.TryTranslateAopExpression(args.Expression, args.FreeParse, out var sql))
                    args.Result = sql;
            };
        }

        static bool ContainsSkipLocked(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return false;
            var tokens = SqlLexer.Tokenize(sql);
            for (var index = 0; index < tokens.Count; index++)
                if (tokens[index].Kind == TokenKind.IdentifierLiteral &&
                    string.Equals(tokens[index].Text, "skip", StringComparison.OrdinalIgnoreCase) &&
                    index + 1 < tokens.Count && tokens[index + 1].Kind == TokenKind.IdentifierLiteral &&
                    string.Equals(tokens[index + 1].Text, "locked", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        void ConfigureDataReader()
        {
            // Older FreeSql releases only allow reader overrides for instance
            // DbDataReader methods. SonnetDB temporal values are returned as
            // integer milliseconds, so use the existing AOP reader hook to
            // convert them without changing FreeSql's common reader model.
            this.Aop.AuditDataReader += (_, args) =>
            {
                if (args.Value == null || args.Value == DBNull.Value) return;
                var targetType = args.Property?.PropertyType.NullableTypeOrThis();
                if (targetType == typeof(DateTimeOffset))
                    args.Value = SonnetDBUtils.ConvertDateTimeOffsetValue(args.Value);
                else if (targetType == typeof(DateTime))
                    args.Value = SonnetDBUtils.ConvertDateTimeValue(args.Value);
                else if (targetType == typeof(DateOnly))
                    args.Value = args.Value is DateOnly day ? day : DateOnly.FromDateTime(SonnetDBUtils.ConvertDateTimeValue(args.Value));
                else if (targetType == typeof(TimeSpan) && args.Value is long ticks)
                    args.Value = TimeSpan.FromTicks(ticks);
            };
        }

        ~SonnetDBProvider() => this.Dispose();
        int _disposeCounter;

        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
