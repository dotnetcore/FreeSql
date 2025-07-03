using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.DataAnnotations
{
    public class TableNameTest
    {
        IFreeSql fsql => g.sqlite;

        [Fact]
        public void ClassTableName()
        {
            //Assert.Equal("", fsql.Select<tnt01>().ToSql().Replace("\r\n", "").Trim());
            //Assert.Equal("", fsql.Select<tnt01>().AsTable((t, old) => "tnt01_t").ToSql().Replace("\r\n", "").Trim());
            //Assert.Equal("", fsql.Insert<tnt01>().ToSql().Replace("\r\n", "").Trim());
            //Assert.Equal("", fsql.Insert<tnt01>().AsTable("tnt01_t").ToSql().Replace("\r\n", "").Trim());
            //Assert.Equal("", fsql.Delete<tnt01>().ToSql().Replace("\r\n", "").Trim());
            //Assert.Equal("", fsql.Delete<tnt01>().AsTable("tnt01_t").ToSql().Replace("\r\n", "").Trim());
            //Assert.Equal("", fsql.Update<tnt01>().SetSource(new tnt01()).ToSql().Replace("\r\n", "").Trim());
            //Assert.Equal("", fsql.Update<tnt01>().SetSource(new tnt01()).AsTable("tnt01_t").ToSql().Replace("\r\n", "").Trim());
            //Assert.Equal("", fsql.InsertOrUpdate<tnt01>().SetSource(new tnt01()).ToSql().Replace("\r\n", "").Trim());
            //Assert.Equal("", fsql.InsertOrUpdate<tnt01>().SetSource(new tnt01()).AsTable("tnt01_t").ToSql().Replace("\r\n", "").Trim());
        }
        class tnt01
        {
            public int id { get; set; }
            public string name { get; set; }
        }
    }
}
