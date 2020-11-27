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
    public class _519
    {
        [Fact]
        public void SelectTest()
        {
            IFreeSql fsql = g.sqlserver;

            fsql.Delete<ST_Stock519>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new[]
            {
                new ST_Stock519 { StoreHouse = "001", Works = "101", MaterialCode = "201", BatchCode = "301", CreatedTime = DateTime.Now },
                new ST_Stock519 { StoreHouse = "002", Works = "102", MaterialCode = "202", BatchCode = "302", CreatedTime = DateTime.Now },
                new ST_Stock519 { StoreHouse = "003", Works = "103", MaterialCode = "203", BatchCode = "303", CreatedTime = DateTime.Now }
            }).ExecuteAffrows();

            var list = fsql.Select<ST_Stock519>().ToList();
            var sql1 = fsql.Insert(list).NoneParameter().ToSql();
            var sql2 = fsql.Update<ST_Stock519>().SetSource(list).NoneParameter().ToSql();
            var sql3 = fsql.InsertOrUpdate<ST_Stock519>().SetSource(list).ToSql();
        }
        class ST_Stock519
        {
            /// <summary>
            /// 库位
            /// </summary>
            [Column(IsPrimary = true, DbType = "varchar(50)")]
            public string StoreHouse { get; set; } = string.Empty;

            /// <summary>
            /// 工厂
            /// </summary>
            [Column(IsPrimary = true, DbType = "varchar(50)")]
            public string Works { get; set; } = string.Empty;
            /// <summary>
            /// 物料号
            /// </summary>
            [Column(IsPrimary = true, DbType = "varchar(50)")]
            public string MaterialCode { get; set; } = string.Empty;
            /// <summary>
            /// 条码号
            /// </summary>
            [Column(IsPrimary = true, DbType = "varchar(50)")]
            public string BatchCode { get; set; } = string.Empty;

            /// <summary>
            /// 创建时间
            /// </summary>
            public DateTime? CreatedTime { get; set; }

            /// <summary>
            /// 创建人
            /// </summary>
            public string CreatorID { get; set; } = string.Empty;

            /// <summary>
            /// 创建人名称
            /// </summary>
            public string CreatorName { get; set; } = string.Empty;
        }
    }
}
