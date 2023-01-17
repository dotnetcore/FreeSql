using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;

namespace FreeSql.Tests.Internal
{

    public class UtilsTest
    {
        [Fact]
        public void TestGetDbParamtersByObject()
        {
            var ps = FreeSql.Internal.Utils.
                   GetDbParamtersByObject<DbParameter>("select @p",
                   new { p = (DbParameter)new SqlParameter() { ParameterName = "p", Value = "test" } },
                   "@",
                   (name, type, value) =>
                      {
                          if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
                          var ret = new SqlParameter { ParameterName = $"@{name}", Value = value };
                          return ret;
                      });
            Assert.Single(ps);
            Assert.Equal("test", ps[0].Value);
            Assert.Equal("p", ps[0].ParameterName);
            Assert.Equal(typeof(SqlParameter), ps[0].GetType());


            var ps2 = FreeSql.Internal.Utils.
                   GetDbParamtersByObject<DbParameter>("select @p",
                   new Dictionary<string, DbParameter> { { "p", (DbParameter)new SqlParameter() { ParameterName = "p", Value = "test" } } },
                   "@",
                   (name, type, value) =>
                   {
                       if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
                       var ret = new SqlParameter { ParameterName = $"@{name}", Value = value };
                       return ret;
                   });
            Assert.Single(ps2);
            Assert.Equal("test", ps2[0].Value);
            Assert.Equal("p", ps2[0].ParameterName);
            Assert.Equal(typeof(SqlParameter), ps2[0].GetType());


            //测试匿名对象支持
            dynamic expObj = new ExpandoObject();
            expObj.p = "test";

            Func<string, Type, object, DbParameter> constructorParamter = (name, type, value) =>
                               {
                                   if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
                                   var ret = new SqlParameter { ParameterName = $"@{name}", Value = value };
                                   return ret;
                               };

            var ps3 = FreeSql.Internal.Utils.
                   GetDbParamtersByObject<DbParameter>("select @p",
                   expObj,
                   "@",
                   constructorParamter);
            Assert.Single(ps3);
            Assert.Equal("test", ps3[0].Value);
            Assert.Equal("p", ps3[0].ParameterName);
            Assert.Equal(typeof(SqlParameter), ps3[0].GetType());


        }

