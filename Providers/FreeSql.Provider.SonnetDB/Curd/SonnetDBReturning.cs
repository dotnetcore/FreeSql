using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.SonnetDB.Curd
{
    // SonnetDB RETURNING accepts column names, without projection aliases.
    // Reuse FreeSql's reader and type handlers while resolving CLR property
    // names to the actual returned database names, including Column(Name).
    static class SonnetDBReturning
    {
        internal static List<T> Query<T>(IFreeSql orm, CommonUtils commonUtils, TableInfo table, Type resultType,
            DbConnection connection, DbTransaction transaction, string sql, int timeout, DbParameter[] parameters)
        {
            var result = new List<T>();
            var type = resultType ?? typeof(T);
            var projectionReader = CreateProjectionReader(orm, type);
            int[] indexes = null;
            string flag = null;
            orm.Ado.ExecuteReader(connection, transaction, fetch =>
            {
                if (projectionReader != null)
                {
                    result.Add((T)projectionReader(fetch.Object));
                    return;
                }
                if (indexes == null) indexes = GetIndexes(type, table, fetch.Object, out flag);
                result.Add((T)Utils.ExecuteReaderToClass(flag, type, indexes, fetch.Object, 0, commonUtils));
            }, CommandType.Text, sql, timeout, parameters);
            return result;
        }

        internal static async Task<List<T>> QueryAsync<T>(IFreeSql orm, CommonUtils commonUtils, TableInfo table, Type resultType,
            DbConnection connection, DbTransaction transaction, string sql, int timeout, DbParameter[] parameters, CancellationToken cancellationToken)
        {
            var result = new List<T>();
            var type = resultType ?? typeof(T);
            var projectionReader = CreateProjectionReader(orm, type);
            int[] indexes = null;
            string flag = null;
            await orm.Ado.ExecuteReaderAsync(connection, transaction, fetch =>
            {
                if (projectionReader != null)
                {
                    result.Add((T)projectionReader(fetch.Object));
                    return Task.CompletedTask;
                }
                if (indexes == null) indexes = GetIndexes(type, table, fetch.Object, out flag);
                result.Add((T)Utils.ExecuteReaderToClass(flag, type, indexes, fetch.Object, 0, commonUtils));
                return Task.CompletedTask;
            }, CommandType.Text, sql, timeout, parameters, cancellationToken);
            return result;
        }

        static Func<DbDataReader, object> CreateProjectionReader(IFreeSql orm, Type type)
        {
            if (IsTemporalType(type)) return reader => ReadProjectionValue(orm, reader, 0, type);
            if (!type.IsAnonymousType()) return null;
            var constructor = type.GetConstructors().Single();
            var parameters = constructor.GetParameters();
            if (!parameters.Any(parameter => IsTemporalType(parameter.ParameterType))) return null;

            // Anonymous projections have constructor parameters rather than
            // entity properties. Convert their raw RETURNING values explicitly;
            // the shared reader cannot infer an INT duration's ticks unit.
            return reader => constructor.Invoke(parameters.Select((parameter, index) =>
                ReadProjectionValue(orm, reader, index, parameter.ParameterType)).ToArray());
        }

        static bool IsTemporalType(Type type)
        {
            type = type.NullableTypeOrThis();
            return type == typeof(TimeSpan) || type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
                type == typeof(DateOnly) || type == typeof(TimeOnly);
        }

        static object ReadProjectionValue(IFreeSql orm, DbDataReader reader, int index, Type targetType)
        {
            object value;
            if (orm.Aop.AuditDataReaderHandler == null) value = reader.GetValue(index);
            else
            {
                var args = new Aop.AuditDataReaderEventArgs(reader, index, null);
                orm.Aop.AuditDataReaderHandler(orm, args);
                value = args.Value;
            }
            if (value == null || value == DBNull.Value)
                return targetType.IsValueType && !targetType.IsNullableType() ? Activator.CreateInstance(targetType) : null;

            var type = targetType.NullableTypeOrThis();
            if (type.IsInstanceOfType(value)) return value;
            if (type == typeof(TimeSpan))
            {
                if (value is string text && !long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    return TimeSpan.Parse(text, CultureInfo.InvariantCulture);
                return TimeSpan.FromTicks(Convert.ToInt64(value, CultureInfo.InvariantCulture));
            }
            if (type == typeof(DateTime)) return SonnetDBUtils.ConvertDateTimeValue(value);
            if (type == typeof(DateTimeOffset)) return SonnetDBUtils.ConvertDateTimeOffsetValue(value);
            if (type == typeof(DateOnly)) return DateOnly.FromDateTime(SonnetDBUtils.ConvertDateTimeValue(value));
            if (type == typeof(TimeOnly))
            {
                if (value is TimeSpan duration) return TimeOnly.FromTimeSpan(duration);
                if (value is DateTime dateTime) return TimeOnly.FromDateTime(dateTime);
                if (value is DateTimeOffset dateTimeOffset) return TimeOnly.FromTimeSpan(dateTimeOffset.TimeOfDay);
                return TimeOnly.Parse(Convert.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            }
            return Utils.GetDataReaderValue(targetType, value);
        }

        static int[] GetIndexes(Type type, TableInfo table, DbDataReader reader, out string flag)
        {
            var names = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < reader.FieldCount; index++) names[reader.GetName(index)] = index;
            var indexes = type.IsAnonymousType() ? Enumerable.Range(0, reader.FieldCount).ToArray() : AdoProvider.GetQueryTypeProperties(type).Select(property =>
            {
                var name = table.ColumnsByCs.TryGetValue(property.Key, out var column) ? column.Attribute.Name : property.Key;
                return names.TryGetValue(name, out var index) ? index : -1;
            }).ToArray();
            flag = "SonnetDBReturning:" + table.Type.FullName + ":" + string.Join(",", names.Keys) + ":" + string.Join(",", indexes);
            return indexes;
        }
    }
}
