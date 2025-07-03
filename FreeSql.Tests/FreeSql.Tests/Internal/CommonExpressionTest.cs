using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata;
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

        [Fact]
        public void IIFTest02()
        {
            var parameters = new List<DbParameter>();

            var fsql = g.sqlite;
            var sql = "";
            var startTime = DateTime.UtcNow;
            var query = fsql.Select<IIFTest02Model1, IIFTest02Model2>()
                .WithParameters(parameters)
                .InnerJoin((model1, model2) =>
                    model2.StartDateTime == startTime
                    && model1.StartTime <= model2.EndDateTime
                    && model1.StopTime > model2.StartDateTime)
                .GroupBy((model1, model2) => new
                {
                    model1.EntityId,
                    model2.StartDateTime,
                    model2.EndDateTime,
                })
                .WithTempQuery(group => new
                {
                    EntityId = group.Key.EntityId,
                    DateTime = group.Key.StartDateTime,
                    TotalRunTime = group.Sum<double>(((group.Value.Item1.StopTime > group.Key.EndDateTime ? group.Key.EndDateTime : group.Value.Item1.StopTime)
                        - (group.Value.Item1.StartTime < group.Key.StartDateTime ? group.Key.StartDateTime : group.Value.Item1.StartTime)).TotalSeconds),
                    Count = group.Count()
                })
                .WithParameters(parameters);

            sql = query.ToSql();
            Assert.Equal($@"SELECT * 
FROM ( 
    SELECT a.""EntityId"", b.""StartDateTime"" ""DateTime"", sum(((strftime('%s',case when a.""StopTime"" > b.""EndDateTime"" then b.""EndDateTime"" else a.""StopTime"" end)-strftime('%s',case when a.""StartTime"" < b.""StartDateTime"" then b.""StartDateTime"" else a.""StartTime"" end)))) ""TotalRunTime"", count(1) ""Count"" 
    FROM ""IIFTest02Model1"" a 
    INNER JOIN ""IIFTest02Model2"" b ON b.""StartDateTime"" = '{startTime.ToString("yyyy-MM-dd HH:mm:ss")}' AND a.""StartTime"" <= b.""EndDateTime"" AND a.""StopTime"" > b.""StartDateTime"" 
    GROUP BY a.""EntityId"", b.""StartDateTime"", b.""EndDateTime"" ) a", sql);

        }


        public class IIFTest02Model1
        {
            public long _Id { get; set; }
            public long Id { get; set; }


            public long EntityId { get; set; }

            public DateTime StartTime { get; set; }
            public DateTime StopTime { get; set; }

        }

        /// <summary>
        /// 任务统计日期
        /// </summary>
        public class IIFTest02Model2
        {
            public long Id { get; set; }
            public DateTime StartDateTime { get; set; }
            public DateTime EndDateTime { get; set; }

        }


        [Fact]
        public void IIFTest03()
        {
            var parameters = new List<DbParameter>();

            var fsql = g.dameng;
            var sql = "";
            var startTime = DateTime.UtcNow;
            //fsql.CodeFirst.SyncStructure<IIFTest03Model>();
            //fsql.Insert(new IIFTest03Model { StartDateTime = startTime, EndDateTime = startTime.AddHours(1) }).ExecuteAffrows();

            var query = fsql.Select<IIFTest03Model>();

            //sql = query.ToSql(model => new
            //{
            //    Timespan = (model.EndDateTime - model.StartDateTime).TotalSeconds,
            //});

            var result = query.ToList(model => new
                {
                    Timespan = (model.EndDateTime - model.StartDateTime),
                });

            //            sql = query.ToSql();
            //            Assert.Equal($@"SELECT * 
            //FROM ( 
            //    SELECT a.""EntityId"", b.""StartDateTime"" ""DateTime"", sum(((strftime('%s',case when a.""StopTime"" > b.""EndDateTime"" then b.""EndDateTime"" else a.""StopTime"" end)-strftime('%s',case when a.""StartTime"" < b.""StartDateTime"" then b.""StartDateTime"" else a.""StartTime"" end)))) ""TotalRunTime"", count(1) ""Count"" 
            //    FROM ""IIFTest02Model1"" a 
            //    INNER JOIN ""IIFTest02Model2"" b ON b.""StartDateTime"" = '{startTime.ToString("yyyy-MM-dd HH:mm:ss")}' AND a.""StartTime"" <= b.""EndDateTime"" AND a.""StopTime"" > b.""StartDateTime"" 
            //    GROUP BY a.""EntityId"", b.""StartDateTime"", b.""EndDateTime"" ) a", sql);

        }


        /// <summary>
        /// 任务统计日期
        /// </summary>
        public class IIFTest03Model
        {
            public long Id { get; set; }
            public DateTime StartDateTime { get; set; }
            public DateTime EndDateTime { get; set; }

        }
    }
}
