// SonnetDB 表值函数辅助入口。
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// 生成 SonnetDB 3.1 表值函数，并将其安全地应用到 FreeSql 查询的 FROM 位置。
    /// 表值函数返回的列由调用方提供 DTO 映射；提供程序会自动包成派生查询，
    /// 因此 DTO 不会被自动同步为一张实体表。
    /// </summary>
    public static class SonnetDBTableValuedFunctions
    {
        /// <summary>生成时序预测表值函数。</summary>
        public static string Forecast(string measurement, string field, int horizon,
            string algorithm, int? season = null)
        {
            RequirePositive(horizon, nameof(horizon));
            var algorithmName = NormalizeAlgorithm(algorithm);
            var sql = $"forecast({QuoteIdentifier(measurement)}, {QuoteIdentifier(field)}, " +
                      $"{horizon.ToString(CultureInfo.InvariantCulture)}, '{algorithmName}'";
            if (season.HasValue)
            {
                RequireNonNegative(season.Value, nameof(season));
                sql += $", {season.Value.ToString(CultureInfo.InvariantCulture)}";
            }
            return sql + ")";
        }

        /// <summary>生成时序测量 VECTOR 前 K 检索表值函数。</summary>
        public static string Knn(string measurement, string vectorColumn,
            IEnumerable<float> queryVector, int k, string metric = null)
        {
            RequirePositive(k, nameof(k));
            var vector = FormatVector(queryVector);
            var metricName = NormalizeMetric(metric);
            return $"knn({QuoteIdentifier(measurement)}, {QuoteIdentifier(vectorColumn)}, " +
                   $"{vector}, {k.ToString(CultureInfo.InvariantCulture)}, '{metricName}')";
        }

        /// <summary>生成 JSON 文件行表值函数。</summary>
        public static string JsonEach(string filePath, string mode = null, string idPath = null)
            => BuildJsonFileFunction("json_each", filePath, mode, idPath);

        /// <summary>生成 JSON 文件行表值函数的兼容别名。</summary>
        public static string JsonTable(string filePath, string mode = null, string idPath = null)
            => BuildJsonFileFunction("json_table", filePath, mode, idPath);

        /// <summary>生成文档集合向量检索表值函数。</summary>
        public static string VectorSearch(string source, IEnumerable<float> queryVector,
            string vectorField = "$.embedding", int k = 20, string metric = null)
        {
            RequirePositive(k, nameof(k));
            var path = RequireText(vectorField, nameof(vectorField));
            var metricName = NormalizeMetric(metric);
            return "vector_search(" +
                   $"source => {QuoteIdentifier(source)}, " +
                   $"vector_field => '{QuoteString(path)}', " +
                   $"vector => {FormatVector(queryVector)}, " +
                   $"k => {k.ToString(CultureInfo.InvariantCulture)}, " +
                   $"metric => '{metricName}')";
        }

        /// <summary>生成文档集合全文与向量融合检索表值函数。</summary>
        public static string HybridSearch(string source, IEnumerable<float> queryVector,
            string text = null, string vectorField = "$.embedding", int k = 20,
            string metric = null, string textIndex = null, string textField = null,
            double? textWeight = null, double? vectorWeight = null)
        {
            RequirePositive(k, nameof(k));
            var queryText = RequireText(text, nameof(text));
            var parts = new List<string>
            {
                $"source => {QuoteIdentifier(source)}",
                $"vector_field => '{QuoteString(RequireText(vectorField, nameof(vectorField)))}'",
                $"vector => {FormatVector(queryVector)}",
                $"k => {k.ToString(CultureInfo.InvariantCulture)}",
                $"text => '{QuoteString(queryText)}'"
            };
            if (metric != null) parts.Add($"metric => '{NormalizeMetric(metric)}'");
            if (textIndex != null) parts.Add($"text_index => {QuoteIdentifier(textIndex)}");
            if (textField != null) parts.Add($"text_field => '{QuoteString(RequireText(textField, nameof(textField)))}'");
            if (textWeight.HasValue)
                parts.Add($"text_weight => {FormatNonNegative(textWeight.Value, nameof(textWeight))}");
            if (vectorWeight.HasValue)
                parts.Add($"vector_weight => {FormatNonNegative(vectorWeight.Value, nameof(vectorWeight))}");
            return $"hybrid_search({string.Join(", ", parts)})";
        }

        /// <summary>
        /// 生成时序测量 KNN 与文档集合知识文档的融合检索表值函数。
        /// 文本、文档向量和对应权重均为可选项；至少会保留时序测量 KNN 得分。
        /// </summary>
        public static string MeasurementHybridSearch(string measurement, string documents,
            string vectorField, IEnumerable<float> queryVector, string measurementJoinTag,
            string documentJoinPath, int k = 20, string metric = null, int? measurementTopK = null,
            string text = null, string textIndex = null, string textField = null,
            string documentJoinIndex = null, string documentVectorField = null,
            double? measurementWeight = null, double? textWeight = null,
            double? documentVectorWeight = null)
        {
            RequirePositive(k, nameof(k));
            if (measurementTopK.HasValue)
                RequirePositive(measurementTopK.Value, nameof(measurementTopK));

            var queryText = text == null ? null : RequireText(text, nameof(text));
            if (queryText == null && (textIndex != null || textField != null || textWeight.HasValue))
            {
                throw new ArgumentException(
                    "未提供全文查询文本时，不能指定全文索引、全文字段或全文权重。", nameof(text));
            }
            if (documentVectorField == null && documentVectorWeight.HasValue)
            {
                throw new ArgumentException(
                    "未提供文档向量字段时，不能指定文档向量权重。", nameof(documentVectorField));
            }

            var parts = new List<string>
            {
                $"source => {QuoteIdentifier(measurement)}",
                $"documents => {QuoteIdentifier(documents)}",
                $"vector_field => {QuoteIdentifier(vectorField)}",
                $"vector => {FormatVector(queryVector)}",
                $"k => {k.ToString(CultureInfo.InvariantCulture)}",
                $"measurement_join_tag => {QuoteIdentifier(measurementJoinTag)}",
                $"document_join_path => '{QuoteString(RequireText(documentJoinPath, nameof(documentJoinPath)))}'"
            };
            if (metric != null) parts.Add($"metric => '{NormalizeMetric(metric)}'");
            if (measurementTopK.HasValue)
                parts.Add($"measurement_top_k => {measurementTopK.Value.ToString(CultureInfo.InvariantCulture)}");
            if (documentJoinIndex != null)
                parts.Add($"document_join_index => {QuoteIdentifier(documentJoinIndex)}");
            if (queryText != null)
            {
                parts.Add($"text => '{QuoteString(queryText)}'");
                if (textIndex != null) parts.Add($"text_index => {QuoteIdentifier(textIndex)}");
                if (textField != null)
                    parts.Add($"text_field => '{QuoteString(RequireText(textField, nameof(textField)))}'");
            }
            if (documentVectorField != null)
            {
                parts.Add($"document_vector_field => " +
                    $"'{QuoteString(RequireText(documentVectorField, nameof(documentVectorField)))}'");
            }
            if (measurementWeight.HasValue)
                parts.Add($"measurement_weight => {FormatNonNegative(measurementWeight.Value, nameof(measurementWeight))}");
            if (textWeight.HasValue)
                parts.Add($"text_weight => {FormatNonNegative(textWeight.Value, nameof(textWeight))}");
            if (documentVectorWeight.HasValue)
            {
                parts.Add($"document_vector_weight => " +
                    FormatNonNegative(documentVectorWeight.Value, nameof(documentVectorWeight)));
            }

            var effectiveMeasurementWeight = measurementWeight ?? 0.6d;
            var effectiveTextWeight = textWeight ?? (queryText == null ? 0d : 0.3d);
            var effectiveDocumentVectorWeight = documentVectorWeight ??
                (documentVectorField == null ? 0d : 0.1d);
            if (effectiveMeasurementWeight == 0d && effectiveTextWeight == 0d && effectiveDocumentVectorWeight == 0d)
            {
                throw new ArgumentException(
                    "measurement、全文和文档向量权重不能同时为零。", nameof(measurementWeight));
            }

            return $"hybrid_search({string.Join(", ", parts)})";
        }

        /// <summary>从预测表值函数创建强类型 FreeSql 查询。</summary>
        public static ISelect<T> SelectForecast<T>(this IFreeSql orm, string measurement,
            string field, int horizon, string algorithm, int? season = null) where T : class
            => SelectTable<T>(orm, Forecast(measurement, field, horizon, algorithm, season));

        /// <summary>从时序测量 KNN 表值函数创建强类型 FreeSql 查询。</summary>
        public static ISelect<T> SelectKnn<T>(this IFreeSql orm, string measurement,
            string vectorColumn, IEnumerable<float> queryVector, int k, string metric = null)
            where T : class
            => SelectTable<T>(orm, Knn(measurement, vectorColumn, queryVector, k, metric));

        /// <summary>从 JSON 文件表值函数创建强类型 FreeSql 查询。</summary>
        public static ISelect<T> SelectJsonEach<T>(this IFreeSql orm, string filePath,
            string mode = null, string idPath = null) where T : class
            => SelectTable<T>(orm, JsonEach(filePath, mode, idPath));

        /// <summary>从 JSON 文件表值函数兼容别名创建强类型 FreeSql 查询。</summary>
        public static ISelect<T> SelectJsonTable<T>(this IFreeSql orm, string filePath,
            string mode = null, string idPath = null) where T : class
            => SelectTable<T>(orm, JsonTable(filePath, mode, idPath));

        /// <summary>从文档集合向量检索表值函数创建查询。</summary>
        public static ISelect<T> SelectVectorSearch<T>(this IFreeSql orm, string source,
            IEnumerable<float> queryVector, string vectorField = "$.embedding", int k = 20,
            string metric = null) where T : class
            => SelectTable<T>(orm, VectorSearch(source, queryVector, vectorField, k, metric));

        /// <summary>从文档集合融合检索表值函数创建查询。</summary>
        public static ISelect<T> SelectHybridSearch<T>(this IFreeSql orm, string source,
            IEnumerable<float> queryVector, string text = null, string vectorField = "$.embedding",
            int k = 20, string metric = null, string textIndex = null, string textField = null,
            double? textWeight = null, double? vectorWeight = null) where T : class
            => SelectTable<T>(orm, HybridSearch(source, queryVector, text, vectorField, k,
                metric, textIndex, textField, textWeight, vectorWeight));

        /// <summary>从时序测量 KNN 与文档集合融合检索创建强类型 FreeSql 查询。</summary>
        public static ISelect<T> SelectMeasurementHybridSearch<T>(this IFreeSql orm,
            string measurement, string documents, string vectorField, IEnumerable<float> queryVector,
            string measurementJoinTag, string documentJoinPath, int k = 20, string metric = null,
            int? measurementTopK = null, string text = null, string textIndex = null,
            string textField = null, string documentJoinIndex = null, string documentVectorField = null,
            double? measurementWeight = null, double? textWeight = null,
            double? documentVectorWeight = null) where T : class
            => SelectTable<T>(orm, MeasurementHybridSearch(measurement, documents, vectorField,
                queryVector, measurementJoinTag, documentJoinPath, k, metric, measurementTopK,
                text, textIndex, textField, documentJoinIndex, documentVectorField,
                measurementWeight, textWeight, documentVectorWeight));

        static ISelect<T> SelectTable<T>(IFreeSql orm, string tableFunction) where T : class
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm), "FreeSql 实例不能为空。");
            return orm.Select<T>().AsTable((_, __) => tableFunction);
        }

        static string BuildJsonFileFunction(string name, string filePath, string mode, string idPath)
        {
            var parts = new List<string> { $"'{QuoteString(RequireText(filePath, nameof(filePath)))}'" };
            if (mode != null) parts.Add($"'{QuoteString(RequireText(mode, nameof(mode)))}'");
            if (idPath != null) parts.Add($"'{QuoteString(RequireText(idPath, nameof(idPath)))}'");
            return $"{name}({string.Join(", ", parts)})";
        }

        static string FormatVector(IEnumerable<float> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values), "向量参数不能为空。");
            var vector = values.ToArray();
            if (vector.Length == 0)
                throw new ArgumentException("SonnetDB VECTOR 不能是空向量。", nameof(values));
            if (vector.Any(value => float.IsNaN(value) || float.IsInfinity(value)))
                throw new ArgumentException("SonnetDB VECTOR 只能包含有限数值。", nameof(values));
            return "[" + string.Join(", ", vector.Select(value => value.ToString("R", CultureInfo.InvariantCulture))) + "]";
        }

        static string NormalizeAlgorithm(string value)
        {
            var algorithm = RequireText(value, nameof(value)).ToLowerInvariant();
            if (algorithm != "linear" && algorithm != "holt_winters" && algorithm != "hw")
                throw new ArgumentException("SonnetDB forecast 只支持 linear、holt_winters 或 hw。", nameof(value));
            return algorithm;
        }

        static string NormalizeMetric(string value)
        {
            var metric = string.IsNullOrWhiteSpace(value) ? "cosine" : value.Trim().ToLowerInvariant();
            return metric switch
            {
                "cosine" or "cosine_distance" => "cosine",
                "l2" or "l2_distance" or "euclidean" => "l2",
                "inner_product" or "dot" or "ip" => "inner_product",
                _ => throw new ArgumentException("SonnetDB 向量检索只支持 cosine、l2 或 inner_product。", nameof(value))
            };
        }

        static string QuoteIdentifier(string value)
        {
            var text = RequireText(value, nameof(value));
            return string.Join(".", text.Split('.').Select(part =>
                $"\"{RequireText(part, nameof(value)).Replace("\"", "\"\"")}\""));
        }

        static string QuoteString(string value) => value.Replace("'", "''");

        static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("参数不能为空。", parameterName);
            return value.Trim();
        }

        static void RequirePositive(int value, string parameterName)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(parameterName, "参数必须大于零。");
        }

        static void RequireNonNegative(int value, string parameterName)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(parameterName, "参数不能为负数。");
        }

        static string FormatNonNegative(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
                throw new ArgumentOutOfRangeException(parameterName, "参数必须是非负有限数值。");
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
