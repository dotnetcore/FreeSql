using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using Apache.IoTDB.Data;

namespace FreeSql.IoTDB
{
    internal sealed class IoTDBUtils : CommonUtils
    {
        static readonly Regex PathPattern = new Regex(@"^root(?:\.[\p{L}_$][\p{L}\p{N}_$-]*)+$", RegexOptions.Compiled);

        public IoTDBUtils(IFreeSql orm) : base(orm) { }

        public override string Now => "now()";
        public override string NowUtc => "now()";

        public override DbParameter AppendParamter(List<DbParameter> parameters, string parameterName, ColumnInfo column, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{parameters?.Count ?? 0}";
            var parameter = new IoTDBParameter(QuoteParamterName(parameterName), value);
            var dbType = _orm.CodeFirst.GetDbInfo(type)?.type;
            if (dbType != null) parameter.DbType = (DbType)dbType.Value;
            parameters?.Add(parameter);
            return parameter;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<DbParameter>(sql, obj, "@", (name, type, value) =>
            {
                var parameter = new IoTDBParameter("@" + name, value);
                var dbType = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (dbType != null) parameter.DbType = (DbType)dbType.Value;
                return parameter;
            });

        public override string FormatSql(string sql, params object[] args)
        {
            if (sql == null) return null;
            var literals = (args ?? Array.Empty<object>()).Select(value => (object)IoTDBSqlParameterBinder.ToLiteral(value)).ToArray();
            return string.Format(CultureInfo.InvariantCulture, sql, literals);
        }

        public override string QuoteSqlNameAdapter(params string[] name)
        {
            if (name == null || name.Length == 0) return string.Empty;
            if (name.Length == 1)
            {
                var value = (name[0] ?? string.Empty).Trim();
                if (value.StartsWith("(", StringComparison.Ordinal) && value.EndsWith(")", StringComparison.Ordinal)) return value;
                if (IsDevicePath(value)) return value;
                if (value.StartsWith("`", StringComparison.Ordinal) && value.EndsWith("`", StringComparison.Ordinal)) return value;
                return "`" + value.Replace("`", "``") + "`";
            }
            return string.Join(".", name.Select(part => QuotePathNode(part)));
        }

        public override string TrimQuoteSqlName(string name)
        {
            var value = (name ?? string.Empty).Trim();
            if (value.StartsWith("(", StringComparison.Ordinal) && value.EndsWith(")", StringComparison.Ordinal)) return value;
            return value.Replace("`.`", ".").Trim('`');
        }

        public override string[] SplitTableName(string name) => new[] { name };
        public override string QuoteParamterName(string name) => "@" + name;
        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;
        public override string IsNull(string sql, object value) => $"coalesce({sql}, {IoTDBSqlParameterBinder.ToLiteral(value)})";
        public override string StringConcat(string[] objs, Type[] types) => string.Join(" || ", objs);
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"({left} % {right})";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"({left} / {right})";

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value) =>
            IoTDBSqlParameterBinder.ToLiteral(value);

        internal static bool IsDevicePath(string value) => !string.IsNullOrWhiteSpace(value) && PathPattern.IsMatch(value);

        internal static string RequireDevicePath(string value)
        {
            if (!IsDevicePath(value))
                throw new ArgumentException($"IoTDB Tree entity names must be absolute device paths such as root.database.device. Invalid path: '{value}'.");
            return value;
        }

        static string QuotePathNode(string value)
        {
            var node = (value ?? string.Empty).Trim();
            if (node.Length == 0 || node.IndexOfAny(new[] { '.', ';', '\'', '"', '\r', '\n' }) >= 0)
                throw new ArgumentException($"Invalid IoTDB path node '{value}'.");
            return "`" + node.Replace("`", "``") + "`";
        }
    }
}
