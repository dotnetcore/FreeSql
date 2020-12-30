using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
	public class _623
	{
		[Fact]
		public void MySqlGroupBy()
		{
            var fsql = g.mysql;
			fsql.Delete<S_facility>().Where("1=1").ExecuteAffrows();
			fsql.Insert(new S_facility
			{
				Date = DateTime.Parse("2020-12-30"),
				EnterpriseId = 5,
				FacilityType = 10,
				Dot = 11,
				FacilityCount = 3,
				FacilityOpenCount = 21
			}).ExecuteAffrows();


			var date = DateTime.Parse("2020-12-30");
			int[] enterpriseIds = new[] { 5 };
			var sql = fsql.Select<S_facility>()
				.Where(a => a.Date == date.Date)
				.Where(a => enterpriseIds.Contains(a.EnterpriseId))
				.Where(a => a.FacilityType == 1)
				.GroupBy(a => a.Dot)
				.ToSql(a => new {
					Dot = a.Key,
					FacilityCount = a.Sum(a.Value.FacilityCount),
					FacilityOpenCount = a.Sum(a.Value.FacilityOpenCount)
				});
			Assert.Equal(@"SELECT a.`Dot`, sum(a.`FacilityCount`) as1, sum(a.`FacilityOpenCount`) as2 
FROM `ts_facility` a 
WHERE (a.`Date` = cast(date_format('2020-12-30 00:00:00.000','%Y-%m-%d') as datetime)) AND (((a.`EnterpriseId`) in (5))) AND (a.`FacilityType` = 1) 
GROUP BY a.`Dot`", sql);

			var rows = fsql.Select<S_facility>()
				.Where(a => a.Date == date.Date)
				.Where(a => enterpriseIds.Contains(a.EnterpriseId))
				.Where(a => a.FacilityType == 1)
				.GroupBy(a => a.Dot)
				.ToList(a => new {
					Dot = a.Key,
					FacilityCount = a.Sum(a.Value.FacilityCount),
					FacilityOpenCount = a.Sum(a.Value.FacilityOpenCount)
				});
		}

		[Table(Name = "ts_facility")]
		public partial class S_facility
		{
			[JsonProperty, Column(DbType = "date", IsPrimary = true)]
			public DateTime Date { get; set; }

			[JsonProperty, Column(IsPrimary = true)]
			public int EnterpriseId { get; set; }

			[JsonProperty, Column(IsPrimary = true)]
			public byte FacilityType { get; set; }

			[JsonProperty, Column(DbType = "tinyint(4)", IsPrimary = true)]
			public sbyte Dot { get; set; }

			[JsonProperty]
			public ushort FacilityCount { get; set; }

			[JsonProperty]
			public ushort FacilityOpenCount { get; set; }

		}
	}
}
