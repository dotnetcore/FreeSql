using FreeSql.Internal;
using FreeSql.Internal.Model;
using ClickHouse.Client.ADO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Data;
using ClickHouse.Client.ADO.Parameters;
using System.Text.RegularExpressions;

namespace FreeSql.ClickHouse
{
    internal class ClickHouseUtils : CommonUtils
    {
        public ClickHouseUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (value is string str)
                value = str?.Replace("\t", "\\t")
                    .Replace("\r\n", "\\r\\n")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("/", "\\/");
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            var dbtype = (DbType?)_orm.CodeFirst.GetDbInfo(type)?.type;
            DbParameter ret = new ClickHouseDbParameter { ParameterName = parameterName };//QuoteParamterName(parameterName)
            if (dbtype != null) ret.DbType = dbtype.Value;
            ret.Value = value;
            if (col != null)
            {
                var dbtype2 = (DbType)_orm.DbFirst.GetDbType(new DatabaseModel.DbColumnInfo { DbTypeText = col.DbTypeText, DbTypeTextFull = col.Attribute.DbType, MaxLength = col.DbSize });
                switch (dbtype2)
                {
                    case DbType.Binary:
                    default:
                        dbtype = dbtype2;
                        //if (col.DbSize != 0) ret.Size = col.DbSize;
                        if (col.DbPrecision != 0) ret.Precision = col.DbPrecision;
                        if (col.DbScale != 0) ret.Scale = col.DbScale;
                        break;
                }
                if (value is bool)
                    ret.Value = (bool)value ? 1 : 0;
            }
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<DbParameter>(sql, obj, "@", (name, type, value) =>
            {
                if (value is string str) 
                    value = str?.Replace("\t", "\\t")
                        .Replace("\r\n", "\\r\\n")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                        .Replace("/", "\\/");
                DbParameter ret = new ClickHouseDbParameter { ParameterName = $"@{name}", Value = value };
                var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (tp != null)
                    ret.DbType = (DbType)tp.Value;
                return ret;
            });

        public override string RewriteColumn(ColumnInfo col, string sql)
        {
            col.Attribute.DbType = col.Attribute.DbType.Replace(" NOT NULL", "");
            if (string.IsNullOrWhiteSpace(col?.Attribute.RewriteSql) == false)
                return string.Format(col.Attribute.RewriteSql, sql);
            if (Regex.IsMatch(sql, @"\{\{[\w\d]+_+\d:\{\d\}\}\}"))
                return string.Format(sql, col.Attribute.DbType);
            else
                return sql;
        }

        public override string FormatSql(string sql, params object[] args) => sql?.FormatClickHouse(args);

        public override string QuoteSqlNameAdapter(params string[] name)
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

        public override string QuoteParamterName(string name) => $"{{{{{name}:{{0}}}}}}";

        public override string IsNull(string sql, object value) => $"ifnull({sql}, {value})";

        public override string StringConcat(string[] objs, Type[] types) => $"concat({string.Join(", ", objs)})";

        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";

        public override string Div(string left, string right, Type leftType, Type rightType) => $"{left} div {right}";

        public override string Now => "now()";
        public override string NowUtc => "now('UTC')";

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