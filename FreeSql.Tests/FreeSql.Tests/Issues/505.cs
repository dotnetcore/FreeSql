using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _505
    {
        [Fact]
        public void ByteLengthTest()
        {
            TestLocal(g.sqlserver);
            TestLocal(g.mysql);
            TestLocal(g.pgsql);
            TestLocal(g.oracle);
            TestLocal(g.sqlite);
            TestLocal(g.firebird);
            TestLocal(g.dameng);
            //TestLocal(g.kingbaseES); //
            //TestLocal(g.shentong); // OCTET_LENGTH(xx) 返回结果32，值不符合
            //TestLocal(g.msaccess); //lenb(xx) 返回结果 15，值不符合

            void TestLocal(IFreeSql fsql)
            {
                var byteArray = Encoding.UTF8.GetBytes("我是中国人");
                fsql.Delete<Model505>().Where("1=1").ExecuteAffrows();
                fsql.Insert(new Model505 { ByteLength = byteArray.Length, ByteArray = byteArray }).ExecuteAffrows();

                var item = fsql.Select<Model505>()
                    //.Where(x => x.ByteArray.Length == x.ByteLength)
                    .First(a => new { item = a, length = a.ByteArray.Length });

                Assert.NotNull(item);
                Assert.Equal(Encoding.UTF8.GetString(byteArray), Encoding.UTF8.GetString(item.item.ByteArray));
                Assert.Equal(byteArray.Length, item.item.ByteLength);
                Assert.Equal(byteArray.Length, item.length);
            }
        }
        public class Model505
        {
            public Guid id { get; set; }
            public byte[] ByteArray { get; set; }
            public int ByteLength { get; set; }
        }

    }
}
