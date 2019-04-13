using FreeSql.Internal;
using FreeSql.Internal.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace FreeSql.MySql {

	class MySqlUtils : CommonUtils {
		public MySqlUtils(IFreeSql orm) : base(orm) {
		}

		internal override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value) {
			if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
			var ret = new MySqlParameter { ParameterName = QuoteParamterName(parameterName), Value = value };
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
		internal override string QuoteSqlName(string name) {
			var nametrim = name.Trim();
			if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
				return nametrim; //原生SQL
			return $"`{nametrim.Trim('`').Replace(".", "`.`")}`";
		}
		internal override string QuoteParamterName(string name) => $"?{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
		internal override string IsNull(string sql, object value) => $"ifnull({sql}, {value})";
		internal override string StringConcat(string left, string right, Type leftType, Type rightType) => $"concat({left}, {right})";
		internal override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";
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

		internal override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, Type type, object value) {
			if (value == null) return "NULL";
			if (type == typeof(byte[])) {
				var bytes = value as byte[];
				var sb = new StringBuilder().Append("0x");
				foreach (var vc in bytes) {
					if (vc < 10) sb.Append("0");
					sb.Append(vc.ToString("X"));
				}
				return sb.ToString(); //val = Encoding.UTF8.GetString(val as byte[]);
			} else if (type == typeof(TimeSpan) || type == typeof(TimeSpan?)) {
				var ts = (TimeSpan)value;
				value = $"{Math.Floor(ts.TotalHours)}:{ts.Minutes}:{ts.Seconds}";
			}
			return FormatSql("{0}", value, 1);
		}
	}
}