        [Fact]
        public void TestReplaceSqlConstString()
        {
            var dict = new Dictionary<string, string>();
            string sql1 = "", sql2 = "", sql3 = "";

            sql2 = FreeSql.Internal.Utils.ReplaceSqlConstString(sql1 = @"UPDATE ""as_table_log_202201"" SET ""msg"" = 'msg01', ""createtime"" = '2022-01-01 13:00:11'
WHERE (""id"" = '6252a2e6-5df3-bb10-00c1-bda60c4053fe')", dict);
            Assert.Equal(3, dict.Count);
            sql3 = sql2;
            dict.Select(a => sql3 = sql3.Replace(a.Key, "{0}".FormatMySql(a.Value))).ToList();
            Assert.Equal(sql1, sql3);

            dict.Clear();
            sql2 = FreeSql.Internal.Utils.ReplaceSqlConstString(sql1 = @"INSERT INTO ""as_table_log_202201""(""id"", ""msg"", ""createtime"") VALUES('6252a2e6-5df3-bb10-00c1-bda60c4053fe', 'msg01', '2022-01-01 13:00:11'), ('6252a2e6-5df3-bb10-00c1-bda773467785', 'msg02', '2022-01-02 14:00:12')", dict);
            Assert.Equal(6, dict.Count);
            sql3 = sql2;
            dict.Select(a => sql3 = sql3.Replace(a.Key, "{0}".FormatMySql(a.Value))).ToList();
            Assert.Equal(sql1, sql3);

            dict.Clear();
            sql2 = FreeSql.Internal.Utils.ReplaceSqlConstString(sql1 = @"DELETE FROM ""as_table_log_202205"" WHERE (""id"" = @exp_0 AND ""createtime"" between '2022-03-01 00:00:00' and '2022-05-01 00:00:00')", dict);
            Assert.Equal(2, dict.Count);
            sql3 = sql2;
            dict.Select(a => sql3 = sql3.Replace(a.Key, "{0}".FormatMySql(a.Value))).ToList();
            Assert.Equal(sql1, sql3);

            dict.Clear();
            sql2 = FreeSql.Internal.Utils.ReplaceSqlConstString(sql1 = @"UPDATE ""as_table_log_202202"" SET ""msg"" = CASE ""id""
WHEN '6252a2e6-5df3-bb10-00c1-bda818f4b93f' THEN 'msg03'
WHEN '6252a2e6-5df3-bb10-00c1-bda95dbadefd' THEN 'msg04' END, ""createtime"" = CASE ""id""
WHEN '6252a2e6-5df3-bb10-00c1-bda818f4b93f' THEN '2022-02-02 15:00:13'
WHEN '6252a2e6-5df3-bb10-00c1-bda95dbadefd' THEN '2022-02-08 15:00:13' END
WHERE (""id"" IN ('6252a2e6-5df3-bb10-00c1-bda818f4b93f','6252a2e6-5df3-bb10-00c1-bda95dbadefd'))", dict);
            Assert.Equal(6, dict.Count);
            sql3 = sql2;
            dict.Select(a => sql3 = sql3.Replace(a.Key, "{0}".FormatMySql(a.Value))).ToList();
            Assert.Equal(sql1, sql3);

            dict.Clear();
            sql2 = FreeSql.Internal.Utils.ReplaceSqlConstString(sql1 = @"UPDATE ""as_table_log_202207"" SET ""msg"" = 'msg07', ""createtime"" = '2022-07-01 00:00:00'
WHERE (""id"" = '6252a2e6-5df3-bb10-00c1-bdad01a608fb')", dict);
            Assert.Equal(3, dict.Count);
            sql3 = sql2;
            dict.Select(a => sql3 = sql3.Replace(a.Key, "{0}".FormatMySql(a.Value))).ToList();
            Assert.Equal(sql1, sql3);

            dict.Clear();
            sql2 = FreeSql.Internal.Utils.ReplaceSqlConstString(sql1 = @"UPDATE ""as_table_log_202207"" SET ""msg"" = 'newmsg'
WHERE (""id"" = 'acc5df07-11a5-45b5-8af1-7b1ffac19f68')", dict);
            Assert.Equal(2, dict.Count);
            sql3 = sql2;
            dict.Select(a => sql3 = sql3.Replace(a.Key, "{0}".FormatMySql(a.Value))).ToList();
            Assert.Equal(sql1, sql3);

            dict.Clear();
            sql2 = FreeSql.Internal.Utils.ReplaceSqlConstString(sql1 = @"UPDATE ""as_table_log_202203"" SET ""msg"" = 'newmsg'
WHERE (""id"" = '29bf2df7-3dfc-4005-a2e3-0421e50b2910') AND (""createtime"" between '2022-03-01 00:00:00' and '2022-05-01 00:00:00')", dict);
            Assert.Equal(4, dict.Count);
            sql3 = sql2;
            dict.Select(a => sql3 = sql3.Replace(a.Key, "{0}".FormatMySql(a.Value))).ToList();
            Assert.Equal(sql1, sql3);

            dict.Clear();
            sql2 = FreeSql.Internal.Utils.ReplaceSqlConstString(sql1 = @"UPDATE ""as_table_log_202203"" SET ""msg"" = 'newmsg'
WHERE (""id"" = '4c9b5b32-49b2-44ee-beee-1e399e86b933') AND (""createtime"" > '2022-03-01 00:00:00' AND ""createtime"" < '2022-05-01 00:00:00')", dict);
            Assert.Equal(4, dict.Count);
            sql3 = sql2;
            dict.Select(a => sql3 = sql3.Replace(a.Key, "{0}".FormatMySql(a.Value))).ToList();
            Assert.Equal(sql1, sql3);

            dict.Clear();
            sql2 = FreeSql.Internal.Utils.ReplaceSqlConstString(sql1 = @"UPDATE ""as_table_log_202201"" SET ""msg"" = 'newmsg'
WHERE (""id"" = '15d2a84f-bd72-4d73-8ad1-466ba8beea60') AND (""createtime"" < '2022-05-01 00:00:00')", dict);
            Assert.Equal(3, dict.Count);
            sql3 = sql2;
            dict.Select(a => sql3 = sql3.Replace(a.Key, "{0}".FormatMySql(a.Value))).ToList();
            Assert.Equal(sql1, sql3);

            dict.Clear();
            sql2 = FreeSql.Internal.Utils.ReplaceSqlConstString(sql1 = @"SELECT  * from (SELECT a.""id"", a.""msg"", a.""createtime""
FROM ""as_table_log_202204"" a
WHERE (a.""createtime"" < '2022-05-01 00:00:00')) ftb

UNION ALL

SELECT  * from (SELECT a.""id"", a.""msg"", a.""createtime""
FROM ""as_table_log_202203"" a
WHERE (a.""createtime"" < '2022-05-01 00:00:00')) ftb

UNION ALL

SELECT  * from (SELECT a.""id"", a.""msg"", a.""createtime""
FROM ""as_table_log_202202"" a
WHERE (a.""createtime"" < '2022-05-01 00:00:00')) ftb

UNION ALL

SELECT  * from (SELECT a.""id"", a.""msg"", a.""createtime""
FROM ""as_table_log_202201"" a
WHERE (a.""createtime"" < '2022-05-01 00:00:00')) ftb", dict);
            Assert.Single(dict);
            sql3 = sql2;
            dict.Select(a => sql3 = sql3.Replace(a.Key, "{0}".FormatMySql(a.Value))).ToList();
            Assert.Equal(sql1, sql3);
        }
    }
}
