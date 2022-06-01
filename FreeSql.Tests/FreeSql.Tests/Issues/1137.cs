using System.Collections.Generic;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1137
    {
        [Fact]
        public void ListContains()
        {
            using (var fsql = new FreeSqlBuilder()
             .UseConnectionString(DataType.Sqlite, "data source=:memory:")
             .UseGenerateCommandParameterWithLambda(true)
             .Build())
            {
                fsql.Aop.ConfigEntityProperty += (s, e) =>
                {
                    if (e.Property.PropertyType.IsEnum)
                        e.ModifyResult.MapType = typeof(int);
                };
                var listEnum = new List<UserType> { UserType.Client };
                var sql = fsql.Select<User>().Where(a => listEnum.Contains(a.Type)).ToSql(a => a);
                Assert.Equal(@"SELECT a.""Type"" as1 
FROM ""User"" a 
WHERE (((a.""Type"") in (1)))", sql);
            }

            using (var fsql = new FreeSqlBuilder()
             .UseConnectionString(DataType.Sqlite, "data source=:memory:")
             .UseGenerateCommandParameterWithLambda(true)
             .Build())
            {
                fsql.CodeFirst.ConfigEntity<User>(a => { });
                fsql.Aop.ConfigEntityProperty += (s, e) =>
                {
                    if (e.Property.PropertyType.IsEnum)
                        e.ModifyResult.MapType = typeof(string);
                };
                var listEnum = new List<UserType> { UserType.Client };
                var sql = fsql.Select<User>().Where(a => listEnum.Contains(a.Type)).ToSql(a => a);
                Assert.Equal(@"SELECT a.""Type"" as1 
FROM ""User"" a 
WHERE (((a.""Type"") in ('Client')))", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=5;Allow User Variables=True")
             .UseGenerateCommandParameterWithLambda(true)
             .Build())
            {
                fsql.CodeFirst.Entity<User>(a => a.ToTable("issues1137_user"));
                var listEnum = new List<UserType> { UserType.Client };
                var sql = fsql.Select<User>().Where(a => listEnum.Contains(a.Type)).ToSql(a => a);
                Assert.Equal(@"SELECT a.`Type` as1 
FROM `issues1137_user` a 
WHERE (((a.`Type`) in ('Client')))", sql);
            }

            using (var fsql = new FreeSqlBuilder()
               .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=5;Allow User Variables=True")
            .UseGenerateCommandParameterWithLambda(true)
            .Build())
            {
                fsql.CodeFirst.Entity<User>(a => a.ToTable("issues1137_user"));
                fsql.Aop.ConfigEntityProperty += (s, e) =>
                {
                    if (e.Property.PropertyType.IsEnum)
                        e.ModifyResult.MapType = typeof(int);
                };
                var listEnum = new List<UserType> { UserType.Client };
                var sql = fsql.Select<User>().Where(a => listEnum.Contains(a.Type)).ToSql(a => a);
                Assert.Equal(@"SELECT a.`Type` as1 
FROM `issues1137_user` a 
WHERE (((a.`Type`) in (1)))", sql);
            }

            using (var fsql = new FreeSqlBuilder()
               .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=5;Allow User Variables=True")
            .UseGenerateCommandParameterWithLambda(true)
            .Build())
            {
                fsql.CodeFirst.Entity<User>(a => a.ToTable("issues1137_user"));
                fsql.Aop.ConfigEntityProperty += (s, e) =>
                {
                    if (e.Property.PropertyType.IsEnum)
                        e.ModifyResult.MapType = typeof(string);
                };
                var listEnum = new List<UserType> { UserType.Client };
                var sql = fsql.Select<User>().Where(a => listEnum.Contains(a.Type)).ToSql(a => a);
                Assert.Equal(@"SELECT a.`Type` as1 
FROM `issues1137_user` a 
WHERE (((a.`Type`) in ('Client')))", sql);
            }
        }

        public enum UserType
        {
            Client = 1,
            Internal = 2
        }
        public class User
        {
            public UserType Type { get; set; }
        }


    }

}
