using FreeSql.DataAnnotations;
using System;
using System.Numerics;
using Xunit;

namespace FreeSql.Tests.Odbc.SqlServerMapType
{
    public class DateTimeOffSetTest
    {
        class DateTimeOffSetTestMap
        {
            public Guid id { get; set; }

            [Column(MapType = typeof(DateTime))]
            public DateTimeOffset dtos_to_dt { get; set; }
            [Column(MapType = typeof(DateTime))]
            public DateTimeOffset? dtosnullable_to_dt { get; set; }
        }
        [Fact]
        public void DateTimeToDateTimeOffSet()
        {
            //insert
            var orm = g.sqlserver;
            var item = new DateTimeOffSetTestMap { dtos_to_dt = DateTimeOffset.Now, dtosnullable_to_dt = DateTimeOffset.Now };
            Assert.Equal(1, orm.Insert<DateTimeOffSetTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<DateTimeOffSetTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.dtos_to_dt.ToString("g"), find.dtos_to_dt.ToString("g"));
            Assert.Equal(item.dtosnullable_to_dt.Value.ToString("g"), find.dtosnullable_to_dt.Value.ToString("g"));

            //update all
            item.dtos_to_dt = DateTimeOffset.Now;
            Assert.Equal(1, orm.Update<DateTimeOffSetTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<DateTimeOffSetTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.dtos_to_dt.ToString("g"), find.dtos_to_dt.ToString("g"));
            Assert.Equal(item.dtosnullable_to_dt.Value.ToString("g"), find.dtosnullable_to_dt.Value.ToString("g"));

            //update set
            Assert.Equal(1, orm.Update<DateTimeOffSetTestMap>().Where(a => a.id == item.id).Set(a => a.dtos_to_dt, item.dtos_to_dt = DateTimeOffset.Now).ExecuteAffrows());
            find = orm.Select<DateTimeOffSetTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.dtos_to_dt.ToString("g"), find.dtos_to_dt.ToString("g"));
            Assert.Equal(item.dtosnullable_to_dt.Value.ToString("g"), find.dtosnullable_to_dt.Value.ToString("g"));

            //delete
            Assert.Equal(1, orm.Delete<DateTimeOffSetTestMap>().Where(a => a.id == item.id).ExecuteAffrows());
            Assert.Null(orm.Select<DateTimeOffSetTestMap>().Where(a => a.id == item.id).First());
        }
        
    }
}
