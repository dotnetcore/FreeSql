using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Npgsql.LegacyPostgis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Data.SqlClient;
using kwlib;
using System.Diagnostics;
using System.IO;
using System.Text;
using FreeSql.Internal;

namespace FreeSql.InternalTests
{
    public class CommonUtilsTest
    {

        [Fact]
        public void GetSplitTableNames()
        {
            var tbname = CommonUtils.GetSplitTableNames("table1", '`', '`', 2);
            Assert.Equal("table1", tbname[0]);

            tbname = CommonUtils.GetSplitTableNames("table1", '"', '"', 2);
            Assert.Equal("table1", tbname[0]);

            tbname = CommonUtils.GetSplitTableNames("table1", '[', ']', 2);
            Assert.Equal("table1", tbname[0]);

            //---

            tbname = CommonUtils.GetSplitTableNames("schema1.table1", '`', '`', 2);
            Assert.Equal("schema1", tbname[0]);
            Assert.Equal("table1", tbname[1]);

            tbname = CommonUtils.GetSplitTableNames("schema1.table1", '"', '"', 2);
            Assert.Equal("schema1", tbname[0]);
            Assert.Equal("table1", tbname[1]);

            tbname = CommonUtils.GetSplitTableNames("schema1.table1", '[', ']', 2);
            Assert.Equal("schema1", tbname[0]);
            Assert.Equal("table1", tbname[1]);

            //---

            tbname = CommonUtils.GetSplitTableNames("`sys.table1`", '`', '`', 2);
            Assert.Equal("sys.table1", tbname[0]);

            tbname = CommonUtils.GetSplitTableNames("\"sys.table1\"", '"', '"', 2);
            Assert.Equal("sys.table1", tbname[0]);

            tbname = CommonUtils.GetSplitTableNames("[sys.table1]", '[', ']', 2);
            Assert.Equal("sys.table1", tbname[0]);

            //---

            tbname = CommonUtils.GetSplitTableNames("`schema1`.`sys.table1`", '`', '`', 2);
            Assert.Equal("schema1", tbname[0]);
            Assert.Equal("sys.table1", tbname[1]);

            tbname = CommonUtils.GetSplitTableNames("\"schema1\".\"sys.table1\"", '"', '"', 2);
            Assert.Equal("schema1", tbname[0]);
            Assert.Equal("sys.table1", tbname[1]);

            tbname = CommonUtils.GetSplitTableNames("[schema1].[sys.table1]", '[', ']', 2);
            Assert.Equal("schema1", tbname[0]);
            Assert.Equal("sys.table1", tbname[1]);
        }
    }
}
