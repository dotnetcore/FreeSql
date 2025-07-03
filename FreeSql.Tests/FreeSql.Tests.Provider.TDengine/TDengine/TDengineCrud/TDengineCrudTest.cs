using System;
using FreeSql.Tests.Provider.TDengine.TDengine.Tables;
using Xunit;

namespace FreeSql.Tests.Provider.TDengine.TDengine.TDengineAdo
{
    public class TDengineCrudTest
    {
        IFreeSql fsql => g.tdengine;

        [Fact]
        void CodeFirstTest()
        {
            fsql.CodeFirst.SyncStructure<D1001>();
            fsql.CodeFirst.SyncStructure<D1002>();
            fsql.CodeFirst.SyncStructure<Users>();
        }

        [Fact]
        void InsertTest()
        {
            var insertAffrows = fsql.Insert(new D1001()
                {
                    Ts = DateTime.Now,
                    Current = 1,
                    Voltage = 1,
                    Describe = "D10021"
                }
            ).ExecuteAffrows();

            var insertAffrows2 = fsql.Insert(new D1001()
                {
                    Ts = DateTime.Now,
                    Current = 1,
                    Voltage = 1,
                    Describe = "D10021"
                }
            ).ExecuteAffrows();

            var batchInsertAffrows = fsql.Insert(new List<D1002>()
                {
                    new D1002()
                    {
                        Ts = DateTime.Now,
                        Current = 6,
                        Voltage = 6,
                        Describe = "D10026"
                    },
                    new D1002()
                    {
                        Ts = DateTime.Now,
                        Current = 3,
                        Voltage = 3,
                        Describe = "D10023"
                    },
                    new D1002()
                    {
                        Ts = DateTime.Now,
                        Current = 4,
                        Voltage = 4,
                        Describe = "D10024"
                    }
                }
            ).ExecuteAffrows();
        }

        [Fact]
        void SelectTest()
        {
            var subList = fsql.Select<D1001>().ToList(d => new
            {
                GroupId = d.GroupId
            });

            var superMetersList = fsql.Select<Meters>().ToList();
        }

        [Fact]
        void WhereSelectTest()
        {
            var list = fsql.Select<Meters>().Where(d => d.GroupId == 2).ToList();
        }

        [Fact]
        void DeleteTest()
        {
            var startTime = DateTime.Parse("2024-11-30T02:33:52.308+00:00");
            var endTime = DateTime.Parse("2024-11-30T02:40:58.961+00:00");
            var executeAffrows = fsql.Delete<Meters>()
                .Where(meters => meters.Ts >= startTime && meters.Ts <= endTime && meters.GroupId == 1)
                .ExecuteAffrows();
        }

        [Fact]
        void DbFirst_GetDatabases()
        {
            var databases = fsql.DbFirst.GetDatabases();
            foreach (var database in databases)
            {
                Console.WriteLine(database);
            }
        }
    }
}