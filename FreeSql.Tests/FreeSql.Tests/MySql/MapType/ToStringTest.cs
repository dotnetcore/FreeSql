using FreeSql.DataAnnotations;
using System;
using System.Numerics;
using Xunit;

namespace FreeSql.Tests.MySqlMapType
{
    public class ToStringTest
    {
        class ToStringMap
        {
            public Guid id { get; set; }

            [Column(MapType = typeof(string))]
            public TimeSpan timespan_to_string { get; set; }
            [Column(MapType = typeof(string))]
            public TimeSpan? timespannullable_to_string { get; set; }

            [Column(MapType = typeof(string))]
            public DateTime datetime_to_string { get; set; }
            [Column(MapType = typeof(string))]
            public DateTime? datetimenullable_to_string { get; set; }

            [Column(MapType = typeof(string))]
            public Guid guid_to_string { get; set; }
            [Column(MapType = typeof(string))]
            public Guid? guidnullable_to_string { get; set; }

            [Column(MapType = typeof(string))]
            public ToStringMapEnum enum_to_string { get; set; }
            [Column(MapType = typeof(string))]
            public ToStringMapEnum? enumnullable_to_string { get; set; }

            [Column(MapType = typeof(string))]
            public BigInteger biginteger_to_string { get; set; }
            [Column(MapType = typeof(string))]
            public BigInteger? bigintegernullable_to_string { get; set; }
        }
        public enum ToStringMapEnum { 中国人, abc, 香港 }
        [Fact]
        public void Enum1()
        {
            //insert
            var orm = g.mysql;
            var item = new ToStringMap { };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.中国人).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.中国人, find.enum_to_string);

