using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;

namespace FreeSql.Sqlite
{

    class SqliteUtils : CommonUtils
    {
        public SqliteUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            var dbtype = (DbType)_orm.CodeFirst.GetDbInfo(type)?.type;
            switch (dbtype)
            {
                case DbType.Guid:
                    if (value == null) value = null;
                    else value = ((Guid)value).ToString();
                    dbtype = DbType.String;
                    break;
                case DbType.Time:
                    if (value == null) value = null;
                    else value = ((TimeSpan)value).Ticks / 10000;
                    dbtype = DbType.Int64;
                    break;
            }
            var ret = new SQLiteParameter();
            ret.ParameterName = QuoteParamterName(parameterName);
            ret.DbType = dbtype;
            ret.Value = value;
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<DbParameter>(sql, obj, "@", (name, type, value) =>
            {
                var typeint = _orm.CodeFirst.GetDbInfo(type)?.type;
                var dbtype = typeint != null ? (DbType?)typeint : null;
                if (dbtype != null)
                {
                    switch (dbtype.Value)
                    {
                        case DbType.Guid:
                            if (value == null) value = null;
                            else value = ((Guid)value).ToString();
                            dbtype = DbType.String;
                            break;
                        case DbType.Time:
                            if (value == null) value = null;
                            else value = ((TimeSpan)value).Ticks / 10000;
                            dbtype = DbType.Int64;
                            break;
                    }
                }
                var ret = new SQLiteParameter();
                ret.ParameterName = $"@{name}";
                if (dbtype != null) ret.DbType = dbtype.Value;
                ret.Value = value;
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatSqlite(args);
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
        public override string QuoteParamterName(string name) => $"@{name}";
        public override string IsNull(string sql, object value) => $"ifnull({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"{string.Join(" || ", objs)}";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"{left} / {right}";
        public override string Now => "datetime(current_timestamp,'localtime')";
        public override string NowUtc => "current_timestamp";

        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (type == typeof(byte[]))
            {
                var pam = AppendParamter(specialParams, $"p_{specialParams?.Count}{specialParamFlag}", null, type, value);
                return pam.ParameterName;
            }
            return FormatSql("{0}", value, 1);
        }
    }
}
