using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.SqliteMapType
{
    public class BoolTest
    {

        class BoolMap
        {
            public Guid id { get; set; }
            [Column(MapType = typeof(bool?))]
            public bool toboolnullable { get; set; } = true;

            [Column(MapType = typeof(sbyte))]
            public bool tosbyte { get; set; } = true;
            [Column(MapType = typeof(sbyte?))]
            public bool tosbytenullable { get; set; } = true;

            [Column(MapType = typeof(short))]
            public bool toshort { get; set; } = true;

            [Column(MapType = typeof(short?))]
            public bool toshortnullable { get; set; } = true;

            [Column(MapType = typeof(int))]
            public bool toint { get; set; } = true;

            [Column(MapType = typeof(int?))]
            public bool tointnullable { get; set; } = true;

            [Column(MapType = typeof(long))]
            public bool tolong { get; set; } = true;
            [Column(MapType = typeof(long?))]
            public bool tolongnullable { get; set; } = true;

            [Column(MapType = typeof(byte))]
            public bool tobyte { get; set; } = true;
            [Column(MapType = typeof(byte?))]
            public bool tobytenullable { get; set; } = true;

            [Column(MapType = typeof(ushort))]
            public bool toushort { get; set; } = true;

            [Column(MapType = typeof(ushort?))]
            public bool toushortnullable { get; set; } = true;

            [Column(MapType = typeof(uint))]
            public bool touint { get; set; } = true;

            [Column(MapType = typeof(uint?))]
            public bool touintnullable { get; set; } = true;

            [Column(MapType = typeof(ulong))]
            public bool toulong { get; set; } = true;
            [Column(MapType = typeof(ulong?))]
            public bool toulongnullable { get; set; } = true;

            [Column(MapType = typeof(string))]
            public bool tostring { get; set; } = true;
        }

        [Fact]
        public void BoolNullable()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toboolnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toboolnullable, find.toboolnullable);
            Assert.True(find.toboolnullable);

            item = new BoolMap { toboolnullable = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toboolnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toboolnullable, find.toboolnullable);
            Assert.False(find.toboolnullable);

            //update all
            item.toboolnullable = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toboolnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toboolnullable, find.toboolnullable);
            Assert.True(find.toboolnullable);

            item.toboolnullable = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toboolnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toboolnullable, find.toboolnullable);
            Assert.False(find.toboolnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toboolnullable, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toboolnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.toboolnullable);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toboolnullable, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toboolnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.toboolnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toboolnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toboolnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void SByte()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbyte, find.tosbyte);
            Assert.True(find.tosbyte);

            item = new BoolMap { tosbyte = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbyte, find.tosbyte);
            Assert.False(find.tosbyte);

            //update all
            item.tosbyte = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbyte, find.tosbyte);
            Assert.True(find.tosbyte);

            item.tosbyte = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbyte, find.tosbyte);
            Assert.False(find.tosbyte);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tosbyte, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.tosbyte);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tosbyte, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.tosbyte);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tosbyte == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tosbyte == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void SByteNullable()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbytenullable, find.tosbytenullable);
            Assert.True(find.tosbytenullable);

            item = new BoolMap { tosbytenullable = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbytenullable, find.tosbytenullable);
            Assert.False(find.tosbytenullable);

            //update all
            item.tosbytenullable = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbytenullable, find.tosbytenullable);
            Assert.True(find.tosbytenullable);

            item.tosbytenullable = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbytenullable, find.tosbytenullable);
            Assert.False(find.tosbytenullable);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tosbytenullable, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.tosbytenullable);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tosbytenullable, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tosbytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.tosbytenullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tosbytenullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tosbytenullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void Short()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshort, find.toshort);
            Assert.True(find.toshort);

            item = new BoolMap { toshort = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshort, find.toshort);
            Assert.False(find.toshort);

            //update all
            item.toshort = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshort, find.toshort);
            Assert.True(find.toshort);

            item.toshort = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshort, find.toshort);
            Assert.False(find.toshort);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toshort, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.toshort);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toshort, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.toshort);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toshort == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toshort == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void ShortNullable()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshortnullable, find.toshortnullable);
            Assert.True(find.toshortnullable);

            item = new BoolMap { toshortnullable = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshortnullable, find.toshortnullable);
            Assert.False(find.toshortnullable);

            //update all
            item.toshortnullable = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshortnullable, find.toshortnullable);
            Assert.True(find.toshortnullable);

            item.toshortnullable = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshortnullable, find.toshortnullable);
            Assert.False(find.toshortnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toshortnullable, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.toshortnullable);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toshortnullable, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toshortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.toshortnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toshortnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toshortnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void Int()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toint, find.toint);
            Assert.True(find.toint);

            item = new BoolMap { toint = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toint, find.toint);
            Assert.False(find.toint);

            //update all
            item.toint = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toint, find.toint);
            Assert.True(find.toint);

            item.toint = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toint, find.toint);
            Assert.False(find.toint);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toint, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.toint);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toint, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.toint);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toint == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toint == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void IntNullable()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tointnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tointnullable, find.tointnullable);
            Assert.True(find.tointnullable);

            item = new BoolMap { tointnullable = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tointnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tointnullable, find.tointnullable);
            Assert.False(find.tointnullable);

            //update all
            item.tointnullable = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tointnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tointnullable, find.tointnullable);
            Assert.True(find.tointnullable);

            item.tointnullable = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tointnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tointnullable, find.tointnullable);
            Assert.False(find.tointnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tointnullable, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tointnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.tointnullable);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tointnullable, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tointnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.tointnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tointnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tointnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void Long()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolong, find.tolong);
            Assert.True(find.tolong);

            item = new BoolMap { tolong = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolong, find.tolong);
            Assert.False(find.tolong);

            //update all
            item.tolong = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolong, find.tolong);
            Assert.True(find.tolong);

            item.tolong = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolong, find.tolong);
            Assert.False(find.tolong);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tolong, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.tolong);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tolong, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.tolong);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tolong == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tolong == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void LongNullable()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolongnullable, find.tolongnullable);
            Assert.True(find.tolongnullable);

            item = new BoolMap { tolongnullable = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolongnullable, find.tolongnullable);
            Assert.False(find.tolongnullable);

            //update all
            item.tolongnullable = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolongnullable, find.tolongnullable);
            Assert.True(find.tolongnullable);

            item.tolongnullable = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolongnullable, find.tolongnullable);
            Assert.False(find.tolongnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tolongnullable, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.tolongnullable);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tolongnullable, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tolongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.tolongnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tolongnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tolongnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }

        [Fact]
        public void Byte()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobyte, find.tobyte);
            Assert.True(find.tobyte);

            item = new BoolMap { tobyte = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobyte, find.tobyte);
            Assert.False(find.tobyte);

            //update all
            item.tobyte = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobyte, find.tobyte);
            Assert.True(find.tobyte);

            item.tobyte = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobyte, find.tobyte);
            Assert.False(find.tobyte);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tobyte, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.tobyte);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tobyte, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.tobyte);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tobyte == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tobyte == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void ByteNullable()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobytenullable, find.tobytenullable);
            Assert.True(find.tobytenullable);

            item = new BoolMap { tobytenullable = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobytenullable, find.tobytenullable);
            Assert.False(find.tobytenullable);

            //update all
            item.tobytenullable = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobytenullable, find.tobytenullable);
            Assert.True(find.tobytenullable);

            item.tobytenullable = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobytenullable, find.tobytenullable);
            Assert.False(find.tobytenullable);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tobytenullable, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.tobytenullable);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tobytenullable, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tobytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.tobytenullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tobytenullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tobytenullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void UShort()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushort, find.toushort);
            Assert.True(find.toushort);

            item = new BoolMap { toushort = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushort, find.toushort);
            Assert.False(find.toushort);

            //update all
            item.toushort = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushort, find.toushort);
            Assert.True(find.toushort);

            item.toushort = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushort, find.toushort);
            Assert.False(find.toushort);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toushort, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.toushort);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toushort, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.toushort);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toushort == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toushort == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void UShortNullable()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushortnullable, find.toushortnullable);
            Assert.True(find.toushortnullable);

            item = new BoolMap { toushortnullable = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushortnullable, find.toushortnullable);
            Assert.False(find.toushortnullable);

            //update all
            item.toushortnullable = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushortnullable, find.toushortnullable);
            Assert.True(find.toushortnullable);

            item.toushortnullable = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushortnullable, find.toushortnullable);
            Assert.False(find.toushortnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toushortnullable, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.toushortnullable);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toushortnullable, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toushortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.toushortnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toushortnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toushortnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void UInt()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touint, find.touint);
            Assert.True(find.touint);

            item = new BoolMap { touint = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touint, find.touint);
            Assert.False(find.touint);

            //update all
            item.touint = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touint, find.touint);
            Assert.True(find.touint);

            item.touint = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touint, find.touint);
            Assert.False(find.touint);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.touint, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.touint);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.touint, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.touint);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.touint == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.touint == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void UIntNullable()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touintnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touintnullable, find.touintnullable);
            Assert.True(find.touintnullable);

            item = new BoolMap { touintnullable = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touintnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touintnullable, find.touintnullable);
            Assert.False(find.touintnullable);

            //update all
            item.touintnullable = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touintnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touintnullable, find.touintnullable);
            Assert.True(find.touintnullable);

            item.touintnullable = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touintnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touintnullable, find.touintnullable);
            Assert.False(find.touintnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.touintnullable, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touintnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.touintnullable);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.touintnullable, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.touintnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.touintnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.touintnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.touintnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void ULong()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulong, find.toulong);
            Assert.True(find.toulong);

            item = new BoolMap { toulong = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulong, find.toulong);
            Assert.False(find.toulong);

            //update all
            item.toulong = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulong, find.toulong);
            Assert.True(find.toulong);

            item.toulong = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulong, find.toulong);
            Assert.False(find.toulong);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toulong, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.toulong);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toulong, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.toulong);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toulong == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toulong == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void ULongNullable()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulongnullable, find.toulongnullable);
            Assert.True(find.toulongnullable);

            item = new BoolMap { toulongnullable = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulongnullable, find.toulongnullable);
            Assert.False(find.toulongnullable);

            //update all
            item.toulongnullable = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulongnullable, find.toulongnullable);
            Assert.True(find.toulongnullable);

            item.toulongnullable = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulongnullable, find.toulongnullable);
            Assert.False(find.toulongnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toulongnullable, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.toulongnullable);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.toulongnullable, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.toulongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.toulongnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toulongnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.toulongnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }

        [Fact]
        public void TimeSpan()
        {
        }
        [Fact]
        public void TimeSpanNullable()
        {
        }
        [Fact]
        public void DateTime()
        {
        }
        [Fact]
        public void DateTimeNullable()
        {
        }

        [Fact]
        public void ByteArray()
        {
        }
        [Fact]
        public void String()
        {
            //insert
            var orm = g.sqlite;
            var item = new BoolMap { };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tostring == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tostring, find.tostring);
            Assert.True(find.tostring);

            item = new BoolMap { tostring = false };
            Assert.Equal(1, orm.Insert<BoolMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tostring == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tostring, find.tostring);
            Assert.False(find.tostring);

            //update all
            item.tostring = true;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tostring == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tostring, find.tostring);
            Assert.True(find.tostring);

            item.tostring = false;
            Assert.Equal(1, orm.Update<BoolMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tostring == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tostring, find.tostring);
            Assert.False(find.tostring);

            //update set
            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tostring, true).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tostring == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.True(find.tostring);

            Assert.Equal(1, orm.Update<BoolMap>().Where(a => a.id == item.id).Set(a => a.tostring, false).ExecuteAffrows());
            find = orm.Select<BoolMap>().Where(a => a.id == item.id && a.tostring == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.False(find.tostring);

            //delete
            Assert.Equal(0, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tostring == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolMap>().Where(a => a.id == item.id && a.tostring == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolMap>().Where(a => a.id == item.id).First());
        }

        [Fact]
        public void Guid()
        {
        }
        [Fact]
        public void GuidNullable()
        {
        }

        [Fact]
        public void MygisPoint()
        {
        }
        [Fact]
        public void MygisLineString()
        {
        }
        [Fact]
        public void MygisPolygon()
        {
        }
        [Fact]
        public void MygisMultiPoint()
        {
        }
        [Fact]
        public void MygisMultiLineString()
        {
        }
        [Fact]
        public void MygisMultiPolygon()
        {
        }
    }
}
