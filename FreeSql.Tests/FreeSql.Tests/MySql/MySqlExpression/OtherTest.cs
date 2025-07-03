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
        public void BitEnum()
        {
            var fsql = g.mysql;
            var sql1 = fsql.Select<BitEnum01>().Where(a => a.enum1 == TableAllTypeEnumType1.e5).ToSql();
            var enum1 = TableAllTypeEnumType1.e5;
            var sql2 = fsql.Select<BitEnum01>().Where(a => a.enum1 == enum1).ToSql();
            Assert.Equal(sql1, sql2);
            Assert.Equal(@"SELECT a.`id`, a.`enum1`, a.`enum2`, a.`set1`, a.`set2` 
FROM `BitEnum01` a 
WHERE (a.`enum1` = 'e5')", sql1);
            sql1 = fsql.Select<BitEnum01>().Where(a => a.enum2 == TableAllTypeEnumType1.e5).ToSql();
            sql2 = fsql.Select<BitEnum01>().Where(a => a.enum2 == enum1).ToSql();
            Assert.Equal(sql1, sql2);
            Assert.Equal(@"SELECT a.`id`, a.`enum1`, a.`enum2`, a.`set1`, a.`set2` 
FROM `BitEnum01` a 
WHERE (a.`enum2` = 3)", sql1);

            sql1 = fsql.Select<BitEnum01>().Where(a => (a.enum1 & TableAllTypeEnumType1.e2) == TableAllTypeEnumType1.e2).ToSql();
            enum1 = TableAllTypeEnumType1.e2;
            sql2 = fsql.Select<BitEnum01>().Where(a => (a.enum1 & enum1) == enum1).ToSql();
            Assert.Equal(sql1, sql2);
            Assert.Equal(@"SELECT a.`id`, a.`enum1`, a.`enum2`, a.`set1`, a.`set2` 
FROM `BitEnum01` a 
WHERE ((a.`enum1` & 'e2') = 'e2')", sql1);
            sql1 = fsql.Select<BitEnum01>().Where(a => (a.enum2 & TableAllTypeEnumType1.e2) == TableAllTypeEnumType1.e2).ToSql();
            enum1 = TableAllTypeEnumType1.e2;
            sql2 = fsql.Select<BitEnum01>().Where(a => (a.enum2 & enum1) == enum1).ToSql();
            Assert.Equal(sql1, sql2);
            Assert.Equal(@"SELECT a.`id`, a.`enum1`, a.`enum2`, a.`set1`, a.`set2` 
FROM `BitEnum01` a 
WHERE ((a.`enum2` & 1) = 1)", sql1);


            sql1 = fsql.Select<BitEnum01>().Where(a => a.set1 == TableAllTypeEnumType2.f3).ToSql();
            var set1 = TableAllTypeEnumType2.f3;
            sql2 = fsql.Select<BitEnum01>().Where(a => a.set1 == set1).ToSql();
            Assert.Equal(sql1, sql2);
            Assert.Equal(@"SELECT a.`id`, a.`enum1`, a.`enum2`, a.`set1`, a.`set2` 
FROM `BitEnum01` a 
WHERE (a.`set1` = 'f3')", sql1);
            sql1 = fsql.Select<BitEnum01>().Where(a => a.set2 == TableAllTypeEnumType2.f3).ToSql();
            sql2 = fsql.Select<BitEnum01>().Where(a => a.set2 == set1).ToSql();
            Assert.Equal(sql1, sql2);
            Assert.Equal(@"SELECT a.`id`, a.`enum1`, a.`enum2`, a.`set1`, a.`set2` 
FROM `BitEnum01` a 
WHERE (a.`set2` = 2)", sql1);

            sql1 = fsql.Select<BitEnum01>().Where(a => (a.set1 & TableAllTypeEnumType2.f2) == TableAllTypeEnumType2.f2).ToSql();
            set1 = TableAllTypeEnumType2.f2;
            sql2 = fsql.Select<BitEnum01>().Where(a => (a.set1 & set1) == set1).ToSql();
            Assert.Equal(sql1, sql2);
            Assert.Equal(@"SELECT a.`id`, a.`enum1`, a.`enum2`, a.`set1`, a.`set2` 
FROM `BitEnum01` a 
WHERE ((a.`set1` & 'f2') = 'f2')", sql1);
            sql1 = fsql.Select<BitEnum01>().Where(a => (a.set2 & TableAllTypeEnumType2.f2) == TableAllTypeEnumType2.f2).ToSql();
            set1 = TableAllTypeEnumType2.f2;
            sql2 = fsql.Select<BitEnum01>().Where(a => (a.set2 & set1) == set1).ToSql();
            Assert.Equal(sql1, sql2);
            Assert.Equal(@"SELECT a.`id`, a.`enum1`, a.`enum2`, a.`set1`, a.`set2` 
FROM `BitEnum01` a 
WHERE ((a.`set2` & 1) = 1)", sql1);
        }
        class BitEnum01
        {
            public int id { get; set; }
            public TableAllTypeEnumType1 enum1 { get; set; }
            [Column(MapType = typeof(int))]
            public TableAllTypeEnumType1 enum2 { get; set; }
            public TableAllTypeEnumType2 set1 { get; set; }
            [Column(MapType = typeof(long))]
            public TableAllTypeEnumType2 set2 { get; set; }
        }

        [Fact]
        public void ArrayAnyOr()
        {
            var arr = new int[] { 1, 3, 8 };
            var t1 = select.Where(a => arr.Any(x => a.testFieldInt == x)).ToSql();
            Assert.Equal(@"SELECT a.`Id`, a.`testFieldBool`, a.`testFieldSByte`, a.`testFieldShort`, a.`testFieldInt`, a.`testFieldLong`, a.`testFieldByte`, a.`testFieldUShort`, a.`testFieldUInt`, a.`testFieldULong`, a.`testFieldDouble`, a.`testFieldFloat`, a.`testFieldDecimal`, a.`testFieldTimeSpan`, a.`testFieldDateTime`, a.`testFieldBytes`, a.`testFieldString`, a.`testFieldGuid`, a.`testFieldBoolNullable`, a.`testFieldSByteNullable`, a.`testFieldShortNullable`, a.`testFieldIntNullable`, a.`testFielLongNullable`, a.`testFieldByteNullable`, a.`testFieldUShortNullable`, a.`testFieldUIntNullable`, a.`testFieldULongNullable`, a.`testFieldDoubleNullable`, a.`testFieldFloatNullable`, a.`testFieldDecimalNullable`, a.`testFieldTimeSpanNullable`, a.`testFieldDateTimeNullable`, a.`testFieldGuidNullable`, ST_AsText(a.`testFieldPoint`) `testFieldPoint`, ST_AsText(a.`testFieldLineString`) `testFieldLineString`, ST_AsText(a.`testFieldPolygon`) `testFieldPolygon`, ST_AsText(a.`testFieldMultiPoint`) `testFieldMultiPoint`, ST_AsText(a.`testFieldMultiLineString`) `testFieldMultiLineString`, ST_AsText(a.`testFieldMultiPolygon`) `testFieldMultiPolygon`, a.`testFieldEnum1`, a.`testFieldEnum1Nullable`, a.`testFieldEnum2`, a.`testFieldEnum2Nullable` 
FROM `tb_alltype` a 
WHERE ((a.`testFieldInt` = 1 OR a.`testFieldInt` = 3 OR a.`testFieldInt` = 8))", t1);
            var t2 = select.Where(a => a.Id == 100 && arr.Any(x => a.testFieldInt == x)).ToSql();
            Assert.Equal(@"SELECT a.`Id`, a.`testFieldBool`, a.`testFieldSByte`, a.`testFieldShort`, a.`testFieldInt`, a.`testFieldLong`, a.`testFieldByte`, a.`testFieldUShort`, a.`testFieldUInt`, a.`testFieldULong`, a.`testFieldDouble`, a.`testFieldFloat`, a.`testFieldDecimal`, a.`testFieldTimeSpan`, a.`testFieldDateTime`, a.`testFieldBytes`, a.`testFieldString`, a.`testFieldGuid`, a.`testFieldBoolNullable`, a.`testFieldSByteNullable`, a.`testFieldShortNullable`, a.`testFieldIntNullable`, a.`testFielLongNullable`, a.`testFieldByteNullable`, a.`testFieldUShortNullable`, a.`testFieldUIntNullable`, a.`testFieldULongNullable`, a.`testFieldDoubleNullable`, a.`testFieldFloatNullable`, a.`testFieldDecimalNullable`, a.`testFieldTimeSpanNullable`, a.`testFieldDateTimeNullable`, a.`testFieldGuidNullable`, ST_AsText(a.`testFieldPoint`) `testFieldPoint`, ST_AsText(a.`testFieldLineString`) `testFieldLineString`, ST_AsText(a.`testFieldPolygon`) `testFieldPolygon`, ST_AsText(a.`testFieldMultiPoint`) `testFieldMultiPoint`, ST_AsText(a.`testFieldMultiLineString`) `testFieldMultiLineString`, ST_AsText(a.`testFieldMultiPolygon`) `testFieldMultiPolygon`, a.`testFieldEnum1`, a.`testFieldEnum1Nullable`, a.`testFieldEnum2`, a.`testFieldEnum2Nullable` 
FROM `tb_alltype` a 
WHERE (a.`Id` = 100 AND (a.`testFieldInt` = 1 OR a.`testFieldInt` = 3 OR a.`testFieldInt` = 8))", t2);
            var t3 = select.Where(a => arr.Any(x => a.testFieldInt == x) && a.Id == 101).ToSql();
            Assert.Equal(@"SELECT a.`Id`, a.`testFieldBool`, a.`testFieldSByte`, a.`testFieldShort`, a.`testFieldInt`, a.`testFieldLong`, a.`testFieldByte`, a.`testFieldUShort`, a.`testFieldUInt`, a.`testFieldULong`, a.`testFieldDouble`, a.`testFieldFloat`, a.`testFieldDecimal`, a.`testFieldTimeSpan`, a.`testFieldDateTime`, a.`testFieldBytes`, a.`testFieldString`, a.`testFieldGuid`, a.`testFieldBoolNullable`, a.`testFieldSByteNullable`, a.`testFieldShortNullable`, a.`testFieldIntNullable`, a.`testFielLongNullable`, a.`testFieldByteNullable`, a.`testFieldUShortNullable`, a.`testFieldUIntNullable`, a.`testFieldULongNullable`, a.`testFieldDoubleNullable`, a.`testFieldFloatNullable`, a.`testFieldDecimalNullable`, a.`testFieldTimeSpanNullable`, a.`testFieldDateTimeNullable`, a.`testFieldGuidNullable`, ST_AsText(a.`testFieldPoint`) `testFieldPoint`, ST_AsText(a.`testFieldLineString`) `testFieldLineString`, ST_AsText(a.`testFieldPolygon`) `testFieldPolygon`, ST_AsText(a.`testFieldMultiPoint`) `testFieldMultiPoint`, ST_AsText(a.`testFieldMultiLineString`) `testFieldMultiLineString`, ST_AsText(a.`testFieldMultiPolygon`) `testFieldMultiPolygon`, a.`testFieldEnum1`, a.`testFieldEnum1Nullable`, a.`testFieldEnum2`, a.`testFieldEnum2Nullable` 
FROM `tb_alltype` a 
WHERE ((a.`testFieldInt` = 1 OR a.`testFieldInt` = 3 OR a.`testFieldInt` = 8) AND a.`Id` = 101)", t3);
            var t4 = select.Where(a => a.Id == 100 && arr.Any(x => a.testFieldInt == x) && a.Id == 101).ToSql();
            Assert.Equal(@"SELECT a.`Id`, a.`testFieldBool`, a.`testFieldSByte`, a.`testFieldShort`, a.`testFieldInt`, a.`testFieldLong`, a.`testFieldByte`, a.`testFieldUShort`, a.`testFieldUInt`, a.`testFieldULong`, a.`testFieldDouble`, a.`testFieldFloat`, a.`testFieldDecimal`, a.`testFieldTimeSpan`, a.`testFieldDateTime`, a.`testFieldBytes`, a.`testFieldString`, a.`testFieldGuid`, a.`testFieldBoolNullable`, a.`testFieldSByteNullable`, a.`testFieldShortNullable`, a.`testFieldIntNullable`, a.`testFielLongNullable`, a.`testFieldByteNullable`, a.`testFieldUShortNullable`, a.`testFieldUIntNullable`, a.`testFieldULongNullable`, a.`testFieldDoubleNullable`, a.`testFieldFloatNullable`, a.`testFieldDecimalNullable`, a.`testFieldTimeSpanNullable`, a.`testFieldDateTimeNullable`, a.`testFieldGuidNullable`, ST_AsText(a.`testFieldPoint`) `testFieldPoint`, ST_AsText(a.`testFieldLineString`) `testFieldLineString`, ST_AsText(a.`testFieldPolygon`) `testFieldPolygon`, ST_AsText(a.`testFieldMultiPoint`) `testFieldMultiPoint`, ST_AsText(a.`testFieldMultiLineString`) `testFieldMultiLineString`, ST_AsText(a.`testFieldMultiPolygon`) `testFieldMultiPolygon`, a.`testFieldEnum1`, a.`testFieldEnum1Nullable`, a.`testFieldEnum2`, a.`testFieldEnum2Nullable` 
FROM `tb_alltype` a 
WHERE (a.`Id` = 100 AND (a.`testFieldInt` = 1 OR a.`testFieldInt` = 3 OR a.`testFieldInt` = 8) AND a.`Id` = 101)", t4);

            var t11 = select.Where(a => arr.Any(x => a.testFieldInt == x && a.testFieldLong > x)).ToSql();
            Assert.Equal(@"SELECT a.`Id`, a.`testFieldBool`, a.`testFieldSByte`, a.`testFieldShort`, a.`testFieldInt`, a.`testFieldLong`, a.`testFieldByte`, a.`testFieldUShort`, a.`testFieldUInt`, a.`testFieldULong`, a.`testFieldDouble`, a.`testFieldFloat`, a.`testFieldDecimal`, a.`testFieldTimeSpan`, a.`testFieldDateTime`, a.`testFieldBytes`, a.`testFieldString`, a.`testFieldGuid`, a.`testFieldBoolNullable`, a.`testFieldSByteNullable`, a.`testFieldShortNullable`, a.`testFieldIntNullable`, a.`testFielLongNullable`, a.`testFieldByteNullable`, a.`testFieldUShortNullable`, a.`testFieldUIntNullable`, a.`testFieldULongNullable`, a.`testFieldDoubleNullable`, a.`testFieldFloatNullable`, a.`testFieldDecimalNullable`, a.`testFieldTimeSpanNullable`, a.`testFieldDateTimeNullable`, a.`testFieldGuidNullable`, ST_AsText(a.`testFieldPoint`) `testFieldPoint`, ST_AsText(a.`testFieldLineString`) `testFieldLineString`, ST_AsText(a.`testFieldPolygon`) `testFieldPolygon`, ST_AsText(a.`testFieldMultiPoint`) `testFieldMultiPoint`, ST_AsText(a.`testFieldMultiLineString`) `testFieldMultiLineString`, ST_AsText(a.`testFieldMultiPolygon`) `testFieldMultiPolygon`, a.`testFieldEnum1`, a.`testFieldEnum1Nullable`, a.`testFieldEnum2`, a.`testFieldEnum2Nullable` 
FROM `tb_alltype` a 
WHERE ((a.`testFieldInt` = 1 AND a.`testFieldLong` > 1 OR a.`testFieldInt` = 3 AND a.`testFieldLong` > 3 OR a.`testFieldInt` = 8 AND a.`testFieldLong` > 8))", t11);
            var t22 = select.Where(a => a.Id == 100 && arr.Any(x => a.testFieldInt == x && a.testFieldLong > x)).ToSql();
            Assert.Equal(@"SELECT a.`Id`, a.`testFieldBool`, a.`testFieldSByte`, a.`testFieldShort`, a.`testFieldInt`, a.`testFieldLong`, a.`testFieldByte`, a.`testFieldUShort`, a.`testFieldUInt`, a.`testFieldULong`, a.`testFieldDouble`, a.`testFieldFloat`, a.`testFieldDecimal`, a.`testFieldTimeSpan`, a.`testFieldDateTime`, a.`testFieldBytes`, a.`testFieldString`, a.`testFieldGuid`, a.`testFieldBoolNullable`, a.`testFieldSByteNullable`, a.`testFieldShortNullable`, a.`testFieldIntNullable`, a.`testFielLongNullable`, a.`testFieldByteNullable`, a.`testFieldUShortNullable`, a.`testFieldUIntNullable`, a.`testFieldULongNullable`, a.`testFieldDoubleNullable`, a.`testFieldFloatNullable`, a.`testFieldDecimalNullable`, a.`testFieldTimeSpanNullable`, a.`testFieldDateTimeNullable`, a.`testFieldGuidNullable`, ST_AsText(a.`testFieldPoint`) `testFieldPoint`, ST_AsText(a.`testFieldLineString`) `testFieldLineString`, ST_AsText(a.`testFieldPolygon`) `testFieldPolygon`, ST_AsText(a.`testFieldMultiPoint`) `testFieldMultiPoint`, ST_AsText(a.`testFieldMultiLineString`) `testFieldMultiLineString`, ST_AsText(a.`testFieldMultiPolygon`) `testFieldMultiPolygon`, a.`testFieldEnum1`, a.`testFieldEnum1Nullable`, a.`testFieldEnum2`, a.`testFieldEnum2Nullable` 
FROM `tb_alltype` a 
WHERE (a.`Id` = 100 AND (a.`testFieldInt` = 1 AND a.`testFieldLong` > 1 OR a.`testFieldInt` = 3 AND a.`testFieldLong` > 3 OR a.`testFieldInt` = 8 AND a.`testFieldLong` > 8))", t22);
            var t33 = select.Where(a => arr.Any(x => a.testFieldInt == x && a.testFieldLong > x) && a.Id == 101).ToSql();
            Assert.Equal(@"SELECT a.`Id`, a.`testFieldBool`, a.`testFieldSByte`, a.`testFieldShort`, a.`testFieldInt`, a.`testFieldLong`, a.`testFieldByte`, a.`testFieldUShort`, a.`testFieldUInt`, a.`testFieldULong`, a.`testFieldDouble`, a.`testFieldFloat`, a.`testFieldDecimal`, a.`testFieldTimeSpan`, a.`testFieldDateTime`, a.`testFieldBytes`, a.`testFieldString`, a.`testFieldGuid`, a.`testFieldBoolNullable`, a.`testFieldSByteNullable`, a.`testFieldShortNullable`, a.`testFieldIntNullable`, a.`testFielLongNullable`, a.`testFieldByteNullable`, a.`testFieldUShortNullable`, a.`testFieldUIntNullable`, a.`testFieldULongNullable`, a.`testFieldDoubleNullable`, a.`testFieldFloatNullable`, a.`testFieldDecimalNullable`, a.`testFieldTimeSpanNullable`, a.`testFieldDateTimeNullable`, a.`testFieldGuidNullable`, ST_AsText(a.`testFieldPoint`) `testFieldPoint`, ST_AsText(a.`testFieldLineString`) `testFieldLineString`, ST_AsText(a.`testFieldPolygon`) `testFieldPolygon`, ST_AsText(a.`testFieldMultiPoint`) `testFieldMultiPoint`, ST_AsText(a.`testFieldMultiLineString`) `testFieldMultiLineString`, ST_AsText(a.`testFieldMultiPolygon`) `testFieldMultiPolygon`, a.`testFieldEnum1`, a.`testFieldEnum1Nullable`, a.`testFieldEnum2`, a.`testFieldEnum2Nullable` 
FROM `tb_alltype` a 
WHERE ((a.`testFieldInt` = 1 AND a.`testFieldLong` > 1 OR a.`testFieldInt` = 3 AND a.`testFieldLong` > 3 OR a.`testFieldInt` = 8 AND a.`testFieldLong` > 8) AND a.`Id` = 101)", t33);
            var t44 = select.Where(a => a.Id == 100 && arr.Any(x => a.testFieldInt == x && a.testFieldLong > x) && a.Id == 101).ToSql();
            Assert.Equal(@"SELECT a.`Id`, a.`testFieldBool`, a.`testFieldSByte`, a.`testFieldShort`, a.`testFieldInt`, a.`testFieldLong`, a.`testFieldByte`, a.`testFieldUShort`, a.`testFieldUInt`, a.`testFieldULong`, a.`testFieldDouble`, a.`testFieldFloat`, a.`testFieldDecimal`, a.`testFieldTimeSpan`, a.`testFieldDateTime`, a.`testFieldBytes`, a.`testFieldString`, a.`testFieldGuid`, a.`testFieldBoolNullable`, a.`testFieldSByteNullable`, a.`testFieldShortNullable`, a.`testFieldIntNullable`, a.`testFielLongNullable`, a.`testFieldByteNullable`, a.`testFieldUShortNullable`, a.`testFieldUIntNullable`, a.`testFieldULongNullable`, a.`testFieldDoubleNullable`, a.`testFieldFloatNullable`, a.`testFieldDecimalNullable`, a.`testFieldTimeSpanNullable`, a.`testFieldDateTimeNullable`, a.`testFieldGuidNullable`, ST_AsText(a.`testFieldPoint`) `testFieldPoint`, ST_AsText(a.`testFieldLineString`) `testFieldLineString`, ST_AsText(a.`testFieldPolygon`) `testFieldPolygon`, ST_AsText(a.`testFieldMultiPoint`) `testFieldMultiPoint`, ST_AsText(a.`testFieldMultiLineString`) `testFieldMultiLineString`, ST_AsText(a.`testFieldMultiPolygon`) `testFieldMultiPolygon`, a.`testFieldEnum1`, a.`testFieldEnum1Nullable`, a.`testFieldEnum2`, a.`testFieldEnum2Nullable` 
FROM `tb_alltype` a 
WHERE (a.`Id` = 100 AND (a.`testFieldInt` = 1 AND a.`testFieldLong` > 1 OR a.`testFieldInt` = 3 AND a.`testFieldLong` > 3 OR a.`testFieldInt` = 8 AND a.`testFieldLong` > 8) AND a.`Id` = 101)", t44);
        }

        [Fact]
        public void Div()
        {
            var t1 = select.Where(a => a.testFieldInt / 3 > 3).Limit(10).ToList();
            var t2 = select.Where(a => a.testFieldLong / 3 > 3).Limit(10).ToList();
            var t3 = select.Where(a => a.testFieldShort / 3 > 3).Limit(10).ToList();

            var t4 = select.Where(a => a.testFieldInt / 3.0 > 3).Limit(10).ToList();
            var t5 = select.Where(a => a.testFieldLong / 3.0 > 3).Limit(10).ToList();
            var t6 = select.Where(a => a.testFieldShort / 3.0 > 3).Limit(10).ToList();

            var t7 = select.Where(a => a.testFieldDouble / 3 > 3).Limit(10).ToList();
            var t8 = select.Where(a => a.testFieldDecimal / 3 > 3).Limit(10).ToList();
            var t9 = select.Where(a => a.testFieldFloat / 3 > 3).Limit(10).ToList();
        }

        [Fact]
        public void Boolean()
        {
            var t1 = select.Where(a => a.testFieldBool == true).ToList();
            var t2 = select.Where(a => a.testFieldBool != true).ToList();
            var t3 = select.Where(a => a.testFieldBool == false).ToList();
            var t4 = select.Where(a => !a.testFieldBool).ToList();
            var t5 = select.Where(a => a.testFieldBool).ToList();
            var t51 = select.WhereCascade(a => a.testFieldBool).ToList();

            var t11 = select.Where(a => a.testFieldBoolNullable == true).ToList();
            var t22 = select.Where(a => a.testFieldBoolNullable != true).ToList();
            var t33 = select.Where(a => a.testFieldBoolNullable == false).ToList();
            var t44 = select.Where(a => !a.testFieldBoolNullable.Value).ToList();
            var t55 = select.Where(a => a.testFieldBoolNullable.Value).ToList();

            var t111 = select.Where(a => a.testFieldBool == true && a.Id > 0).ToList();
            var t222 = select.Where(a => a.testFieldBool != true && a.Id > 0).ToList();
            var t333 = select.Where(a => a.testFieldBool == false && a.Id > 0).ToList();
            var t444 = select.Where(a => !a.testFieldBool && a.Id > 0).ToList();
            var t555 = select.Where(a => a.testFieldBool && a.Id > 0).ToList();

            var t1111 = select.Where(a => a.testFieldBoolNullable == true && a.Id > 0).ToList();
            var t2222 = select.Where(a => a.testFieldBoolNullable != true && a.Id > 0).ToList();
            var t3333 = select.Where(a => a.testFieldBoolNullable == false && a.Id > 0).ToList();
            var t4444 = select.Where(a => !a.testFieldBoolNullable.Value && a.Id > 0).ToList();
            var t5555 = select.Where(a => a.testFieldBoolNullable.Value && a.Id > 0).ToList();

            var t11111 = select.Where(a => a.testFieldBool == true && a.Id > 0 && a.testFieldBool == true).ToList();
            var t22222 = select.Where(a => a.testFieldBool != true && a.Id > 0 && a.testFieldBool != true).ToList();
            var t33333 = select.Where(a => a.testFieldBool == false && a.Id > 0 && a.testFieldBool == false).ToList();
            var t44444 = select.Where(a => !a.testFieldBool && a.Id > 0 && !a.testFieldBool).ToList();
            var t55555 = select.Where(a => a.testFieldBool && a.Id > 0 && a.testFieldBool).ToList();

            var t111111 = select.Where(a => a.testFieldBoolNullable == true && a.Id > 0 && a.testFieldBoolNullable == true).ToList();
            var t222222 = select.Where(a => a.testFieldBoolNullable != true && a.Id > 0 && a.testFieldBoolNullable != true).ToList();
            var t333333 = select.Where(a => a.testFieldBoolNullable == false && a.Id > 0 && a.testFieldBoolNullable == false).ToList();
            var t444444 = select.Where(a => !a.testFieldBoolNullable.Value && a.Id > 0 && !a.testFieldBoolNullable.Value).ToList();
            var t555555 = select.Where(a => a.testFieldBoolNullable.Value && a.Id > 0 && a.testFieldBoolNullable.Value).ToList();
        }

        [Fact]
        public void Array()
        {
            int[] nullarr = null;
            Assert.Throws<Exception>(() => { select.Where(a => nullarr.Contains(a.testFieldInt)).ToList(); });
            Assert.Throws<Exception>(() => { select.Where(a => new int[0].Contains(a.testFieldInt)).ToList(); });

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

            var inarray2n = Enumerable.Range(1, 3333).ToArray();
            var sql1111111 = select.Where(a => inarray2n.Contains(a.testFieldInt)).ToList();
            var sql1122222 = select.Where(a => inarray2n.Contains(a.testFieldInt) == false).ToList();
            var sql1133333 = select.Where(a => !inarray2n.Contains(a.testFieldInt)).ToList();
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
