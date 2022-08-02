using FreeSql.Internal;
using FreeSql.Internal.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;

namespace FreeSql.Oracle
{

    class OracleUtils : CommonUtils
    {
        public OracleUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            var dbtype = (OracleDbType?)_orm.CodeFirst.GetDbInfo(type)?.type;
            if (dbtype == OracleDbType.Boolean)
            {
                if (value == null) value = null;
                else value = (bool)value == true ? 1 : 0;
                dbtype = OracleDbType.Int16;
            }
            var ret = new OracleParameter { ParameterName = QuoteParamterName(parameterName) };
            if (dbtype != null) ret.OracleDbType = dbtype.Value;
            ret.Value = value;
            if (col != null)
            {
                var dbtype2 = (OracleDbType)_orm.DbFirst.GetDbType(new DatabaseModel.DbColumnInfo { DbTypeTextFull = col.Attribute.DbType?.Replace("NOT NULL", "").Replace(" NULL", "").Trim(), DbTypeText = col.DbTypeText });
                switch (dbtype2)
                {
                    case OracleDbType.Char:
                    case OracleDbType.Varchar2:
                    case OracleDbType.NChar:
                    case OracleDbType.NVarchar2:
                    case OracleDbType.Decimal:
                        dbtype = dbtype2;
                        //if (col.DbSize != 0) ret.Size = col.DbSize;
                        if (col.DbPrecision != 0) ret.Precision = col.DbPrecision;
                        if (col.DbScale != 0) ret.Scale = col.DbScale;
                        break;
                    case OracleDbType.Clob:
                    case OracleDbType.NClob:
                        ret = new OracleParameter { ParameterName = QuoteParamterName(parameterName), OracleDbType = dbtype2, Value = value };
                        break;
                    case OracleDbType.Blob:
                        ret = new OracleParameter { ParameterName = QuoteParamterName(parameterName), OracleDbType = dbtype2, Value = value };
                        break;
                }
            }
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<OracleParameter>(sql, obj, ":", (name, type, value) =>
            {
                var dbtypeint = _orm.CodeFirst.GetDbInfo(type)?.type;
                var dbtype = dbtypeint != null ? (OracleDbType?)dbtypeint : null;
                if (dbtype == OracleDbType.Boolean)
                {
                    if (value == null) value = null;
                    else value = (bool)value == true ? 1 : 0;
                    dbtype = OracleDbType.Int16;
                }
                var ret = new OracleParameter { ParameterName = $":{name}" };
                if (dbtype != null) ret.OracleDbType = dbtype.Value;
                if (value is IList valueList && value is Array == false && valueList.Count > 0)
                {
                    var valueItemType = valueList[0]?.GetType();
                    if (valueItemType == typeof(int)) LocalSetListValue<int>();
                    else if (valueItemType == typeof(long)) LocalSetListValue<long>();
                    else if (valueItemType == typeof(short)) LocalSetListValue<short>();
                    else if (valueItemType == typeof(string)) LocalSetListValue<string>();
                    else if(valueItemType == typeof(Guid)) LocalSetListValue<Guid>();
                    else if (valueItemType == typeof(char)) LocalSetListValue<char>();
                    else if (valueItemType == typeof(bool)) LocalSetListValue<bool>();
                    else if (valueItemType == typeof(uint)) LocalSetListValue<uint>();
                    else if (valueItemType == typeof(ulong)) LocalSetListValue<ulong>();
                    else if (valueItemType == typeof(ushort)) LocalSetListValue<ushort>();
                    else if (valueItemType == typeof(decimal)) LocalSetListValue<decimal>();
                    else if (valueItemType == typeof(double)) LocalSetListValue<double>();
                    else if (valueItemType == typeof(float)) LocalSetListValue<float>();
                    else if (valueItemType == typeof(DateTime)) LocalSetListValue<DateTime>();

                    void LocalSetListValue<T>()
                    {
                        var valueCopy = new List<T>();
                        foreach (var valueItem in valueList) valueCopy.Add((T)Utils.GetDataReaderValue(valueItemType, valueItem));
                        value = valueCopy.ToArray();
                    }
                }
                ret.Value = value; //IList 赋值会报错
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatOracle(args);
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
