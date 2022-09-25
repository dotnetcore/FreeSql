using FreeSql.DataAnnotations;
using FreeSql.Internal;
using System.Diagnostics;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1270
    {
        [Fact]
        public void UseNameConvert()
        {
            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                .UseNameConvert(NameConvertType.None)
                .UseAutoSyncStructure(true)
                .Build())
            {
                var ddl = fsql.CodeFirst.GetComparisonDDLStatements<SysUser>();
                Assert.Equal(@"CREATE TABLE IF NOT EXISTS ""main"".""SysUser"" (  
  ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  ""UserName"" NVARCHAR(255)
) 
;
", ddl);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                .UseNameConvert(NameConvertType.PascalCaseToUnderscore)
                .UseAutoSyncStructure(true)
                .Build())
            {
                fsql.CodeFirst.ConfigEntity<SysUser>(a => { });
                var ddl = fsql.CodeFirst.GetComparisonDDLStatements<SysUser>();
                Assert.Equal(@"CREATE TABLE IF NOT EXISTS ""main"".""Sys_User"" (  
  ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  ""User_Name"" NVARCHAR(255)
) 
;
", ddl);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                .UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithLower)
                .UseAutoSyncStructure(true)
                .Build())
            {
                fsql.CodeFirst.ConfigEntity<SysUser>(a => { });
                var ddl = fsql.CodeFirst.GetComparisonDDLStatements<SysUser>();
                Assert.Equal(@"CREATE TABLE IF NOT EXISTS ""main"".""sys_user"" (  
  ""id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  ""user_name"" NVARCHAR(255)
) 
;
", ddl);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                .UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithUpper)
                .UseAutoSyncStructure(true)
                .Build())
            {
                fsql.CodeFirst.ConfigEntity<SysUser>(a => { });
                var ddl = fsql.CodeFirst.GetComparisonDDLStatements<SysUser>();
                Assert.Equal(@"CREATE TABLE IF NOT EXISTS ""main"".""SYS_USER"" (  
  ""ID"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  ""USER_NAME"" NVARCHAR(255)
) 
;
", ddl);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                .UseNameConvert(NameConvertType.ToLower)
                .UseAutoSyncStructure(true)
                .Build())
            {
                fsql.CodeFirst.ConfigEntity<SysUser>(a => { });
                var ddl = fsql.CodeFirst.GetComparisonDDLStatements<SysUser>();
                Assert.Equal(@"CREATE TABLE IF NOT EXISTS ""main"".""sysuser"" (  
  ""id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  ""username"" NVARCHAR(255)
) 
;
", ddl);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                .UseNameConvert(NameConvertType.ToUpper)
                .UseAutoSyncStructure(true)
                .Build())
            {
                fsql.CodeFirst.ConfigEntity<SysUser>(a => { });
                var ddl = fsql.CodeFirst.GetComparisonDDLStatements<SysUser>();
                Assert.Equal(@"CREATE TABLE IF NOT EXISTS ""main"".""SYSUSER"" (  
  ""ID"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  ""USERNAME"" NVARCHAR(255)
) 
;
", ddl);
            }
        }

        public class SysUser
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public int Id { get; set; }

            public string UserName { get; set; }
        }

    }

}
