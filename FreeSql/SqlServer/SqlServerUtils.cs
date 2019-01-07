using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace FreeSql.SqlServer {

	class SqlServerUtils : CommonUtils {
		IFreeSql _orm;
		public SqlServerUtils(IFreeSql orm) {
			_orm = orm;
		}

		internal override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value) {
			if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
			else if (_orm.CodeFirst.IsSyncStructureToLower) parameterName = parameterName.ToLower();
			if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
			var ret = new SqlParameter { ParameterName = $"@{parameterName}", Value = value };
			var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
			if (tp != null) ret.SqlDbType = (SqlDbType)tp.Value;
			_params?.Add(ret);
			return ret;
		}

		internal override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
			Utils.GetDbParamtersByObject<SqlParameter>(sql, obj, "@", (name, type, value) => {
				if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
				var ret = new SqlParameter { ParameterName = $"@{name}", Value = value };
				var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
				if (tp != null) ret.SqlDbType = (SqlDbType)tp.Value;
				return ret;
			});

		internal override string FormatSql(string sql, params object[] args) => sql?.FormatSqlServer(args);
		internal override string QuoteSqlName(string name) => $"[{name.TrimStart('[').TrimEnd(']').Replace(".", "].[")}]";
		internal override string QuoteParamterName(string name) => $"@{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
		internal override string IsNull(string sql, object value) => $"isnull({sql}, {value})";
		internal override string StringConcat(string left, string right, Type leftType, Type rightType) => $"{(leftType.FullName == "System.String" ? left : $"cast({left} as nvarchar)")} + {(rightType.FullName == "System.String" ? right : $"cast({right} as nvarchar)")}";
		internal override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";

		internal override string QuoteWriteParamter(Type type, string paramterName) => paramterName;
		internal override string QuoteReadColumn(Type type, string columnName) => columnName;
	}
}
