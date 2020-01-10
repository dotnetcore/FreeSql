using FreeSql.Internal;
using FreeSql.Internal.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;

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
            var dbtype = (OracleDbType)_orm.CodeFirst.GetDbInfo(type)?.type;
            if (dbtype == OracleDbType.Boolean)
            {
                if (value == null) value = null;
                else value = (bool)value == true ? 1 : 0;
                dbtype = OracleDbType.Int16;
            }
            var ret = new OracleParameter { ParameterName = QuoteParamterName(parameterName), OracleDbType = dbtype, Value = value };
            if (col != null)
            {
                var dbtype2 = (OracleDbType)_orm.DbFirst.GetDbType(new DatabaseModel.DbColumnInfo { DbTypeTextFull = col.Attribute.DbType.Replace("NOT NULL", "").Trim(), DbTypeText = col.DbTypeText });
                switch (dbtype2)
                {
                    case OracleDbType.Char:
                    case OracleDbType.Varchar2:
                    case OracleDbType.NChar:
                    case OracleDbType.NVarchar2:
                    case OracleDbType.Decimal:
                        dbtype = dbtype2;
                        if (col.DbSize != 0) ret.Size = col.DbSize;
                        if (col.DbPrecision != 0) ret.Precision = col.DbPrecision;
                        if (col.DbScale != 0) ret.Scale = col.DbScale;
                        break;
                }
            }
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<OracleParameter>(sql, obj, ":", (name, type, value) =>
            {
                var dbtype = (OracleDbType)_orm.CodeFirst.GetDbInfo(type)?.type;
                if (dbtype == OracleDbType.Boolean)
                {
                    if (value == null) value = null;
                    else value = (bool)value == true ? 1 : 0;
                    dbtype = OracleDbType.Int16;
                }
                var ret = new OracleParameter { ParameterName = $":{name}", OracleDbType = dbtype, Value = value };
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
        public override string QuoteParamterName(string name) => $":{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
        public override string IsNull(string sql, object value) => $"nvl({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"{string.Join(" || ", objs)}";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"mod({left}, {right})";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"trunc({left} / {right})";
        public override string Now => "systimestamp";
        public override string NowUtc => "sys_extract_utc(systimestamp)";

        public override string QuoteWriteParamter(Type type, string paramterName) => paramterName;
        public override string QuoteReadColumn(Type type, Type mapType, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type == typeof(byte[])) return $"hextoraw('{CommonUtils.BytesSqlRaw(value as byte[])}')";
            return FormatSql("{0}", value, 1);
        }
    }
}
