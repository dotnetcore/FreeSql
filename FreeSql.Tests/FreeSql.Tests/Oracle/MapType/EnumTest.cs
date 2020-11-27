using FreeSql.DataAnnotations;
using System;
using System.Numerics;
using Xunit;

namespace FreeSql.Tests.OracleMapType
{
    public class EnumTest
    {
        class EnumTestMap
        {
            public Guid id { get; set; }

            [Column(MapType = typeof(string))]
            public ToStringMapEnum enum_to_string { get; set; }
            [Column(MapType = typeof(string))]
            public ToStringMapEnum? enumnullable_to_string { get; set; }

            [Column(MapType = typeof(int))]
            public ToStringMapEnum enum_to_int { get; set; }
            [Column(MapType = typeof(int?))]
            public ToStringMapEnum? enumnullable_to_int { get; set; }
        }
        public enum ToStringMapEnum { 中国人, abc, 香港 }
        [Fact]
        public void EnumToString()
        {
            //insert
            var orm = g.oracle;
            var item = new EnumTestMap { };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.中国人).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.中国人, find.enum_to_string);

            item = new EnumTestMap { enum_to_string = ToStringMapEnum.abc };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.abc, find.enum_to_string);

            //update all
            item.enum_to_string = ToStringMapEnum.香港;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.香港).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.香港, find.enum_to_string);

            item.enum_to_string = ToStringMapEnum.中国人;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.中国人).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.中国人, find.enum_to_string);

            //update set
            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enum_to_string, ToStringMapEnum.香港).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.香港).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.香港, find.enum_to_string);

            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enum_to_string, ToStringMapEnum.abc).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.abc, find.enum_to_string);

            //delete
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.中国人).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.香港).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.abc).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void EnumNullableToString()
        {
            //insert
            var orm = g.oracle;
            var item = new EnumTestMap { };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Null(find.enumnullable_to_string);

            item = new EnumTestMap { enumnullable_to_string = ToStringMapEnum.中国人 };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.中国人).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Equal(ToStringMapEnum.中国人, find.enumnullable_to_string);

            //update all
            item.enumnullable_to_string = ToStringMapEnum.香港;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.香港).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Equal(ToStringMapEnum.香港, find.enumnullable_to_string);

            item.enumnullable_to_string = null;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.香港).First());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Null(find.enumnullable_to_string);

            //update set
            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enumnullable_to_string, ToStringMapEnum.abc).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.abc, find.enumnullable_to_string);


            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enumnullable_to_string, null).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.abc).First());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.enumnullable_to_string);

            //delete
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.中国人).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.香港).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id).First());
        }

        [Fact]
        public void EnumToInt()
        {
            //insert
            var orm = g.oracle;
            var item = new EnumTestMap { };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.中国人).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_int, find.enum_to_int);
            Assert.Equal(ToStringMapEnum.中国人, find.enum_to_int);

            item = new EnumTestMap { enum_to_int = ToStringMapEnum.abc };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_int, find.enum_to_int);
            Assert.Equal(ToStringMapEnum.abc, find.enum_to_int);

            //update all
            item.enum_to_int = ToStringMapEnum.香港;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.香港).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_int, find.enum_to_int);
            Assert.Equal(ToStringMapEnum.香港, find.enum_to_int);

            item.enum_to_int = ToStringMapEnum.中国人;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.中国人).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_int, find.enum_to_int);
            Assert.Equal(ToStringMapEnum.中国人, find.enum_to_int);

            //update set
            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enum_to_int, ToStringMapEnum.香港).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.香港).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.香港, find.enum_to_int);

            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enum_to_int, ToStringMapEnum.abc).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.abc, find.enum_to_int);

            //delete
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.中国人).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.香港).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.abc).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void EnumNullableToInt()
        {
            //insert
            var orm = g.oracle;
            var item = new EnumTestMap { };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_int, find.enumnullable_to_int);
            Assert.Null(find.enumnullable_to_int);

            item = new EnumTestMap { enumnullable_to_int = ToStringMapEnum.中国人 };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.中国人).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_int, find.enumnullable_to_int);
            Assert.Equal(ToStringMapEnum.中国人, find.enumnullable_to_int);

            //update all
            item.enumnullable_to_int = ToStringMapEnum.香港;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.香港).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_int, find.enumnullable_to_int);
            Assert.Equal(ToStringMapEnum.香港, find.enumnullable_to_int);

            item.enumnullable_to_int = null;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.香港).First());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_int, find.enumnullable_to_int);
            Assert.Null(find.enumnullable_to_int);

            //update set
            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enumnullable_to_int, ToStringMapEnum.abc).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.abc, find.enumnullable_to_int);


            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enumnullable_to_int, null).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.abc).First());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.enumnullable_to_int);

            //delete
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.中国人).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.香港).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == null).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id).First());
        }
    }
}
