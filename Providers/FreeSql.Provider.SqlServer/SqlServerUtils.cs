using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.SqlServer
{

    class SqlServerUtils : CommonUtils
    {
        public SqlServerUtils(IFreeSql orm) : base(orm)
        {
        }

        public bool IsSelectRowNumber = true;

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
            var ret = new SqlParameter { ParameterName = QuoteParamterName(parameterName), Value = value };
            var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
            if (tp != null) ret.SqlDbType = (SqlDbType)tp.Value;
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<SqlParameter>(sql, obj, "@", (name, type, value) =>
            {
                if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
                var ret = new SqlParameter { ParameterName = $"@{name}", Value = value };
                var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (tp != null) ret.SqlDbType = (SqlDbType)tp.Value;
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatSqlServer(args);
        public override string QuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"[{nametrim.TrimStart('[').TrimEnd(']').Replace(".", "].[")}]";
        }
        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"{nametrim.TrimStart('[').TrimEnd(']').Replace("].[", ".").Replace(".[", ".")}";
        }
        public override string QuoteParamterName(string name) => $"@{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
        public override string IsNull(string sql, object value) => $"isnull({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types)
        {
            var sb = new StringBuilder();
            var news = new string[objs.Length];
            for (var a = 0; a < objs.Length; a++)
                news[a] = types[a] == typeof(string) ? objs[a] : $"cast({objs[a]} as nvarchar)";
            return string.Join(" + ", news);
        }
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";

        public override string QuoteWriteParamter(Type type, string paramterName) => paramterName;
        public override string QuoteReadColumn(Type type, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type == typeof(byte[]))
            {
                var bytes = value as byte[];
                var sb = new StringBuilder().Append("0x");
                foreach (var vc in bytes)
                {
                    if (vc < 10) sb.Append("0");
                    sb.Append(vc.ToString("X"));
                }
                return sb.ToString();
            }
            else if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
            {
                var ts = (TimeSpan)value;
                value = $"{ts.Hours}:{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}";
            }
            return FormatSql("{0}", value, 1);
        }
    }
}
