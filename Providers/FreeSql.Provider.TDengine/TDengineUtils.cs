using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace FreeSql.TDengine
{
    internal class TDengineUtils : CommonUtils
    {
        public TDengineUtils(IFreeSql orm) : base(orm)
        {
        }

        public override string Now => "now()";

        public override string NowUtc => throw new NotImplementedException();

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            throw new NotImplementedException();
        }

        public override string Div(string left, string right, Type leftType, Type rightType)
        {
            throw new NotImplementedException();
        }

        public override string FormatSql(string sql, params object[] args)
        {
            throw new NotImplementedException();
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj)
        {
            throw new NotImplementedException();
        }

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            throw new NotImplementedException();
        }

        public override string IsNull(string sql, object value)
        {
            throw new NotImplementedException();
        }

        public override string Mod(string left, string right, Type leftType, Type rightType)
        {
            throw new NotImplementedException();
        }

        public override string QuoteParamterName(string name)
        {
            throw new NotImplementedException();
        }

        public override string QuoteSqlNameAdapter(params string[] name)
        {
            throw new NotImplementedException();
        }

        public override string QuoteWriteParamterAdapter(Type type, string paramterName)
        {
            throw new NotImplementedException();
        }

        public override string[] SplitTableName(string name)
        {
            throw new NotImplementedException();
        }

        public override string StringConcat(string[] objs, Type[] types)
        {
            throw new NotImplementedException();
        }

        public override string TrimQuoteSqlName(string name)
        {
            throw new NotImplementedException();
        }

        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName)
        {
            throw new NotImplementedException();
        }
    }
}
