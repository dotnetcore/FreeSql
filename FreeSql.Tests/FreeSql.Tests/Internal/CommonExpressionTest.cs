using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;

namespace FreeSql.Tests.Internal
{

    public class CommonExpressionTest
    {
        [Fact]
        public void IIFTest01()
        {
            var fsql = g.sqlite;
            var sql = "";
            var sb = new StringBuilder();

            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool == true).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 1)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool != true).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" <> 1)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool == false).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => !a.Bool).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 1)", sql);
            sql = fsql.Select<IIFTest01Model>().WhereCascade(a => a.Bool).Limit(10).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 1) 
limit 0,10", sql);

            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable == true).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 1)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable != true).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" <> 1)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable == false).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => !a.BoolNullable.Value).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable.Value).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 1)", sql);

            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool == true && a.Id > 0).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 1 AND a.""Id"" > 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool != true && a.Id > 0).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" <> 1 AND a.""Id"" > 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool == false && a.Id > 0).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 0 AND a.""Id"" > 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => !a.Bool && a.Id > 0).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 0 AND a.""Id"" > 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool && a.Id > 0).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 1 AND a.""Id"" > 0)", sql);

            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable == true && a.Id > 0).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 1 AND a.""Id"" > 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable != true && a.Id > 0).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" <> 1 AND a.""Id"" > 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable == false && a.Id > 0).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 0 AND a.""Id"" > 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => !a.BoolNullable.Value && a.Id > 0).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 0 AND a.""Id"" > 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable.Value && a.Id > 0).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 1 AND a.""Id"" > 0)", sql);

            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool == true && a.Id > 0 && a.Bool == true).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 1 AND a.""Id"" > 0 AND a.""Bool"" = 1)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool != true && a.Id > 0 && a.Bool != true).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" <> 1 AND a.""Id"" > 0 AND a.""Bool"" <> 1)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool == false && a.Id > 0 && a.Bool == false).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 0 AND a.""Id"" > 0 AND a.""Bool"" = 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => !a.Bool && a.Id > 0 && !a.Bool).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 0 AND a.""Id"" > 0 AND a.""Bool"" = 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool && a.Id > 0 && a.Bool).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""Bool"" = 1 AND a.""Id"" > 0 AND a.""Bool"" = 1)", sql); 
            sql = fsql.Select<IIFTest01Model>().Where(a => a.Bool && a.Id > 0 || a.Bool).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE ((a.""Bool"" = 1 AND a.""Id"" > 0 OR a.""Bool"" = 1))", sql);

            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable == true && a.Id > 0 && a.BoolNullable == true).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 1 AND a.""Id"" > 0 AND a.""BoolNullable"" = 1)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable != true && a.Id > 0 && a.BoolNullable != true).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" <> 1 AND a.""Id"" > 0 AND a.""BoolNullable"" <> 1)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable == false && a.Id > 0 && a.BoolNullable == false).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 0 AND a.""Id"" > 0 AND a.""BoolNullable"" = 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => !a.BoolNullable.Value && a.Id > 0 && !a.BoolNullable.Value).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 0 AND a.""Id"" > 0 AND a.""BoolNullable"" = 0)", sql);
            sql = fsql.Select<IIFTest01Model>().Where(a => a.BoolNullable.Value && a.Id > 0 && a.BoolNullable.Value).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Bool"", a.""BoolNullable"" 
FROM ""IIFTest01Model"" a 
WHERE (a.""BoolNullable"" = 1 AND a.""Id"" > 0 AND a.""BoolNullable"" = 1)", sql);

            // IIF
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool == true ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool != true ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" <> 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool == false ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => !a.Bool ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);

            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable == true ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable != true ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" <> 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable == false ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => !a.BoolNullable.Value ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable.Value ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);

            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool == true && a.Id > 0 ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 1 AND a.""Id"" > 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool != true && a.Id > 0 ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" <> 1 AND a.""Id"" > 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool == false && a.Id > 0 ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 0 AND a.""Id"" > 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => !a.Bool && a.Id > 0 ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 0 AND a.""Id"" > 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool && a.Id > 0 ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 1 AND a.""Id"" > 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);

            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable == true && a.Id > 0 ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 1 AND a.""Id"" > 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable != true && a.Id > 0 ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" <> 1 AND a.""Id"" > 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable == false && a.Id > 0 ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 0 AND a.""Id"" > 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => !a.BoolNullable.Value && a.Id > 0 ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 0 AND a.""Id"" > 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable.Value && a.Id > 0 ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 1 AND a.""Id"" > 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);

            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool == true && a.Id > 0 && a.Bool == true ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 1 AND a.""Id"" > 0 AND a.""Bool"" = 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool != true && a.Id > 0 && a.Bool != true ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" <> 1 AND a.""Id"" > 0 AND a.""Bool"" <> 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool == false && a.Id > 0 && a.Bool == false ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 0 AND a.""Id"" > 0 AND a.""Bool"" = 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => !a.Bool && a.Id > 0 && !a.Bool ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 0 AND a.""Id"" > 0 AND a.""Bool"" = 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.Bool && a.Id > 0 && a.Bool ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""Bool"" = 1 AND a.""Id"" > 0 AND a.""Bool"" = 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);

            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable == true && a.Id > 0 && a.BoolNullable == true ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 1 AND a.""Id"" > 0 AND a.""BoolNullable"" = 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable != true && a.Id > 0 && a.BoolNullable != true ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" <> 1 AND a.""Id"" > 0 AND a.""BoolNullable"" <> 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable == false && a.Id > 0 && a.BoolNullable == false ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 0 AND a.""Id"" > 0 AND a.""BoolNullable"" = 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => !a.BoolNullable.Value && a.Id > 0 && !a.BoolNullable.Value ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 0 AND a.""Id"" > 0 AND a.""BoolNullable"" = 0 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
            sql = fsql.Select<IIFTest01Model>().ToSql(a => a.BoolNullable.Value && a.Id > 0 && a.BoolNullable.Value ? 10 : 11);
            Assert.Equal(@"SELECT case when a.""BoolNullable"" = 1 AND a.""Id"" > 0 AND a.""BoolNullable"" = 1 then 10 else 11 end as1 
FROM ""IIFTest01Model"" a", sql);
        }

        class IIFTest01Model
        {
            public int Id { get; set; }
            public bool Bool { get; set; }
            public bool? BoolNullable { get; set; }
        }
    }
}
