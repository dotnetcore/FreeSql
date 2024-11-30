using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using FreeSql.Provider.TDengine.Attributes;

namespace FreeSql.Tests.Provider.TDengine.TDengine.Tables
{
    [TDengineSuperTable(Name = "meters")]
    public class Meters
    {
        [Column(Name = "ts")]
        public DateTime Ts { get; set; }

        [Column(Name = "current")]
        public float Current { get; set; }

        [Column(Name = "voltage")]
        public int Voltage { get; set; }

        [Column(Name = "describe", StringLength = 50)]
        public string? Describe { get; set; }

        [TDengineTag(Name = "location")]
        public virtual string? Location { get; set; }

        [TDengineTag(Name = "group_id")]
        public virtual int GroupId { get; set; }
    }

    [TDengineSubTable(SuperTableName = "meters", Name = "d1001")]
    public class D1001 : Meters
    {
        [TDengineTag(Name = "location")]
        public override string Location { get; set; } = "BeiJIng.ChaoYang";

        [TDengineTag(Name = "group_id")]
        public override int GroupId { get; set; } = 1;
    }

    [TDengineSubTable(SuperTableName = "meters", Name = "d1002")]
    public class D1002 : Meters
    {
        [TDengineTag(Name = "location")]
        public new string Location { get; set; } = "California.SanFrancisco";

        [TDengineTag(Name = "group_id")]
        public new int GroupId { get; set; } = 2;
    }
}