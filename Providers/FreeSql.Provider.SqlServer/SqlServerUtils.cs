using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
#if microsoft
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Globalization;
using System.Text;

namespace FreeSql.SqlServer
{

    public class SqlServerUtils : CommonUtils
    {
        public SqlServerUtils(IFreeSql orm) : base(orm)
        {
        }

        public bool IsSelectRowNumber => ServerVersion <= 10;
        public bool IsSqlServer2005 => ServerVersion == 9;
        public int ServerVersion = 0;

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
            var ret = new SqlParameter { ParameterName = QuoteParamterName(parameterName), Value = value };
            var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
            if (tp != null) ret.SqlDbType = (SqlDbType)tp.Value;
            if (col != null)
            {
                var dbtype = (SqlDbType)_orm.DbFirst.GetDbType(new DatabaseModel.DbColumnInfo { DbTypeText = col.DbTypeText });
                if (dbtype != SqlDbType.Variant)
                {
                    ret.SqlDbType = dbtype;
                    //if (col.DbSize != 0) ret.Size = col.DbSize;
                    if (col.DbPrecision != 0) ret.Precision = col.DbPrecision;
                    if (col.DbScale != 0) ret.Scale = col.DbScale;
                }
            }
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<DbParameter>(sql, obj, "@", (name, type, value) =>
            {
                if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
                var ret = new SqlParameter { ParameterName = $"@{name}", Value = value };
                var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (tp != null) ret.SqlDbType = (SqlDbType)tp.Value;
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatSqlServer(args);
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
        public override string[] SplitTableName(string name) => GetSplitTableNames(name, '[', ']', 3);
        public override string QuoteParamterName(string name) => $"@{name}";
        public override string IsNull(string sql, object value) => $"isnull({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types)
        {
            var sb = new StringBuilder();
            var news = new string[objs.Length];
            for (var a = 0; a < objs.Length; a++)
            {
                if (types[a] == typeof(string)) news[a] = objs[a];
                else if (types[a].NullableTypeOrThis() == typeof(Guid)) news[a] = $"cast({objs[a]} as char(36))";
                else if (types[a].IsNumberType()) news[a] = $"cast({objs[a]} as varchar)";
                else news[a] = $"cast({objs[a]} as nvarchar(max))";
            }
            return string.Join(" + ", news);
        }
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"{left} / {right}";
        public override string Now => "getdate()";
        public override string NowUtc => "getutcdate()";

        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (type == typeof(byte[])) return $"0x{CommonUtils.BytesSqlRaw(value as byte[])}";
            if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
            {
                var ts = (TimeSpan)value;
                value = $"{ts.Hours}:{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}";
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}", (_orm.Ado as AdoProvider).AddslashesProcessParam(value, type, col));
        }
    }
}
