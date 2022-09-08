using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace FreeSql.Custom.Oracle
{

    class CustomOracleUtils : CommonUtils
    {
        DbProviderFactory Factory => FreeSqlCustomAdapterGlobalExtensions.GetDbProviderFactory(_orm);

        public CustomOracleUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            var dbtype = (DbType?)_orm.CodeFirst.GetDbInfo(type)?.type;
            switch (dbtype)
            {
                case DbType.Boolean:
                    if (value == null) value = null;
                    else value = (bool)value == true ? 1 : 0;
                    dbtype = DbType.Int32;
                    break;
               
                case DbType.String:
                case DbType.AnsiString:
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                    value = string.Concat(value);
                    break;
            }
            var ret = Factory.CreateParameter();
            ret.ParameterName = QuoteParamterName(parameterName);
            if (dbtype != null) ret.DbType = dbtype.Value;
            ret.Value = value;
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<DbParameter>(sql, obj, null, (name, type, value) =>
            {
                var dbtype = (DbType?)_orm.CodeFirst.GetDbInfo(type)?.type;
                switch (dbtype)
                {
                    case DbType.Boolean:
                        if (value == null) value = null;
                        else value = (bool)value == true ? 1 : 0;
                        dbtype = DbType.Int32;
                        break;

                    case DbType.String:
                    case DbType.AnsiString:
                    case DbType.StringFixedLength:
                    case DbType.AnsiStringFixedLength:
                        value = string.Concat(value);
                        break;
                }
                var ret = Factory.CreateParameter();
                ret.ParameterName = $":{name}";
                if (dbtype != null) ret.DbType = dbtype.Value;
                ret.Value = value;
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatCustomOracle(args);
        public override string QuoteSqlName(params string[] name)
        {
            if (name.Length == 1)
            {
                var nametrim = name[0].Trim();
                if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                    return nametrim; //原生SQL
                if (nametrim.StartsWith("\"") && nametrim.EndsWith("\""))
                    return nametrim;
                return $"\"{nametrim.Replace(".", "\".\"")}\"";
            }
            return $"\"{string.Join("\".\"", name)}\"";
        }
        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"{nametrim.Trim('"').Replace("\".\"", ".").Replace(".\"", ".")}";
        }
        public override string[] SplitTableName(string name) => GetSplitTableNames(name, '"', '"', 2);
        public override string QuoteParamterName(string name) => $":{name}";
        public override string IsNull(string sql, object value) => $"nvl({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"{string.Join(" || ", objs)}";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"mod({left}, {right})";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"trunc({left} / {right})";
        public override string Now => "systimestamp";
        public override string NowUtc => "sys_extract_utc(systimestamp)";

        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (type == typeof(string))
            {
                var valueString = value as string;
                if (valueString != null)
                {
                    if (valueString.Length < 4000) return string.Concat("'", valueString.Replace("'", "''"), "'");
                    var pam = AppendParamter(specialParams, $"p_{specialParams?.Count}{specialParamFlag}", col, type, value);
                    return pam.ParameterName;
                }
            }
            if (type == typeof(byte[]))
            {
                var valueBytes = value as byte[];
                if (valueBytes != null)
                {
                    if (valueBytes.Length < 4000) return $"hextoraw('{CommonUtils.BytesSqlRaw(valueBytes)}')";
                    var pam = AppendParamter(specialParams, $"p_{specialParams?.Count}{specialParamFlag}", col, type, value);
                    return pam.ParameterName;
                }
            }
            return FormatSql("{0}", value, 1);
        }
    }
}
