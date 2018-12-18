public static class Const {
	public static IFreeSql mysql = new FreeSql.FreeSqlBuilder()
		.UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10")
		.Build();
}
