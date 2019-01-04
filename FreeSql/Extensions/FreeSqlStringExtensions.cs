public static class FreeSqlStringExtensions {

	/// <summary>
	/// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
	/// </summary>
	/// <param name="that"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public static string FormatMySql(this string that, params object[] args) => _mysqlAdo.Addslashes(that, args);
	static FreeSql.MySql.MySqlAdo _mysqlAdo = new FreeSql.MySql.MySqlAdo();
	/// <summary>
	/// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
	/// </summary>
	/// <param name="that"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public static string FormatSqlServer(this string that, params object[] args) => _sqlserverAdo.Addslashes(that, args);
	static FreeSql.SqlServer.SqlServerAdo _sqlserverAdo = new FreeSql.SqlServer.SqlServerAdo();
	/// <summary>
	/// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
	/// </summary>
	/// <param name="that"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public static string FormatPostgreSQL(this string that, params object[] args) => _postgresqlAdo.Addslashes(that, args);
	static FreeSql.PostgreSQL.PostgreSQLAdo _postgresqlAdo = new FreeSql.PostgreSQL.PostgreSQLAdo();
	/// <summary>
	/// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
	/// </summary>
	/// <param name="that"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public static string FormatOracleSQL(this string that, params object[] args) => _oracleAdo.Addslashes(that, args);
	static FreeSql.Oracle.OracleAdo _oracleAdo = new FreeSql.Oracle.OracleAdo();
}

namespace System.Runtime.CompilerServices {
	public class ExtensionAttribute : Attribute { }
}