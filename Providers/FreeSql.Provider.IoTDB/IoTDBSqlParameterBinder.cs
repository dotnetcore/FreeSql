using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FreeSql.IoTDB
{
    public static class IoTDBSqlParameterBinder
    {
        public static string Bind(string sql, IEnumerable<DbParameter> parameters)
        {
            if (sql == null) throw new ArgumentNullException(nameof(sql));
            var parameterMap = new Dictionary<string, DbParameter>(StringComparer.OrdinalIgnoreCase);
            foreach (var parameter in parameters ?? Enumerable.Empty<DbParameter>())
            {
                if (parameter == null) continue;
                if (parameter.Direction != ParameterDirection.Input)
                    throw new NotSupportedException("IoTDB SQL parameters support input values only.");
                var name = NormalizeName(parameter.ParameterName);
                if (name.Length == 0) throw new InvalidOperationException("IoTDB SQL parameters must have a name.");
                if (parameterMap.ContainsKey(name))
                    throw new InvalidOperationException($"Duplicate IoTDB SQL parameter '{name}'.");
                parameterMap.Add(name, parameter);
            }

            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var output = new StringBuilder(sql.Length + parameterMap.Count * 12);
            var quote = '\0';
            var lineComment = false;
            var blockComment = false;
            for (var index = 0; index < sql.Length; index++)
            {
                var current = sql[index];
                var next = index + 1 < sql.Length ? sql[index + 1] : '\0';

                if (lineComment)
                {
                    output.Append(current);
                    if (current == '\n') lineComment = false;
                    continue;
                }
                if (blockComment)
                {
                    output.Append(current);
                    if (current == '*' && next == '/')
                    {
                        output.Append(next);
                        index++;
                        blockComment = false;
                    }
                    continue;
                }
                if (quote != '\0')
                {
                    output.Append(current);
                    if (current == quote)
                    {
                        if (next == quote)
                        {
                            output.Append(next);
                            index++;
                        }
                        else quote = '\0';
                    }
                    else if (current == '\\' && next != '\0')
                    {
                        output.Append(next);
                        index++;
                    }
                    continue;
                }

                if (current == '-' && next == '-')
                {
                    output.Append(current).Append(next);
                    index++;
                    lineComment = true;
                    continue;
                }
                if (current == '/' && next == '*')
                {
                    output.Append(current).Append(next);
                    index++;
                    blockComment = true;
                    continue;
                }
                if (current == '\'' || current == '"' || current == '`')
                {
                    quote = current;
                    output.Append(current);
                    continue;
                }
                if ((current == '@' || current == ':' || current == '$') && IsIdentifierStart(next))
                {
                    var start = index + 1;
                    var end = start + 1;
                    while (end < sql.Length && IsIdentifierPart(sql[end])) end++;
                    var name = sql.Substring(start, end - start);
                    if (!parameterMap.TryGetValue(name, out var parameter))
                        throw new InvalidOperationException($"No value was supplied for IoTDB SQL parameter '{name}'.");
                    output.Append(ToLiteral(parameter.Value, parameter.DbType));
                    used.Add(name);
                    index = end - 1;
                    continue;
                }
                output.Append(current);
            }

            var unused = parameterMap.Keys.Where(name => !used.Contains(name)).ToArray();
            if (unused.Length > 0)
                throw new InvalidOperationException($"IoTDB SQL parameters were not bound: {string.Join(", ", unused)}.");
            return output.ToString();
        }

        public static string ToLiteral(object value, DbType dbType = DbType.Object)
        {
            if (value == null || value == DBNull.Value) return "NULL";
            if (value is string text) return QuoteString(text);
            if (value is char character) return QuoteString(character.ToString());
            if (value is bool boolean) return boolean ? "true" : "false";
            if (value is DateTime dateTime) return dateTime.ToUniversalTime().Ticks == 0
                ? "0"
                : new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
            if (value is DateTimeOffset dateTimeOffset) return dateTimeOffset.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
            if (value is byte[] bytes) return "X'" + BitConverter.ToString(bytes).Replace("-", string.Empty) + "'";
            if (value is Guid guid) return QuoteString(guid.ToString("D"));
            if (value is Enum enumValue) return Convert.ToInt64(enumValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
            if (value is float single)
            {
                if (float.IsNaN(single) || float.IsInfinity(single)) throw new NotSupportedException("IoTDB does not accept non-finite numeric literals.");
                return single.ToString("R", CultureInfo.InvariantCulture);
            }
            if (value is double doubleValue)
            {
                if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue)) throw new NotSupportedException("IoTDB does not accept non-finite numeric literals.");
                return doubleValue.ToString("R", CultureInfo.InvariantCulture);
            }
            if (value is decimal decimalValue) return decimalValue.ToString(CultureInfo.InvariantCulture);
            if (value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong)
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            if (value is TimeSpan timeSpan) return timeSpan.TotalMilliseconds.ToString("R", CultureInfo.InvariantCulture);
#if NET6_0_OR_GREATER
            if (value is DateOnly dateOnly) return QuoteString(dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
#endif
            if (value is System.Collections.IEnumerable)
                throw new NotSupportedException("Collection parameters must use FreeSql's IN-list expansion; nested IoTDB values are unsupported.");
            if (dbType == DbType.String || dbType == DbType.AnsiString || dbType == DbType.StringFixedLength || dbType == DbType.AnsiStringFixedLength)
                return QuoteString(Convert.ToString(value, CultureInfo.InvariantCulture));
            throw new NotSupportedException($"IoTDB SQL parameter type '{value.GetType().FullName}' is not supported.");
        }

        internal static string[] SplitStatements(string sql)
        {
            var statements = new List<string>();
            var start = 0;
            var quote = '\0';
            for (var index = 0; index < sql.Length; index++)
            {
                var current = sql[index];
                if (quote != '\0')
                {
                    if (current == quote)
                    {
                        if (index + 1 < sql.Length && sql[index + 1] == quote) index++;
                        else quote = '\0';
                    }
                    else if (current == '\\' && index + 1 < sql.Length) index++;
                    continue;
                }
                if (current == '\'' || current == '"' || current == '`')
                {
                    quote = current;
                    continue;
                }
                if (current != ';') continue;
                AddStatement(statements, sql.Substring(start, index - start));
                start = index + 1;
            }
            AddStatement(statements, sql.Substring(start));
            return statements.ToArray();
        }

        private static string NormalizeName(string name)
        {
            return (name ?? string.Empty).Trim().TrimStart('@', ':', '$', '?');
        }

        private static string QuoteString(string value) => "'" + (value ?? string.Empty).Replace("'", "''") + "'";

        private static bool IsIdentifierStart(char value) => value == '_' || char.IsLetter(value);
        private static bool IsIdentifierPart(char value) => value == '_' || char.IsLetterOrDigit(value);

        private static void AddStatement(List<string> statements, string statement)
        {
            if (!string.IsNullOrWhiteSpace(statement)) statements.Add(statement.Trim());
        }
    }
}
