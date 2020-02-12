using FreeSql.DataAnnotations;
using System;
using System.Numerics;
using Xunit;

namespace FreeSql.Tests.Odbc.DamengMapType
{
    public class DateTimeOffSetTest
    {
        class Dtos_dt
        {
            public Guid id { get; set; }

            [Column(MapType = typeof(DateTime))]
            public DateTimeOffset dtos_to_dt { get; set; }
            [Column(MapType = typeof(DateTime))]
            public DateTimeOffset? dtofnil_to_dt { get; set; }
        }
        [Fact]
        public void DateTimeToDateTimeOffSet()
        {
            //insert
            var orm = g.dameng;
            var item = new Dtos_dt { dtos_to_dt = DateTimeOffset.Now, dtofnil_to_dt = DateTimeOffset.Now };
            Assert.Equal(1, orm.Insert<Dtos_dt>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<Dtos_dt>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.dtos_to_dt.ToString("g"), find.dtos_to_dt.ToString("g"));
            Assert.Equal(item.dtofnil_to_dt.Value.ToString("g"), find.dtofnil_to_dt.Value.ToString("g"));

            //update all
            item.dtos_to_dt = DateTimeOffset.Now;
            Assert.Equal(1, orm.Update<Dtos_dt>().SetSource(item).ExecuteAffrows());
            find = orm.Select<Dtos_dt>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.dtos_to_dt.ToString("g"), find.dtos_to_dt.ToString("g"));
            Assert.Equal(item.dtofnil_to_dt.Value.ToString("g"), find.dtofnil_to_dt.Value.ToString("g"));

            //update set
            Assert.Equal(1, orm.Update<Dtos_dt>().Where(a => a.id == item.id).Set(a => a.dtos_to_dt, item.dtos_to_dt = DateTimeOffset.Now).ExecuteAffrows());
            find = orm.Select<Dtos_dt>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.dtos_to_dt.ToString("g"), find.dtos_to_dt.ToString("g"));
            Assert.Equal(item.dtofnil_to_dt.Value.ToString("g"), find.dtofnil_to_dt.Value.ToString("g"));

            //delete
            Assert.Equal(1, orm.Delete<Dtos_dt>().Where(a => a.id == item.id).ExecuteAffrows());
            Assert.Null(orm.Select<Dtos_dt>().Where(a => a.id == item.id).First());
        }
        
    }
}
