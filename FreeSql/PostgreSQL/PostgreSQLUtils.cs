using FreeSql.Internal;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace FreeSql.PostgreSQL {

	class PostgreSQLUtils : CommonUtils {
		IFreeSql _orm;
		public PostgreSQLUtils(IFreeSql orm) {
			_orm = orm;
		}

		internal override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, object value) {
			if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
			else if (_orm.CodeFirst.IsSyncStructureToLower) parameterName = parameterName.ToLower();
			NpgsqlParameter ret = null;
			if (value == null) ret = new NpgsqlParameter { ParameterName = $"{parameterName}", Value = DBNull.Value };
			else {
				var type = value.GetType();
				ret = new NpgsqlParameter {
					ParameterName = parameterName,
					Value = value
				};
				//if (value.GetType().IsEnum || value.GetType().GenericTypeArguments.FirstOrDefault()?.IsEnum == true) {
				//	ret.DataTypeName = "";
				//} else {
					var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
					if (tp != null) ret.NpgsqlDbType = (NpgsqlDbType)tp.Value;
				//}
			}
			_params?.Add(ret);
			return ret;
		}

		internal override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
			Utils.GetDbParamtersByObject<NpgsqlParameter>(sql, obj, "@", (name, type, value) => {
				var ret = new NpgsqlParameter {
					ParameterName = name,
					Value = value ?? DBNull.Value
				};
				//if (value.GetType().IsEnum || value.GetType().GenericTypeArguments.FirstOrDefault()?.IsEnum == true) {
				//	ret.DataTypeName = "";
				//} else {
					var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
					if (tp != null) ret.NpgsqlDbType = (NpgsqlDbType)tp.Value;
				//}
				return ret;
			});

		internal override string FormatSql(string sql, params object[] args) => sql?.FormatPostgreSQL(args);
		internal override string QuoteSqlName(string name) => $"\"{name.Trim('"').Replace(".", "\".\"")}\"";
		internal override string QuoteParamterName(string name) => $"@{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
		internal override string IsNull(string sql, object value) => $"coalesce({sql}, {value})";
		internal override string StringConcat(string left, string right, Type leftType, Type rightType) => $"{left} || {right}";
	}
}
