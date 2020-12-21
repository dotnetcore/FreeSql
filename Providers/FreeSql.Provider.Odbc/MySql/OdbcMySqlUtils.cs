using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using System.Globalization;

namespace FreeSql.Odbc.MySql
{

    class OdbcMySqlUtils : CommonUtils
    {
        public OdbcMySqlUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            var ret = new OdbcParameter { ParameterName = QuoteParamterName(parameterName), Value = value };
            var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
            if (tp != null)
                ret.OdbcType = (OdbcType)tp.Value;
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<OdbcParameter>(sql, obj, null, (name, type, value) =>
            {
                var ret = new OdbcParameter { ParameterName = $"?{name}", Value = value };
                var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (tp != null)
                    ret.OdbcType = (OdbcType)tp.Value;
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatOdbcMySql(args);
        public override string QuoteSqlName(params string[] name)
        {
            if (name.Length == 1)
            {
                var nametrim = name[0].Trim();
                if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                    return nametrim; //原生SQL
                if (nametrim.StartsWith("`") && nametrim.EndsWith("`"))
                    return nametrim;
                return $"`{nametrim.Replace(".", "`.`")}`";
            }
            return $"`{string.Join("`.`", name)}`";
        }
        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"{nametrim.Trim('`').Replace("`.`", ".").Replace(".`", ".")}";
        }
        public override string[] SplitTableName(string name) => GetSplitTableNames(name, '`', '`', 2);
        public override string QuoteParamterName(string name) => $"?{name}";
        public override string IsNull(string sql, object value) => $"ifnull({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"concat({string.Join(", ", objs)})";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"{left} div {right}";
        public override string Now => "now()";
        public override string NowUtc => "utc_timestamp()";

        public override string QuoteWriteParamterAdapter(Type type, string paramterName)
        {
            switch (type.FullName)
            {
                case "MygisPoint":
                case "MygisLineString":
                case "MygisPolygon":
                case "MygisMultiPoint":
                case "MygisMultiLineString":
                case "MygisMultiPolygon": return $"ST_GeomFromText({paramterName})";
            }
            return paramterName;
        }
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName)
        {
            switch (mapType.FullName)
            {
                case "MygisPoint":
                case "MygisLineString":
                case "MygisPolygon":
                case "MygisMultiPoint":
                case "MygisMultiLineString":
                case "MygisMultiPolygon": return $"ST_AsText({columnName})";
            }
            return columnName;
        }

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (type == typeof(byte[])) return $"0x{CommonUtils.BytesSqlRaw(value as byte[])}";
            if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
            {
                var ts = (TimeSpan)value;
                value = $"{Math.Floor(ts.TotalHours)}:{ts.Minutes}:{ts.Seconds}";
            }
            return FormatSql("{0}", value, 1);
        }
    }
}
