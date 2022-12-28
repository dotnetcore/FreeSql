using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using System.Globalization;

namespace FreeSql.GBase
{

    class GBaseUtils : CommonUtils
    {
        public GBaseUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = "?";
            var ret = new OdbcParameter { ParameterName = "?", Value = value };
            var dbtype = (OdbcType?)_orm.CodeFirst.GetDbInfo(type)?.type;
            if (col != null)
            {
                var dbtype2 = (OdbcType)_orm.DbFirst.GetDbType(new DatabaseModel.DbColumnInfo { DbTypeText = col.DbTypeText, DbTypeTextFull = col.Attribute.DbType, MaxLength = col.DbSize });
                switch (dbtype2)
                {
                    case OdbcType.VarBinary:
                        break;
                    default:
                        dbtype = dbtype2;
                        //if (col.DbSize != 0) ret.Size = col.DbSize;
                        if (col.DbPrecision != 0) ret.Precision = col.DbPrecision;
                        if (col.DbScale != 0) ret.Scale = col.DbScale;
                        break;
                }
            }
            if (dbtype != null) ret.OdbcType = dbtype.Value;
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<DbParameter>("*", obj, "?", (name, type, value) =>
            {
                var ret = new OdbcParameter { ParameterName = $"{name}" };
                var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (tp != null) ret.OdbcType = (OdbcType)tp.Value;
                else
                {
                    ret.OdbcType = OdbcType.VarChar;
                    ret.Size = 8000;
                }
                ret.Value = value;
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatGBase(args);
        public override string QuoteSqlNameAdapter(params string[] name)
        {
            if (name.Length == 1)
            {
                var nametrim = name[0].Trim();
                if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                    return nametrim; //原生SQL
                return nametrim;
            }
            return string.Join(":", name);
        }
        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return nametrim;
        }
        public override string[] SplitTableName(string name) => name?.Split(new char[] { ':' }, 1);
        public override string QuoteParamterName(string name) => $"?{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
        public override string IsNull(string sql, object value) => $"nvl({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"{string.Join(" || ", objs)}";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"mod({left},{right})";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"trunc({left}/{right})";
        public override string Now => "current";
        public override string NowUtc => "current";

        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (type == typeof(byte[]))
            {
                var pam = AppendParamter(specialParams, "", null, type, value);
                return pam.ParameterName;
            }
            if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
            {
                var ts = (TimeSpan)value;
                return $"interval({ts.Days} {ts.Hours}:{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}) day(9) to fraction";
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                if (col?.DbPrecision > 0)
                    return string.Concat("'", ((DateTime)value).ToString($"yyyy-MM-dd HH:mm:ss.{"f".PadRight(col.DbPrecision, 'f')}"), "'");
                return string.Concat("'", ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"), "'");
            }
            if (type == typeof(string) && ((string)value)?.Length > 8000)
            {
                var pam = AppendParamter(specialParams, "", null, type, value);
                ((OdbcParameter)pam).OdbcType = OdbcType.Text;
                return pam.ParameterName;
            }
            return FormatSql("{0}", value, 1);
        }
    }
}
