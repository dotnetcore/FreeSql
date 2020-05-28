using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _311
    {
        [Fact]
        public void SelectTest()
        {
            IFreeSql db = g.mysql;

            var sql = db.Update<UpdateSetEnum01>(1).Set(a => new UpdateSetEnum01 { Status01 = EnumStatus01.E01001, Status02 = EnumStatus02.E02003 }).ToSql();
            Assert.Equal(@"UPDATE `UpdateSetEnum01` SET `Status01` = 'E01001', `Status02` = 2 
WHERE (`ID` = 1)", sql);

            sql = db.Update<UpdateSetEnum01>(1).Set(a => new UpdateSetEnum01 { Status01 = null, Status02 = null }).ToSql();
            Assert.Equal(@"UPDATE `UpdateSetEnum01` SET `Status01` = NULL, `Status02` = NULL 
WHERE (`ID` = 1)", sql);

            sql = db.Update<UpdateSetEnum01>(1).Set(a => a.Status01 == null).Set(a => a.Status02 == null).ToSql();
            Assert.Equal(@"UPDATE `UpdateSetEnum01` SET `Status01` = NULL, `Status02` = NULL 
WHERE (`ID` = 1)", sql);
        }

        public class UpdateSetEnum01
        {
            public int ID { get; set; }
            public EnumStatus01? Status01 { get; set; }
            [Column(MapType = typeof(int))]
            public EnumStatus02? Status02 { get; set; }
        }

        public enum EnumStatus01 { E01001, E01002, E01003 }
        public enum EnumStatus02 { E02001, E02002, E02003 }
    }
}
