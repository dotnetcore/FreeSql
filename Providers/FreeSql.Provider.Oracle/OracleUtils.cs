using FreeSql.Internal;
using FreeSql.Internal.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Oracle
{

    class OracleUtils : CommonUtils
    {
        public OracleUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value)
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

        public override string FormatSql(string sql, params object[] args) => sql?.FormatOracleSQL(args);
        public override string QuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"\"{nametrim.Trim('"').Replace(".", "\".\"")}\"";
        }
        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"{nametrim.Trim('"').Replace("\".\"", ".").Replace(".\"", ".")}";
        }
        public override string QuoteParamterName(string name) => $":{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
        public override string IsNull(string sql, object value) => $"nvl({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"{string.Join(" || ", objs)}";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"mod({left}, {right})";

        public override string QuoteWriteParamter(Type type, string paramterName) => paramterName;
        public override string QuoteReadColumn(Type type, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type == typeof(byte[]))
            {
                var bytes = value as byte[];
                var sb = new StringBuilder().Append("rawtohex('0x");
                foreach (var vc in bytes)
                {
                    if (vc < 10) sb.Append("0");
                    sb.Append(vc.ToString("X"));
                }
                return sb.Append("')").ToString();
            }
            return FormatSql("{0}", value, 1);
        }
    }
}
