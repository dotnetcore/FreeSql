using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.SonnetDB.Curd;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// SonnetDB 3.1 的 FreeSql 提供程序。
    /// 标记 <see cref="Provider.SonnetDB.Attributes.SonnetDBTableAttribute"/> 的实体使用
    /// 关系表语义；未标记的旧实体继续使用时序测量语义。
    /// </summary>
    public class SonnetDBProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        static readonly AsyncLocal<HashSet<string>> _temporalResultAliases = new AsyncLocal<HashSet<string>>();
        internal const string UnsupportedUpsertMessage =
            "SonnetDB 3.1 尚未提供原子的 INSERT ... ON CONFLICT/UPSERT 操作，" +
            "因此 FreeSql InsertOrUpdate 不可用；如能接受竞态风险，请在显式事务中分别执行插入和更新。";

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) =>
            new SonnetDBSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsert<T1> CreateInsertProvider<T1>() =>
            new SonnetDBInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) =>
            new SonnetDBUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) =>
            new SonnetDBDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() =>
            typeof(T1) == typeof(object)
                ? (IInsertOrUpdate<T1>)(object)new SonnetDBMemoryInsertOrUpdate(this, this.InternalCommonUtils, this.InternalCommonExpression)
                : throw new NotSupportedException(UnsupportedUpsertMessage);

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

            // These SQL forms have no stable SonnetDB 3.1 contract. The old
            // FreeSql core exposes some of their entry points as non-virtual,
            // so reject them at the provider's last pre-execution boundary.
            this.Aop.CurdBefore += (_, args) =>
            {
                var sql = StripSqlLiterals(args.Sql ?? string.Empty);
                _temporalResultAliases.Value = ExtractTemporalResultAliases(args.Sql);
                if (sql.IndexOf("UNION ALL", StringComparison.OrdinalIgnoreCase) >= 0)
                    throw new NotSupportedException("SonnetDB 3.1 不支持 UNION ALL；请改写为单条查询或分步处理。");
                if (sql.IndexOf("FOR UPDATE", StringComparison.OrdinalIgnoreCase) >= 0)
                    throw new NotSupportedException("SonnetDB 3.1 不支持 SELECT ... FOR UPDATE。");
                if (sql.IndexOf("INSERT INTO", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    sql.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase) >= 0)
                    throw new NotSupportedException("SonnetDB 3.1 不支持 INSERT ... SELECT；请改用分批 INSERT。");
                if (sql.TrimStart().StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
                    throw new NotSupportedException("SonnetDB 3.1 不支持 WITH/递归 CTE 查询；请改写为普通查询或分步处理。");
            };
            this.Aop.CurdAfter += (_, __) => _temporalResultAliases.Value = null;

            this.Aop.ParseExpression += (_, args) =>
            {
                if (SonnetDBExpression.TryTranslateAopExpression(args.Expression, args.FreeParse, out var sql))
                    args.Result = sql;
            };
        }

        static string StripSqlLiterals(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return string.Empty;
            var result = new char[sql.Length];
            var quote = '\0';
            for (var index = 0; index < sql.Length; index++)
            {
                var current = sql[index];
                if (quote != '\0')
                {
                    result[index] = ' ';
                    if (current == quote)
                    {
                        if (index + 1 < sql.Length && sql[index + 1] == quote)
                            result[++index] = ' ';
                        else quote = '\0';
                    }
                    continue;
                }
                if (current == '\'' || current == '"')
                {
                    quote = current;
                    result[index] = ' ';
                    continue;
                }
                result[index] = current;
            }
            return new string(result);
        }

        static HashSet<string> ExtractTemporalResultAliases(string sql)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(sql)) return result;
            var clean = StripSqlLiterals(sql);
            var selectIndex = clean.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
            if (selectIndex < 0) return result;
            var fromIndex = clean.IndexOf("FROM", selectIndex + 6, StringComparison.OrdinalIgnoreCase);
            if (fromIndex < 0) return result;

            foreach (var projection in SplitSqlList(clean.Substring(selectIndex + 6, fromIndex - selectIndex - 6)))
            {
                var projectionSql = StripSqlLiterals(projection).Trim();
                if (!ContainsSqlIdentifier(projectionSql, "time")) continue;
                var tokens = projectionSql.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                var alias = tokens.Length > 1 ? tokens[tokens.Length - 1].Trim(',', ')') : "time";
                if (string.Equals(alias, "AS", StringComparison.OrdinalIgnoreCase)) alias = "time";
                result.Add(alias.Trim('"', '`', '[', ']'));
            }
            return result;
        }

        static IEnumerable<string> SplitSqlList(string sql)
        {
            var start = 0;
            var depth = 0;
            for (var index = 0; index < sql.Length; index++)
            {
                if (sql[index] == '(') depth++;
                else if (sql[index] == ')' && depth > 0) depth--;
                else if (sql[index] == ',' && depth == 0)
                {
                    yield return sql.Substring(start, index - start);
                    start = index + 1;
                }
            }
            yield return sql.Substring(start);
        }

        static bool ContainsSqlIdentifier(string sql, string identifier)
        {
            var offset = 0;
            while ((offset = sql.IndexOf(identifier, offset, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                var before = offset == 0 ? '\0' : sql[offset - 1];
                var afterIndex = offset + identifier.Length;
                var after = afterIndex >= sql.Length ? '\0' : sql[afterIndex];
                if ((char.IsLetterOrDigit(before) == false && before != '_') &&
                    (char.IsLetterOrDigit(after) == false && after != '_')) return true;
                offset = afterIndex;
            }
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
                var targetType = args.Property?.PropertyType.NullableTypeOrThis();
                if (targetType == typeof(DateTimeOffset))
                    args.Value = SonnetDBUtils.ConvertDateTimeOffsetValue(args.Value);
                else if (targetType == typeof(DateTime))
                    args.Value = SonnetDBUtils.ConvertDateTimeValue(args.Value);
                else if (args.Property == null &&
                    (_temporalResultAliases.Value?.Contains(args.DataReader.GetName(args.Index)) == true ||
                     string.Equals(args.DataReader.GetName(args.Index), "time", StringComparison.OrdinalIgnoreCase)))
                {
                    // Scalar projections do not carry PropertyInfo through
                    // the legacy reader pipeline. SonnetDB's implicit time
                    // column is always temporal, so expose a DateTimeOffset;
                    // FreeSql's conversion block can then materialize either
                    // DateTime or DateTimeOffset.
                    args.Value = SonnetDBUtils.ConvertDateTimeOffsetValue(args.Value);
                }
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
