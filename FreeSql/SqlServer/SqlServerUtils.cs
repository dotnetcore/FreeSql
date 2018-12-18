using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace FreeSql.SqlServer {

	class SqlServerUtils : CommonUtils {
		IFreeSql _orm;
		public SqlServerUtils(IFreeSql mysql) {
			_orm = mysql;
		}

		internal override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, object value) {
			if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
			SqlParameter ret = null;
			if (value == null) ret = new SqlParameter { ParameterName = $"{parameterName}", Value = DBNull.Value };
			else {
				var type = value.GetType();
				ret = new SqlParameter {
					ParameterName = parameterName,
					Value = value
				};
				var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
				if (tp != null) ret.SqlDbType = (SqlDbType)tp.Value;
			}
			_params?.Add(ret);
			return ret;
		}

		internal override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
			Utils.GetDbParamtersByObject<SqlParameter>(sql, obj, "@", (name, type, value) => {
				var cp = new SqlParameter {
					ParameterName = name,
					Value = value ?? DBNull.Value
				};
				var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
				if (tp != null) cp.SqlDbType = (SqlDbType)tp.Value;
				return cp;
			});

		internal override string FormatSql(string sql, params object[] args) => sql?.FormatSqlServer(args);
		internal override string QuoteSqlName(string name) => $"[{name.TrimStart('[').TrimEnd(']').Replace(".", "].[")}]";
		internal override string QuoteParamterName(string name) => $"@{name}";
		internal override string IsNull(string sql, object value) => $"isnull({sql}, {value})";
	}
}
