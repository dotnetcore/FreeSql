using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Xunit;
using static FreeSql.Tests.MySqlConnectorMapType.EnumTest;

namespace FreeSql.Tests.MySqlConnectorMapType
{
    public class EnumTest
    {
        public enum OrgType
        {
            Bank = 1,
            Brokerage,
        }
        public record Org2040(OrgType Type, string Id);
        public class Staff2040
        {
            [JsonMap]
            public Org2040 Org { get; init; }
        }
        [Fact]
        public void Issues2040()
        {
            using (var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd_mysqlconnector;Charset=utf8;SslMode=none;Max pool size=10;AllowLoadLocalInfile=true;AllowZeroDateTime=True ")
                .UseAutoSyncStructure(true)
                .Build())
            {
                fsql.UseJsonMap();
                var sql01 = fsql.Select<Staff2040>().Where(x => x.Org.Type == OrgType.Bank).ToSql();
                Assert.Equal("SELECT a.`Org` \r\nFROM `Staff2040` a \r\nWHERE (json_extract(a.`Org`,'$.Type') = 1)", sql01);
                var orgType = OrgType.Bank;
                var sql02 = fsql.Select<Staff2040>().Where(x => x.Org.Type == orgType).ToSql();
                Assert.Equal("SELECT a.`Org` \r\nFROM `Staff2040` a \r\nWHERE (json_extract(a.`Org`,'$.Type') = 1)", sql01);

                var sql03 = fsql.Select<Staff2040>().Where(x => new[] { OrgType.Bank, OrgType.Brokerage }.Contains(x.Org.Type)).ToSql();
                Assert.Equal("SELECT a.`Org` \r\nFROM `Staff2040` a \r\nWHERE (json_extract(a.`Org`,'$.Type') in (1,2))", sql03);
                var orgTypes1 = new[] { OrgType.Bank, OrgType.Brokerage };
                var sql04 = fsql.Select<Staff2040>().Where(x => orgTypes1.Contains(x.Org.Type)).ToSql();
                Assert.Equal("SELECT a.`Org` \r\nFROM `Staff2040` a \r\nWHERE (json_extract(a.`Org`,'$.Type') in (1,2))", sql04);
                var orgTypes2 = new List<OrgType> { OrgType.Bank, OrgType.Brokerage };
                var sql05 = fsql.Select<Staff2040>().Where(x => orgTypes2.Contains(x.Org.Type)).ToSql();
                Assert.Equal("SELECT a.`Org` \r\nFROM `Staff2040` a \r\nWHERE (json_extract(a.`Org`,'$.Type') in (1,2))", sql05);
            }
        }


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
            var orm = g.mysql;
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
            var orm = g.mysql;
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
            var orm = g.mysql;
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
            var orm = g.mysql;
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
