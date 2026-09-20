using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.Model.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// 将 SonnetDB VECTOR 结果转换为 <c>float[]</c>。
    /// 发布版驱动的嵌入式和帧协议路径可直接返回数组；JSON/NDJSON 路径
    /// 返回数组文本时也在这里统一解析，避免退化为字符串属性。
    /// </summary>
    sealed class SonnetDBVectorTypeHandler : TypeHandler<float[]>
    {
        public override object Serialize(float[] value) => value;

        public override float[] Deserialize(object value)
        {
            if (value is float[] vector) return vector;
            if (value is JsonElement element) return ParseJsonArray(element);
            if (value is string text) return ParseJsonArrayText(text);
            if (value is IReadOnlyList<float> list)
            {
                var result = new float[list.Count];
                for (int i = 0; i < result.Length; i++) result[i] = list[i];
                return result;
            }

            throw new InvalidCastException(
                $"SonnetDB VECTOR 值类型 '{value?.GetType().FullName ?? "NULL"}' 无法转换为 float[]。");
        }

        static float[] ParseJsonArrayText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCastException("SonnetDB VECTOR 文本为空，无法转换为 float[]。");
            text = text.Trim();
            // 表达式树中的 new float[] { ... } 会先被 FreeSql 通用解析器
            // 表示成一层或多层括号；这里仅在内容仍是数值列表时转换为 JSON 数组。
            while (text.Length >= 4 && text.StartsWith("((", StringComparison.Ordinal) &&
                   text.EndsWith("))", StringComparison.Ordinal))
                text = text.Substring(1, text.Length - 2).Trim();
            if (text.StartsWith("(", StringComparison.Ordinal) && text.EndsWith(")", StringComparison.Ordinal))
                text = "[" + text.Substring(1, text.Length - 2) + "]";
            try
            {
                using var document = JsonDocument.Parse(text);
                return ParseJsonArray(document.RootElement);
            }
            catch (JsonException exception)
            {
                throw new InvalidCastException("SonnetDB VECTOR 文本不是有效的 JSON 数组。", exception);
            }
        }

        static float[] ParseJsonArray(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Array)
                throw new InvalidCastException("SonnetDB VECTOR 结果不是 JSON 数组。");

            var result = new float[element.GetArrayLength()];
            int index = 0;
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Number
                    || item.TryGetSingle(out var number) == false
                    || float.IsFinite(number) == false)
                    throw new InvalidCastException(
                        $"SonnetDB VECTOR 第 {index} 个分量不是有限的数值。" );
                result[index++] = number;
            }

            if (result.Length == 0)
                throw new InvalidCastException("SonnetDB VECTOR 结果不能是空数组。");
            return result;
        }
    }

    static class SonnetDBVectorTypeHandlers
    {
        static SonnetDBVectorTypeHandlers()
        {
            // TypeHandlers 是进程级共享注册表，保留先注册的处理器，避免覆盖其他提供程序配置。
            Utils.TypeHandlers.TryAdd(typeof(float[]), new SonnetDBVectorTypeHandler());
            lock (Utils.dicExecuteArrayRowReadClassOrTuple)
            {
                if (Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(typeof(float[])) == false)
                    Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(float[]), true);
            }
        }

        public static void EnsureRegistered()
        {
            // 访问静态类型以确保静态构造函数已执行。
        }
    }
}
