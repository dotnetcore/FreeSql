using FreeSql.Internal;
using FreeSql.Internal.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace FreeSql.MySql {

	class MySqlUtils : CommonUtils {
		IFreeSql _orm;
		public MySqlUtils(IFreeSql orm) {
			_orm = orm;
		}

		internal override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value) {
			if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
			else if (_orm.CodeFirst.IsSyncStructureToLower) parameterName = parameterName.ToLower();
			var ret = new MySqlParameter { ParameterName = $"?{parameterName}", Value = value };
			var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
			if (tp != null) {
				if ((MySqlDbType)tp.Value == MySqlDbType.Geometry) {
					ret.MySqlDbType = MySqlDbType.Text;
					if (value != null) ret.Value = (value as MygisGeometry).AsText();
				} else
					ret.MySqlDbType = (MySqlDbType)tp.Value;
			}
			_params?.Add(ret);
			return ret;
		}

		internal override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
			Utils.GetDbParamtersByObject<MySqlParameter>(sql, obj, "?", (name, type, value) => {
				var ret = new MySqlParameter { ParameterName = $"?{name}", Value = value };
				var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
				if (tp != null) {
					if ((MySqlDbType)tp.Value == MySqlDbType.Geometry) {
						ret.MySqlDbType = MySqlDbType.Text;
						if (value != null) ret.Value = (value as MygisGeometry).AsText();
					} else
						ret.MySqlDbType = (MySqlDbType)tp.Value;
				}
				return ret;
			});

		internal override string FormatSql(string sql, params object[] args) => sql?.FormatMySql(args);
		internal override string QuoteSqlName(string name) => $"`{name.Trim('`').Replace(".", "`.`")}`";
		internal override string QuoteParamterName(string name) => $"?{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
		internal override string IsNull(string sql, object value) => $"ifnull({sql}, {value})";
		internal override string StringConcat(string left, string right, Type leftType, Type rightType) => $"concat({left}, {right})";

		internal override string QuoteWriteParamter(Type type, string paramterName) {
			switch (type.FullName) {
				case "MygisPoint":
				case "MygisLineString": 
				case "MygisPolygon": 
				case "MygisMultiPoint": 
				case "MygisMultiLineString":
				case "MygisMultiPolygon": return $"ST_GeomFromText({paramterName})";
			}
			return paramterName;
		}

		internal override string QuoteReadColumn(Type type, string columnName) {
			switch (type.FullName) {
				case "MygisPoint":
				case "MygisLineString":
				case "MygisPolygon":
				case "MygisMultiPoint":
				case "MygisMultiLineString":
				case "MygisMultiPolygon": return $"AsText({columnName})";
			}
			return columnName;
		}
	}
}
