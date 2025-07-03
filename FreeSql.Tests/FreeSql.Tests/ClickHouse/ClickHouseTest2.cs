using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;
using Xunit;
using Xunit.Abstractions;

namespace FreeSql.Tests.ClickHouse
{
    public class ClickHouseTest2
    {


        private static IFreeSql fsql = new FreeSqlBuilder().UseConnectionString(DataType.ClickHouse,
                "Host=127.0.0.1;Port=8123;Database=test;Compress=True;Min Pool Size=1")
            .UseMonitorCommand(cmd => Console.WriteLine($"线程：{cmd.CommandText}\r\n"))
            .UseNoneCommandParameter(true)
            .Build();
        [Fact]
        public void CodeFirst()
        {
            fsql.CodeFirst.SyncStructure(typeof(CollectDataEntityUpdate01));
        }

        [Fact]
        public void Issuse1587Test()
        {
            fsql.CodeFirst.SyncStructure(typeof(PositionInfoModel));
        }

        [Fact]
        public void Issuse1587TestOnePrimary()
        {
            fsql.CodeFirst.SyncStructure(typeof(PositionInfoModel2));
        }
    }

    [Table(Name = "table_1")]
    [Index("stcd_index", "STCD", false)]
    [Index("tm_index", "TM", false)]
    [Index("type_index", "TYPE", false)]
    public class PositionInfoModel
    {
        [Column(IsPrimary = true)]
        public string STCD
        {
            set; get;
        }

        [Column(IsPrimary = true)]
        public DateTime TM
        {
            set; get;
        }

        [Column(IsNullable = false)]
        public decimal LON
        {
            set; get;
        }

        [Column(IsNullable = false)]
        public decimal LAT
        {
            set; get;
        }

        [Column(IsNullable = false)]
        public int TYPE
        {
            set; get;
        }

        [Column(IsNullable = true)]
        public decimal SPD
        {
            set; get;
        }

        [Column(IsNullable = true)]
        public decimal COG
        {
            set; get;
        }

        [Column(IsNullable = true)]
        public DateTime? UT
        {
            set; get;
        }
    }


    [Table(Name = "table_2")]
    [Index("stcd_index", "STCD", false)]
    [Index("tm_index", "TM", false)]
    [Index("type_index", "TYPE", false)]
    public class PositionInfoModel2
    {
        public string STCD
        {
            set; get;
        }

        [Column(IsPrimary = true)]
        public DateTime TM
        {
            set; get;
        }

        [Column(IsNullable = false)]
        public decimal LON
        {
            set; get;
        }

        [Column(IsNullable = false)]
        public decimal LAT
        {
            set; get;
        }

        [Column(IsNullable = false)]
        public int TYPE
        {
            set; get;
        }

        [Column(IsNullable = true)]
        public decimal SPD
        {
            set; get;
        }

        [Column(IsNullable = true)]
        public decimal COG
        {
            set; get;
        }

        [Column(IsNullable = true)]
        public DateTime? UT
        {
            set; get;
        }
    }
}