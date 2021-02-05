using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace FreeSql.Custom
{

    public class CustomUtils : CommonUtils
    {
        public CustomUtils(IFreeSql orm) : base(orm) { }
        public CustomAdapter Adapter => _orm.GetCustomAdapter();

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
            var ret = (_orm.Ado as CustomAdo)?.CreateParameter();
            ret.ParameterName = QuoteParamterName(parameterName);
            ret.Value = value;
            var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
            if (tp != null) ret.DbType = (DbType)tp.Value;
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<DbParameter>(sql, obj, null, (name, type, value) =>
            {
                if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
                var ret = (_orm.Ado as CustomAdo)?.CreateParameter();
                ret.ParameterName = QuoteParamterName(name);
                ret.Value = value;
                var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (tp != null) ret.DbType = (DbType)tp.Value;
                return ret;
            });

        static FreeSql.Custom.CustomAdo _customAdo = new FreeSql.Custom.CustomAdo();
        public override string FormatSql(string sql, params object[] args) => _customAdo.Addslashes(sql, args);
        public override string QuoteSqlName(params string[] name)
        {
            if (name.Length == 1)
            {
                var nametrim = name[0].Trim();
                if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                    return nametrim; //原生SQL
                if (nametrim.StartsWith(Adapter.QuoteSqlNameLeft.ToString()) && nametrim.EndsWith(Adapter.QuoteSqlNameRight.ToString()))
                    return nametrim;
                return $"{Adapter.QuoteSqlNameLeft}{nametrim.TrimStart(Adapter.QuoteSqlNameLeft).TrimEnd(Adapter.QuoteSqlNameRight).Replace(".", $"{Adapter.QuoteSqlNameRight}.{Adapter.QuoteSqlNameLeft}")}{Adapter.QuoteSqlNameRight}";
            }
            return $"{Adapter.QuoteSqlNameLeft}{string.Join($"{Adapter.QuoteSqlNameRight}.{Adapter.QuoteSqlNameLeft}", name)}{Adapter.QuoteSqlNameRight}";
        }
        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            //return $"{nametrim.TrimStart('[').TrimEnd(']').Replace("].[", ".").Replace(".[", ".")}";
            return $"{nametrim.TrimStart(Adapter.QuoteSqlNameLeft).TrimEnd(Adapter.QuoteSqlNameRight).Replace($"{Adapter.QuoteSqlNameRight}.{Adapter.QuoteSqlNameLeft}", ".").Replace($".{Adapter.QuoteSqlNameLeft}", ".")}";
        }
        public override string[] SplitTableName(string name) => GetSplitTableNames(name, Adapter.QuoteSqlNameLeft, Adapter.QuoteSqlNameRight, 2);
        public override string QuoteParamterName(string name) => $"@{name}";
        public override string IsNull(string sql, object value) => Adapter.IsNullSql(sql, value);
        public override string StringConcat(string[] objs, Type[] types) => Adapter.ConcatSql(objs, types);
        public override string Mod(string left, string right, Type leftType, Type rightType) => Adapter.Mod(left, right, leftType, rightType);
        public override string Div(string left, string right, Type leftType, Type rightType) => Adapter.Div(left, right, leftType, rightType);
        public override string Now => Adapter.LambdaDateTime_Now;
        public override string NowUtc => Adapter.LambdaDateTime_UtcNow;

        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => Adapter.FieldSql(type, columnName);

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (type == typeof(byte[])) return Adapter.ByteRawSql(value);
            return FormatSql("{0}", value, 1);
        }
    }
}
