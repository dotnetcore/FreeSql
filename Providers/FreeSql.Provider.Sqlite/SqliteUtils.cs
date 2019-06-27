using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Sqlite
{

    class SqliteUtils : CommonUtils
    {
        public SqliteUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value)
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
            var ret = new SQLiteParameter { ParameterName = QuoteParamterName(parameterName), DbType = dbtype, Value = value };
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<SQLiteParameter>(sql, obj, "@", (name, type, value) =>
            {
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
                var ret = new SQLiteParameter { ParameterName = $"@{name}", DbType = dbtype, Value = value };
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatSqlite(args);
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
        public override string QuoteParamterName(string name) => $"@{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
        public override string IsNull(string sql, object value) => $"ifnull({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"{string.Join(" || ", objs)}";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";

        public override string QuoteWriteParamter(Type type, string paramterName) => paramterName;
        public override string QuoteReadColumn(Type type, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type == typeof(byte[])) value = Encoding.UTF8.GetString(value as byte[]);
            return FormatSql("{0}", value, 1);
        }
    }
}
