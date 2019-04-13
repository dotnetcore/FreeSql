using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace FreeSql.SqlServer {

	class SqlServerUtils : CommonUtils {
		public SqlServerUtils(IFreeSql orm) : base(orm) {
		}

		internal bool IsSelectRowNumber = true;

		internal override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value) {
			if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
			if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
			var ret = new SqlParameter { ParameterName = QuoteParamterName(parameterName), Value = value };
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
		internal override string QuoteSqlName(string name) {
			var nametrim = name.Trim();
			if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
				return nametrim; //原生SQL
			return $"[{nametrim.TrimStart('[').TrimEnd(']').Replace(".", "].[")}]";
		}
		internal override string QuoteParamterName(string name) => $"@{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
		internal override string IsNull(string sql, object value) => $"isnull({sql}, {value})";
		internal override string StringConcat(string left, string right, Type leftType, Type rightType) => $"{(leftType.FullName == "System.String" ? left : $"cast({left} as nvarchar)")} + {(rightType.FullName == "System.String" ? right : $"cast({right} as nvarchar)")}";
		internal override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";

		internal override string QuoteWriteParamter(Type type, string paramterName) => paramterName;
		internal override string QuoteReadColumn(Type type, string columnName) => columnName;

		internal override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, Type type, object value) {
			if (value == null) return "NULL";
			if (type == typeof(byte[])) {
				var bytes = value as byte[];
				var sb = new StringBuilder().Append("0x");
				foreach (var vc in bytes) {
					if (vc < 10) sb.Append("0");
					sb.Append(vc.ToString("X"));
				}
				return sb.ToString();
			} else if (type == typeof(TimeSpan) || type == typeof(TimeSpan?)) {
				var ts = (TimeSpan)value;
				value = $"{ts.Hours}:{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}";
			}
			return FormatSql("{0}", value, 1);
		}
	}
}
