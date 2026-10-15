using FreeSql.Internal.Model;
using FreeSql.Provider.SonnetDB.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace FreeSql.SonnetDB
{
    enum SonnetDBModelKind
    {
        Measurement,
        Table
    }

    static class SonnetDBModel
    {
        public static SonnetDBModelKind GetKind(TableInfo table)
        {
            if (table == null || table.Type == null) return SonnetDBModelKind.Measurement;
            return GetKind(table.Type, table);
        }

        public static SonnetDBModelKind GetKind(Type entityType, TableInfo table = null)
        {
            if (entityType == null) return SonnetDBModelKind.Measurement;

            // FreeSql 的动态字典表没有实体特性；字典表模型由 CRUD 数据中的
            // 列集合构建，明确按关系表处理，避免误走时序追加语义。
            if (table?.IsDictionaryType == true)
                return SonnetDBModelKind.Table;

            var tableMarker = entityType.GetCustomAttribute<SonnetDBTableAttribute>(true) != null;
            var hasRoleMarker = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(property => property.GetCustomAttribute<SonnetDBTagAttribute>(true) != null ||
                                property.GetCustomAttribute<SonnetDBFieldAttribute>(true) != null);
            var hasRoleDbType = table?.Columns?.Values.Any(column =>
                (column.Attribute?.DbType ?? string.Empty).TrimStart().StartsWith("TAG", StringComparison.OrdinalIgnoreCase) ||
                (column.Attribute?.DbType ?? string.Empty).TrimStart().StartsWith("FIELD", StringComparison.OrdinalIgnoreCase)) == true;

            if (tableMarker && (hasRoleMarker || hasRoleDbType))
                throw new InvalidOperationException(
                    $"SonnetDB 实体“{entityType.FullName}”不能同时使用 [SonnetDBTable] 和 TAG/FIELD 列映射。");

            return tableMarker ? SonnetDBModelKind.Table : SonnetDBModelKind.Measurement;
        }

        public static bool IsTable(TableInfo table) => GetKind(table) == SonnetDBModelKind.Table;
        public static bool IsMeasurement(TableInfo table) => GetKind(table) == SonnetDBModelKind.Measurement;

        public static void EnsureTable(TableInfo table, string operation)
        {
            if (!IsTable(table))
                throw new NotSupportedException(
                    $"SonnetDB {operation} 仅支持关系表实体；时序测量写入使用追加/删除语义，" +
                    "如需关系表能力，请为实体添加 [SonnetDBTable]。");
        }

        public static void EnsureMeasurement(TableInfo table, string operation)
        {
            if (IsTable(table))
                throw new NotSupportedException(
                    $"SonnetDB {operation} 不是时序测量操作；当前实体已标记 [SonnetDBTable]。");
        }
    }
}
