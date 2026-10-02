using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.Model.Interface;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// 将关系表 JSON 列返回的字符串转换为 System.Text.Json DOM，
    /// 并在参数写入前还原为 JSON 文本。
    /// </summary>
    sealed class SonnetDBJsonDocumentTypeHandler : TypeHandler<JsonDocument>
    {
        public override object Serialize(JsonDocument value) =>
            value == null ? null : value.RootElement.GetRawText();

        public override JsonDocument Deserialize(object value)
        {
            if (value is JsonDocument document) return document;
            var text = SonnetDBJsonTypeHandlers.GetJsonText(value);
            try
            {
                return JsonDocument.Parse(text);
            }
            catch (JsonException exception)
            {
                throw new InvalidCastException("SonnetDB 返回的 JSON 文本无效，无法转换为 JsonDocument。", exception);
            }
        }
    }

    sealed class SonnetDBJsonElementTypeHandler : TypeHandler<JsonElement>
    {
        public override object Serialize(JsonElement value) =>
            value.ValueKind == JsonValueKind.Undefined ? "null" : value.GetRawText();

        public override JsonElement Deserialize(object value)
        {
            if (value is JsonElement element) return element;
            if (value is JsonDocument document) return document.RootElement.Clone();
            var text = SonnetDBJsonTypeHandlers.GetJsonText(value);
            try
            {
                using var documentValue = JsonDocument.Parse(text);
                return documentValue.RootElement.Clone();
            }
            catch (JsonException exception)
            {
                throw new InvalidCastException("SonnetDB 返回的 JSON 文本无效，无法转换为 JsonElement。", exception);
            }
        }
    }

    sealed class SonnetDBNullableJsonElementTypeHandler : TypeHandler<JsonElement?>
    {
        public override object Serialize(JsonElement? value) =>
            value.HasValue ? new SonnetDBJsonElementTypeHandler().Serialize(value.Value) : null;

        public override JsonElement? Deserialize(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            return new SonnetDBJsonElementTypeHandler().Deserialize(value);
        }
    }

    sealed class SonnetDBJsonNodeTypeHandler : TypeHandler<JsonNode>
    {
        public override object Serialize(JsonNode value) =>
            value?.ToJsonString();

        public override JsonNode Deserialize(object value)
        {
            if (value is JsonNode node) return node;
            var text = SonnetDBJsonTypeHandlers.GetJsonText(value);
            try
            {
                return JsonNode.Parse(text);
            }
            catch (JsonException exception)
            {
                throw new InvalidCastException("SonnetDB 返回的 JSON 文本无效，无法转换为 JsonNode。", exception);
            }
        }
    }

    sealed class SonnetDBJsonObjectTypeHandler : TypeHandler<JsonObject>
    {
        public override object Serialize(JsonObject value) => value?.ToJsonString();

        public override JsonObject Deserialize(object value)
        {
            if (value is JsonObject obj) return obj;
            var node = new SonnetDBJsonNodeTypeHandler().Deserialize(value);
            if (node == null) return null;
            if (node is JsonObject objValue) return objValue;
            throw new InvalidCastException("SonnetDB 返回的 JSON 根值不是对象，无法转换为 JsonObject。");
        }
    }

    sealed class SonnetDBJsonArrayTypeHandler : TypeHandler<JsonArray>
    {
        public override object Serialize(JsonArray value) => value?.ToJsonString();

        public override JsonArray Deserialize(object value)
        {
            if (value is JsonArray array) return array;
            var node = new SonnetDBJsonNodeTypeHandler().Deserialize(value);
            if (node == null) return null;
            if (node is JsonArray arrayValue) return arrayValue;
            throw new InvalidCastException("SonnetDB 返回的 JSON 根值不是数组，无法转换为 JsonArray。");
        }
    }

    sealed class SonnetDBJsonValueTypeHandler : TypeHandler<JsonValue>
    {
        public override object Serialize(JsonValue value) => value?.ToJsonString();

        public override JsonValue Deserialize(object value)
        {
            if (value is JsonValue jsonValue) return jsonValue;
            var node = new SonnetDBJsonNodeTypeHandler().Deserialize(value);
            if (node == null) return null;
            if (node is JsonValue valueNode) return valueNode;
            throw new InvalidCastException("SonnetDB 返回的 JSON 根值不是标量，无法转换为 JsonValue。");
        }
    }

    static class SonnetDBJsonTypeHandlers
    {
        static SonnetDBJsonTypeHandlers()
        {
            Register(typeof(JsonDocument), new SonnetDBJsonDocumentTypeHandler());
            Register(typeof(JsonElement), new SonnetDBJsonElementTypeHandler());
            Register(typeof(JsonElement?), new SonnetDBNullableJsonElementTypeHandler());
            Register(typeof(JsonNode), new SonnetDBJsonNodeTypeHandler());
            Register(typeof(JsonObject), new SonnetDBJsonObjectTypeHandler());
            Register(typeof(JsonArray), new SonnetDBJsonArrayTypeHandler());
            Register(typeof(JsonValue), new SonnetDBJsonValueTypeHandler());
        }

        public static void EnsureRegistered()
        {
            // 访问静态类型以确保注册已完成；CLR 保证静态构造函数只执行一次。
        }

        internal static string GetJsonText(object value)
        {
            switch (value)
            {
                case string text:
                    return text;
                case JsonDocument document:
                    return document.RootElement.GetRawText();
                case JsonElement element:
                    return element.ValueKind == JsonValueKind.Undefined ? "null" : element.GetRawText();
                case JsonNode node:
                    return node.ToJsonString();
                default:
                    throw new InvalidCastException(
                        $"SonnetDB JSON 值类型 '{value?.GetType().FullName ?? "NULL"}' 无法转换为 JSON 文本。");
            }
        }

        static void Register(Type type, ITypeHandler handler)
        {
            // FreeSql 的 TypeHandlers 是进程级共享注册表；保留先注册的处理器，
            // 避免 SonnetDB 覆盖其他提供程序或用户已经配置的行为。
            Utils.TypeHandlers.TryAdd(type, handler);
            lock (Utils.dicExecuteArrayRowReadClassOrTuple)
            {
                if (Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(type) == false)
                    Utils.dicExecuteArrayRowReadClassOrTuple.Add(type, true);
            }
        }
    }
}
