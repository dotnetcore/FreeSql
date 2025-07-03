using FreeSql.DataAnnotations;
using FreeSql.Internal.CommonProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.SqliteExpression
{
    public class OtherTest
    {

        ISelect<TableAllType> select => g.sqlite.Select<TableAllType>();

        public OtherTest()
        {
        }

        [Fact]
        public void ArrayAny()
        {
            var fsql = g.sqlite;
            fsql.Delete<ArrayAny01>().Where("1=1").ExecuteAffrows();

            var t1 = fsql.Select<ArrayAny01>().Where(a => new[] {
                new ArrayAny02 { Name1 = "name01", Click1 = 1 },
            }.Any(b => b.Name1 == a.Name && b.Click1 == a.Click || a.Click > 10)).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Name"", a.""Click"" 
FROM ""ArrayAny01"" a 
WHERE ((('name01' = a.""Name"" AND 1 = a.""Click"" OR a.""Click"" > 10)))", t1);

            var t2 = fsql.Select<ArrayAny01>().Where(a => new[] {
                new ArrayAny02 { Name1 = "name01", Click1 = 1 },
                new ArrayAny02 { Name1 = "name02", Click1 = 2 },
            }.Any(b => b.Name1 == a.Name && b.Click1 == a.Click || a.Click > 10)).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Name"", a.""Click"" 
FROM ""ArrayAny01"" a 
WHERE ((('name01' = a.""Name"" AND 1 = a.""Click"" OR a.""Click"" > 10) OR ('name02' = a.""Name"" AND 2 = a.""Click"" OR a.""Click"" > 10)))", t2);

            var aa03 = new[] {
                new ArrayAny02 { Name1 = "name01", Click1 = 1 },
            };
            var t3 = fsql.Select<ArrayAny01>().Where(a => aa03.Any(b => b.Name1 == a.Name && b.Click1 == a.Click || a.Click > 10)).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Name"", a.""Click"" 
FROM ""ArrayAny01"" a 
WHERE ((('name01' = a.""Name"" AND 1 = a.""Click"" OR a.""Click"" > 10)))", t3);

            var aa04 = new[] {
                new ArrayAny02 { Name1 = "name01", Click1 = 1 },
                new ArrayAny02 { Name1 = "name02", Click1 = 2 },
            };
            var t4 = fsql.Select<ArrayAny01>().Where(a => aa04.Any(b => b.Name1 == a.Name && b.Click1 == a.Click || a.Click > 10)).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Name"", a.""Click"" 
FROM ""ArrayAny01"" a 
WHERE ((('name01' = a.""Name"" AND 1 = a.""Click"" OR a.""Click"" > 10) OR ('name02' = a.""Name"" AND 2 = a.""Click"" OR a.""Click"" > 10)))", t4);

            // List

            var aa05 = new List<ArrayAny02> {
                new ArrayAny02 { Name1 = "name01", Click1 = 1 },
            };
            var t5 = fsql.Select<ArrayAny01>().Where(a => aa05.Any(b => b.Name1 == a.Name && b.Click1 == a.Click || a.Click > 10)).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Name"", a.""Click"" 
FROM ""ArrayAny01"" a 
WHERE ((('name01' = a.""Name"" AND 1 = a.""Click"" OR a.""Click"" > 10)))", t5);

            var aa06 = new List<ArrayAny02> {
                new ArrayAny02 { Name1 = "name01", Click1 = 1 },
                new ArrayAny02 { Name1 = "name02", Click1 = 2 },
            };
            var t6 = fsql.Select<ArrayAny01>().Where(a => aa06.Any(b => b.Name1 == a.Name && b.Click1 == a.Click || a.Click > 10)).ToSql();
            Assert.Equal(@"SELECT a.""Id"", a.""Name"", a.""Click"" 
FROM ""ArrayAny01"" a 
WHERE ((('name01' = a.""Name"" AND 1 = a.""Click"" OR a.""Click"" > 10) OR ('name02' = a.""Name"" AND 2 = a.""Click"" OR a.""Click"" > 10)))", t6);
        }
        class ArrayAny01
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public long Click { get; set; }
        }
        class ArrayAny02
        {
            public string Name1 { get; set; }
            public long Click1 { get; set; }
        }

        [Fact]
        public void Div()
        {
            var t1 = select.Where(a => a.Int / 3 > 3).Limit(10).ToList();
            var t2 = select.Where(a => a.Long / 3 > 3).Limit(10).ToList();
            var t3 = select.Where(a => a.Short / 3 > 3).Limit(10).ToList();

            var t4 = select.Where(a => a.Int / 3.0 > 3).Limit(10).ToList();
            var t5 = select.Where(a => a.Long / 3.0 > 3).Limit(10).ToList();
            var t6 = select.Where(a => a.Short / 3.0 > 3).Limit(10).ToList();

            var t7 = select.Where(a => a.Double / 3 > 3).Limit(10).ToList();
            var t8 = select.Where(a => a.Decimal / 3 > 3).Limit(10).ToList();
            var t9 = select.Where(a => a.Float / 3 > 3).Limit(10).ToList();
        }

        [Fact]
        public void Boolean()
        {
            var t1 = select.Where(a => a.Bool == true).ToList();
            var t2 = select.Where(a => a.Bool != true).ToList();
            var t3 = select.Where(a => a.Bool == false).ToList();
            var t4 = select.Where(a => !a.Bool).ToList();
            var t5 = select.Where(a => a.Bool).ToList();
            var t51 = select.WhereCascade(a => a.Bool).Limit(10).ToList();

            var t11 = select.Where(a => a.BoolNullable == true).ToList();
            var t22 = select.Where(a => a.BoolNullable != true).ToList();
            var t33 = select.Where(a => a.BoolNullable == false).ToList();
            var t44 = select.Where(a => !a.BoolNullable.Value).ToList();
            var t55 = select.Where(a => a.BoolNullable.Value).ToList();

            var t111 = select.Where(a => a.Bool == true && a.Id > 0).ToList();
            var t222 = select.Where(a => a.Bool != true && a.Id > 0).ToList();
            var t333 = select.Where(a => a.Bool == false && a.Id > 0).ToList();
            var t444 = select.Where(a => !a.Bool && a.Id > 0).ToList();
            var t555 = select.Where(a => a.Bool && a.Id > 0).ToList();

            var t1111 = select.Where(a => a.BoolNullable == true && a.Id > 0).ToList();
            var t2222 = select.Where(a => a.BoolNullable != true && a.Id > 0).ToList();
            var t3333 = select.Where(a => a.BoolNullable == false && a.Id > 0).ToList();
            var t4444 = select.Where(a => !a.BoolNullable.Value && a.Id > 0).ToList();
            var t5555 = select.Where(a => a.BoolNullable.Value && a.Id > 0).ToList();

            var t11111 = select.Where(a => a.Bool == true && a.Id > 0 && a.Bool == true).ToList();
            var t22222 = select.Where(a => a.Bool != true && a.Id > 0 && a.Bool != true).ToList();
            var t33333 = select.Where(a => a.Bool == false && a.Id > 0 && a.Bool == false).ToList();
            var t44444 = select.Where(a => !a.Bool && a.Id > 0 && !a.Bool).ToList();
            var t55555 = select.Where(a => a.Bool && a.Id > 0 && a.Bool).ToList();

            var t111111 = select.Where(a => a.BoolNullable == true && a.Id > 0 && a.BoolNullable == true).ToList();
            var t222222 = select.Where(a => a.BoolNullable != true && a.Id > 0 && a.BoolNullable != true).ToList();
            var t333333 = select.Where(a => a.BoolNullable == false && a.Id > 0 && a.BoolNullable == false).ToList();
            var t444444 = select.Where(a => !a.BoolNullable.Value && a.Id > 0 && !a.BoolNullable.Value).ToList();
            var t555555 = select.Where(a => a.BoolNullable.Value && a.Id > 0 && a.BoolNullable.Value).ToList();
        }

        [Fact]
        public void Array()
        {
            IEnumerable<int> testlinqlist = new List<int>(new[] { 1, 2, 3 });
            var testlinq = select.Where(a => testlinqlist.Contains(a.Int)).ToList();
            var testlinq2list = new string[] { };
            var testlinq2 = g.sqlite.Delete<TableAllType>().Where(a => testlinq2list.Contains(a.String)).ToSql();
            Assert.Equal("DELETE FROM \"tb_alltype\" WHERE (((\"String\") in (NULL)))", testlinq2);

            //in not in
            var sql111 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.Int)).ToList();
            var sql112 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.Int) == false).ToList();
            var sql113 = select.Where(a => !new[] { 1, 2, 3 }.Contains(a.Int)).ToList();

            var inarray = new[] { 1, 2, 3 };
            var sql1111 = select.Where(a => inarray.Contains(a.Int)).ToList();
            var sql1122 = select.Where(a => inarray.Contains(a.Int) == false).ToList();
            var sql1133 = select.Where(a => !inarray.Contains(a.Int)).ToList();

            //in not in
            var sql11111 = select.Where(a => new List<int>() { 1, 2, 3 }.Contains(a.Int)).ToList();
            var sql11222 = select.Where(a => new List<int>() { 1, 2, 3 }.Contains(a.Int) == false).ToList();
            var sql11333 = select.Where(a => !new List<int>() { 1, 2, 3 }.Contains(a.Int)).ToList();

            var sql11111a = select.Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.Int)).ToList();
            var sql11222b = select.Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.Int) == false).ToList();
            var sql11333c = select.Where(a => !new List<int>(new[] { 1, 2, 3 }).Contains(a.Int)).ToList();

            var inarray2 = new List<int>() { 1, 2, 3 };
            var sql111111 = select.Where(a => inarray.Contains(a.Int)).ToList();
            var sql112222 = select.Where(a => inarray.Contains(a.Int) == false).ToList();
            var sql113333 = select.Where(a => !inarray.Contains(a.Int)).ToList();

            var inarray2n = Enumerable.Range(1, 3333).ToArray();
            var sql1111111 = select.Where(a => inarray2n.Contains(a.Int)).ToList();
            var sql1122222 = select.Where(a => inarray2n.Contains(a.Int) == false).ToList();
            var sql1133333 = select.Where(a => !inarray2n.Contains(a.Int)).ToList();
        }

        [Fact]
        public void SubSelectUseGenerateCommandParameterWithLambda()
        {
            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "data source=:memory:")
                .UseConnectionString(DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=issues684;Pooling=true;Max Pool Size=3;TrustServerCertificate=true")
                .UseGenerateCommandParameterWithLambda(true)
                .UseAutoSyncStructure(true)
                .UseMonitorCommand(null, (cmd, log) => Trace.WriteLine(log))
                .Build())
            {
                var guidval = Guid.NewGuid();
                var strval = "nameval";
                var timeval = DateTime.Now;
                var decval1 = 1.1M;
                var decval2 = 2.2M;

                var subselect = fsql.Select<ssugcpwl01>();
                var sql = subselect.ToSql(a => new
                {
                    a.id, a.name, a.createTime,
                    sum1 = fsql.Select<TableAllType>().Where(b => b.Guid == guidval).Sum(b => b.Int),
                    sum2 = fsql.Select<TableAllType>().Where(b => b.String == strval).Sum(b => b.Long),
                    sum3 = fsql.Select<TableAllType>().Where(b => b.DateTime == timeval).Sum(b => b.Decimal),
                    sum4 = fsql.Select<TableAllType>().Where(b => b.Decimal == decval1).Sum(b => b.Decimal),
                    sum5 = fsql.Select<TableAllType>().Where(b => b.Decimal == decval2).Sum(b => b.Decimal),
                });
                var subselect0 = subselect as Select0Provider;
                Assert.Equal(5, subselect0._params.Count);
                Assert.Equal("@exp_0", subselect0._params[0].ParameterName);
                Assert.Equal("@exp_1", subselect0._params[1].ParameterName);
                Assert.Equal("@exp_2", subselect0._params[2].ParameterName);
                Assert.Equal("@exp_3", subselect0._params[3].ParameterName);
                Assert.Equal("@exp_4", subselect0._params[4].ParameterName);
                Assert.Equal(@"SELECT a.[id] as1, a.[name] as2, a.[createTime] as3, isnull((SELECT sum(b.[Int]) 
    FROM [tb_alltype] b 
    WHERE (b.[Guid] = @exp_0)), 0) as4, isnull((SELECT sum(b.[Long]) 
    FROM [tb_alltype] b 
    WHERE (b.[String] = @exp_1)), 0) as5, isnull((SELECT sum(b.[Decimal]) 
    FROM [tb_alltype] b 
    WHERE (b.[DateTime] = @exp_2)), 0) as6, isnull((SELECT sum(b.[Decimal]) 
    FROM [tb_alltype] b 
    WHERE (b.[Decimal] = @exp_3)), 0) as7, isnull((SELECT sum(b.[Decimal]) 
    FROM [tb_alltype] b 
    WHERE (b.[Decimal] = @exp_4)), 0) as8 
