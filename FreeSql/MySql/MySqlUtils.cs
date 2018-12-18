using FreeSql.Internal;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace FreeSql.MySql {

	class MySqlUtils : CommonUtils {
		IFreeSql _orm;
		public MySqlUtils(IFreeSql mysql) {
			_orm = mysql;
		}

		internal override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, object value) {
			if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
			MySqlParameter ret = null;
			if (value == null) ret = new MySqlParameter { ParameterName = $"{parameterName}", Value = DBNull.Value };
			else {
				var type = value.GetType();
				ret = new MySqlParameter {
					ParameterName = parameterName,
					Value = value
				};
				var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
				if (tp != null) ret.MySqlDbType = (MySqlDbType)tp.Value;
			}
			_params?.Add(ret);
			return ret;
		}

		internal override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
			Utils.GetDbParamtersByObject<MySqlParameter>(sql, obj, "?", (name, type, value) => {
				var cp = new MySqlParameter {
					ParameterName = name,
					Value = value ?? DBNull.Value
				};
				var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
				if (tp != null) cp.MySqlDbType = (MySqlDbType)tp.Value;
				return cp;
			});

		internal override string FormatSql(string sql, params object[] args) => sql?.FormatMySql(args);
		internal override string QuoteSqlName(string name) => $"`{name.Trim('`').Replace(".", "`.`")}`";
		internal override string QuoteParamterName(string name) => $"?{name}";
		internal override string IsNull(string sql, object value) => $"ifnull({sql}, {value})";
	}
}
