using FreeSql.Internal;
using FreeSql.Internal.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace FreeSql.Oracle {

	class OracleUtils : CommonUtils {
		IFreeSql _orm;
		public OracleUtils(IFreeSql orm) {
			_orm = orm;
		}

		internal override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value) {
			if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
			else if (_orm.CodeFirst.IsSyncStructureToLower) parameterName = parameterName.ToLower();
			var ret = new OracleParameter { ParameterName = $":{parameterName}", Value = value };
			var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
			if (tp != null) {
				ret.OracleDbType = (OracleDbType)tp.Value;
			}
			_params?.Add(ret);
			return ret;
		}

		internal override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
			Utils.GetDbParamtersByObject<OracleParameter>(sql, obj, ":", (name, type, value) => {
				var ret = new OracleParameter { ParameterName = $":{name}", Value = value };
				var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
				if (tp != null) {
					ret.OracleDbType = (OracleDbType)tp.Value;
				}
				return ret;
			});

		internal override string FormatSql(string sql, params object[] args) => sql?.FormatOracleSQL(args);
		internal override string QuoteSqlName(string name) => $"\"{name.Trim('"').Replace(".", "\".\"")}\"";
		internal override string QuoteParamterName(string name) => $":{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
		internal override string IsNull(string sql, object value) => $"nvl({sql}, {value})";
		internal override string StringConcat(string left, string right, Type leftType, Type rightType) => $"{left} || {right}";

		internal override string QuoteWriteParamter(Type type, string paramterName) => paramterName;
		internal override string QuoteReadColumn(Type type, string columnName) => columnName;
	}
}