FROM [ssugcpwl01] a", sql);

                var groupselect = fsql.Select<ssugcpwl01>().GroupBy(a => a.name);
                sql = groupselect.ToSql(a => new
                {
                    a.Key,
                    sum1 = fsql.Select<TableAllType>().Where(b => b.Guid == guidval).Sum(b => b.Int),
                    sum2 = fsql.Select<TableAllType>().Where(b => b.String == strval).Sum(b => b.Long),
                    sum3 = fsql.Select<TableAllType>().Where(b => b.DateTime == timeval).Sum(b => b.Decimal),
                    sum4 = fsql.Select<TableAllType>().Where(b => b.Decimal == decval1).Sum(b => b.Decimal),
                    sum5 = fsql.Select<TableAllType>().Where(b => b.Decimal == decval2).Sum(b => b.Decimal),
                });
                var groupselect0 = groupselect as SelectGroupingProvider;
                Assert.Equal(5, groupselect0._select._params.Count);
                Assert.Equal("@exp_0", groupselect0._select._params[0].ParameterName);
                Assert.Equal("@exp_1", groupselect0._select._params[1].ParameterName);
                Assert.Equal("@exp_2", groupselect0._select._params[2].ParameterName);
                Assert.Equal("@exp_3", groupselect0._select._params[3].ParameterName);
                Assert.Equal("@exp_4", groupselect0._select._params[4].ParameterName);
                Assert.Equal(@"SELECT a.[name] as1, isnull((SELECT sum(b.[Int]) 
    FROM [tb_alltype] b 
    WHERE (b.[Guid] = @exp_0)), 0) as2, isnull((SELECT sum(b.[Long]) 
    FROM [tb_alltype] b 
    WHERE (b.[String] = @exp_1)), 0) as3, isnull((SELECT sum(b.[Decimal]) 
    FROM [tb_alltype] b 
    WHERE (b.[DateTime] = @exp_2)), 0) as4, isnull((SELECT sum(b.[Decimal]) 
    FROM [tb_alltype] b 
    WHERE (b.[Decimal] = @exp_3)), 0) as5, isnull((SELECT sum(b.[Decimal]) 
    FROM [tb_alltype] b 
    WHERE (b.[Decimal] = @exp_4)), 0) as6 
