using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.PostgreSQLMapType
{
    public class BoolNullableTest
    {
        class BoolNullableMap
        {
            public Guid id { get; set; }
            [Column(MapType = typeof(bool))]
            public bool? tobool { get; set; } = true;

            [Column(MapType = typeof(sbyte))]
            public bool? tosbyte { get; set; } = true;
            [Column(MapType = typeof(sbyte?))]
            public bool? tosbytenullable { get; set; } = true;

            [Column(MapType = typeof(short))]
            public bool? toshort { get; set; } = true;

            [Column(MapType = typeof(short?))]
            public bool? toshortnullable { get; set; } = true;

            [Column(MapType = typeof(int))]
            public bool? toint { get; set; } = true;

            [Column(MapType = typeof(int?))]
            public bool? tointnullable { get; set; } = true;

            [Column(MapType = typeof(long))]
            public bool? tolong { get; set; } = true;
            [Column(MapType = typeof(long?))]
            public bool? tolongnullable { get; set; } = true;

            [Column(MapType = typeof(byte))]
            public bool? tobyte { get; set; } = true;
            [Column(MapType = typeof(byte?))]
            public bool? tobytenullable { get; set; } = true;

            [Column(MapType = typeof(ushort))]
            public bool? toushort { get; set; } = true;

            [Column(MapType = typeof(ushort?))]
            public bool? toushortnullable { get; set; } = true;

            [Column(MapType = typeof(uint))]
            public bool? touint { get; set; } = true;

            [Column(MapType = typeof(uint?))]
            public bool? touintnullable { get; set; } = true;

            [Column(MapType = typeof(ulong))]
            public bool? toulong { get; set; } = true;
            [Column(MapType = typeof(ulong?))]
            public bool? toulongnullable { get; set; } = true;

            [Column(MapType = typeof(string))]
            public bool? tostring { get; set; } = true;
        }
        [Fact]
        public void Bool()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobool, find.tobool);
            Assert.Equal(true, find.tobool);

            item = new BoolNullableMap { tobool = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobool, find.tobool);
            Assert.Equal(false, find.tobool);

            item = new BoolNullableMap { tobool = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.tobool, find.tobool);
            Assert.Equal(false, find.tobool);

            //update all
            item.tobool = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobool, find.tobool);
            Assert.Equal(true, find.tobool);

            item.tobool = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobool, find.tobool);
            Assert.Equal(false, find.tobool);

            item.tobool = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.tobool, find.tobool);
            Assert.Equal(false, find.tobool);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tobool, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.tobool);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tobool, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tobool);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tobool, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tobool);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == true).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == null).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tobool == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void SByte()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbyte, find.tosbyte);
            Assert.Equal(true, find.tosbyte);

            item = new BoolNullableMap { tosbyte = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbyte, find.tosbyte);
            Assert.Equal(false, find.tosbyte);

            item = new BoolNullableMap { tosbyte = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.tosbyte, find.tosbyte);
            Assert.Equal(false, find.tosbyte);

            //update all
            item.tosbyte = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbyte, find.tosbyte);
            Assert.Equal(true, find.tosbyte);

            item.tosbyte = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbyte, find.tosbyte);
            Assert.Equal(false, find.tosbyte);

            item.tosbyte = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.tosbyte, find.tosbyte);
            Assert.Equal(false, find.tosbyte);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tosbyte, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.tosbyte);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tosbyte, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tosbyte);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tosbyte, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tosbyte);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == true).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == null).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tosbyte == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void SByteNullable()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbytenullable, find.tosbytenullable);
            Assert.Equal(true, find.tosbytenullable);

            item = new BoolNullableMap { tosbytenullable = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbytenullable, find.tosbytenullable);
            Assert.Equal(false, find.tosbytenullable);

            item = new BoolNullableMap { tosbytenullable = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbytenullable, find.tosbytenullable);
            Assert.Null(find.tosbytenullable);

            //update all
            item.tosbytenullable = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbytenullable, find.tosbytenullable);
            Assert.Equal(true, find.tosbytenullable);

            item.tosbytenullable = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbytenullable, find.tosbytenullable);
            Assert.Equal(false, find.tosbytenullable);

            item.tosbytenullable = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tosbytenullable, find.tosbytenullable);
            Assert.Null(find.tosbytenullable);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tosbytenullable, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.tosbytenullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tosbytenullable, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tosbytenullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tosbytenullable, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.tosbytenullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == null).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tosbytenullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void Short()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshort, find.toshort);
            Assert.Equal(true, find.toshort);

            item = new BoolNullableMap { toshort = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshort, find.toshort);
            Assert.Equal(false, find.toshort);

            item = new BoolNullableMap { toshort = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.toshort, find.toshort);
            Assert.Equal(false, find.toshort);

            //update all
            item.toshort = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshort, find.toshort);
            Assert.Equal(true, find.toshort);

            item.toshort = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshort, find.toshort);
            Assert.Equal(false, find.toshort);

            item.toshort = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.toshort, find.toshort);
            Assert.Equal(false, find.toshort);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toshort, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.toshort);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toshort, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toshort);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toshort, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toshort);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == true).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == null).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toshort == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void ShortNullable()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshortnullable, find.toshortnullable);
            Assert.Equal(true, find.toshortnullable);

            item = new BoolNullableMap { toshortnullable = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshortnullable, find.toshortnullable);
            Assert.Equal(false, find.toshortnullable);

            item = new BoolNullableMap { toshortnullable = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshortnullable, find.toshortnullable);
            Assert.Null(find.toshortnullable);

            //update all
            item.toshortnullable = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshortnullable, find.toshortnullable);
            Assert.Equal(true, find.toshortnullable);

            item.toshortnullable = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshortnullable, find.toshortnullable);
            Assert.Equal(false, find.toshortnullable);

            item.toshortnullable = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toshortnullable, find.toshortnullable);
            Assert.Null(find.toshortnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toshortnullable, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.toshortnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toshortnullable, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toshortnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toshortnullable, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.toshortnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == null).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toshortnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void Int()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toint, find.toint);
            Assert.Equal(true, find.toint);

            item = new BoolNullableMap { toint = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toint, find.toint);
            Assert.Equal(false, find.toint);

            item = new BoolNullableMap { toint = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.toint, find.toint);
            Assert.Equal(false, find.toint);

            //update all
            item.toint = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toint, find.toint);
            Assert.Equal(true, find.toint);

            item.toint = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toint, find.toint);
            Assert.Equal(false, find.toint);

            item.toint = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.toint, find.toint);
            Assert.Equal(false, find.toint);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toint, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.toint);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toint, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toint);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toint, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toint);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toint == true).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toint == null).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toint == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void IntNullable()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tointnullable, find.tointnullable);
            Assert.Equal(true, find.tointnullable);

            item = new BoolNullableMap { tointnullable = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tointnullable, find.tointnullable);
            Assert.Equal(false, find.tointnullable);

            item = new BoolNullableMap { tointnullable = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tointnullable, find.tointnullable);
            Assert.Null(find.tointnullable);

            //update all
            item.tointnullable = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tointnullable, find.tointnullable);
            Assert.Equal(true, find.tointnullable);

            item.tointnullable = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tointnullable, find.tointnullable);
            Assert.Equal(false, find.tointnullable);

            item.tointnullable = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tointnullable, find.tointnullable);
            Assert.Null(find.tointnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tointnullable, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.tointnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tointnullable, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tointnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tointnullable, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.tointnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == null).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tointnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void Long()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolong, find.tolong);
            Assert.Equal(true, find.tolong);

            item = new BoolNullableMap { tolong = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolong, find.tolong);
            Assert.Equal(false, find.tolong);

            item = new BoolNullableMap { tolong = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.tolong, find.tolong);
            Assert.Equal(false, find.tolong);

            //update all
            item.tolong = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolong, find.tolong);
            Assert.Equal(true, find.tolong);

            item.tolong = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolong, find.tolong);
            Assert.Equal(false, find.tolong);

            item.tolong = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.tolong, find.tolong);
            Assert.Equal(false, find.tolong);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tolong, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.tolong);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tolong, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tolong);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tolong, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tolong);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == true).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == null).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tolong == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void LongNullable()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolongnullable, find.tolongnullable);
            Assert.Equal(true, find.tolongnullable);

            item = new BoolNullableMap { tolongnullable = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolongnullable, find.tolongnullable);
            Assert.Equal(false, find.tolongnullable);

            item = new BoolNullableMap { tolongnullable = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolongnullable, find.tolongnullable);
            Assert.Null(find.tolongnullable);

            //update all
            item.tolongnullable = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolongnullable, find.tolongnullable);
            Assert.Equal(true, find.tolongnullable);

            item.tolongnullable = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolongnullable, find.tolongnullable);
            Assert.Equal(false, find.tolongnullable);

            item.tolongnullable = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tolongnullable, find.tolongnullable);
            Assert.Null(find.tolongnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tolongnullable, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.tolongnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tolongnullable, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tolongnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tolongnullable, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.tolongnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == null).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tolongnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }

        [Fact]
        public void Byte()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobyte, find.tobyte);
            Assert.Equal(true, find.tobyte);

            item = new BoolNullableMap { tobyte = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobyte, find.tobyte);
            Assert.Equal(false, find.tobyte);

            item = new BoolNullableMap { tobyte = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.tobyte, find.tobyte);
            Assert.Equal(false, find.tobyte);

            //update all
            item.tobyte = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobyte, find.tobyte);
            Assert.Equal(true, find.tobyte);

            item.tobyte = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobyte, find.tobyte);
            Assert.Equal(false, find.tobyte);

            item.tobyte = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.tobyte, find.tobyte);
            Assert.Equal(false, find.tobyte);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tobyte, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.tobyte);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tobyte, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tobyte);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tobyte, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tobyte);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == true).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == null).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tobyte == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void ByteNullable()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobytenullable, find.tobytenullable);
            Assert.Equal(true, find.tobytenullable);

            item = new BoolNullableMap { tobytenullable = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobytenullable, find.tobytenullable);
            Assert.Equal(false, find.tobytenullable);

            item = new BoolNullableMap { tobytenullable = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobytenullable, find.tobytenullable);
            Assert.Null(find.tobytenullable);

            //update all
            item.tobytenullable = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobytenullable, find.tobytenullable);
            Assert.Equal(true, find.tobytenullable);

            item.tobytenullable = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobytenullable, find.tobytenullable);
            Assert.Equal(false, find.tobytenullable);

            item.tobytenullable = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tobytenullable, find.tobytenullable);
            Assert.Null(find.tobytenullable);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tobytenullable, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.tobytenullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tobytenullable, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tobytenullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tobytenullable, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.tobytenullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == null).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tobytenullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void UShort()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushort, find.toushort);
            Assert.Equal(true, find.toushort);

            item = new BoolNullableMap { toushort = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushort, find.toushort);
            Assert.Equal(false, find.toushort);

            item = new BoolNullableMap { toushort = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.toushort, find.toushort);
            Assert.Equal(false, find.toushort);

            //update all
            item.toushort = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushort, find.toushort);
            Assert.Equal(true, find.toushort);

            item.toushort = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushort, find.toushort);
            Assert.Equal(false, find.toushort);

            item.toushort = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.toushort, find.toushort);
            Assert.Equal(false, find.toushort);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toushort, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.toushort);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toushort, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toushort);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toushort, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toushort);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == true).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == null).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toushort == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void UShortNullable()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushortnullable, find.toushortnullable);
            Assert.Equal(true, find.toushortnullable);

            item = new BoolNullableMap { toushortnullable = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushortnullable, find.toushortnullable);
            Assert.Equal(false, find.toushortnullable);

            item = new BoolNullableMap { toushortnullable = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushortnullable, find.toushortnullable);
            Assert.Null(find.toushortnullable);

            //update all
            item.toushortnullable = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushortnullable, find.toushortnullable);
            Assert.Equal(true, find.toushortnullable);

            item.toushortnullable = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushortnullable, find.toushortnullable);
            Assert.Equal(false, find.toushortnullable);

            item.toushortnullable = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toushortnullable, find.toushortnullable);
            Assert.Null(find.toushortnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toushortnullable, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.toushortnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toushortnullable, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toushortnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toushortnullable, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.toushortnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == null).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toushortnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void UInt()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touint, find.touint);
            Assert.Equal(true, find.touint);

            item = new BoolNullableMap { touint = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touint, find.touint);
            Assert.Equal(false, find.touint);

            item = new BoolNullableMap { touint = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.touint, find.touint);
            Assert.Equal(false, find.touint);

            //update all
            item.touint = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touint, find.touint);
            Assert.Equal(true, find.touint);

            item.touint = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touint, find.touint);
            Assert.Equal(false, find.touint);

            item.touint = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.touint, find.touint);
            Assert.Equal(false, find.touint);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.touint, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.touint);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.touint, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.touint);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.touint, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touint == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.touint);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.touint == true).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.touint == null).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.touint == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void UIntNullable()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touintnullable, find.touintnullable);
            Assert.Equal(true, find.touintnullable);

            item = new BoolNullableMap { touintnullable = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touintnullable, find.touintnullable);
            Assert.Equal(false, find.touintnullable);

            item = new BoolNullableMap { touintnullable = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touintnullable, find.touintnullable);
            Assert.Null(find.touintnullable);

            //update all
            item.touintnullable = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touintnullable, find.touintnullable);
            Assert.Equal(true, find.touintnullable);

            item.touintnullable = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touintnullable, find.touintnullable);
            Assert.Equal(false, find.touintnullable);

            item.touintnullable = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.touintnullable, find.touintnullable);
            Assert.Null(find.touintnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.touintnullable, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.touintnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.touintnullable, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.touintnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.touintnullable, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.touintnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == null).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.touintnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void ULong()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulong, find.toulong);
            Assert.Equal(true, find.toulong);

            item = new BoolNullableMap { toulong = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulong, find.toulong);
            Assert.Equal(false, find.toulong);

            item = new BoolNullableMap { toulong = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.toulong, find.toulong);
            Assert.Equal(false, find.toulong);

            //update all
            item.toulong = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulong, find.toulong);
            Assert.Equal(true, find.toulong);

            item.toulong = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulong, find.toulong);
            Assert.Equal(false, find.toulong);

            item.toulong = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.NotEqual(item.toulong, find.toulong);
            Assert.Equal(false, find.toulong);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toulong, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.toulong);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toulong, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toulong);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toulong, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == null).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toulong);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == true).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == null).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toulong == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void ULongNullable()
        {
            //insert
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulongnullable, find.toulongnullable);
            Assert.Equal(true, find.toulongnullable);

            item = new BoolNullableMap { toulongnullable = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulongnullable, find.toulongnullable);
            Assert.Equal(false, find.toulongnullable);

            item = new BoolNullableMap { toulongnullable = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulongnullable, find.toulongnullable);
            Assert.Null(find.toulongnullable);

            //update all
            item.toulongnullable = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulongnullable, find.toulongnullable);
            Assert.Equal(true, find.toulongnullable);

            item.toulongnullable = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulongnullable, find.toulongnullable);
            Assert.Equal(false, find.toulongnullable);

            item.toulongnullable = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.toulongnullable, find.toulongnullable);
            Assert.Null(find.toulongnullable);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toulongnullable, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.toulongnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toulongnullable, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.toulongnullable);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.toulongnullable, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.toulongnullable);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == true).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == null).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.toulongnullable == false).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
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
            var orm = g.pgsql;
            var item = new BoolNullableMap { };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tostring, find.tostring);
            Assert.Equal(true, find.tostring);

            item = new BoolNullableMap { tostring = false };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tostring, find.tostring);
            Assert.Equal(false, find.tostring);

            item = new BoolNullableMap { tostring = null };
            Assert.Equal(1, orm.Insert<BoolNullableMap>().AppendData(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tostring, find.tostring);
            Assert.Null(find.tostring);

            //update all
            item.tostring = true;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tostring, find.tostring);
            Assert.Equal(true, find.tostring);

            item.tostring = false;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tostring, find.tostring);
            Assert.Equal(false, find.tostring);

            item.tostring = null;
            Assert.Equal(1, orm.Update<BoolNullableMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.tostring, find.tostring);
            Assert.Null(find.tostring);

            //update set
            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tostring, true).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == true).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(true, find.tostring);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tostring, false).ExecuteAffrows());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == false).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(false, find.tostring);

            Assert.Equal(1, orm.Update<BoolNullableMap>().Where(a => a.id == item.id).Set(a => a.tostring, null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == false).First());
            find = orm.Select<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.tostring);

            //delete
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == true).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == false).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<BoolNullableMap>().Where(a => a.id == item.id && a.tostring == null).ExecuteAffrows());
            Assert.Null(orm.Select<BoolNullableMap>().Where(a => a.id == item.id).First());
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
