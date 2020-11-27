using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _306
    {
        [Fact]
        public void SelectTest()
        {
            IFreeSql db = g.sqlserver;

            db.Select<ElectricEnergyValue, BranchMeter>()
                .InnerJoin((a, b) => a.MeterSN == b.MeterSN)
                .ToAggregate((a, b) => new VM_PeriodEnergy
                {
                    Sharp = a.Sum(a.Key.Sharp),
                    Peak = a.Sum(a.Key.Peak),
                    Shoulder = a.Sum(a.Key.Shoulder),
                    Off = a.Sum(a.Key.Off),
                });
        }

        public class ElectricEnergyValue
        {
            public int ID { get; set; }

            public string MeterSN { get; set; }

            public DateTime CollectTime { get; set; }

            public decimal Sharp { get; set; }

            public decimal Peak { get; set; }

            public decimal Shoulder { get; set; }

            public decimal Off { get; set; }
        }

        public class BranchMeter
        {
            public int BranchID { get; set; }

            public string MeterSN { get; set; }
        }

        public class VM_PeriodEnergy
        {
            public decimal Sharp { get; set; }

            public decimal Peak { get; set; }

            public decimal Shoulder { get; set; }

            public decimal Off { get; set; }
        }
    }
}
