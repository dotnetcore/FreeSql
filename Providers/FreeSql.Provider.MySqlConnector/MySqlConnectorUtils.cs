using FreeSql.Internal;
using FreeSql.Internal.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Globalization;

namespace FreeSql.MySql
{

    class MySqlUtils : CommonUtils
    {
        public MySqlUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            var ret = new MySqlParameter { ParameterName = QuoteParamterName(parameterName), Value = value };
            var dbtype = (MySqlDbType)_orm.CodeFirst.GetDbInfo(type)?.type;
            if (col != null)
            {
                var dbtype2 = (MySqlDbType)_orm.DbFirst.GetDbType(new DatabaseModel.DbColumnInfo { DbTypeText = col.DbTypeText, DbTypeTextFull = col.Attribute.DbType, MaxLength = col.DbSize });
                switch (dbtype2)
                {
                    //case MySqlDbType.Binary:
                    //case MySqlDbType.VarBinary:
                    //    break;
                    default:
                        dbtype = dbtype2;
                        //if (col.DbSize != 0) ret.Size = col.DbSize;
                        if (col.DbPrecision != 0) ret.Precision = col.DbPrecision;
                        if (col.DbScale != 0) ret.Scale = col.DbScale;
                        break;
                }
            }
            if (dbtype == MySqlDbType.Geometry)
            {
                ret.MySqlDbType = MySqlDbType.Text;
                if (value != null) ret.Value = (value as MygisGeometry).AsText();
            }
            else
            {
                ret.MySqlDbType = dbtype;
                if (ret.MySqlDbType == MySqlDbType.Enum && value != null)
                    ret.Value = EnumValueToMySql(value);
            }
            _params?.Add(ret);
            return ret;
        }
        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<MySqlParameter>(sql, obj, "@", (name, type, value) =>
            {
                var ret = new MySqlParameter { ParameterName = $"@{name}", Value = value };
                var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (tp != null)
                {
                    if ((MySqlDbType)tp.Value == MySqlDbType.Geometry)
                    {
                        ret.MySqlDbType = MySqlDbType.Text;
                        if (value != null) ret.Value = (value as MygisGeometry).AsText();
                    }
                    else
                    {
                        ret.MySqlDbType = (MySqlDbType)tp.Value;
                        if (ret.MySqlDbType == MySqlDbType.Enum && value != null)
                            ret.Value = EnumValueToMySql(value);
                    }
                }
                return ret;
            });

        static ConcurrentDictionary<Type, Dictionary<string, int>> _dicEnumNames = new ConcurrentDictionary<Type, Dictionary<string, int>>();
        static long EnumValueToMySql(object value) //mysqlConnector 不支持 enumString 作为参数化传递，所以要计算出下标值，mysql 下标从 1 开始，并且 c# 设置了枚举元素值会影响下标
        {
            if (value == null) return 0;
            var valueType = value.GetType().NullableTypeOrThis();
            if (valueType.IsEnum == false) return 0;
            var names = _dicEnumNames.GetOrAdd(valueType, type =>
            {
                var dic = new Dictionary<string, int>();
                var ns = Enum.GetNames(type);
                for (var a = 0; a < ns.Length; a++) dic.Add(ns[a], a + 1);
                return dic;
            });
            return names.TryGetValue(value.ToString(), out var tryval) ? tryval : 0;
        }

        public override string FormatSql(string sql, params object[] args) => sql?.FormatMySql(args);
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
        public override string QuoteParamterName(string name) => $"@{name}";
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
