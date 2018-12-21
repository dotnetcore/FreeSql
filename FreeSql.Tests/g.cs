using System;
using System.Collections.Generic;
using System.Text;


public class g {

	public static IFreeSql mysql = new FreeSql.FreeSqlBuilder()
		.UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3307;User ID=root;Password=abc123456;Database=FreeSqlTest;Charset=utf8;SslMode=none;Max pool size=10")
		.Build();

	public static IFreeSql sqlserver = new FreeSql.FreeSqlBuilder()
		.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=shop;Pooling=true;Max Pool Size=10")
		.Build();
}