            item = new ToStringMap { enum_to_string = ToStringMapEnum.abc };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.abc, find.enum_to_string);

            //update all
            item.enum_to_string = ToStringMapEnum.香港;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.香港).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.香港, find.enum_to_string);

            item.enum_to_string = ToStringMapEnum.中国人;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.中国人).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.中国人, find.enum_to_string);

            //update set
            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.enum_to_string, ToStringMapEnum.香港).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.香港).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.香港, find.enum_to_string);

            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.enum_to_string, ToStringMapEnum.abc).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.abc, find.enum_to_string);

            //delete
            Assert.Equal(0, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.中国人).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.香港).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.abc).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void EnumNullable()
        {
            //insert
            var orm = g.mysql;
            var item = new ToStringMap { };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Null(find.enumnullable_to_string);

            item = new ToStringMap { enumnullable_to_string = ToStringMapEnum.中国人 };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.中国人).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Equal(ToStringMapEnum.中国人, find.enumnullable_to_string);

            //update all
            item.enumnullable_to_string = ToStringMapEnum.香港;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.香港).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Equal(ToStringMapEnum.香港, find.enumnullable_to_string);

            item.enumnullable_to_string = null;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.香港).First());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Null(find.enumnullable_to_string);

            //update set
            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.enumnullable_to_string, ToStringMapEnum.abc).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.abc, find.enumnullable_to_string);


            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.enumnullable_to_string, null).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.abc).First());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.enumnullable_to_string);

            //delete
            Assert.Equal(0, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.中国人).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.香港).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void BigInteger1()
        {
            //insert
            var orm = g.mysql;
            var item = new ToStringMap { };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.biginteger_to_string == 0).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.biginteger_to_string, find.biginteger_to_string);
            Assert.Equal(0, find.biginteger_to_string);

            item = new ToStringMap { biginteger_to_string = 100 };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.biginteger_to_string == 100).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.biginteger_to_string, find.biginteger_to_string);
            Assert.Equal(100, find.biginteger_to_string);

            //update all
            item.biginteger_to_string = 200;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.biginteger_to_string == 200).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.biginteger_to_string, find.biginteger_to_string);
            Assert.Equal(200, find.biginteger_to_string);

            item.biginteger_to_string = 205;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.biginteger_to_string == 205).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.biginteger_to_string, find.biginteger_to_string);
            Assert.Equal(205, find.biginteger_to_string);

            //update set
            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.biginteger_to_string, 522).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.biginteger_to_string == 522).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(522, find.biginteger_to_string);

            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.biginteger_to_string, 10005).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.biginteger_to_string == 10005).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(10005, find.biginteger_to_string);

            //delete
            Assert.Equal(0, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.biginteger_to_string == 522).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.biginteger_to_string == 205).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.biginteger_to_string == 10005).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void BigIntegerNullable()
        {
            //insert
            var orm = g.mysql;
            var item = new ToStringMap { };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.bigintegernullable_to_string, find.bigintegernullable_to_string);
            Assert.Null(find.bigintegernullable_to_string);

            item = new ToStringMap { bigintegernullable_to_string = 101 };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == 101).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.bigintegernullable_to_string, find.bigintegernullable_to_string);
            Assert.Equal(101, find.bigintegernullable_to_string);

            //update all
            item.bigintegernullable_to_string = 2004;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == 2004).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.bigintegernullable_to_string, find.bigintegernullable_to_string);
            Assert.Equal(2004, find.bigintegernullable_to_string);

            item.bigintegernullable_to_string = null;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == 2004).First());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.bigintegernullable_to_string, find.bigintegernullable_to_string);
            Assert.Null(find.bigintegernullable_to_string);

            //update set
            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.bigintegernullable_to_string, 998).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == 998).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(998, find.bigintegernullable_to_string);


            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.bigintegernullable_to_string, null).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == 998).First());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.bigintegernullable_to_string);

            //delete
            Assert.Equal(0, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == 998).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == 2004).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.bigintegernullable_to_string == null).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void TimeSpan1()
        {
            //insert
            var orm = g.mysql;
            var item = new ToStringMap { };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.timespan_to_string, find.timespan_to_string);
            Assert.Equal(TimeSpan.Zero, find.timespan_to_string);

            item = new ToStringMap { timespan_to_string = TimeSpan.FromDays(1) };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.timespan_to_string, find.timespan_to_string);
            Assert.Equal(TimeSpan.FromDays(1), find.timespan_to_string);

            //update all
            item.timespan_to_string = TimeSpan.FromHours(10);
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.timespan_to_string, find.timespan_to_string);
            Assert.Equal(TimeSpan.FromHours(10), find.timespan_to_string);

            //update set
            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.timespan_to_string, TimeSpan.FromHours(11)).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(TimeSpan.FromHours(11), find.timespan_to_string);

            //delete
            Assert.Equal(1, orm.Delete<ToStringMap>().Where(a => a.id == item.id).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void TimeSpanNullable()
        {
            //insert
            var orm = g.mysql;
            var item = new ToStringMap { };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.timespannullable_to_string, find.timespannullable_to_string);
            Assert.Null(find.timespannullable_to_string);

            item = new ToStringMap { timespannullable_to_string = TimeSpan.FromDays(1) };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.timespannullable_to_string, find.timespannullable_to_string);
            Assert.Equal(TimeSpan.FromDays(1), find.timespannullable_to_string);

            //update all
            item.timespannullable_to_string = TimeSpan.FromHours(10);
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.timespannullable_to_string, find.timespannullable_to_string);
            Assert.Equal(TimeSpan.FromHours(10), find.timespannullable_to_string);

            item.timespannullable_to_string = null;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.timespannullable_to_string, find.timespannullable_to_string);
            Assert.Null(find.timespannullable_to_string);

            //update set
            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.timespannullable_to_string, TimeSpan.FromHours(11)).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(TimeSpan.FromHours(11), find.timespannullable_to_string);

            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.timespannullable_to_string, null).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.timespannullable_to_string);

            //delete
            Assert.Equal(1, orm.Delete<ToStringMap>().Where(a => a.id == item.id).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void DateTime1()
        {
            //insert
            var orm = g.mysql;
            var item = new ToStringMap { };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.datetime_to_string, find.datetime_to_string);
            Assert.Equal(DateTime.MinValue, find.datetime_to_string);

            item = new ToStringMap { datetime_to_string = DateTime.Parse("2000-1-1") };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.datetime_to_string, find.datetime_to_string);
            Assert.Equal(DateTime.Parse("2000-1-1"), find.datetime_to_string);

            //update all
            item.datetime_to_string = DateTime.Parse("2000-1-11");
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.datetime_to_string, find.datetime_to_string);
            Assert.Equal(DateTime.Parse("2000-1-11"), find.datetime_to_string);

            //update set
            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.datetime_to_string, DateTime.Parse("2000-1-12")).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(DateTime.Parse("2000-1-12"), find.datetime_to_string);

            //delete
            Assert.Equal(1, orm.Delete<ToStringMap>().Where(a => a.id == item.id).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void DateTimeNullable()
        {
            //insert
            var orm = g.mysql;
            var item = new ToStringMap { };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.datetimenullable_to_string, find.datetimenullable_to_string);
            Assert.Null(find.datetimenullable_to_string);

            item = new ToStringMap { datetimenullable_to_string = DateTime.Parse("2000-1-1") };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.datetimenullable_to_string, find.datetimenullable_to_string);
            Assert.Equal(DateTime.Parse("2000-1-1"), find.datetimenullable_to_string);

            //update all
            item.datetimenullable_to_string = DateTime.Parse("2000-1-11");
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.datetimenullable_to_string, find.datetimenullable_to_string);
            Assert.Equal(DateTime.Parse("2000-1-11"), find.datetimenullable_to_string);

            item.datetimenullable_to_string = null;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.datetimenullable_to_string, find.datetimenullable_to_string);
            Assert.Null(find.datetimenullable_to_string);

            //update set
            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.datetimenullable_to_string, DateTime.Parse("2000-1-12")).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(DateTime.Parse("2000-1-12"), find.datetimenullable_to_string);

            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.datetimenullable_to_string, null).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.datetimenullable_to_string);

            //delete
            Assert.Equal(1, orm.Delete<ToStringMap>().Where(a => a.id == item.id).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id).First());
        }

        [Fact]
        public void Guid1()
        {
            //insert
            var orm = g.mysql;
            var item = new ToStringMap { };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.guid_to_string == Guid.Empty).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.guid_to_string, find.guid_to_string);
            Assert.Equal(Guid.Empty, find.guid_to_string);

            var newid = Guid.NewGuid();
            item = new ToStringMap { guid_to_string = newid };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.guid_to_string == newid).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.guid_to_string, find.guid_to_string);
            Assert.Equal(newid, find.guid_to_string);

            //update all
            newid = Guid.NewGuid();
            item.guid_to_string = newid;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.guid_to_string == newid).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.guid_to_string, find.guid_to_string);
            Assert.Equal(newid, find.guid_to_string);

            //update set
            newid = Guid.NewGuid();
            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.guid_to_string, newid).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.guid_to_string == newid).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(newid, find.guid_to_string);

            //delete
            Assert.Equal(1, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.guid_to_string == newid).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void GuidNullable()
        {
            //insert
            var orm = g.mysql;
            var item = new ToStringMap { };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.guidnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.guidnullable_to_string, find.guidnullable_to_string);
            Assert.Null(find.guidnullable_to_string);

            var newid = Guid.NewGuid();
            item = new ToStringMap { guidnullable_to_string = newid };
            Assert.Equal(1, orm.Insert<ToStringMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.guidnullable_to_string == newid).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.guidnullable_to_string, find.guidnullable_to_string);
            Assert.Equal(newid, find.guidnullable_to_string);

            //update all
            newid = Guid.NewGuid();
            item.guidnullable_to_string = newid;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.guidnullable_to_string == newid).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.guidnullable_to_string, find.guidnullable_to_string);
            Assert.Equal(newid, find.guidnullable_to_string);

            item.guidnullable_to_string = null;
            Assert.Equal(1, orm.Update<ToStringMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.guidnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.guidnullable_to_string, find.guidnullable_to_string);
            Assert.Null(find.guidnullable_to_string);

            //update set
            newid = Guid.NewGuid();
            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.guidnullable_to_string, newid).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.guidnullable_to_string == newid).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(newid, find.guidnullable_to_string);

            Assert.Equal(1, orm.Update<ToStringMap>().Where(a => a.id == item.id).Set(a => a.guidnullable_to_string, null).ExecuteAffrows());
            find = orm.Select<ToStringMap>().Where(a => a.id == item.id && a.guidnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.guidnullable_to_string);

            //delete
            Assert.Equal(1, orm.Delete<ToStringMap>().Where(a => a.id == item.id && a.guidnullable_to_string == null).ExecuteAffrows());
            Assert.Null(orm.Select<ToStringMap>().Where(a => a.id == item.id).First());
        }
    }
}
