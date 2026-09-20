using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.Model.Interface;
using System;
using System.Globalization;
using SonnetDB.Model;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// 将 SonnetDB 驱动返回的 GEOPOINT 值转换为 GeoPoint。
    /// </summary>
    sealed class SonnetDBGeoPointTypeHandler : TypeHandler<GeoPoint>
    {
        public override object Serialize(GeoPoint value) => value;

        public override GeoPoint Deserialize(object value)
        {
            if (value is GeoPoint point) return point;
            if (value is string text && TryParse(text, out point)) return point;
            throw new InvalidCastException(
                $"SonnetDB GEOPOINT 值类型 '{value?.GetType().FullName ?? "NULL"}' 无法转换为 GeoPoint。");
        }

        static bool TryParse(string text, out GeoPoint point)
        {
            point = default;
            if (string.IsNullOrWhiteSpace(text)) return false;
            var value = text.Trim();
            if (value.StartsWith("POINT(", StringComparison.OrdinalIgnoreCase) && value.EndsWith(")", StringComparison.Ordinal))
                value = value.Substring(6, value.Length - 7);
            var parts = value.Split(',');
            if (parts.Length != 2 ||
                double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) == false ||
                double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) == false)
                return false;
            try
            {
                point = GeoPoint.Create(lat, lon);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }
    }

    static class SonnetDBTypeHandlers
    {
        static SonnetDBTypeHandlers()
        {
            // TypeHandlers 是进程级共享注册表，保留先注册的处理器，避免覆盖其他提供程序的配置。
            Utils.TypeHandlers.TryAdd(typeof(GeoPoint), new SonnetDBGeoPointTypeHandler());
            // 复杂对象读取器需要先把该类型识别为单列值，随后才会调用类型处理器。
            lock (Utils.dicExecuteArrayRowReadClassOrTuple)
            {
                if (Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(typeof(GeoPoint)) == false)
                    Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(GeoPoint), true);
            }
        }

        public static void EnsureRegistered()
        {
            // 访问静态类型以确保注册已完成；CLR 保证静态构造函数只执行一次。
        }
    }
}
