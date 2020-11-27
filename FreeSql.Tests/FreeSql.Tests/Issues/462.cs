using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _462
    {
        [Fact]
        public void SelectTest()
        {
            using (var db = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Oracle, "user id=1user;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=1")
                .UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
                .UseGenerateCommandParameterWithLambda(true)
                .UseAutoSyncStructure(true)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .Build())
            {

                var startTime = DateTime.Now;
                var endTime = DateTime.Now;

                var exp0 = 10;
                var cou = db.Select<V_HospitalReport>()
                    .Where(a => a.ScheduledDttm.Date >= startTime.Date && a.ScheduledDttm.Date <= (endTime.AddDays(1)).Date)
                    .ToList(a => new
                    {
                        subCount = db.Select<V_HOSPITALREPORT>().Where(b => b.SCHEDULED_DTTM == exp0).Count()
                    });
            }
        }

        [Table(Name = "V_HospitalReport22")]
        public class V_HospitalReport
        {
            [Column(Name = "hospital_name")]
            public string HospitalName { get; set; }

            [Column(Name = "dep")]
            public string Dep { get; set; }

            [Column(Name = "instrna")]
            public string Instrna { get; set; }

            [Column(Name = "confirm_doctor_name")]
            public string ConfirmDoctorName { get; set; }

            [Column(Name = "Scheduled_Dttm")]
            public DateTime ScheduledDttm { get; set; }
        }
        [Table(Name = "V_HOSPITALREPORT22111")]
        public class V_HOSPITALREPORT
        {
            public int SCHEDULED_DTTM { get; set; }
        }
    }
}