FROM [ssugcpwl01] a 
GROUP BY a.[name]", sql);
            }
        }
        class ssugcpwl01
        {
            public Guid id { get; set; }
            public string name { get; set; }
            public DateTime createTime { get; set; }
        }
        [Fact]
        public void ArrayUseGenerateCommandParameterWithLambda()
        {
            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "data source=:memory:")
                .UseGenerateCommandParameterWithLambda(true)
                .UseAutoSyncStructure(true)
                .UseMonitorCommand(null, (cmd, log) => Trace.WriteLine(log))
                .Build())
            {
                var arr = new[] { 1L, 2L, 3L }.Select(x => x);
                var ids = arr.Select(x => x);
                var sql001 = fsql.Select<TableAllType>().Where(x => ids.Contains(x.Id)).ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""id2"", a.""Bool"", a.""SByte"", a.""Short"", a.""Int"", a.""Long"", a.""Byte"", a.""UShort"", a.""UInt"", a.""ULong"", a.""Double"", a.""Float"", a.""Decimal"", a.""TimeSpan"", a.""DateTime"", a.""DateTimeOffSet"", a.""Bytes"", a.""String"", a.""Guid"", a.""BoolNullable"", a.""SByteNullable"", a.""ShortNullable"", a.""IntNullable"", a.""testFielLongNullable"", a.""ByteNullable"", a.""UShortNullable"", a.""UIntNullable"", a.""ULongNullable"", a.""DoubleNullable"", a.""FloatNullable"", a.""DecimalNullable"", a.""TimeSpanNullable"", a.""DateTimeNullable"", a.""DateTimeOffSetNullable"", a.""GuidNullable"", a.""Enum1"", a.""Enum1Nullable"", a.""Enum2"", a.""Enum2Nullable"" 
FROM ""tb_alltype"" a 
WHERE (((a.""Id"") in (1,2,3)))", sql001);

                IEnumerable<int> testlinqlist = new List<int>(new[] { 1, 2, 3 });
                var testlinq = fsql.Select<TableAllType>().Where(a => testlinqlist.Contains(a.Int)).ToList();
                var testlinq2list = new string[] { };
                var testlinq2 = g.sqlite.Delete<TableAllType>().Where(a => testlinq2list.Contains(a.String)).ToSql();
                Assert.Equal("DELETE FROM \"tb_alltype\" WHERE (((\"String\") in (NULL)))", testlinq2);

                //in not in
                var sql111 = fsql.Select<TableAllType>().Where(a => new[] { 1, 2, 3 }.Contains(a.Int)).ToList();
                var sql112 = fsql.Select<TableAllType>().Where(a => new[] { 1, 2, 3 }.Contains(a.Int) == false).ToList();
                var sql113 = fsql.Select<TableAllType>().Where(a => !new[] { 1, 2, 3 }.Contains(a.Int)).ToList();

                var inarray = new[] { 1, 2, 3 };
                var sql1111 = fsql.Select<TableAllType>().Where(a => inarray.Contains(a.Int)).ToList();
                var sql1122 = fsql.Select<TableAllType>().Where(a => inarray.Contains(a.Int) == false).ToList();
                var sql1133 = fsql.Select<TableAllType>().Where(a => !inarray.Contains(a.Int)).ToList();

                //in not in
                var sql11111 = fsql.Select<TableAllType>().Where(a => new List<int>() { 1, 2, 3 }.Contains(a.Int)).ToList();
                var sql11222 = fsql.Select<TableAllType>().Where(a => new List<int>() { 1, 2, 3 }.Contains(a.Int) == false).ToList();
                var sql11333 = fsql.Select<TableAllType>().Where(a => !new List<int>() { 1, 2, 3 }.Contains(a.Int)).ToList();

                var sql11111a = fsql.Select<TableAllType>().Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.Int)).ToList();
                var sql11222b = fsql.Select<TableAllType>().Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.Int) == false).ToList();
                var sql11333c = fsql.Select<TableAllType>().Where(a => !new List<int>(new[] { 1, 2, 3 }).Contains(a.Int)).ToList();

                var inarray2 = new List<int>() { 1, 2, 3 };
                var sql111111 = fsql.Select<TableAllType>().Where(a => inarray.Contains(a.Int)).ToList();
                var sql112222 = fsql.Select<TableAllType>().Where(a => inarray.Contains(a.Int) == false).ToList();
                var sql113333 = fsql.Select<TableAllType>().Where(a => !inarray.Contains(a.Int)).ToList();

                var inarray2n = Enumerable.Range(1, 3333).ToArray();
                var sql1111111 = fsql.Select<TableAllType>().Where(a => inarray2n.Contains(a.Int)).ToList();
                var sql1122222 = fsql.Select<TableAllType>().Where(a => inarray2n.Contains(a.Int) == false).ToList();
                var sql1133333 = fsql.Select<TableAllType>().Where(a => !inarray2n.Contains(a.Int)).ToList();
            }
        }

        [Table(Name = "tb_alltype")]
        class TableAllType
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public string id2 { get; set; } = "id2=10";

            public bool Bool { get; set; }
            public sbyte SByte { get; set; }
            public short Short { get; set; }
            public int Int { get; set; }
            public long Long { get; set; }
            public byte Byte { get; set; }
            public ushort UShort { get; set; }
            public uint UInt { get; set; }
            public ulong ULong { get; set; }
            public double Double { get; set; }
            public float Float { get; set; }
            public decimal Decimal { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public DateTime DateTime { get; set; }
            public DateTime DateTimeOffSet { get; set; }
            public byte[] Bytes { get; set; }
            public string String { get; set; }
            public Guid Guid { get; set; }

            public bool? BoolNullable { get; set; }
            public sbyte? SByteNullable { get; set; }
            public short? ShortNullable { get; set; }
            public int? IntNullable { get; set; }
            public long? testFielLongNullable { get; set; }
            public byte? ByteNullable { get; set; }
            public ushort? UShortNullable { get; set; }
            public uint? UIntNullable { get; set; }
            public ulong? ULongNullable { get; set; }
            public double? DoubleNullable { get; set; }
            public float? FloatNullable { get; set; }
            public decimal? DecimalNullable { get; set; }
            public TimeSpan? TimeSpanNullable { get; set; }
            public DateTime? DateTimeNullable { get; set; }
            public DateTime? DateTimeOffSetNullable { get; set; }
            public Guid? GuidNullable { get; set; }

            public TableAllTypeEnumType1 Enum1 { get; set; }
            public TableAllTypeEnumType1? Enum1Nullable { get; set; }
            public TableAllTypeEnumType2 Enum2 { get; set; }
            public TableAllTypeEnumType2? Enum2Nullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
