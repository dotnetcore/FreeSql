using System;

namespace FreeSql.Site.DAL
{
    public class Db
    {
        
        public static IFreeSql mysql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.MySql, AppSettingsManager.Get("ConnectionStrings:DefaultDbContext"))
        .Build();

    }
}
