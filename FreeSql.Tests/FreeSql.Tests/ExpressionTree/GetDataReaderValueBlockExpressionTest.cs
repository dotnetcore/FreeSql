using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Npgsql.LegacyPostgis;
using FreeSql.Internal;
using System.Linq.Expressions;

namespace FreeSql.ExpressionTree
{
    public class GetDataReaderValueBlockExpressionTest
    {

        [Fact]
        public void Guid2()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(Guid), Expression.Constant(Guid.Empty));
            Assert.Equal(Guid.Empty, Utils.GetDataReaderValue(typeof(Guid), Guid.Empty));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(Guid), Expression.Constant(Guid.NewGuid()));
            var newguid = Guid.NewGuid();
            Assert.Equal(newguid, Utils.GetDataReaderValue(typeof(Guid), newguid));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(Guid), Expression.Constant(null));
            Assert.Equal(Guid.Empty, Utils.GetDataReaderValue(typeof(Guid), null));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(Guid?), Expression.Constant(Guid.Empty));
            Assert.Equal(Guid.Empty, Utils.GetDataReaderValue(typeof(Guid?), Guid.Empty));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(Guid?), Expression.Constant(newguid));
            Assert.Equal(newguid, Utils.GetDataReaderValue(typeof(Guid?), newguid));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(Guid?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(Guid?), null));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(Guid), Expression.Constant(Guid.Empty.ToString()));
            Assert.Equal(Guid.Empty, Utils.GetDataReaderValue(typeof(Guid), Guid.Empty.ToString()));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(Guid), Expression.Constant(newguid.ToString()));
            Assert.Equal(newguid, Utils.GetDataReaderValue(typeof(Guid), newguid.ToString()));
            var exp333 = Utils.GetDataReaderValueBlockExpression(typeof(Guid), Expression.Constant("-1"));
            Assert.Equal(Guid.Empty, Utils.GetDataReaderValue(typeof(Guid), "-1"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(Guid?), Expression.Constant(Guid.Empty.ToString()));
            Assert.Equal(Guid.Empty, Utils.GetDataReaderValue(typeof(Guid?), Guid.Empty.ToString()));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(Guid?), Expression.Constant(newguid.ToString()));
            Assert.Equal(newguid, Utils.GetDataReaderValue(typeof(Guid?), newguid.ToString()));
            var exp3333 = Utils.GetDataReaderValueBlockExpression(typeof(Guid?), Expression.Constant("-1"));
            Assert.Null(Utils.GetDataReaderValue(typeof(Guid?), "-1"));
        }

        [Fact]
        public void Boolean()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(bool), Expression.Constant(true));
            Assert.Equal(true, Utils.GetDataReaderValue(typeof(bool), true));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(bool), Expression.Constant(false));
            Assert.Equal(false, Utils.GetDataReaderValue(typeof(bool), false));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(bool), Expression.Constant(null));
            Assert.Equal(false, Utils.GetDataReaderValue(typeof(bool), null));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(bool?), Expression.Constant(true));
            Assert.Equal(true, Utils.GetDataReaderValue(typeof(bool?), true));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(bool?), Expression.Constant(false));
            Assert.Equal(false, Utils.GetDataReaderValue(typeof(bool?), false));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(bool?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(bool?), null));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(bool), Expression.Constant("1"));
            Assert.Equal(true, Utils.GetDataReaderValue(typeof(bool), true));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(bool), Expression.Constant("0"));
            Assert.Equal(false, Utils.GetDataReaderValue(typeof(bool), false));
            var exp333 = Utils.GetDataReaderValueBlockExpression(typeof(bool), Expression.Constant("-1"));
            Assert.Equal(true, Utils.GetDataReaderValue(typeof(bool), true));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(bool), Expression.Constant("true"));
            Assert.Equal(true, Utils.GetDataReaderValue(typeof(bool?), true));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(bool), Expression.Constant("True"));
            Assert.Equal(true, Utils.GetDataReaderValue(typeof(bool?), true));
            var exp3333 = Utils.GetDataReaderValueBlockExpression(typeof(bool), Expression.Constant("false"));
            Assert.Equal(false, Utils.GetDataReaderValue(typeof(bool?), false));
            var exp4444 = Utils.GetDataReaderValueBlockExpression(typeof(bool), Expression.Constant("False"));
            Assert.Equal(false, Utils.GetDataReaderValue(typeof(bool?), false));
        }

        [Fact]
        public void SByte()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(sbyte), Expression.Constant(sbyte.MinValue));
            Assert.Equal(sbyte.MinValue, Utils.GetDataReaderValue(typeof(sbyte), sbyte.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(sbyte), Expression.Constant(sbyte.MaxValue));
            Assert.Equal(sbyte.MaxValue, Utils.GetDataReaderValue(typeof(sbyte), sbyte.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(sbyte), Expression.Constant("127"));
            Assert.Equal((sbyte)127, Utils.GetDataReaderValue(typeof(sbyte), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(sbyte?), Expression.Constant(sbyte.MinValue));
            Assert.Equal(sbyte.MinValue, Utils.GetDataReaderValue(typeof(sbyte?), sbyte.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(sbyte?), Expression.Constant(sbyte.MaxValue));
            Assert.Equal(sbyte.MaxValue, Utils.GetDataReaderValue(typeof(sbyte?), sbyte.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(sbyte?), Expression.Constant("127"));
            Assert.Equal((sbyte)127, Utils.GetDataReaderValue(typeof(sbyte?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(sbyte), Expression.Constant(null));
            Assert.Equal(default(sbyte), Utils.GetDataReaderValue(typeof(sbyte), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(sbyte), Expression.Constant("aaa"));
            Assert.Equal(default(sbyte), Utils.GetDataReaderValue(typeof(sbyte), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(sbyte?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(sbyte?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(sbyte?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(sbyte?), "aaa"));
        }

        [Fact]
        public void Short()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(short), Expression.Constant(short.MinValue));
            Assert.Equal(short.MinValue, Utils.GetDataReaderValue(typeof(short), short.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(short), Expression.Constant(short.MaxValue));
            Assert.Equal(short.MaxValue, Utils.GetDataReaderValue(typeof(short), short.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(short), Expression.Constant("127"));
            Assert.Equal((short)127, Utils.GetDataReaderValue(typeof(short), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(short?), Expression.Constant(short.MinValue));
            Assert.Equal(short.MinValue, Utils.GetDataReaderValue(typeof(short?), short.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(short?), Expression.Constant(short.MaxValue));
            Assert.Equal(short.MaxValue, Utils.GetDataReaderValue(typeof(short?), short.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(short?), Expression.Constant("127"));
            Assert.Equal((short)127, Utils.GetDataReaderValue(typeof(short?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(short), Expression.Constant(null));
            Assert.Equal(default(short), Utils.GetDataReaderValue(typeof(short), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(short), Expression.Constant("aaa"));
            Assert.Equal(default(short), Utils.GetDataReaderValue(typeof(short), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(short?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(short?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(short?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(short?), "aaa"));
        }

        [Fact]
        public void Int()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(int), Expression.Constant(int.MinValue));
            Assert.Equal(int.MinValue, Utils.GetDataReaderValue(typeof(int), int.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(int), Expression.Constant(int.MaxValue));
            Assert.Equal(int.MaxValue, Utils.GetDataReaderValue(typeof(int), int.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(int), Expression.Constant("127"));
            Assert.Equal((int)127, Utils.GetDataReaderValue(typeof(int), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(int?), Expression.Constant(int.MinValue));
            Assert.Equal(int.MinValue, Utils.GetDataReaderValue(typeof(int?), int.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(int?), Expression.Constant(int.MaxValue));
            Assert.Equal(int.MaxValue, Utils.GetDataReaderValue(typeof(int?), int.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(int?), Expression.Constant("127"));
            Assert.Equal((int)127, Utils.GetDataReaderValue(typeof(int?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(int), Expression.Constant(null));
            Assert.Equal(default(int), Utils.GetDataReaderValue(typeof(int), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(int), Expression.Constant("aaa"));
            Assert.Equal(default(int), Utils.GetDataReaderValue(typeof(int), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(int?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(int?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(int?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(int?), "aaa"));
        }

        [Fact]
        public void Long()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(long), Expression.Constant(long.MinValue));
            Assert.Equal(long.MinValue, Utils.GetDataReaderValue(typeof(long), long.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(long), Expression.Constant(long.MaxValue));
            Assert.Equal(long.MaxValue, Utils.GetDataReaderValue(typeof(long), long.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(long), Expression.Constant("127"));
            Assert.Equal((long)127, Utils.GetDataReaderValue(typeof(long), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(long?), Expression.Constant(long.MinValue));
            Assert.Equal(long.MinValue, Utils.GetDataReaderValue(typeof(long?), long.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(long?), Expression.Constant(long.MaxValue));
            Assert.Equal(long.MaxValue, Utils.GetDataReaderValue(typeof(long?), long.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(long?), Expression.Constant("127"));
            Assert.Equal((long)127, Utils.GetDataReaderValue(typeof(long?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(long), Expression.Constant(null));
            Assert.Equal(default(long), Utils.GetDataReaderValue(typeof(long), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(long), Expression.Constant("aaa"));
            Assert.Equal(default(long), Utils.GetDataReaderValue(typeof(long), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(long?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(long?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(long?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(long?), "aaa"));
        }

        [Fact]
        public void Byte()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(byte), Expression.Constant(byte.MinValue));
            Assert.Equal(byte.MinValue, Utils.GetDataReaderValue(typeof(byte), byte.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(byte), Expression.Constant(byte.MaxValue));
            Assert.Equal(byte.MaxValue, Utils.GetDataReaderValue(typeof(byte), byte.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(byte), Expression.Constant("127"));
            Assert.Equal((byte)127, Utils.GetDataReaderValue(typeof(byte), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(byte?), Expression.Constant(byte.MinValue));
            Assert.Equal(byte.MinValue, Utils.GetDataReaderValue(typeof(byte?), byte.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(byte?), Expression.Constant(byte.MaxValue));
            Assert.Equal(byte.MaxValue, Utils.GetDataReaderValue(typeof(byte?), byte.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(byte?), Expression.Constant("127"));
            Assert.Equal((byte)127, Utils.GetDataReaderValue(typeof(byte?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(byte), Expression.Constant(null));
            Assert.Equal(default(byte), Utils.GetDataReaderValue(typeof(byte), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(byte), Expression.Constant("aaa"));
            Assert.Equal(default(byte), Utils.GetDataReaderValue(typeof(byte), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(byte?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(byte?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(byte?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(byte?), "aaa"));
        }

        [Fact]
        public void UShort()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(ushort), Expression.Constant(ushort.MinValue));
            Assert.Equal(ushort.MinValue, Utils.GetDataReaderValue(typeof(ushort), ushort.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(ushort), Expression.Constant(ushort.MaxValue));
            Assert.Equal(ushort.MaxValue, Utils.GetDataReaderValue(typeof(ushort), ushort.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(ushort), Expression.Constant("127"));
            Assert.Equal((ushort)127, Utils.GetDataReaderValue(typeof(ushort), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(ushort?), Expression.Constant(ushort.MinValue));
            Assert.Equal(ushort.MinValue, Utils.GetDataReaderValue(typeof(ushort?), ushort.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(ushort?), Expression.Constant(ushort.MaxValue));
            Assert.Equal(ushort.MaxValue, Utils.GetDataReaderValue(typeof(ushort?), ushort.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(ushort?), Expression.Constant("127"));
            Assert.Equal((ushort)127, Utils.GetDataReaderValue(typeof(ushort?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(ushort), Expression.Constant(null));
            Assert.Equal(default(ushort), Utils.GetDataReaderValue(typeof(ushort), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(ushort), Expression.Constant("aaa"));
            Assert.Equal(default(ushort), Utils.GetDataReaderValue(typeof(ushort), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(ushort?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(ushort?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(ushort?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(ushort?), "aaa"));
        }

        [Fact]
        public void UInt()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(uint), Expression.Constant(uint.MinValue));
            Assert.Equal(uint.MinValue, Utils.GetDataReaderValue(typeof(uint), uint.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(uint), Expression.Constant(uint.MaxValue));
            Assert.Equal(uint.MaxValue, Utils.GetDataReaderValue(typeof(uint), uint.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(uint), Expression.Constant("127"));
            Assert.Equal((uint)127, Utils.GetDataReaderValue(typeof(uint), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(uint?), Expression.Constant(uint.MinValue));
            Assert.Equal(uint.MinValue, Utils.GetDataReaderValue(typeof(uint?), uint.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(uint?), Expression.Constant(uint.MaxValue));
            Assert.Equal(uint.MaxValue, Utils.GetDataReaderValue(typeof(uint?), uint.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(uint?), Expression.Constant("127"));
            Assert.Equal((uint)127, Utils.GetDataReaderValue(typeof(uint?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(uint), Expression.Constant(null));
            Assert.Equal(default(uint), Utils.GetDataReaderValue(typeof(uint), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(uint), Expression.Constant("aaa"));
            Assert.Equal(default(uint), Utils.GetDataReaderValue(typeof(uint), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(uint?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(uint?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(uint?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(uint?), "aaa"));
        }

        [Fact]
        public void ULong()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(ulong), Expression.Constant(ulong.MinValue));
            Assert.Equal(ulong.MinValue, Utils.GetDataReaderValue(typeof(ulong), ulong.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(ulong), Expression.Constant(ulong.MaxValue));
            Assert.Equal(ulong.MaxValue, Utils.GetDataReaderValue(typeof(ulong), ulong.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(ulong), Expression.Constant("127"));
            Assert.Equal((ulong)127, Utils.GetDataReaderValue(typeof(ulong), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(ulong?), Expression.Constant(ulong.MinValue));
            Assert.Equal(ulong.MinValue, Utils.GetDataReaderValue(typeof(ulong?), ulong.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(ulong?), Expression.Constant(ulong.MaxValue));
            Assert.Equal(ulong.MaxValue, Utils.GetDataReaderValue(typeof(ulong?), ulong.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(ulong?), Expression.Constant("127"));
            Assert.Equal((ulong)127, Utils.GetDataReaderValue(typeof(ulong?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(ulong), Expression.Constant(null));
            Assert.Equal(default(ulong), Utils.GetDataReaderValue(typeof(ulong), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(ulong), Expression.Constant("aaa"));
            Assert.Equal(default(ulong), Utils.GetDataReaderValue(typeof(ulong), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(ulong?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(ulong?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(ulong?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(ulong?), "aaa"));
        }

        [Fact]
        public void Float()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(float), Expression.Constant(float.MinValue));
            Assert.Equal(float.MinValue, Utils.GetDataReaderValue(typeof(float), float.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(float), Expression.Constant(float.MaxValue));
            Assert.Equal(float.MaxValue, Utils.GetDataReaderValue(typeof(float), float.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(float), Expression.Constant("127"));
            Assert.Equal((float)127, Utils.GetDataReaderValue(typeof(float), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(float?), Expression.Constant(float.MinValue));
            Assert.Equal(float.MinValue, Utils.GetDataReaderValue(typeof(float?), float.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(float?), Expression.Constant(float.MaxValue));
            Assert.Equal(float.MaxValue, Utils.GetDataReaderValue(typeof(float?), float.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(float?), Expression.Constant("127"));
            Assert.Equal((float)127, Utils.GetDataReaderValue(typeof(float?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(float), Expression.Constant(null));
            Assert.Equal(default(float), Utils.GetDataReaderValue(typeof(float), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(float), Expression.Constant("aaa"));
            Assert.Equal(default(float), Utils.GetDataReaderValue(typeof(float), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(float?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(float?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(float?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(float?), "aaa"));
        }

        [Fact]
        public void Double()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(double), Expression.Constant(double.MinValue));
            Assert.Equal(double.MinValue, Utils.GetDataReaderValue(typeof(double), double.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(double), Expression.Constant(double.MaxValue));
            Assert.Equal(double.MaxValue, Utils.GetDataReaderValue(typeof(double), double.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(double), Expression.Constant("127"));
            Assert.Equal((double)127, Utils.GetDataReaderValue(typeof(double), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(double?), Expression.Constant(double.MinValue));
            Assert.Equal(double.MinValue, Utils.GetDataReaderValue(typeof(double?), double.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(double?), Expression.Constant(double.MaxValue));
            Assert.Equal(double.MaxValue, Utils.GetDataReaderValue(typeof(double?), double.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(double?), Expression.Constant("127"));
            Assert.Equal((double)127, Utils.GetDataReaderValue(typeof(double?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(double), Expression.Constant(null));
            Assert.Equal(default(double), Utils.GetDataReaderValue(typeof(double), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(double), Expression.Constant("aaa"));
            Assert.Equal(default(double), Utils.GetDataReaderValue(typeof(double), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(double?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(double?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(double?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(double?), "aaa"));
        }

        [Fact]
        public void Decimal()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(decimal), Expression.Constant(decimal.MinValue));
            Assert.Equal(decimal.MinValue, Utils.GetDataReaderValue(typeof(decimal), decimal.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(decimal), Expression.Constant(decimal.MaxValue));
            Assert.Equal(decimal.MaxValue, Utils.GetDataReaderValue(typeof(decimal), decimal.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(decimal), Expression.Constant("127"));
            Assert.Equal((decimal)127, Utils.GetDataReaderValue(typeof(decimal), "127"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(decimal?), Expression.Constant(decimal.MinValue));
            Assert.Equal(decimal.MinValue, Utils.GetDataReaderValue(typeof(decimal?), decimal.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(decimal?), Expression.Constant(decimal.MaxValue));
            Assert.Equal(decimal.MaxValue, Utils.GetDataReaderValue(typeof(decimal?), decimal.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(decimal?), Expression.Constant("127"));
            Assert.Equal((decimal)127, Utils.GetDataReaderValue(typeof(decimal?), "127"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(decimal), Expression.Constant(null));
            Assert.Equal(default(decimal), Utils.GetDataReaderValue(typeof(decimal), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(decimal), Expression.Constant("aaa"));
            Assert.Equal(default(decimal), Utils.GetDataReaderValue(typeof(decimal), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(decimal?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(decimal?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(decimal?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(decimal?), "aaa"));
        }

        [Fact]
        public void DateTime2()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(DateTime), Expression.Constant(DateTime.MinValue));
            Assert.Equal(DateTime.MinValue, Utils.GetDataReaderValue(typeof(DateTime), DateTime.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(DateTime), Expression.Constant(DateTime.MaxValue));
            Assert.Equal(DateTime.MaxValue, Utils.GetDataReaderValue(typeof(DateTime), DateTime.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(DateTime), Expression.Constant("2000-1-1"));
            Assert.Equal(DateTime.Parse("2000-1-1"), Utils.GetDataReaderValue(typeof(DateTime), "2000-1-1"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(DateTime?), Expression.Constant(DateTime.MinValue));
            Assert.Equal(DateTime.MinValue, Utils.GetDataReaderValue(typeof(DateTime?), DateTime.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(DateTime?), Expression.Constant(DateTime.MaxValue));
            Assert.Equal(DateTime.MaxValue, Utils.GetDataReaderValue(typeof(DateTime?), DateTime.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(DateTime?), Expression.Constant("2000-1-1"));
            Assert.Equal(DateTime.Parse("2000-1-1"), Utils.GetDataReaderValue(typeof(DateTime?), "2000-1-1"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(DateTime), Expression.Constant(null));
            Assert.Equal(default(DateTime), Utils.GetDataReaderValue(typeof(DateTime), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(DateTime), Expression.Constant("aaa"));
            Assert.Equal(default(DateTime), Utils.GetDataReaderValue(typeof(DateTime), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(DateTime?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(DateTime?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(DateTime?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(DateTime?), "aaa"));
        }

        [Fact]
        public void DateTimeOffset2()
        {
            var exp1 = Utils.GetDataReaderValueBlockExpression(typeof(DateTimeOffset), Expression.Constant(DateTimeOffset.MinValue));
            Assert.Equal(DateTimeOffset.MinValue, Utils.GetDataReaderValue(typeof(DateTimeOffset), DateTimeOffset.MinValue));
            var exp2 = Utils.GetDataReaderValueBlockExpression(typeof(DateTimeOffset), Expression.Constant(DateTimeOffset.MaxValue));
            Assert.Equal(DateTimeOffset.MaxValue, Utils.GetDataReaderValue(typeof(DateTimeOffset), DateTimeOffset.MaxValue));
            var exp3 = Utils.GetDataReaderValueBlockExpression(typeof(DateTimeOffset), Expression.Constant("2000-1-1"));
            Assert.Equal(DateTimeOffset.Parse("2000-1-1"), Utils.GetDataReaderValue(typeof(DateTimeOffset), "2000-1-1"));

            var exp11 = Utils.GetDataReaderValueBlockExpression(typeof(DateTimeOffset?), Expression.Constant(DateTimeOffset.MinValue));
            Assert.Equal(DateTimeOffset.MinValue, Utils.GetDataReaderValue(typeof(DateTimeOffset?), DateTimeOffset.MinValue));
            var exp22 = Utils.GetDataReaderValueBlockExpression(typeof(DateTimeOffset?), Expression.Constant(DateTimeOffset.MaxValue));
            Assert.Equal(DateTimeOffset.MaxValue, Utils.GetDataReaderValue(typeof(DateTimeOffset?), DateTimeOffset.MaxValue));
            var exp33 = Utils.GetDataReaderValueBlockExpression(typeof(DateTimeOffset?), Expression.Constant("2000-1-1"));
            Assert.Equal(DateTimeOffset.Parse("2000-1-1"), Utils.GetDataReaderValue(typeof(DateTimeOffset?), "2000-1-1"));

            var exp111 = Utils.GetDataReaderValueBlockExpression(typeof(DateTimeOffset), Expression.Constant(null));
            Assert.Equal(default(DateTimeOffset), Utils.GetDataReaderValue(typeof(DateTimeOffset), null));
            var exp222 = Utils.GetDataReaderValueBlockExpression(typeof(DateTimeOffset), Expression.Constant("aaa"));
            Assert.Equal(default(DateTimeOffset), Utils.GetDataReaderValue(typeof(DateTimeOffset), "aaa"));

            var exp1111 = Utils.GetDataReaderValueBlockExpression(typeof(DateTimeOffset?), Expression.Constant(null));
            Assert.Null(Utils.GetDataReaderValue(typeof(DateTimeOffset?), null));
            var exp2222 = Utils.GetDataReaderValueBlockExpression(typeof(DateTimeOffset?), Expression.Constant("aaa"));
            Assert.Null(Utils.GetDataReaderValue(typeof(DateTimeOffset?), "aaa"));
        }
    }
}
