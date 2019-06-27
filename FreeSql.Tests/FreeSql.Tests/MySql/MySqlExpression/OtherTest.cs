using FreeSql.DataAnnotations;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MySqlExpression
{
    public class OtherTest
    {

        ISelect<TableAllType> select => g.mysql.Select<TableAllType>();

        public OtherTest()
        {

        }

        [Fact]
        public void Boolean()
        {
            var t1 = select.Where(a => a.testFieldBool == true).ToList();
            var t2 = select.Where(a => a.testFieldBool != true).ToList();
            var t3 = select.Where(a => a.testFieldBool == false).ToList();
            var t4 = select.Where(a => !a.testFieldBool).ToList();
            var t5 = select.Where(a => a.testFieldBool).ToList();

            var t11 = select.Where(a => a.testFieldBoolNullable == true).ToList();
            var t22 = select.Where(a => a.testFieldBoolNullable != true).ToList();
            var t33 = select.Where(a => a.testFieldBoolNullable == false).ToList();
            var t44 = select.Where(a => !a.testFieldBoolNullable.Value).ToList();
            var t55 = select.Where(a => a.testFieldBoolNullable.Value).ToList();
        }

        [Fact]
        public void Array()
        {
            int[] nullarr = null;
            Assert.Throws<MySqlException>(() => { select.Where(a => nullarr.Contains(a.testFieldInt)).ToList(); });
            Assert.Throws<MySqlException>(() => { select.Where(a => new int[0].Contains(a.testFieldInt)).ToList(); });

            IEnumerable<int> testlinqlist = new List<int>(new[] { 1, 2, 3 });
            var testlinq = select.Where(a => testlinqlist.Contains(a.testFieldInt)).ToList();

            //in not in
            var sql111 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();
            var sql112 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.testFieldInt) == false).ToList();
            var sql113 = select.Where(a => !new[] { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();

            var inarray = new[] { 1, 2, 3 };
            var sql1111 = select.Where(a => inarray.Contains(a.testFieldInt)).ToList();
            var sql1122 = select.Where(a => inarray.Contains(a.testFieldInt) == false).ToList();
            var sql1133 = select.Where(a => !inarray.Contains(a.testFieldInt)).ToList();

            //in not in
            var sql11111 = select.Where(a => new List<int>() { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();
            var sql11222 = select.Where(a => new List<int>() { 1, 2, 3 }.Contains(a.testFieldInt) == false).ToList();
            var sql11333 = select.Where(a => !new List<int>() { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();

            var sql11111a = select.Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.testFieldInt)).ToList();
            var sql11222b = select.Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.testFieldInt) == false).ToList();
            var sql11333c = select.Where(a => !new List<int>(new[] { 1, 2, 3 }).Contains(a.testFieldInt)).ToList();

            var inarray2 = new List<int>() { 1, 2, 3 };
            var sql111111 = select.Where(a => inarray.Contains(a.testFieldInt)).ToList();
            var sql112222 = select.Where(a => inarray.Contains(a.testFieldInt) == false).ToList();
            var sql113333 = select.Where(a => !inarray.Contains(a.testFieldInt)).ToList();
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
            public DateTime testFieldDateTime { get; set; }
            public byte[] testFieldBytes { get; set; }
            public string testFieldString { get; set; }
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
            public DateTime? testFieldDateTimeNullable { get; set; }
            public Guid? testFieldGuidNullable { get; set; }

            public MygisPoint testFieldPoint { get; set; }
            public MygisLineString testFieldLineString { get; set; }
            public MygisPolygon testFieldPolygon { get; set; }
            public MygisMultiPoint testFieldMultiPoint { get; set; }
            public MygisMultiLineString testFieldMultiLineString { get; set; }
            public MygisMultiPolygon testFieldMultiPolygon { get; set; }

            public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
            public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
            public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
            public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
