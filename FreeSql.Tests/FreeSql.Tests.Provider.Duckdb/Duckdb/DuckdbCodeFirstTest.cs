using FreeSql.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace FreeSql.Tests.Duckdb
{
    public class DuckdbCodeFirstTest
    {
        IFreeSql fsql => g.duckdb;


        [Fact]
        public void CompositeTypeCrud()
        {
        }
        public class test_CompositeTypeCrud
        {
            public Dictionary<string, object> testFieldStruct { get; set; }
        }

        [Fact]
        public void DateOnlyTimeOnly()
        {
            var item = new test_DateOnlyTimeOnly01 { testFieldDateOnly = DateOnly.FromDateTime(DateTime.Now) };
            item.Id = (int)fsql.Insert(item).ExecuteIdentity();

            var newitem = fsql.Select<test_DateOnlyTimeOnly01>().Where(a => a.Id == item.Id).ToOne();

            var now = DateTime.Parse("2024-8-20 23:00:11");
            var item2 = new test_DateOnlyTimeOnly01
            {
                testFieldDateTime = now,
                testFieldDateTimeNullable = now.AddDays(-1),
                testFieldDateOnly = DateOnly.FromDateTime(now),
                testFieldDateOnlyNullable = DateOnly.FromDateTime(now.AddDays(-1)),

                testFieldTimeSpan = TimeSpan.FromHours(16),
                testFieldTimeSpanNullable = TimeSpan.FromSeconds(90),
                testFieldTimeOnly = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)),
                testFieldTimeOnlyNullable = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(90)),
            };

            var sqlPar = fsql.Insert(item2).ToSql();
            var sqlText = fsql.Insert(item2).NoneParameter().ToSql();
            Assert.Equal("INSERT INTO \"test_dateonlytimeonly01\"(\"testfieldtimespan\", \"testfieldtimeonly\", \"testfielddatetime\", \"testfielddateonly\", \"testfieldtimespannullable\", \"testfieldtimeonlynullable\", \"testfielddatetimenullable\", \"testfielddateonlynullable\") VALUES(time '16:0:0.0', time '11:0:0', current_timestamp, date '2024-08-20', time '0:1:30.0', time '0:1:30', current_timestamp, date '2024-08-19')", sqlText);
            item2.Id = (int)fsql.Insert(item2).NoneParameter().ExecuteIdentity();
            var item3NP = fsql.Select<test_DateOnlyTimeOnly01>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.testFieldDateOnly, item2.testFieldDateOnly);
            Assert.Equal(item3NP.testFieldDateOnlyNullable, item2.testFieldDateOnlyNullable);
            Assert.True(Math.Abs((item3NP.testFieldTimeOnly - item2.testFieldTimeOnly).TotalSeconds) < 1);
            Assert.True(Math.Abs((item3NP.testFieldTimeOnlyNullable - item2.testFieldTimeOnlyNullable).Value.TotalSeconds) < 1);

            item2.Id = (int)fsql.Insert(item2).ExecuteIdentity();
            item3NP = fsql.Select<test_DateOnlyTimeOnly01>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.testFieldDateOnly, item2.testFieldDateOnly);
            Assert.Equal(item3NP.testFieldDateOnlyNullable, item2.testFieldDateOnlyNullable);
            Assert.True(Math.Abs((item3NP.testFieldTimeOnly - item2.testFieldTimeOnly).TotalSeconds) < 1);
            Assert.True(Math.Abs((item3NP.testFieldTimeOnlyNullable - item2.testFieldTimeOnlyNullable).Value.TotalSeconds) < 1);

            var items = fsql.Select<test_DateOnlyTimeOnly01>().ToList();
            var itemstb = fsql.Select<test_DateOnlyTimeOnly01>().ToDataTable();
        }
        class test_DateOnlyTimeOnly01
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public TimeSpan testFieldTimeSpan { get; set; }
            public TimeOnly testFieldTimeOnly { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime testFieldDateTime { get; set; }
            public DateOnly testFieldDateOnly { get; set; }

            public TimeSpan? testFieldTimeSpanNullable { get; set; }
            public TimeOnly? testFieldTimeOnlyNullable { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime? testFieldDateTimeNullable { get; set; }
            public DateOnly? testFieldDateOnlyNullable { get; set; }
        }

        [Fact]
        public void DateOnlyTimeOnlyArrayTypeCrud()
        {
            var item = new test_DateOnlyTimeOnlyArrayTypeCrud { };
            item.Id = (int)fsql.Insert(item).ExecuteIdentity();

            var newitem = fsql.Select<test_DateOnlyTimeOnlyArrayTypeCrud>().Where(a => a.Id == item.Id).ToOne();

            var now = DateTime.Parse("2024-8-20 23:00:11");
            var item2 = new test_DateOnlyTimeOnlyArrayTypeCrud
            {
                testFieldDateTimeArray = new[] { now, now.AddHours(2) },
                testFieldDateTimeArrayNullable = new DateTime?[] { now, null, now.AddHours(2) },
                testFieldDateOnlyArray = new[] { DateOnly.FromDateTime(now), DateOnly.FromDateTime(now.AddHours(2)) },
                testFieldDateOnlyArrayNullable = new DateOnly?[] { DateOnly.FromDateTime(now), null, DateOnly.FromDateTime(now.AddHours(2)) },
                
                testFieldTimeSpanArray = new[] { TimeSpan.FromHours(11), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60) },
                testFieldTimeSpanArrayNullable = new TimeSpan?[] { TimeSpan.FromHours(11), TimeSpan.FromSeconds(10), null, TimeSpan.FromSeconds(60) },
                testFieldTimeOnlyArray = new[] { TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(10)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(60)) },
                testFieldTimeOnlyArrayNullable = new TimeOnly?[] { TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(10)), null, TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(60)) },
            };

            var sqlText = fsql.Insert(item2).NoneParameter().ToSql();
            Assert.Equal("INSERT INTO \"test_dateonlytimeonlyarraytypecrud\"(\"testfieldtimespanarray\", \"testfieldtimeonlyarray\", \"testfielddatetimearray\", \"testfielddateonlyarray\", \"testfieldtimespanarraynullable\", \"testfieldtimeonlyarraynullable\", \"testfielddatetimearraynullable\", \"testfielddateonlyarraynullable\") VALUES([time '11:0:0.0',time '0:0:10.0',time '0:1:0.0'], [time '11:0:0',time '0:0:10',time '0:1:0'], [timestamp '2024-08-20 23:00:11.000000',timestamp '2024-08-21 01:00:11.000000'], [date '2024-08-20',date '2024-08-21'], [time '11:0:0.0',time '0:0:10.0',time '0:0:0.0',time '0:1:0.0'], [time '11:0:0',time '0:0:10',time '0:0:0',time '0:1:0'], [timestamp '2024-08-20 23:00:11.000000',timestamp '0001-01-01 00:00:00.000000',timestamp '2024-08-21 01:00:11.000000'], [date '2024-08-20',date '0001-01-01',date '2024-08-21'])", sqlText);
            item2.Id = (int)fsql.Insert(item2).NoneParameter().ExecuteIdentity();
            var item3NP = fsql.Select<test_DateOnlyTimeOnlyArrayTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.Id, item2.Id);
            Assert.Equal("2024-08-20 23:00:11, 2024-08-21 01:00:11", string.Join(", ", item3NP.testFieldDateTimeArray.Select(a => a.ToString("yyyy-MM-dd HH:mm:ss"))));
            Assert.Equal("2024-08-20 23:00:11, 0001-01-01 00:00:00, 2024-08-21 01:00:11", string.Join(", ", item3NP.testFieldDateTimeArrayNullable.Select(a => a?.ToString("yyyy-MM-dd HH:mm:ss"))));
            Assert.Equal("2024-08-20, 2024-08-21", string.Join(", ", item3NP.testFieldDateOnlyArray.Select(a => a.ToString("yyyy-MM-dd"))));
            Assert.Equal("2024-08-20, 0001-01-01, 2024-08-21", string.Join(", ", item3NP.testFieldDateOnlyArrayNullable.Select(a => a?.ToString("yyyy-MM-dd"))));

            Assert.Equal("11:00:00, 00:00:10, 00:01:00", string.Join(", ", item3NP.testFieldTimeSpanArray.Select(a => $"{a.Hours.ToString().PadLeft(2, '0')}:{a.Minutes.ToString().PadLeft(2, '0')}:{a.Seconds.ToString().PadLeft(2, '0')}")));
            Assert.Equal("11:00:00, 00:00:10, 00:00:00, 00:01:00", string.Join(", ", item3NP.testFieldTimeSpanArrayNullable.Select(a => $"{a?.Hours.ToString().PadLeft(2, '0')}:{a?.Minutes.ToString().PadLeft(2, '0')}:{a?.Seconds.ToString().PadLeft(2, '0')}")));
            Assert.Equal("11:00:00, 00:00:10, 00:01:00", string.Join(", ", item3NP.testFieldTimeOnlyArray.Select(a => $"{a.Hour.ToString().PadLeft(2, '0')}:{a.Minute.ToString().PadLeft(2, '0')}:{a.Second.ToString().PadLeft(2, '0')}")));
            Assert.Equal("11:00:00, 00:00:10, 00:00:00, 00:01:00", string.Join(", ", item3NP.testFieldTimeOnlyArrayNullable.Select(a => $"{a?.Hour.ToString().PadLeft(2, '0')}:{a?.Minute.ToString().PadLeft(2, '0')}:{a?.Second.ToString().PadLeft(2, '0')}")));

            sqlText = fsql.Insert(item2).ToSql();
            Assert.Equal("INSERT INTO \"test_dateonlytimeonlyarraytypecrud\"(\"testfieldtimespanarray\", \"testfieldtimeonlyarray\", \"testfielddatetimearray\", \"testfielddateonlyarray\", \"testfieldtimespanarraynullable\", \"testfieldtimeonlyarraynullable\", \"testfielddatetimearraynullable\", \"testfielddateonlyarraynullable\") VALUES([time '11:0:0.0',time '0:0:10.0',time '0:1:0.0'], [time '11:0:0',time '0:0:10',time '0:1:0'], [timestamp '2024-08-20 23:00:11.000000',timestamp '2024-08-21 01:00:11.000000'], [date '2024-08-20',date '2024-08-21'], [time '11:0:0.0',time '0:0:10.0',time '0:0:0.0',time '0:1:0.0'], [time '11:0:0',time '0:0:10',time '0:0:0',time '0:1:0'], [timestamp '2024-08-20 23:00:11.000000',timestamp '0001-01-01 00:00:00.000000',timestamp '2024-08-21 01:00:11.000000'], [date '2024-08-20',date '0001-01-01',date '2024-08-21'])", sqlText);
            item2.Id = (int)fsql.Insert(item2).ExecuteIdentity();
            item3NP = fsql.Select<test_DateOnlyTimeOnlyArrayTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.Id, item2.Id);
            Assert.Equal("2024-08-20 23:00:11, 2024-08-21 01:00:11", string.Join(", ", item3NP.testFieldDateTimeArray.Select(a => a.ToString("yyyy-MM-dd HH:mm:ss"))));
            Assert.Equal("2024-08-20 23:00:11, 0001-01-01 00:00:00, 2024-08-21 01:00:11", string.Join(", ", item3NP.testFieldDateTimeArrayNullable.Select(a => a?.ToString("yyyy-MM-dd HH:mm:ss"))));
            Assert.Equal("2024-08-20, 2024-08-21", string.Join(", ", item3NP.testFieldDateOnlyArray.Select(a => a.ToString("yyyy-MM-dd"))));
            Assert.Equal("2024-08-20, 0001-01-01, 2024-08-21", string.Join(", ", item3NP.testFieldDateOnlyArrayNullable.Select(a => a?.ToString("yyyy-MM-dd"))));

            
            Assert.Equal("11:00:00, 00:00:10, 00:01:00", string.Join(", ", item3NP.testFieldTimeSpanArray.Select(a => $"{a.Hours.ToString().PadLeft(2, '0')}:{a.Minutes.ToString().PadLeft(2, '0')}:{a.Seconds.ToString().PadLeft(2, '0')}")));
            Assert.Equal("11:00:00, 00:00:10, 00:00:00, 00:01:00", string.Join(", ", item3NP.testFieldTimeSpanArrayNullable.Select(a => $"{a?.Hours.ToString().PadLeft(2, '0')}:{a?.Minutes.ToString().PadLeft(2, '0')}:{a?.Seconds.ToString().PadLeft(2, '0')}")));
            Assert.Equal("11:00:00, 00:00:10, 00:01:00", string.Join(", ", item3NP.testFieldTimeOnlyArray.Select(a => $"{a.Hour.ToString().PadLeft(2, '0')}:{a.Minute.ToString().PadLeft(2, '0')}:{a.Second.ToString().PadLeft(2, '0')}")));
            Assert.Equal("11:00:00, 00:00:10, 00:00:00, 00:01:00", string.Join(", ", item3NP.testFieldTimeOnlyArrayNullable.Select(a => $"{a?.Hour.ToString().PadLeft(2, '0')}:{a?.Minute.ToString().PadLeft(2, '0')}:{a?.Second.ToString().PadLeft(2, '0')}")));
            
            var items = fsql.Select<test_DateOnlyTimeOnlyArrayTypeCrud>().ToList();
            var itemstb = fsql.Select<test_DateOnlyTimeOnlyArrayTypeCrud>().ToDataTable();
        }
        class test_DateOnlyTimeOnlyArrayTypeCrud
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public TimeSpan[] testFieldTimeSpanArray { get; set; }
            public TimeOnly[] testFieldTimeOnlyArray { get; set; }
            public DateTime[] testFieldDateTimeArray { get; set; }
            public DateOnly[] testFieldDateOnlyArray { get; set; }
           
            public TimeSpan?[] testFieldTimeSpanArrayNullable { get; set; }
            public TimeOnly?[] testFieldTimeOnlyArrayNullable { get; set; }
            public DateTime?[] testFieldDateTimeArrayNullable { get; set; }
            public DateOnly?[] testFieldDateOnlyArrayNullable { get; set; }
        }

        [Fact]
        public void UInt256Crud2()
        {
            var num = BigInteger.Parse("170141183460469231731687303715884105727");
            fsql.Delete<tuint256tb_01>().Where("1=1").ExecuteAffrows();
            Assert.Equal(1, fsql.Insert(new tuint256tb_01()).ExecuteAffrows());
            var find = fsql.Select<tuint256tb_01>().ToList();
            Assert.Single(find);
            Assert.Equal("0", find[0].Number.ToString());
            var item = new tuint256tb_01 { Number = num };
            Assert.Equal(1, fsql.Insert(item).ExecuteAffrows());
            find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
            Assert.Single(find);
            Assert.Equal(item.Number, find[0].Number);
            num = num - 1;
            item.Number = num;
            Assert.Equal(1, fsql.Update<tuint256tb_01>().SetSource(item).ExecuteAffrows());
            find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
            Assert.Single(find);
            Assert.Equal("170141183460469231731687303715884105726", find[0].Number.ToString());

            num = BigInteger.Parse("170141183460469231731687303715884105727");
            fsql.Delete<tuint256tb_01>().Where("1=1").ExecuteAffrows();
            Assert.Equal(1, fsql.Insert(new tuint256tb_01()).NoneParameter().ExecuteAffrows());
            find = fsql.Select<tuint256tb_01>().ToList();
            Assert.Single(find);
            Assert.Equal("0", find[0].Number.ToString());
            item = new tuint256tb_01 { Number = num };
            Assert.Equal(1, fsql.Insert(item).NoneParameter().ExecuteAffrows());
            find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
            Assert.Single(find);
            Assert.Equal(item.Number, find[0].Number);
            num = num - 1;
            item.Number = num;
            Assert.Equal(1, fsql.Update<tuint256tb_01>().NoneParameter().SetSource(item).ExecuteAffrows());
            find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
            Assert.Single(find);
            Assert.Equal("170141183460469231731687303715884105726", find[0].Number.ToString());
        }
        class tuint256tb_01
        {
            public Guid Id { get; set; }
            public BigInteger Number { get; set; }
        }

        [Fact]
        public void NumberTypeCrud()
        {
            var item = new test_NumberTypeCrud {  };
            item.Id = (int)fsql.Insert(item).ExecuteIdentity();

            var newitem = fsql.Select<test_NumberTypeCrud>().Where(a => a.Id == item.Id).ToOne();

            var item2 = new test_NumberTypeCrud
            {
                testFieldSByte = sbyte.MaxValue,
                testFieldSByteNullable = sbyte.MinValue,
                testFieldShort = short.MaxValue,
                testFieldShortNullable = short.MinValue,
                testFieldInt = int.MaxValue,
                testFieldIntNullable = int.MinValue,
                testFieldLong = long.MaxValue,
                testFieldLongNullable = long.MinValue,

                testFieldByte = byte.MaxValue,
                testFieldByteNullable = byte.MinValue,
                testFieldUShort = ushort.MaxValue,
                testFieldUShortNullable = ushort.MinValue,
                testFieldUInt = uint.MaxValue,
                testFieldUIntNullable = uint.MinValue,
                testFieldULong = ulong.MaxValue,
                testFieldULongNullable = ulong.MinValue,

                testFieldDouble = 888.88,
                testFieldDoubleNullable = 222.22,
                testFieldFloat = 777.77F,
                testFieldFloatNullable = 333.33F,
                testFieldDecimal = 999.99M,
                testFieldDecimalNullable = 111.11M,
            };

            var sqlText = fsql.Insert(item2).NoneParameter().ToSql();
            Assert.Equal("INSERT INTO \"test_numbertypecrud\"(\"testfieldsbyte\", \"testfieldshort\", \"testfieldint\", \"testfieldlong\", \"testfieldbyte\", \"testfieldushort\", \"testfielduint\", \"testfieldulong\", \"testfielddouble\", \"testfieldfloat\", \"testfielddecimal\", \"testfieldsbytenullable\", \"testfieldshortnullable\", \"testfieldintnullable\", \"testfieldlongnullable\", \"testfieldbytenullable\", \"testfieldushortnullable\", \"testfielduintnullable\", \"testfieldulongnullable\", \"testfielddoublenullable\", \"testfieldfloatnullable\", \"testfielddecimalnullable\") VALUES(127, 32767, 2147483647, 9223372036854775807, 255, 65535, 4294967295, 18446744073709551615, 888.88, 777.77, 999.99, -128, -32768, -2147483648, -9223372036854775808, 0, 0, 0, 0, 222.22, 333.33, 111.11)", sqlText);
            item2.Id = (int)fsql.Insert(item2).NoneParameter().ExecuteIdentity();
            var item3NP = fsql.Select<test_NumberTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.Id, item2.Id);
            Assert.Equal(item3NP.testFieldSByte, item2.testFieldSByte);
            Assert.Equal(item3NP.testFieldShort, item2.testFieldShort);
            Assert.Equal(item3NP.testFieldInt, item2.testFieldInt);
            Assert.Equal(item3NP.testFieldLong, item2.testFieldLong);
            Assert.Equal(item3NP.testFieldByte, item2.testFieldByte);
            Assert.Equal(item3NP.testFieldUShort, item2.testFieldUShort);
            Assert.Equal(item3NP.testFieldUInt, item2.testFieldUInt);
            Assert.Equal(item3NP.testFieldULong, item2.testFieldULong);
            Assert.Equal(item3NP.testFieldDouble, item2.testFieldDouble);
            Assert.Equal(item3NP.testFieldFloat, item2.testFieldFloat);
            Assert.Equal(item3NP.testFieldDecimal, item2.testFieldDecimal);

            Assert.Equal(item3NP.testFieldSByteNullable, item2.testFieldSByteNullable);
            Assert.Equal(item3NP.testFieldShortNullable, item2.testFieldShortNullable);
            Assert.Equal(item3NP.testFieldIntNullable, item2.testFieldIntNullable);
            Assert.Equal(item3NP.testFieldLongNullable, item2.testFieldLongNullable);
            Assert.Equal(item3NP.testFieldByteNullable, item2.testFieldByteNullable);
            Assert.Equal(item3NP.testFieldUShortNullable, item2.testFieldUShortNullable);
            Assert.Equal(item3NP.testFieldUIntNullable, item2.testFieldUIntNullable);
            Assert.Equal(item3NP.testFieldULongNullable, item2.testFieldULongNullable);
            Assert.Equal(item3NP.testFieldDoubleNullable, item2.testFieldDoubleNullable);
            Assert.Equal(item3NP.testFieldFloatNullable, item2.testFieldFloatNullable);
            Assert.Equal(item3NP.testFieldDecimalNullable, item2.testFieldDecimalNullable);

            sqlText = fsql.Insert(item2).ToSql();
            Assert.Equal("INSERT INTO \"test_numbertypecrud\"(\"testfieldsbyte\", \"testfieldshort\", \"testfieldint\", \"testfieldlong\", \"testfieldbyte\", \"testfieldushort\", \"testfielduint\", \"testfieldulong\", \"testfielddouble\", \"testfieldfloat\", \"testfielddecimal\", \"testfieldsbytenullable\", \"testfieldshortnullable\", \"testfieldintnullable\", \"testfieldlongnullable\", \"testfieldbytenullable\", \"testfieldushortnullable\", \"testfielduintnullable\", \"testfieldulongnullable\", \"testfielddoublenullable\", \"testfieldfloatnullable\", \"testfielddecimalnullable\") VALUES(127, 32767, 2147483647, 9223372036854775807, 255, 65535, 4294967295, 18446744073709551615, 888.88, 777.77, 999.99, -128, -32768, -2147483648, -9223372036854775808, 0, 0, 0, 0, 222.22, 333.33, 111.11)", sqlText);
            item2.Id = (int)fsql.Insert(item2).ExecuteIdentity();
            item3NP = fsql.Select<test_NumberTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.Id, item2.Id);
            Assert.Equal(item3NP.testFieldSByte, item2.testFieldSByte);
            Assert.Equal(item3NP.testFieldShort, item2.testFieldShort);
            Assert.Equal(item3NP.testFieldInt, item2.testFieldInt);
            Assert.Equal(item3NP.testFieldLong, item2.testFieldLong);
            Assert.Equal(item3NP.testFieldByte, item2.testFieldByte);
            Assert.Equal(item3NP.testFieldUShort, item2.testFieldUShort);
            Assert.Equal(item3NP.testFieldUInt, item2.testFieldUInt);
            Assert.Equal(item3NP.testFieldULong, item2.testFieldULong);
            Assert.Equal(item3NP.testFieldDouble, item2.testFieldDouble);
            Assert.Equal(item3NP.testFieldFloat, item2.testFieldFloat);
            Assert.Equal(item3NP.testFieldDecimal, item2.testFieldDecimal);

            Assert.Equal(item3NP.testFieldSByteNullable, item2.testFieldSByteNullable);
            Assert.Equal(item3NP.testFieldShortNullable, item2.testFieldShortNullable);
            Assert.Equal(item3NP.testFieldIntNullable, item2.testFieldIntNullable);
            Assert.Equal(item3NP.testFieldLongNullable, item2.testFieldLongNullable);
            Assert.Equal(item3NP.testFieldByteNullable, item2.testFieldByteNullable);
            Assert.Equal(item3NP.testFieldUShortNullable, item2.testFieldUShortNullable);
            Assert.Equal(item3NP.testFieldUIntNullable, item2.testFieldUIntNullable);
            Assert.Equal(item3NP.testFieldULongNullable, item2.testFieldULongNullable);
            Assert.Equal(item3NP.testFieldDoubleNullable, item2.testFieldDoubleNullable);
            Assert.Equal(item3NP.testFieldFloatNullable, item2.testFieldFloatNullable);
            Assert.Equal(item3NP.testFieldDecimalNullable, item2.testFieldDecimalNullable);

            var items = fsql.Select<test_NumberTypeCrud>().ToList();
            var itemstb = fsql.Select<test_NumberTypeCrud>().ToDataTable();
        }
        class test_NumberTypeCrud
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public sbyte testFieldSByte { get; set; }
            public short testFieldShort { get; set; }
            public int testFieldInt { get; set; }
            public long testFieldLong { get; set; }
            public byte testFieldByte { get; set; }
            public ushort testFieldUShort { get; set; }
            public uint testFieldUInt { get; set; }
            public ulong testFieldULong { get; set; }
            public double testFieldDouble { get; set; }
            public float testFieldFloat { get; set; }
            public decimal testFieldDecimal { get; set; }

            public sbyte? testFieldSByteNullable { get; set; }
            public short? testFieldShortNullable { get; set; }
            public int? testFieldIntNullable { get; set; }
            public long? testFieldLongNullable { get; set; }
            public byte? testFieldByteNullable { get; set; }
            public ushort? testFieldUShortNullable { get; set; }
            public uint? testFieldUIntNullable { get; set; }
            public ulong? testFieldULongNullable { get; set; }
            public double? testFieldDoubleNullable { get; set; }
            public float? testFieldFloatNullable { get; set; }
            public decimal? testFieldDecimalNullable { get; set; }
        }

        [Fact]
        public void NumberArrayTypeCrud()
        {
            var item = new test_NumberArrayTypeCrud { };
            item.Id = (int)fsql.Insert(item).ExecuteIdentity();

            var newitem = fsql.Select<test_NumberArrayTypeCrud>().Where(a => a.Id == item.Id).ToOne();

            var item2 = new test_NumberArrayTypeCrud
            {
                testFieldByteArrayNullable = new byte?[] { 0, 1, 2, 3, null, 4, 5, 6 },
                testFieldShortArray = new short[] { 1, 2, 3, 4, 5 },
                testFieldShortArrayNullable = new short?[] { 1, 2, 3, null, 4, 5 },
                testFieldIntArray = new[] { 1, 2, 3, 4, 5 },
                testFieldIntArrayNullable = new int?[] { 1, 2, 3, null, 4, 5 },
                testFieldLongArray = new long[] { 10, 20, 30, 40, 50 },
                testFieldLongArrayNullable = new long?[] { 500, 600, 700, null, 999, 1000 },

                testFieldSByteArray = new sbyte[] { 1, 2, 3, 4, 5 },
                testFieldSByteArrayNullable = new sbyte?[] { 1, 2, 3, null, 4, 5 },
                testFieldUShortArray = new ushort[] { 11, 12, 13, 14, 15 },
                testFieldUShortArrayNullable = new ushort?[] { 11, 12, 13, null, 14, 15 },
                testFieldUIntArray = new uint[] { 1, 2, 3, 4, 5 },
                testFieldUIntArrayNullable = new uint?[] { 1, 2, 3, null, 4, 5 },
                testFieldULongArray = new ulong[] { 10, 20, 30, 40, 50 },
                testFieldULongArrayNullable = new ulong?[] { 10, 20, 30, null, 40, 50 },

                testFieldDoubleArray = new[] { 888.81, 888.82, 888.83 },
                testFieldDoubleArrayNullable = new double?[] { 888.11, 888.12, null, 888.13 },
                testFieldFloatArray = new[] { 777.71F, 777.72F, 777.73F },
                testFieldFloatArrayNullable = new float?[] { 777.71F, 777.72F, null, 777.73F },
                testFieldDecimalArray = new[] { 999.91M, 999.92M, 999.93M },
                testFieldDecimalArrayNullable = new decimal?[] { 998.11M, 998.12M, null, 998.13M },
            };

            var sqlText = fsql.Insert(item2).NoneParameter().ToSql();
            Assert.Equal("INSERT INTO \"test_numberarraytypecrud\"(\"testfieldsbytearray\", \"testfieldshortarray\", \"testfieldintarray\", \"testfieldlongarray\", \"testfieldushortarray\", \"testfielduintarray\", \"testfieldulongarray\", \"testfielddoublearray\", \"testfieldfloatarray\", \"testfielddecimalarray\", \"testfieldsbytearraynullable\", \"testfieldshortarraynullable\", \"testfieldintarraynullable\", \"testfieldlongarraynullable\", \"testfieldbytearraynullable\", \"testfieldushortarraynullable\", \"testfielduintarraynullable\", \"testfieldulongarraynullable\", \"testfielddoublearraynullable\", \"testfieldfloatarraynullable\", \"testfielddecimalarraynullable\") VALUES([1,2,3,4,5], [1,2,3,4,5], [1,2,3,4,5], [10,20,30,40,50], [11,12,13,14,15], [1,2,3,4,5], [10,20,30,40,50], [888.81,888.82,888.83], [777.71,777.72,777.73], [999.91,999.92,999.93], [1,2,3,0,4,5], [1,2,3,0,4,5], [1,2,3,0,4,5], [500,600,700,0,999,1000], [0,1,2,3,0,4,5,6], [11,12,13,0,14,15], [1,2,3,0,4,5], [10,20,30,0,40,50], [888.11,888.12,0,888.13], [777.71,777.72,0,777.73], [998.11,998.12,0,998.13])", sqlText);
            item2.Id = (int)fsql.Insert(item2).NoneParameter().ExecuteIdentity();
            var item3NP = fsql.Select<test_NumberArrayTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.Id, item2.Id);
            Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldSByteArray));
            Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldShortArray));
            Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldIntArray));
            Assert.Equal("10, 20, 30, 40, 50", string.Join(", ", item3NP.testFieldLongArray));
            Assert.Equal("11, 12, 13, 14, 15", string.Join(", ", item3NP.testFieldUShortArray));
            Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldUIntArray));
            Assert.Equal("10, 20, 30, 40, 50", string.Join(", ", item3NP.testFieldULongArray));
            Assert.Equal("888.81, 888.82, 888.83", string.Join(", ", item3NP.testFieldDoubleArray));
            Assert.Equal("777.71, 777.72, 777.73", string.Join(", ", item3NP.testFieldFloatArray));
            Assert.Equal("999.91, 999.92, 999.93", string.Join(", ", item3NP.testFieldDecimalArray));

            Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldSByteArrayNullable));
            Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldShortArrayNullable));
            Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldIntArrayNullable));
            Assert.Equal("500, 600, 700, 0, 999, 1000", string.Join(", ", item3NP.testFieldLongArrayNullable));
            Assert.Equal("0, 1, 2, 3, 0, 4, 5, 6", string.Join(", ", item3NP.testFieldByteArrayNullable));
            Assert.Equal("11, 12, 13, 0, 14, 15", string.Join(", ", item3NP.testFieldUShortArrayNullable));
            Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldUIntArrayNullable));
            Assert.Equal("10, 20, 30, 0, 40, 50", string.Join(", ", item3NP.testFieldULongArrayNullable));
            Assert.Equal("888.11, 888.12, 0, 888.13", string.Join(", ", item3NP.testFieldDoubleArrayNullable));
            Assert.Equal("777.71, 777.72, 0, 777.73", string.Join(", ", item3NP.testFieldFloatArrayNullable));
            Assert.Equal("998.11, 998.12, 0, 998.13", string.Join(", ", item3NP.testFieldDecimalArrayNullable));

            sqlText = fsql.Insert(item2).ToSql();
            Assert.Equal("INSERT INTO \"test_numberarraytypecrud\"(\"testfieldsbytearray\", \"testfieldshortarray\", \"testfieldintarray\", \"testfieldlongarray\", \"testfieldushortarray\", \"testfielduintarray\", \"testfieldulongarray\", \"testfielddoublearray\", \"testfieldfloatarray\", \"testfielddecimalarray\", \"testfieldsbytearraynullable\", \"testfieldshortarraynullable\", \"testfieldintarraynullable\", \"testfieldlongarraynullable\", \"testfieldbytearraynullable\", \"testfieldushortarraynullable\", \"testfielduintarraynullable\", \"testfieldulongarraynullable\", \"testfielddoublearraynullable\", \"testfieldfloatarraynullable\", \"testfielddecimalarraynullable\") VALUES([1,2,3,4,5], [1,2,3,4,5], [1,2,3,4,5], [10,20,30,40,50], [11,12,13,14,15], [1,2,3,4,5], [10,20,30,40,50], [888.81,888.82,888.83], [777.71,777.72,777.73], [999.91,999.92,999.93], [1,2,3,0,4,5], [1,2,3,0,4,5], [1,2,3,0,4,5], [500,600,700,0,999,1000], [0,1,2,3,0,4,5,6], [11,12,13,0,14,15], [1,2,3,0,4,5], [10,20,30,0,40,50], [888.11,888.12,0,888.13], [777.71,777.72,0,777.73], [998.11,998.12,0,998.13])", sqlText);
            item2.Id = (int)fsql.Insert(item2).ExecuteIdentity();
            item3NP = fsql.Select<test_NumberArrayTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.Id, item2.Id);
            Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldSByteArray));
            Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldShortArray));
            Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldIntArray));
            Assert.Equal("10, 20, 30, 40, 50", string.Join(", ", item3NP.testFieldLongArray));
            Assert.Equal("11, 12, 13, 14, 15", string.Join(", ", item3NP.testFieldUShortArray));
            Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldUIntArray));
            Assert.Equal("10, 20, 30, 40, 50", string.Join(", ", item3NP.testFieldULongArray));
            Assert.Equal("888.81, 888.82, 888.83", string.Join(", ", item3NP.testFieldDoubleArray));
            Assert.Equal("777.71, 777.72, 777.73", string.Join(", ", item3NP.testFieldFloatArray));
            Assert.Equal("999.91, 999.92, 999.93", string.Join(", ", item3NP.testFieldDecimalArray));

            Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldSByteArrayNullable));
            Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldShortArrayNullable));
            Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldIntArrayNullable));
            Assert.Equal("500, 600, 700, 0, 999, 1000", string.Join(", ", item3NP.testFieldLongArrayNullable));
            Assert.Equal("0, 1, 2, 3, 0, 4, 5, 6", string.Join(", ", item3NP.testFieldByteArrayNullable));
            Assert.Equal("11, 12, 13, 0, 14, 15", string.Join(", ", item3NP.testFieldUShortArrayNullable));
            Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldUIntArrayNullable));
            Assert.Equal("10, 20, 30, 0, 40, 50", string.Join(", ", item3NP.testFieldULongArrayNullable));
            Assert.Equal("888.11, 888.12, 0, 888.13", string.Join(", ", item3NP.testFieldDoubleArrayNullable));
            Assert.Equal("777.71, 777.72, 0, 777.73", string.Join(", ", item3NP.testFieldFloatArrayNullable));
            Assert.Equal("998.11, 998.12, 0, 998.13", string.Join(", ", item3NP.testFieldDecimalArrayNullable));

            var items = fsql.Select<test_NumberArrayTypeCrud>().ToList();
            var itemstb = fsql.Select<test_NumberArrayTypeCrud>().ToDataTable();
        }
        class test_NumberArrayTypeCrud
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public sbyte[] testFieldSByteArray { get; set; }
            public short[] testFieldShortArray { get; set; }
            public int[] testFieldIntArray { get; set; }
            public long[] testFieldLongArray { get; set; }
            public ushort[] testFieldUShortArray { get; set; }
            public uint[] testFieldUIntArray { get; set; }
            public ulong[] testFieldULongArray { get; set; }
            public double[] testFieldDoubleArray { get; set; }
            public float[] testFieldFloatArray { get; set; }
            public decimal[] testFieldDecimalArray { get; set; }

            public sbyte?[] testFieldSByteArrayNullable { get; set; }
            public short?[] testFieldShortArrayNullable { get; set; }
            public int?[] testFieldIntArrayNullable { get; set; }
            public long?[] testFieldLongArrayNullable { get; set; }
            public byte?[] testFieldByteArrayNullable { get; set; }
            public ushort?[] testFieldUShortArrayNullable { get; set; }
            public uint?[] testFieldUIntArrayNullable { get; set; }
            public ulong?[] testFieldULongArrayNullable { get; set; }
            public double?[] testFieldDoubleArrayNullable { get; set; }
            public float?[] testFieldFloatArrayNullable { get; set; }
            public decimal?[] testFieldDecimalArrayNullable { get; set; }
        }

        [Fact]
        public void OtherTypeCrud()
        {
            var item = new test_OtherTypeCrud { };
            item.Id = (int)fsql.Insert(item).ExecuteIdentity();

            var newitem = fsql.Select<test_OtherTypeCrud>().Where(a => a.Id == item.Id).ToOne();

            var newGuid = Guid.Parse("9e461804-7ed6-4a66-a609-408b2c195abf");
            var item2 = new test_OtherTypeCrud
            {
                testFieldBool = true,
                testFieldBoolNullable = true,
                testFieldGuid = newGuid,
                testFieldGuidNullable = newGuid,
                testFieldBytes = Encoding.UTF8.GetBytes("我是中国人"),
                testFieldString = "我是中国人string'\\?!@#$%^&*()_+{}}{~?><<>",
                testFieldChar = 'X',
                testFieldBitArray = new BitArray(Encoding.UTF8.GetBytes("我是")),

                testFieldEnum1 = TableAllTypeEnumType1.e3,
                testFieldEnum1Nullable = TableAllTypeEnumType1.e2,
                testFieldEnum2 = TableAllTypeEnumType2.f2,
                testFieldEnum2Nullable = TableAllTypeEnumType2.f3,
            };

            var sqlText = fsql.Insert(item2).NoneParameter().ToSql();
            Assert.Equal("INSERT INTO \"test_othertypecrud\"(\"testfieldbool\", \"testfieldguid\", \"testfieldbytes\", \"testfieldstring\", \"testfieldchar\", \"testfieldbitarray\", \"testfieldboolnullable\", \"testfieldguidnullable\", \"testfieldenum1\", \"testfieldenum1nullable\", \"testfieldenum2\", \"testfieldenum2nullable\") VALUES(true, '9e461804-7ed6-4a66-a609-408b2c195abf', '\\xE6\\x88\\x91\\xE6\\x98\\xAF\\xE4\\xB8\\xAD\\xE5\\x9B\\xBD\\xE4\\xBA\\xBA'::blob, '我是中国人string''\\?!@#$%^&*()_+{}}{~?><<>', 'X', bit '011001110001000110001001011001110001100111110101', true, '9e461804-7ed6-4a66-a609-408b2c195abf', 2, 1, 1, 2)", sqlText);
            item2.Id = (int)fsql.Insert(item2).NoneParameter().ExecuteIdentity();
            var item3NP = fsql.Select<test_OtherTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.Id, item2.Id);
            Assert.Equal(item3NP.testFieldBool, item2.testFieldBool);
            Assert.Equal(item3NP.testFieldGuid, item2.testFieldGuid);
            Assert.Equal(Convert.ToBase64String(item3NP.testFieldBytes), Convert.ToBase64String(item2.testFieldBytes));
            Assert.Equal(item3NP.testFieldString, item2.testFieldString);
            Assert.Equal(item3NP.testFieldChar, item2.testFieldChar);
            Assert.Equal(Get1010(item3NP.testFieldBitArray), Get1010(item2.testFieldBitArray));

            Assert.Equal(item3NP.testFieldBoolNullable, item2.testFieldBoolNullable);
            Assert.Equal(item3NP.testFieldGuidNullable, item2.testFieldGuidNullable);

            Assert.Equal(item3NP.testFieldEnum1, item2.testFieldEnum1);
            Assert.Equal(item3NP.testFieldEnum1Nullable, item2.testFieldEnum1Nullable);
            Assert.Equal(item3NP.testFieldEnum2, item2.testFieldEnum2);
            Assert.Equal(item3NP.testFieldEnum2Nullable, item2.testFieldEnum2Nullable);

            sqlText = fsql.Insert(item2).ToSql();
            Assert.Equal("INSERT INTO \"test_othertypecrud\"(\"testfieldbool\", \"testfieldguid\", \"testfieldbytes\", \"testfieldstring\", \"testfieldchar\", \"testfieldbitarray\", \"testfieldboolnullable\", \"testfieldguidnullable\", \"testfieldenum1\", \"testfieldenum1nullable\", \"testfieldenum2\", \"testfieldenum2nullable\") VALUES(true, '9e461804-7ed6-4a66-a609-408b2c195abf', '\\xE6\\x88\\x91\\xE6\\x98\\xAF\\xE4\\xB8\\xAD\\xE5\\x9B\\xBD\\xE4\\xBA\\xBA'::blob, '我是中国人string''\\?!@#$%^&*()_+{}}{~?><<>', 'X', bit '011001110001000110001001011001110001100111110101', true, '9e461804-7ed6-4a66-a609-408b2c195abf', 2, 1, 1, 2)", sqlText);
            item2.Id = (int)fsql.Insert(item2).ExecuteIdentity();
            item3NP = fsql.Select<test_OtherTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.Id, item2.Id);
            Assert.Equal(item3NP.testFieldBool, item2.testFieldBool);
            Assert.Equal(item3NP.testFieldGuid, item2.testFieldGuid);
            Assert.Equal(Convert.ToBase64String(item3NP.testFieldBytes), Convert.ToBase64String(item2.testFieldBytes));
            Assert.Equal(item3NP.testFieldString, item2.testFieldString);
            Assert.Equal(item3NP.testFieldChar, item2.testFieldChar);
            Assert.Equal(Get1010(item3NP.testFieldBitArray), Get1010(item2.testFieldBitArray));

            Assert.Equal(item3NP.testFieldBoolNullable, item2.testFieldBoolNullable);
            Assert.Equal(item3NP.testFieldGuidNullable, item2.testFieldGuidNullable);

            Assert.Equal(item3NP.testFieldEnum1, item2.testFieldEnum1);
            Assert.Equal(item3NP.testFieldEnum1Nullable, item2.testFieldEnum1Nullable);
            Assert.Equal(item3NP.testFieldEnum2, item2.testFieldEnum2);
            Assert.Equal(item3NP.testFieldEnum2Nullable, item2.testFieldEnum2Nullable);

            var items = fsql.Select<test_OtherTypeCrud>().ToList();
            var itemstb = fsql.Select<test_OtherTypeCrud>().ToDataTable();
        }
        string Get1010(BitArray ba)
        {
            char[] ba1010 = new char[ba.Length];
            for (int a = 0; a < ba.Length; a++) ba1010[a] = ba[a] ? '1' : '0';
            return new string(ba1010);
        }
        class test_OtherTypeCrud
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public bool testFieldBool { get; set; }
            public Guid testFieldGuid { get; set; }
            public byte[] testFieldBytes { get; set; }
            public string testFieldString { get; set; }
            public char testFieldChar { get; set; }
            public BitArray testFieldBitArray { get; set; }

            public bool? testFieldBoolNullable { get; set; }
            public Guid? testFieldGuidNullable { get; set; }

            public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
            public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
            public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
            public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }
        }

        [Fact]
        public void OtherArrayTypeCrud()
        {
            var item = new test_OtherArrayTypeCrud { };
            item.Id = (int)fsql.Insert(item).ExecuteIdentity();

            var newitem = fsql.Select<test_OtherArrayTypeCrud>().Where(a => a.Id == item.Id).ToOne();

            var newGuid = Guid.Parse("9e461804-7ed6-4a66-a609-408b2c195abf");
            var item2 = new test_OtherArrayTypeCrud
            {
                testFieldBoolArray = new[] { true, true, false, false },
                testFieldBoolArrayNullable = new bool?[] { true, true, null, false, false },
                testFieldBytesArray = new[] { Encoding.UTF8.GetBytes("我是中国人"), Encoding.UTF8.GetBytes("我是中国人") },
                testFieldGuidArray = new[] { newGuid, newGuid },
                testFieldGuidArrayNullable = new Guid?[] { newGuid, null, newGuid },
                testFieldStringArray = new[] { "我是中国人String1", "我是中国人String2", null, "我是中国人String3" },
                testFieldBitArrayArray = new[] { new BitArray(Encoding.UTF8.GetBytes("中国")), new BitArray(Encoding.UTF8.GetBytes("公民")) },

                testFieldEnum1Array = new[] { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, TableAllTypeEnumType1.e1 },
                testFieldEnum1ArrayNullable = new TableAllTypeEnumType1?[] { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, null, TableAllTypeEnumType1.e1 },
                testFieldEnum2Array = new[] { TableAllTypeEnumType2.f3, TableAllTypeEnumType2.f1 },
                testFieldEnum2ArrayNullable = new TableAllTypeEnumType2?[] { TableAllTypeEnumType2.f3, null, TableAllTypeEnumType2.f1 },
            };

            var sqlText = fsql.Insert(item2).NoneParameter().ToSql();
            Assert.Equal("INSERT INTO \"test_otherarraytypecrud\"(\"testfieldboolarray\", \"testfieldbytesarray\", \"testfieldstringarray\", \"testfieldguidarray\", \"testfieldboolarraynullable\", \"testfieldguidarraynullable\", \"testfieldbitarrayarray\", \"testfieldenum1array\", \"testfieldenum1arraynullable\", \"testfieldenum2array\", \"testfieldenum2arraynullable\") VALUES([true,true,false,false], ['\\xE6\\x88\\x91\\xE6\\x98\\xAF\\xE4\\xB8\\xAD\\xE5\\x9B\\xBD\\xE4\\xBA\\xBA'::blob,'\\xE6\\x88\\x91\\xE6\\x98\\xAF\\xE4\\xB8\\xAD\\xE5\\x9B\\xBD\\xE4\\xBA\\xBA'::blob], ['我是中国人String1','我是中国人String2',NULL,'我是中国人String3'], ['9e461804-7ed6-4a66-a609-408b2c195abf','9e461804-7ed6-4a66-a609-408b2c195abf'], [true,true,false,false,false], ['9e461804-7ed6-4a66-a609-408b2c195abf','00000000-0000-0000-0000-000000000000','9e461804-7ed6-4a66-a609-408b2c195abf'], [bit '001001110001110110110101101001111101100110111101',bit '101001111010000100110101011001110000110110001001'], [3,1,0], [3,1,0,0], [2,0], [2,0,0])", sqlText);
            item2.Id = (int)fsql.Insert(item2).NoneParameter().ExecuteIdentity();
            var item3NP = fsql.Select<test_OtherArrayTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.Id, item2.Id);
            Assert.Equal("True, True, False, False", string.Join(", ", item3NP.testFieldBoolArray));
            Assert.Equal("True, True, False, False, False", string.Join(", ", item3NP.testFieldBoolArrayNullable));
            Assert.Equal("5oiR5piv5Lit5Zu95Lq6, 5oiR5piv5Lit5Zu95Lq6", string.Join(", ", item3NP.testFieldBytesArray.Select(a => Convert.ToBase64String(a))));
            Assert.Equal("9e461804-7ed6-4a66-a609-408b2c195abf, 9e461804-7ed6-4a66-a609-408b2c195abf", string.Join(", ", item3NP.testFieldGuidArray));
            Assert.Equal("9e461804-7ed6-4a66-a609-408b2c195abf, 00000000-0000-0000-0000-000000000000, 9e461804-7ed6-4a66-a609-408b2c195abf", string.Join(", ", item3NP.testFieldGuidArrayNullable));
            Assert.Equal("我是中国人String1, 我是中国人String2, , 我是中国人String3", string.Join(", ", item3NP.testFieldStringArray));
            Assert.Equal("001001110001110110110101101001111101100110111101, 101001111010000100110101011001110000110110001001", string.Join(", ", item3NP.testFieldBitArrayArray.Select(a => Get1010(a))));

            Assert.Equal("e5, e2, e1", string.Join(", ", item3NP.testFieldEnum1Array));
            Assert.Equal("e5, e2, e1, e1", string.Join(", ", item3NP.testFieldEnum1ArrayNullable));
            Assert.Equal("f3, f1", string.Join(", ", item3NP.testFieldEnum2Array));
            Assert.Equal("f3, f1, f1", string.Join(", ", item3NP.testFieldEnum2ArrayNullable));

            sqlText = fsql.Insert(item2).ToSql();
            Assert.Equal("INSERT INTO \"test_otherarraytypecrud\"(\"testfieldboolarray\", \"testfieldbytesarray\", \"testfieldstringarray\", \"testfieldguidarray\", \"testfieldboolarraynullable\", \"testfieldguidarraynullable\", \"testfieldbitarrayarray\", \"testfieldenum1array\", \"testfieldenum1arraynullable\", \"testfieldenum2array\", \"testfieldenum2arraynullable\") VALUES([true,true,false,false], ['\\xE6\\x88\\x91\\xE6\\x98\\xAF\\xE4\\xB8\\xAD\\xE5\\x9B\\xBD\\xE4\\xBA\\xBA'::blob,'\\xE6\\x88\\x91\\xE6\\x98\\xAF\\xE4\\xB8\\xAD\\xE5\\x9B\\xBD\\xE4\\xBA\\xBA'::blob], ['我是中国人String1','我是中国人String2',NULL,'我是中国人String3'], ['9e461804-7ed6-4a66-a609-408b2c195abf','9e461804-7ed6-4a66-a609-408b2c195abf'], [true,true,false,false,false], ['9e461804-7ed6-4a66-a609-408b2c195abf','00000000-0000-0000-0000-000000000000','9e461804-7ed6-4a66-a609-408b2c195abf'], [bit '001001110001110110110101101001111101100110111101',bit '101001111010000100110101011001110000110110001001'], [3,1,0], [3,1,0,0], [2,0], [2,0,0])", sqlText);
            item2.Id = (int)fsql.Insert(item2).ExecuteIdentity();
            item3NP = fsql.Select<test_OtherArrayTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item3NP.Id, item2.Id);
            Assert.Equal("True, True, False, False", string.Join(", ", item3NP.testFieldBoolArray));
            Assert.Equal("True, True, False, False, False", string.Join(", ", item3NP.testFieldBoolArrayNullable));
            Assert.Equal("5oiR5piv5Lit5Zu95Lq6, 5oiR5piv5Lit5Zu95Lq6", string.Join(", ", item3NP.testFieldBytesArray.Select(a => Convert.ToBase64String(a))));
            Assert.Equal("9e461804-7ed6-4a66-a609-408b2c195abf, 9e461804-7ed6-4a66-a609-408b2c195abf", string.Join(", ", item3NP.testFieldGuidArray));
            Assert.Equal("9e461804-7ed6-4a66-a609-408b2c195abf, 00000000-0000-0000-0000-000000000000, 9e461804-7ed6-4a66-a609-408b2c195abf", string.Join(", ", item3NP.testFieldGuidArrayNullable));
            Assert.Equal("我是中国人String1, 我是中国人String2, , 我是中国人String3", string.Join(", ", item3NP.testFieldStringArray));
            Assert.Equal("001001110001110110110101101001111101100110111101, 101001111010000100110101011001110000110110001001", string.Join(", ", item3NP.testFieldBitArrayArray.Select(a => Get1010(a))));

            Assert.Equal("e5, e2, e1", string.Join(", ", item3NP.testFieldEnum1Array));
            Assert.Equal("e5, e2, e1, e1", string.Join(", ", item3NP.testFieldEnum1ArrayNullable));
            Assert.Equal("f3, f1", string.Join(", ", item3NP.testFieldEnum2Array));
            Assert.Equal("f3, f1, f1", string.Join(", ", item3NP.testFieldEnum2ArrayNullable));

            var items = fsql.Select<test_OtherArrayTypeCrud>().ToList();
            var itemstb = fsql.Select<test_OtherArrayTypeCrud>().ToDataTable();
        }
        class test_OtherArrayTypeCrud
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public bool[] testFieldBoolArray { get; set; }
            public byte[][] testFieldBytesArray { get; set; }
            public string[] testFieldStringArray { get; set; }
            public Guid[] testFieldGuidArray { get; set; }

            public bool?[] testFieldBoolArrayNullable { get; set; }
            public Guid?[] testFieldGuidArrayNullable { get; set; }

            public BitArray[] testFieldBitArrayArray { get; set; }

            public TableAllTypeEnumType1[] testFieldEnum1Array { get; set; }
            public TableAllTypeEnumType1?[] testFieldEnum1ArrayNullable { get; set; }
            public TableAllTypeEnumType2[] testFieldEnum2Array { get; set; }
            public TableAllTypeEnumType2?[] testFieldEnum2ArrayNullable { get; set; }
        }

        #region List<T> 测试代码，暂时不提供该功能，建议使用 T[]
        //[Fact]
        //public void BasicListTypeCrud()
        //{
        //    var item = new test_BasicListTypeCrud { };
        //    item.Id = (int)fsql.Insert(item).ExecuteIdentity();

        //    var newitem = fsql.Select<test_BasicListTypeCrud>().Where(a => a.Id == item.Id).ToOne();

        //    var now = DateTime.Parse("2024-8-20 23:00:11");
        //    var newGuid = Guid.Parse("9e461804-7ed6-4a66-a609-408b2c195abf");
        //    var item2 = new test_BasicListTypeCrud
        //    {
        //        testFieldByteArrayNullable = new List<byte?> { 0, 1, 2, 3, null, 4, 5, 6 },
        //        testFieldShortArray = new List<short> { 1, 2, 3, 4, 5 },
        //        testFieldShortArrayNullable = new List<short?> { 1, 2, 3, null, 4, 5 },
        //        testFieldIntArray = new List<int> { 1, 2, 3, 4, 5 },
        //        testFieldIntArrayNullable = new List<int?> { 1, 2, 3, null, 4, 5 },
        //        testFieldLongArray = new List<long> { 10, 20, 30, 40, 50 },
        //        testFieldLongArrayNullable = new List<long?> { 500, 600, 700, null, 999, 1000 },

        //        testFieldSByteArray = new List<sbyte> { 1, 2, 3, 4, 5 },
        //        testFieldSByteArrayNullable = new List<sbyte?> { 1, 2, 3, null, 4, 5 },
        //        testFieldUShortArray = new List<ushort> { 11, 12, 13, 14, 15 },
        //        testFieldUShortArrayNullable = new List<ushort?> { 11, 12, 13, null, 14, 15 },
        //        testFieldUIntArray = new List<uint> { 1, 2, 3, 4, 5 },
        //        testFieldUIntArrayNullable = new List<uint?> { 1, 2, 3, null, 4, 5 },
        //        testFieldULongArray = new List<ulong> { 10, 20, 30, 40, 50 },
        //        testFieldULongArrayNullable = new List<ulong?> { 10, 20, 30, null, 40, 50 },

        //        testFieldDoubleArray = new List<double> { 888.81, 888.82, 888.83 },
        //        testFieldDoubleArrayNullable = new List<double?> { 888.11, 888.12, null, 888.13 },
        //        testFieldFloatArray = new List<float> { 777.71F, 777.72F, 777.73F },
        //        testFieldFloatArrayNullable = new List<float?> { 777.71F, 777.72F, null, 777.73F },
        //        testFieldDecimalArray = new List<decimal> { 999.91M, 999.92M, 999.93M },
        //        testFieldDecimalArrayNullable = new List<decimal?> { 998.11M, 998.12M, null, 998.13M },

        //        testFieldBoolArray = new List<bool> { true, true, false, false },
        //        testFieldBoolArrayNullable = new List<bool?> { true, true, null, false, false },
        //        testFieldBytesArray = new List<byte[]> { Encoding.UTF8.GetBytes("我是中国人"), Encoding.UTF8.GetBytes("我是中国人") },
        //        testFieldGuidArray = new List<Guid> { newGuid, newGuid },
        //        testFieldGuidArrayNullable = new List<Guid?> { newGuid, null, newGuid },
        //        testFieldStringArray = new List<string> { "我是中国人String1", "我是中国人String2", null, "我是中国人String3" },

        //        testFieldEnum1Array = new List<TableAllTypeEnumType1> { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, TableAllTypeEnumType1.e1 },
        //        testFieldEnum1ArrayNullable = new List<TableAllTypeEnumType1?> { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, null, TableAllTypeEnumType1.e1 },
        //        testFieldEnum2Array = new List<TableAllTypeEnumType2> { TableAllTypeEnumType2.f3, TableAllTypeEnumType2.f1 },
        //        testFieldEnum2ArrayNullable = new List<TableAllTypeEnumType2?> { TableAllTypeEnumType2.f3, null, TableAllTypeEnumType2.f1 },

        //        testFieldDateTimeArray = new List<DateTime> { now, now.AddHours(2) },
        //        testFieldDateTimeArrayNullable = new List<DateTime?> { now, null, now.AddHours(2) },
        //        testFieldDateOnlyArray = new List<DateOnly> { DateOnly.FromDateTime(now), DateOnly.FromDateTime(now.AddHours(2)) },
        //        testFieldDateOnlyArrayNullable = new List<DateOnly?> { DateOnly.FromDateTime(now), null, DateOnly.FromDateTime(now.AddHours(2)) },

        //        testFieldTimeSpanArray = new List<TimeSpan> { TimeSpan.FromHours(11), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60) },
        //        testFieldTimeSpanArrayNullable = new List<TimeSpan?> { TimeSpan.FromHours(11), TimeSpan.FromSeconds(10), null, TimeSpan.FromSeconds(60) },
        //        testFieldTimeOnlyArray = new List<TimeOnly> { TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(10)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(60)) },
        //        testFieldTimeOnlyArrayNullable = new List<TimeOnly?> { TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(10)), null, TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(60)) },
        //    };

        //    var sqlText = fsql.Insert(item2).NoneParameter().ToSql();
        //    Assert.Equal("INSERT INTO \"test_basiclisttypecrud\"(\"testfieldsbytearray\", \"testfieldshortarray\", \"testfieldintarray\", \"testfieldlongarray\", \"testfieldushortarray\", \"testfielduintarray\", \"testfieldulongarray\", \"testfielddoublearray\", \"testfieldfloatarray\", \"testfielddecimalarray\", \"testfieldsbytearraynullable\", \"testfieldshortarraynullable\", \"testfieldintarraynullable\", \"testfieldlongarraynullable\", \"testfieldbytearraynullable\", \"testfieldushortarraynullable\", \"testfielduintarraynullable\", \"testfieldulongarraynullable\", \"testfielddoublearraynullable\", \"testfieldfloatarraynullable\", \"testfielddecimalarraynullable\", \"testfieldboolarray\", \"testfieldbytesarray\", \"testfieldstringarray\", \"testfieldguidarray\", \"testfieldboolarraynullable\", \"testfieldguidarraynullable\", \"testfieldenum1array\", \"testfieldenum1arraynullable\", \"testfieldenum2array\", \"testfieldenum2arraynullable\", \"testfieldtimespanarray\", \"testfieldtimeonlyarray\", \"testfielddatetimearray\", \"testfielddateonlyarray\", \"testfieldtimespanarraynullable\", \"testfieldtimeonlyarraynullable\", \"testfielddatetimearraynullable\", \"testfielddateonlyarraynullable\") VALUES([1,2,3,4,5], [1,2,3,4,5], [1,2,3,4,5], [10,20,30,40,50], [11,12,13,14,15], [1,2,3,4,5], [10,20,30,40,50], [888.81,888.82,888.83], [777.71,777.72,777.73], [999.91,999.92,999.93], [1,2,3,NULL,4,5], [1,2,3,NULL,4,5], [1,2,3,NULL,4,5], [500,600,700,NULL,999,1000], [0,1,2,3,NULL,4,5,6], [11,12,13,NULL,14,15], [1,2,3,NULL,4,5], [10,20,30,NULL,40,50], [888.11,888.12,NULL,888.13], [777.71,777.72,NULL,777.73], [998.11,998.12,NULL,998.13], [true,true,false,false], ['\\xE6\\x88\\x91\\xE6\\x98\\xAF\\xE4\\xB8\\xAD\\xE5\\x9B\\xBD\\xE4\\xBA\\xBA'::blob,'\\xE6\\x88\\x91\\xE6\\x98\\xAF\\xE4\\xB8\\xAD\\xE5\\x9B\\xBD\\xE4\\xBA\\xBA'::blob], ['我是中国人String1','我是中国人String2',NULL,'我是中国人String3'], ['9e461804-7ed6-4a66-a609-408b2c195abf','9e461804-7ed6-4a66-a609-408b2c195abf'], [true,true,NULL,false,false], ['9e461804-7ed6-4a66-a609-408b2c195abf',NULL,'9e461804-7ed6-4a66-a609-408b2c195abf'], [3,1,0], [3,1,NULL,0], [2,0], [2,NULL,0], [time '11:0:0.0',time '0:0:10.0',time '0:1:0.0'], [time '11:0:0',time '0:0:10',time '0:1:0'], [timestamp '2024-08-20 23:00:11.000000',timestamp '2024-08-21 01:00:11.000000'], [date '2024-08-20',date '2024-08-21'], [time '11:0:0.0',time '0:0:10.0',NULL,time '0:1:0.0'], [time '11:0:0',time '0:0:10',NULL,time '0:1:0'], [timestamp '2024-08-20 23:00:11.000000',NULL,timestamp '2024-08-21 01:00:11.000000'], [date '2024-08-20',NULL,date '2024-08-21'])", sqlText);
        //    item2.Id = (int)fsql.Insert(item2).NoneParameter().ExecuteIdentity();
        //    var item3NP = fsql.Select<test_BasicListTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
        //    Assert.Equal(item3NP.Id, item2.Id);
        //    Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldSByteArray));
        //    Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldShortArray));
        //    Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldIntArray));
        //    Assert.Equal("10, 20, 30, 40, 50", string.Join(", ", item3NP.testFieldLongArray));
        //    Assert.Equal("11, 12, 13, 14, 15", string.Join(", ", item3NP.testFieldUShortArray));
        //    Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldUIntArray));
        //    Assert.Equal("10, 20, 30, 40, 50", string.Join(", ", item3NP.testFieldULongArray));
        //    Assert.Equal("888.81, 888.82, 888.83", string.Join(", ", item3NP.testFieldDoubleArray));
        //    Assert.Equal("777.71, 777.72, 777.73", string.Join(", ", item3NP.testFieldFloatArray));
        //    Assert.Equal("999.91, 999.92, 999.93", string.Join(", ", item3NP.testFieldDecimalArray));

        //    Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldSByteArrayNullable));
        //    Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldShortArrayNullable));
        //    Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldIntArrayNullable));
        //    Assert.Equal("500, 600, 700, 0, 999, 1000", string.Join(", ", item3NP.testFieldLongArrayNullable));
        //    Assert.Equal("0, 1, 2, 3, 0, 4, 5, 6", string.Join(", ", item3NP.testFieldByteArrayNullable));
        //    Assert.Equal("11, 12, 13, 0, 14, 15", string.Join(", ", item3NP.testFieldUShortArrayNullable));
        //    Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldUIntArrayNullable));
        //    Assert.Equal("10, 20, 30, 0, 40, 50", string.Join(", ", item3NP.testFieldULongArrayNullable));
        //    Assert.Equal("888.11, 888.12, 0, 888.13", string.Join(", ", item3NP.testFieldDoubleArrayNullable));
        //    Assert.Equal("777.71, 777.72, 0, 777.73", string.Join(", ", item3NP.testFieldFloatArrayNullable));
        //    Assert.Equal("998.11, 998.12, 0, 998.13", string.Join(", ", item3NP.testFieldDecimalArrayNullable));

        //    Assert.Equal("True, True, False, False", string.Join(", ", item3NP.testFieldBoolArray));
        //    Assert.Equal("True, True, False, False, False", string.Join(", ", item3NP.testFieldBoolArrayNullable));
        //    Assert.Equal("5oiR5piv5Lit5Zu95Lq6, 5oiR5piv5Lit5Zu95Lq6", string.Join(", ", item3NP.testFieldBytesArray.Select(a => Convert.ToBase64String(a))));
        //    Assert.Equal("9e461804-7ed6-4a66-a609-408b2c195abf, 9e461804-7ed6-4a66-a609-408b2c195abf", string.Join(", ", item3NP.testFieldGuidArray));
        //    Assert.Equal("9e461804-7ed6-4a66-a609-408b2c195abf, 00000000-0000-0000-0000-000000000000, 9e461804-7ed6-4a66-a609-408b2c195abf", string.Join(", ", item3NP.testFieldGuidArrayNullable));
        //    Assert.Equal("我是中国人String1, 我是中国人String2, , 我是中国人String3", string.Join(", ", item3NP.testFieldStringArray));

        //    Assert.Equal("e5, e2, e1", string.Join(", ", item3NP.testFieldEnum1Array));
        //    Assert.Equal("e5, e2, e1, e1", string.Join(", ", item3NP.testFieldEnum1ArrayNullable));
        //    Assert.Equal("f3, f1", string.Join(", ", item3NP.testFieldEnum2Array));
        //    Assert.Equal("f3, f1, f1", string.Join(", ", item3NP.testFieldEnum2ArrayNullable));

        //    Assert.Equal("2024-08-20 23:00:11, 2024-08-21 01:00:11", string.Join(", ", item3NP.testFieldDateTimeArray.Select(a => a.ToString("yyyy-MM-dd HH:mm:ss"))));
        //    Assert.Equal("2024-08-20 23:00:11, 0001-01-01 00:00:00, 2024-08-21 01:00:11", string.Join(", ", item3NP.testFieldDateTimeArrayNullable.Select(a => a?.ToString("yyyy-MM-dd HH:mm:ss"))));
        //    Assert.Equal("2024-08-20, 2024-08-21", string.Join(", ", item3NP.testFieldDateOnlyArray.Select(a => a.ToString("yyyy-MM-dd"))));
        //    Assert.Equal("2024-08-20, 0001-01-01, 2024-08-21", string.Join(", ", item3NP.testFieldDateOnlyArrayNullable.Select(a => a?.ToString("yyyy-MM-dd"))));

        //    Assert.Equal("11:00:00, 00:00:10, 00:01:00", string.Join(", ", item3NP.testFieldTimeSpanArray.Select(a => $"{a.Hours.ToString().PadLeft(2, '0')}:{a.Minutes.ToString().PadLeft(2, '0')}:{a.Seconds.ToString().PadLeft(2, '0')}")));
        //    Assert.Equal("11:00:00, 00:00:10, 00:00:00, 00:01:00", string.Join(", ", item3NP.testFieldTimeSpanArrayNullable.Select(a => $"{a?.Hours.ToString().PadLeft(2, '0')}:{a?.Minutes.ToString().PadLeft(2, '0')}:{a?.Seconds.ToString().PadLeft(2, '0')}")));
        //    Assert.Equal("11:00:00, 00:00:10, 00:01:00", string.Join(", ", item3NP.testFieldTimeOnlyArray.Select(a => $"{a.Hour.ToString().PadLeft(2, '0')}:{a.Minute.ToString().PadLeft(2, '0')}:{a.Second.ToString().PadLeft(2, '0')}")));
        //    Assert.Equal("11:00:00, 00:00:10, 00:00:00, 00:01:00", string.Join(", ", item3NP.testFieldTimeOnlyArrayNullable.Select(a => $"{a?.Hour.ToString().PadLeft(2, '0')}:{a?.Minute.ToString().PadLeft(2, '0')}:{a?.Second.ToString().PadLeft(2, '0')}")));

        //    sqlText = fsql.Insert(item2).ToSql();
        //    Assert.Equal("INSERT INTO \"test_basiclisttypecrud\"(\"testfieldsbytearray\", \"testfieldshortarray\", \"testfieldintarray\", \"testfieldlongarray\", \"testfieldushortarray\", \"testfielduintarray\", \"testfieldulongarray\", \"testfielddoublearray\", \"testfieldfloatarray\", \"testfielddecimalarray\", \"testfieldsbytearraynullable\", \"testfieldshortarraynullable\", \"testfieldintarraynullable\", \"testfieldlongarraynullable\", \"testfieldbytearraynullable\", \"testfieldushortarraynullable\", \"testfielduintarraynullable\", \"testfieldulongarraynullable\", \"testfielddoublearraynullable\", \"testfieldfloatarraynullable\", \"testfielddecimalarraynullable\", \"testfieldboolarray\", \"testfieldbytesarray\", \"testfieldstringarray\", \"testfieldguidarray\", \"testfieldboolarraynullable\", \"testfieldguidarraynullable\", \"testfieldenum1array\", \"testfieldenum1arraynullable\", \"testfieldenum2array\", \"testfieldenum2arraynullable\", \"testfieldtimespanarray\", \"testfieldtimeonlyarray\", \"testfielddatetimearray\", \"testfielddateonlyarray\", \"testfieldtimespanarraynullable\", \"testfieldtimeonlyarraynullable\", \"testfielddatetimearraynullable\", \"testfielddateonlyarraynullable\") VALUES([1,2,3,4,5], [1,2,3,4,5], [1,2,3,4,5], [10,20,30,40,50], [11,12,13,14,15], [1,2,3,4,5], [10,20,30,40,50], [888.81,888.82,888.83], [777.71,777.72,777.73], [999.91,999.92,999.93], [1,2,3,NULL,4,5], [1,2,3,NULL,4,5], [1,2,3,NULL,4,5], [500,600,700,NULL,999,1000], [0,1,2,3,NULL,4,5,6], [11,12,13,NULL,14,15], [1,2,3,NULL,4,5], [10,20,30,NULL,40,50], [888.11,888.12,NULL,888.13], [777.71,777.72,NULL,777.73], [998.11,998.12,NULL,998.13], [true,true,false,false], ['\\xE6\\x88\\x91\\xE6\\x98\\xAF\\xE4\\xB8\\xAD\\xE5\\x9B\\xBD\\xE4\\xBA\\xBA'::blob,'\\xE6\\x88\\x91\\xE6\\x98\\xAF\\xE4\\xB8\\xAD\\xE5\\x9B\\xBD\\xE4\\xBA\\xBA'::blob], ['我是中国人String1','我是中国人String2',NULL,'我是中国人String3'], ['9e461804-7ed6-4a66-a609-408b2c195abf','9e461804-7ed6-4a66-a609-408b2c195abf'], [true,true,NULL,false,false], ['9e461804-7ed6-4a66-a609-408b2c195abf',NULL,'9e461804-7ed6-4a66-a609-408b2c195abf'], [3,1,0], [3,1,NULL,0], [2,0], [2,NULL,0], [time '11:0:0.0',time '0:0:10.0',time '0:1:0.0'], [time '11:0:0',time '0:0:10',time '0:1:0'], [timestamp '2024-08-20 23:00:11.000000',timestamp '2024-08-21 01:00:11.000000'], [date '2024-08-20',date '2024-08-21'], [time '11:0:0.0',time '0:0:10.0',NULL,time '0:1:0.0'], [time '11:0:0',time '0:0:10',NULL,time '0:1:0'], [timestamp '2024-08-20 23:00:11.000000',NULL,timestamp '2024-08-21 01:00:11.000000'], [date '2024-08-20',NULL,date '2024-08-21'])", sqlText);
        //    item2.Id = (int)fsql.Insert(item2).ExecuteIdentity();
        //    item3NP = fsql.Select<test_BasicListTypeCrud>().Where(a => a.Id == item2.Id).ToOne();
        //    Assert.Equal(item3NP.Id, item2.Id);
        //    Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldSByteArray));
        //    Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldShortArray));
        //    Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldIntArray));
        //    Assert.Equal("10, 20, 30, 40, 50", string.Join(", ", item3NP.testFieldLongArray));
        //    Assert.Equal("11, 12, 13, 14, 15", string.Join(", ", item3NP.testFieldUShortArray));
        //    Assert.Equal("1, 2, 3, 4, 5", string.Join(", ", item3NP.testFieldUIntArray));
        //    Assert.Equal("10, 20, 30, 40, 50", string.Join(", ", item3NP.testFieldULongArray));
        //    Assert.Equal("888.81, 888.82, 888.83", string.Join(", ", item3NP.testFieldDoubleArray));
        //    Assert.Equal("777.71, 777.72, 777.73", string.Join(", ", item3NP.testFieldFloatArray));
        //    Assert.Equal("999.91, 999.92, 999.93", string.Join(", ", item3NP.testFieldDecimalArray));

        //    Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldSByteArrayNullable));
        //    Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldShortArrayNullable));
        //    Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldIntArrayNullable));
        //    Assert.Equal("500, 600, 700, 0, 999, 1000", string.Join(", ", item3NP.testFieldLongArrayNullable));
        //    Assert.Equal("0, 1, 2, 3, 0, 4, 5, 6", string.Join(", ", item3NP.testFieldByteArrayNullable));
        //    Assert.Equal("11, 12, 13, 0, 14, 15", string.Join(", ", item3NP.testFieldUShortArrayNullable));
        //    Assert.Equal("1, 2, 3, 0, 4, 5", string.Join(", ", item3NP.testFieldUIntArrayNullable));
        //    Assert.Equal("10, 20, 30, 0, 40, 50", string.Join(", ", item3NP.testFieldULongArrayNullable));
        //    Assert.Equal("888.11, 888.12, 0, 888.13", string.Join(", ", item3NP.testFieldDoubleArrayNullable));
        //    Assert.Equal("777.71, 777.72, 0, 777.73", string.Join(", ", item3NP.testFieldFloatArrayNullable));
        //    Assert.Equal("998.11, 998.12, 0, 998.13", string.Join(", ", item3NP.testFieldDecimalArrayNullable));

        //    Assert.Equal("True, True, False, False", string.Join(", ", item3NP.testFieldBoolArray));
        //    Assert.Equal("True, True, False, False, False", string.Join(", ", item3NP.testFieldBoolArrayNullable));
        //    Assert.Equal("5oiR5piv5Lit5Zu95Lq6, 5oiR5piv5Lit5Zu95Lq6", string.Join(", ", item3NP.testFieldBytesArray.Select(a => Convert.ToBase64String(a))));
        //    Assert.Equal("9e461804-7ed6-4a66-a609-408b2c195abf, 9e461804-7ed6-4a66-a609-408b2c195abf", string.Join(", ", item3NP.testFieldGuidArray));
        //    Assert.Equal("9e461804-7ed6-4a66-a609-408b2c195abf, 00000000-0000-0000-0000-000000000000, 9e461804-7ed6-4a66-a609-408b2c195abf", string.Join(", ", item3NP.testFieldGuidArrayNullable));
        //    Assert.Equal("我是中国人String1, 我是中国人String2, , 我是中国人String3", string.Join(", ", item3NP.testFieldStringArray));

        //    Assert.Equal("e5, e2, e1", string.Join(", ", item3NP.testFieldEnum1Array));
        //    Assert.Equal("e5, e2, e1, e1", string.Join(", ", item3NP.testFieldEnum1ArrayNullable));
        //    Assert.Equal("f3, f1", string.Join(", ", item3NP.testFieldEnum2Array));
        //    Assert.Equal("f3, f1, f1", string.Join(", ", item3NP.testFieldEnum2ArrayNullable));

        //    Assert.Equal("2024-08-20 23:00:11, 2024-08-21 01:00:11", string.Join(", ", item3NP.testFieldDateTimeArray.Select(a => a.ToString("yyyy-MM-dd HH:mm:ss"))));
        //    Assert.Equal("2024-08-20 23:00:11, 0001-01-01 00:00:00, 2024-08-21 01:00:11", string.Join(", ", item3NP.testFieldDateTimeArrayNullable.Select(a => a?.ToString("yyyy-MM-dd HH:mm:ss"))));
        //    Assert.Equal("2024-08-20, 2024-08-21", string.Join(", ", item3NP.testFieldDateOnlyArray.Select(a => a.ToString("yyyy-MM-dd"))));
        //    Assert.Equal("2024-08-20, 0001-01-01, 2024-08-21", string.Join(", ", item3NP.testFieldDateOnlyArrayNullable.Select(a => a?.ToString("yyyy-MM-dd"))));

        //    Assert.Equal("11:00:00, 00:00:10, 00:01:00", string.Join(", ", item3NP.testFieldTimeSpanArray.Select(a => $"{a.Hours.ToString().PadLeft(2, '0')}:{a.Minutes.ToString().PadLeft(2, '0')}:{a.Seconds.ToString().PadLeft(2, '0')}")));
        //    Assert.Equal("11:00:00, 00:00:10, 00:00:00, 00:01:00", string.Join(", ", item3NP.testFieldTimeSpanArrayNullable.Select(a => $"{a?.Hours.ToString().PadLeft(2, '0')}:{a?.Minutes.ToString().PadLeft(2, '0')}:{a?.Seconds.ToString().PadLeft(2, '0')}")));
        //    Assert.Equal("11:00:00, 00:00:10, 00:01:00", string.Join(", ", item3NP.testFieldTimeOnlyArray.Select(a => $"{a.Hour.ToString().PadLeft(2, '0')}:{a.Minute.ToString().PadLeft(2, '0')}:{a.Second.ToString().PadLeft(2, '0')}")));
        //    Assert.Equal("11:00:00, 00:00:10, 00:00:00, 00:01:00", string.Join(", ", item3NP.testFieldTimeOnlyArrayNullable.Select(a => $"{a?.Hour.ToString().PadLeft(2, '0')}:{a?.Minute.ToString().PadLeft(2, '0')}:{a?.Second.ToString().PadLeft(2, '0')}")));


        //    var items = fsql.Select<test_BasicListTypeCrud>().ToList();
        //    var itemstb = fsql.Select<test_BasicListTypeCrud>().ToDataTable();
        //}
        //class test_BasicListTypeCrud
        //{
        //    [Column(IsIdentity = true, IsPrimary = true)]
        //    public int Id { get; set; }

        //    public List<sbyte> testFieldSByteArray { get; set; }
        //    public List<short> testFieldShortArray { get; set; }
        //    public List<int> testFieldIntArray { get; set; }
        //    public List<long> testFieldLongArray { get; set; }
        //    public List<ushort> testFieldUShortArray { get; set; }
        //    public List<uint> testFieldUIntArray { get; set; }
        //    public List<ulong> testFieldULongArray { get; set; }
        //    public List<double> testFieldDoubleArray { get; set; }
        //    public List<float> testFieldFloatArray { get; set; }
        //    public List<decimal> testFieldDecimalArray { get; set; }

        //    public List<sbyte?> testFieldSByteArrayNullable { get; set; }
        //    public List<short?> testFieldShortArrayNullable { get; set; }
        //    public List<int?> testFieldIntArrayNullable { get; set; }
        //    public List<long?> testFieldLongArrayNullable { get; set; }
        //    public List<byte?> testFieldByteArrayNullable { get; set; }
        //    public List<ushort?> testFieldUShortArrayNullable { get; set; }
        //    public List<uint?> testFieldUIntArrayNullable { get; set; }
        //    public List<ulong?> testFieldULongArrayNullable { get; set; }
        //    public List<double?> testFieldDoubleArrayNullable { get; set; }
        //    public List<float?> testFieldFloatArrayNullable { get; set; }
        //    public List<decimal?> testFieldDecimalArrayNullable { get; set; }

        //    public List<bool> testFieldBoolArray { get; set; }
        //    public List<byte[]> testFieldBytesArray { get; set; }
        //    public List<string> testFieldStringArray { get; set; }
        //    public List<Guid> testFieldGuidArray { get; set; }

        //    public List<bool?> testFieldBoolArrayNullable { get; set; }
        //    public List<Guid?> testFieldGuidArrayNullable { get; set; }

        //    public List<TableAllTypeEnumType1> testFieldEnum1Array { get; set; }
        //    public List<TableAllTypeEnumType1?> testFieldEnum1ArrayNullable { get; set; }
        //    public List<TableAllTypeEnumType2> testFieldEnum2Array { get; set; }
        //    public List<TableAllTypeEnumType2?> testFieldEnum2ArrayNullable { get; set; }

        //    public List<TimeSpan> testFieldTimeSpanArray { get; set; }
        //    public List<TimeOnly> testFieldTimeOnlyArray { get; set; }
        //    public List<DateTime> testFieldDateTimeArray { get; set; }
        //    public List<DateOnly> testFieldDateOnlyArray { get; set; }

        //    public List<TimeSpan?> testFieldTimeSpanArrayNullable { get; set; }
        //    public List<TimeOnly?> testFieldTimeOnlyArrayNullable { get; set; }
        //    public List<DateTime?> testFieldDateTimeArrayNullable { get; set; }
        //    public List<DateOnly?> testFieldDateOnlyArrayNullable { get; set; }

        //}
        #endregion

        IInsert<TableAllType> insert => fsql.Insert<TableAllType>();
        ISelect<TableAllType> select => fsql.Select<TableAllType>();

        [Fact]
        public void CurdAllField()
        {
            var sql1 = select.Where(a => a.testFieldIntArray.Contains(1)).ToSql();
            Assert.Equal(sql1, "");
            var lst1 = select.Where(a => a.testFieldIntArray.Contains(1)).ToList();

            var item = new TableAllType { };
            item.Id = (int)insert.AppendData(item).ExecuteIdentity();

            var newitem = select.Where(a => a.Id == item.Id).ToOne();

            var item2 = new TableAllType
            {
                testFieldBitArray = new BitArray(Encoding.UTF8.GetBytes("我是")),
                testFieldBitArrayArray = new[] { new BitArray(Encoding.UTF8.GetBytes("中国")), new BitArray(Encoding.UTF8.GetBytes("公民")) },
                testFieldBool = true,
                testFieldBoolArray = new[] { true, true, false, false },
                testFieldBoolArrayNullable = new bool?[] { true, true, null, false, false },
                testFieldBoolNullable = true,
                testFieldByte = byte.MaxValue,
                testFieldByteArrayNullable = new byte?[] { 0, 1, 2, 3, null, 4, 5, 6 },
                testFieldByteNullable = byte.MinValue,
                testFieldBytes = Encoding.UTF8.GetBytes("我是中国人"),
                testFieldBytesArray = new[] { Encoding.UTF8.GetBytes("我是中国人"), Encoding.UTF8.GetBytes("我是中国人") },
                testFieldDateTime = DateTime.Now,
                testFieldDateTimeArray = new[] { DateTime.Now, DateTime.Now.AddHours(2) },
                testFieldDateTimeArrayNullable = new DateTime?[] { DateTime.Now, null, DateTime.Now.AddHours(2) },
                testFieldDateTimeNullable = DateTime.Now.AddDays(-1),
                testFieldDateOnly = DateOnly.FromDateTime(DateTime.Now),
                testFieldDateOnlyArray = new[] { DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now.AddHours(2)) },
                testFieldDateOnlyArrayNullable = new DateOnly?[] { DateOnly.FromDateTime(DateTime.Now), null, DateOnly.FromDateTime(DateTime.Now.AddHours(2)) },
                testFieldDateOnlyNullable = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                testFieldDecimal = 999.99M,
                testFieldDecimalArray = new[] { 999.91M, 999.92M, 999.93M },
                testFieldDecimalArrayNullable = new decimal?[] { 998.11M, 998.12M, 998.13M },
                testFieldDecimalNullable = 111.11M,
                testFieldDouble = 888.88,
                testFieldDoubleArray = new[] { 888.81, 888.82, 888.83 },
                testFieldDoubleArrayNullable = new double?[] { 888.11, 888.12, null, 888.13 },
                testFieldDoubleNullable = 222.22,
                testFieldEnum1 = TableAllTypeEnumType1.e3,
                testFieldEnum1Array = new[] { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, TableAllTypeEnumType1.e1 },
                testFieldEnum1ArrayNullable = new TableAllTypeEnumType1?[] { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, null, TableAllTypeEnumType1.e1 },
                testFieldEnum1Nullable = TableAllTypeEnumType1.e2,
                testFieldEnum2 = TableAllTypeEnumType2.f2,
                testFieldEnum2Array = new[] { TableAllTypeEnumType2.f3, TableAllTypeEnumType2.f1 },
                testFieldEnum2ArrayNullable = new TableAllTypeEnumType2?[] { TableAllTypeEnumType2.f3, null, TableAllTypeEnumType2.f1 },
                testFieldEnum2Nullable = TableAllTypeEnumType2.f3,
                testFieldFloat = 777.77F,
                testFieldFloatArray = new[] { 777.71F, 777.72F, 777.73F },
                testFieldFloatArrayNullable = new float?[] { 777.71F, 777.72F, null, 777.73F },
                testFieldFloatNullable = 333.33F,
                testFieldGuid = Guid.NewGuid(),
                testFieldGuidArray = new[] { Guid.NewGuid(), Guid.NewGuid() },
                testFieldGuidArrayNullable = new Guid?[] { Guid.NewGuid(), null, Guid.NewGuid() },
                testFieldGuidNullable = Guid.NewGuid(),
                testFieldStruct = new Dictionary<string, object> { { "111", "value111" }, { "222", 222 }, { "333", "value333" } },
                testFieldStructArray = new[] { new Dictionary<string, object> { { "111", "value111" }, { "222", 222 }, { "333", "value333" } }, new Dictionary<string, object> { { "444", "value444" }, { "555", 555 }, { "666", "value666" } } },
                testFieldInt = int.MaxValue,
                testFieldIntArray = new[] { 1, 2, 3, 4, 5 },
                testFieldIntArrayNullable = new int?[] { 1, 2, 3, null, 4, 5 },
                testFieldIntNullable = int.MinValue,
                testFieldLong = long.MaxValue,
                testFieldLongArray = new long[] { 10, 20, 30, 40, 50 },
                
                testFieldSByte = sbyte.MaxValue,
                testFieldSByteArray = new sbyte[] { 1, 2, 3, 4, 5 },
                testFieldSByteArrayNullable = new sbyte?[] { 1, 2, 3, null, 4, 5 },
                testFieldSByteNullable = sbyte.MinValue,
                testFieldShort = short.MaxValue,
                testFieldShortArray = new short[] { 1, 2, 3, 4, 5 },
                testFieldShortArrayNullable = new short?[] { 1, 2, 3, null, 4, 5 },
                testFieldShortNullable = short.MinValue,
                testFieldString = "我是中国人string'\\?!@#$%^&*()_+{}}{~?><<>",
                testFieldChar = 'X',
                testFieldStringArray = new[] { "我是中国人String1", "我是中国人String2", null, "我是中国人String3" },
                testFieldTimeSpan = TimeSpan.FromHours(11),
                testFieldTimeSpanArray = new[] { TimeSpan.FromHours(11), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60) },
                testFieldTimeSpanArrayNullable = new TimeSpan?[] { TimeSpan.FromHours(11), TimeSpan.FromSeconds(10), null, TimeSpan.FromSeconds(60) },
                testFieldTimeSpanNullable = TimeSpan.FromSeconds(90),
                testFieldTimeOnly = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)),
                testFieldTimeOnlyArray = new[] { TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(10)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(60)) },
                testFieldTimeOnlyArrayNullable = new TimeOnly?[] { TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(10)), null, TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(60)) },
                testFieldTimeOnlyNullable = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(90)),
                testFieldUInt = uint.MaxValue,
                testFieldUIntArray = new uint[] { 1, 2, 3, 4, 5 },
                testFieldUIntArrayNullable = new uint?[] { 1, 2, 3, null, 4, 5 },
                testFieldUIntNullable = uint.MinValue,
                testFieldULong = ulong.MaxValue,
                testFieldULongArray = new ulong[] { 10, 20, 30, 40, 50 },
                testFieldULongArrayNullable = new ulong?[] { 10, 20, 30, null, 40, 50 },
                testFieldULongNullable = ulong.MinValue,
                testFieldUShort = ushort.MaxValue,
                testFieldUShortArray = new ushort[] { 11, 12, 13, 14, 15 },
                testFieldUShortArrayNullable = new ushort?[] { 11, 12, 13, null, 14, 15 },
                testFieldUShortNullable = ushort.MinValue,
                testFielLongArrayNullable = new long?[] { 500, 600, 700, null, 999, 1000 },
                testFielLongNullable = long.MinValue
            };

            var sqlText = insert.AppendData(item2).ToSql();
            var item3NP = insert.AppendData(item2).ExecuteInserted();

            var item3 = insert.AppendData(item2).ExecuteInserted().First();
            var newitem2 = select.Where(a => a.Id == item3.Id).ToOne();
            Assert.Equal(item2.testFieldString, newitem2.testFieldString);
            Assert.Equal(item2.testFieldChar, newitem2.testFieldChar);

            item3 = insert.NoneParameter().AppendData(item2).ExecuteInserted().First();
            newitem2 = select.Where(a => a.Id == item3.Id).ToOne();
            Assert.Equal(item2.testFieldString, newitem2.testFieldString);
            Assert.Equal(item2.testFieldChar, newitem2.testFieldChar);

            var items = select.ToList();
            var itemstb = select.ToDataTable();
        }

        [Table(Name = "tb_alltype")]
        class TableAllType
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public bool testFieldBool { get; set; }
            public sbyte testFieldSByte { get; set; }
            public short testFieldShort { get; set; }
            public int testFieldInt { get; set; }
            public long testFieldLong { get; set; }
            public byte testFieldByte { get; set; }
            public ushort testFieldUShort { get; set; }
            public uint testFieldUInt { get; set; }
            public ulong testFieldULong { get; set; }
            public double testFieldDouble { get; set; }
            public float testFieldFloat { get; set; }
            public decimal testFieldDecimal { get; set; }
            public TimeSpan testFieldTimeSpan { get; set; }
            public TimeOnly testFieldTimeOnly{ get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime testFieldDateTime { get; set; }
            public DateOnly testFieldDateOnly { get; set; }

            public byte[] testFieldBytes { get; set; }
            public string testFieldString { get; set; }
            public char testFieldChar { get; set; }
            public Guid testFieldGuid { get; set; }

            public bool? testFieldBoolNullable { get; set; }
            public sbyte? testFieldSByteNullable { get; set; }
            public short? testFieldShortNullable { get; set; }
            public int? testFieldIntNullable { get; set; }
            public long? testFielLongNullable { get; set; }
            public byte? testFieldByteNullable { get; set; }
            public ushort? testFieldUShortNullable { get; set; }
            public uint? testFieldUIntNullable { get; set; }
            public ulong? testFieldULongNullable { get; set; }
            public double? testFieldDoubleNullable { get; set; }
            public float? testFieldFloatNullable { get; set; }
            public decimal? testFieldDecimalNullable { get; set; }
            public TimeSpan? testFieldTimeSpanNullable { get; set; }
            public TimeOnly? testFieldTimeOnlyNullable { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime? testFieldDateTimeNullable { get; set; }
            public DateOnly? testFieldDateOnlyNullable { get; set; }

            public Guid? testFieldGuidNullable { get; set; }

            public BitArray testFieldBitArray { get; set; }
            public Dictionary<string, object> testFieldStruct { get; set; }

            public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
            public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
            public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
            public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }

            /* array */
            public bool[] testFieldBoolArray { get; set; }
            public sbyte[] testFieldSByteArray { get; set; }
            public short[] testFieldShortArray { get; set; }
            public int[] testFieldIntArray { get; set; }
            public long[] testFieldLongArray { get; set; }
            public ushort[] testFieldUShortArray { get; set; }
            public uint[] testFieldUIntArray { get; set; }
            public ulong[] testFieldULongArray { get; set; }
            public double[] testFieldDoubleArray { get; set; }
            public float[] testFieldFloatArray { get; set; }
            public decimal[] testFieldDecimalArray { get; set; }
            public TimeSpan[] testFieldTimeSpanArray { get; set; }
            public TimeOnly[] testFieldTimeOnlyArray { get; set; }
            public DateTime[] testFieldDateTimeArray { get; set; }
            public DateOnly[] testFieldDateOnlyArray { get; set; }
            public byte[][] testFieldBytesArray { get; set; }
            public string[] testFieldStringArray { get; set; }
            public Guid[] testFieldGuidArray { get; set; }

            public bool?[] testFieldBoolArrayNullable { get; set; }
            public sbyte?[] testFieldSByteArrayNullable { get; set; }
            public short?[] testFieldShortArrayNullable { get; set; }
            public int?[] testFieldIntArrayNullable { get; set; }
            public long?[] testFielLongArrayNullable { get; set; }
            public byte?[] testFieldByteArrayNullable { get; set; }
            public ushort?[] testFieldUShortArrayNullable { get; set; }
            public uint?[] testFieldUIntArrayNullable { get; set; }
            public ulong?[] testFieldULongArrayNullable { get; set; }
            public double?[] testFieldDoubleArrayNullable { get; set; }
            public float?[] testFieldFloatArrayNullable { get; set; }
            public decimal?[] testFieldDecimalArrayNullable { get; set; }
            public TimeSpan?[] testFieldTimeSpanArrayNullable { get; set; }
            public TimeOnly?[] testFieldTimeOnlyArrayNullable { get; set; }
            public DateTime?[] testFieldDateTimeArrayNullable { get; set; }
            public DateOnly?[] testFieldDateOnlyArrayNullable { get; set; }
            public Guid?[] testFieldGuidArrayNullable { get; set; }

            public BitArray[] testFieldBitArrayArray { get; set; }
            public Dictionary<string, object>[] testFieldStructArray { get; set; }

            public TableAllTypeEnumType1[] testFieldEnum1Array { get; set; }
            public TableAllTypeEnumType1?[] testFieldEnum1ArrayNullable { get; set; }
            public TableAllTypeEnumType2[] testFieldEnum2Array { get; set; }
            public TableAllTypeEnumType2?[] testFieldEnum2ArrayNullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
