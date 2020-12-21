using Dm;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;

namespace FreeSql.Dameng
{

    class DamengUtils : CommonUtils
    {
        public DamengUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            var dbtype = (DmDbType)_orm.CodeFirst.GetDbInfo(type)?.type;
            switch (dbtype)
            {
                case DmDbType.Bit:
                    if (value == null) value = null;
                    else value = (bool) value == true ? 1 : 0;
                    dbtype = DmDbType.Int32;
                    break;
               
                case DmDbType.Char:
                case DmDbType.VarChar:
                case DmDbType.Text:
                    value = string.Concat(value);
                    break;
            }
            var ret = new DmParameter { ParameterName = QuoteParamterName(parameterName), DmSqlType = dbtype, Value = value };
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<DmParameter>(sql, obj, null, (name, type, value) =>
            {
                var typeint = _orm.CodeFirst.GetDbInfo(type)?.type;
                var dbtype = typeint != null ? (DmDbType?)typeint : null;
                if (dbtype != null)
                {
                    switch (dbtype)
                    {
                        case DmDbType.Bit:
                            if (value == null) value = null;
                            else value = (bool)value == true ? 1 : 0;
                            dbtype = DmDbType.Int32;
                            break;

                        case DmDbType.Char:
                        case DmDbType.VarChar:
                        case DmDbType.Text:
                            value = string.Concat(value);
                            break;
                    }
                }
                var ret = new DmParameter { ParameterName = $":{name}", Value = value };
                if (dbtype != null) ret.DmSqlType = dbtype.Value;
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatDameng(args);
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
        public override string NowUtc => "getutcdate";

        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (type == typeof(byte[])) return $"hextoraw('{CommonUtils.BytesSqlRaw(value as byte[])}')";
            return FormatSql("{0}", value, 1);
        }
    }
}
