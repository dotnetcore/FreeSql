using FreeSql.DataAnnotations;
using FreeSql.Provider.TDengine.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Tests.Provider.TDengine.TDengine.Tables;

[TDengineSuperTable(Name = "point_data")]
public class PointDataEntity
{
    [Column(Name = "ts")]
    public DateTime Ts { get; set; }

    [Column(Name = "is_alarm")]
    public bool IsAlarm { get; set; }

    [Column(Name = "point_value")]
    public double PointValue { get; set; }

    [TDengineTag(Name = "point_number")]
    public string PointNumber { get; set; }

}