using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Globalization;
using System.Text;

namespace FreeSql.MsAccess
{

    public class MsAccessUtils : CommonUtils
    {
        public MsAccessUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
            var ret = new OleDbParameter { ParameterName = QuoteParamterName(parameterName), Value = value };
            var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
            if (tp != null) ret.OleDbType = (OleDbType)tp.Value;
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<OleDbParameter>(sql, obj, null, (name, type, value) =>
            {
                if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
                var ret = new OleDbParameter { ParameterName = $"@{name}", Value = value };
                var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (tp != null) ret.OleDbType = (OleDbType)tp.Value;
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatAccess(args);
        public override string QuoteSqlName(params string[] name)
        {
            if (name.Length == 1)
            {
                var nametrim = name[0].Trim();
                if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                    return nametrim; //原生SQL
                if (nametrim.StartsWith("[") && nametrim.EndsWith("]"))
                    return nametrim;
                return $"[{nametrim.Replace(".", "].[")}]";
            }
            return $"[{string.Join("].[", name)}]";
        }
        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"{nametrim.TrimStart('[').TrimEnd(']').Replace("].[", ".").Replace(".[", ".")}";
        }
        public override string[] SplitTableName(string name) => GetSplitTableNames(name, '[', ']', 2);
        public override string QuoteParamterName(string name) => $"@{name}";
        public override string IsNull(string sql, object value) => $"iif(isnull({sql}), {value}, {sql})";
        public override string StringConcat(string[] objs, Type[] types)
        {
            var sb = new StringBuilder();
            var news = new string[objs.Length];
            for (var a = 0; a < objs.Length; a++)
            {
                if (types[a] == typeof(string)) news[a] = objs[a];
                else news[a] = MsAccessUtils.GetCastSql(objs[a], typeof(string));
            }
            return string.Join(" + ", news);
        }
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} mod {right}";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"{left} / {right}";
        public override string Now => "now()";
        public override string NowUtc => "now()";

        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;
        public override string FieldAsAlias(string alias) => $" as {alias}";
        public override string IIF(string test, string ifTrue, string ifElse) => $"iif({test}, {ifTrue}, {ifElse})";

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (type == typeof(byte[])) return $"0x{CommonUtils.BytesSqlRaw(value as byte[])}";
            if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
            {
                var ts = (TimeSpan)value;
                value = $"{ts.Hours}:{ts.Minutes}:{ts.Seconds}";
            }
            return FormatSql("{0}", value, 1);
        }

        public static string GetCastSql(string sqlExp, Type toType)
        {
            switch (toType.ToString())
            {
                case "System.Boolean": return $"(cstr({sqlExp}) not in ('0','false'))";
                case "System.Byte": return $"cbyte({sqlExp})";
                case "System.Char": return $"left(cstr({sqlExp}),1)";
                case "System.DateTime": return $"cdate({sqlExp})";
                case "System.Decimal": return $"ccur({sqlExp})";
                case "System.Double": return $"cdbl({sqlExp})";
                case "System.Int16": return $"cint({sqlExp})";
                case "System.Int32": return $"cint({sqlExp})";
                case "System.Int64": return $"clng({sqlExp})";
                case "System.SByte": return $"cint({sqlExp})";
                case "System.Single": return $"csng({sqlExp})";
                case "System.String": return $"cstr({sqlExp})";
                case "System.UInt16": return $"cint({sqlExp})";
                case "System.UInt32": return $"clng({sqlExp})";
                case "System.UInt64": return $"clng({sqlExp})";
                case "System.Guid": return $"cstr({sqlExp})";
            }
            return sqlExp;
        }
    }
}
