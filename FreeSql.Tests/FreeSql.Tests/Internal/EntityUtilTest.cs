using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;
using FreeSql.Extensions.EntityUtil;

namespace FreeSql.Tests.Internal
{

    public class EntityUtilTest
    {
        [Fact]
        public void GetEntityKeyString()
        {
            var fsql = g.sqlserver;

            var t1 = new GEKS_01 { };
            Assert.Equal("", fsql.GetEntityKeyString(typeof(GEKS_01), t1, false));
            Assert.Equal(Guid.Empty, t1.id);

            var t2 = Guid.NewGuid();
            var t3 = new GEKS_01 { id = t2 };
            Assert.Equal(t2.ToString().ToString(), fsql.GetEntityKeyString(typeof(GEKS_01), t3, false));
            Assert.Equal(t2, t3.id);

            var t4 = new GEKS_01 { };
            Assert.Equal(fsql.GetEntityKeyString(typeof(GEKS_01), t1, true).Length, 36);
            Assert.NotEqual(Guid.Empty, t1.id);


            var t5 = new GEKS_02 { };
            Assert.Equal("0", fsql.GetEntityKeyString(typeof(GEKS_02), t5, false));
            Assert.Equal(0, t5.id);

            var t6 = new GEKS_02 { id = 100 };
            Assert.Equal("100", fsql.GetEntityKeyString(typeof(GEKS_02), t6, false));
            Assert.Equal(100, t6.id);


            var t7 = new GEKS_03 { };
            Assert.Equal("0", fsql.GetEntityKeyString(typeof(GEKS_03), t7, false));
            Assert.Equal(0, t7.id);

            var t8 = new GEKS_03 { id = 100 };
            Assert.Equal("100", fsql.GetEntityKeyString(typeof(GEKS_03), t8, false));
            Assert.Equal(100, t8.id);


            var t9 = new GEKS_04 { };
            Assert.Equal("", fsql.GetEntityKeyString(typeof(GEKS_04), t9, false));
            Assert.Null(t9.id);

            var t10 = new GEKS_04 { id = "admin" };
            Assert.Equal("admin", fsql.GetEntityKeyString(typeof(GEKS_04), t10, false));
            Assert.Equal("admin", t10.id);


            var t11 = new GEKS_05 { };
            Assert.Equal("0", fsql.GetEntityKeyString(typeof(GEKS_05), t11, false));
            Assert.Equal(0, t11.id);

            var t12 = new GEKS_05 { id = 100 };
            Assert.Equal("100", fsql.GetEntityKeyString(typeof(GEKS_05), t12, false));
            Assert.Equal(100, t12.id);

            var t13 = new GEKS_05 { id = 100.000000M };
            Assert.Equal("100", fsql.GetEntityKeyString(typeof(GEKS_05), t13, false));
            Assert.Equal(100, t13.id);
        }

        class GEKS_01
        {
            public Guid id { get; set; }
        }
        class GEKS_02
        {
            public int id { get; set; }
        }
        class GEKS_03
        {
            public long id { get; set; }
        }
        class GEKS_04
        {
            public string id { get; set; }
        }
        class GEKS_05
        {
            public decimal id { get; set; }
        }

        [Fact]
        public void GetEntityKeyStringNullable()
        {
            var fsql = g.sqlserver;

            var t1 = new GEKS_06 { };
            Assert.Equal("", fsql.GetEntityKeyString(typeof(GEKS_06), t1, false));
            Assert.Null(t1.id);

            var t2 = Guid.NewGuid();
            var t3 = new GEKS_06 { id = t2 };
            Assert.Equal(t2.ToString().ToString(), fsql.GetEntityKeyString(typeof(GEKS_06), t3, false));
            Assert.Equal(t2, t3.id);

            var t4 = new GEKS_06 { };
            Assert.Equal(fsql.GetEntityKeyString(typeof(GEKS_06), t1, true).Length, 36);
            Assert.NotNull(t1.id);


            var t5 = new GEKS_07 { };
            Assert.Equal("", fsql.GetEntityKeyString(typeof(GEKS_07), t5, false));
            Assert.Null(t5.id);

            var t6 = new GEKS_07 { id = 100 };
            Assert.Equal("100", fsql.GetEntityKeyString(typeof(GEKS_07), t6, false));
            Assert.Equal(100, t6.id);


            var t7 = new GEKS_08 { };
            Assert.Equal("", fsql.GetEntityKeyString(typeof(GEKS_08), t7, false));
            Assert.Null(t7.id);

            var t8 = new GEKS_08 { id = 100 };
            Assert.Equal("100", fsql.GetEntityKeyString(typeof(GEKS_08), t8, false));
            Assert.Equal(100, t8.id);


            var t9 = new GEKS_09 { };
            Assert.Equal("", fsql.GetEntityKeyString(typeof(GEKS_09), t9, false));
            Assert.Null(t9.id);

            var t10 = new GEKS_09 { id = "admin" };
            Assert.Equal("admin", fsql.GetEntityKeyString(typeof(GEKS_09), t10, false));
            Assert.Equal("admin", t10.id);


            var t11 = new GEKS_10 { };
            Assert.Equal("", fsql.GetEntityKeyString(typeof(GEKS_10), t11, false));
            Assert.Null(t11.id);

            var t12 = new GEKS_10 { id = 100 };
            Assert.Equal("100", fsql.GetEntityKeyString(typeof(GEKS_10), t12, false));
            Assert.Equal(100, t12.id);

            var t13 = new GEKS_10 { id = 100.000000M };
            Assert.Equal("100", fsql.GetEntityKeyString(typeof(GEKS_10), t13, false));
            Assert.Equal(100, t13.id);
        }

        class GEKS_06
        {
            public Guid? id { get; set; }
        }
        class GEKS_07
        {
            public int? id { get; set; }
        }
        class GEKS_08
        {
            public long? id { get; set; }
        }
        class GEKS_09
        {
            public string? id { get; set; }
        }
        class GEKS_10
        {
            public decimal? id { get; set; }
        }
    }
}
