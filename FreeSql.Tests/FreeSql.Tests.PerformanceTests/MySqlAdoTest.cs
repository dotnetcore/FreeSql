using FreeSql.DataAnnotations;
using System;
using System.Diagnostics;
using System.Text;
using Xunit;
using Dapper;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace FreeSql.Tests.PerformanceTest
{
    public class MySqlAdoTest
    {

        [Fact]
        public void Query()
        {
            var sb = new StringBuilder();
            var time = new Stopwatch();

            time.Restart();
            List<xxx> dplist1 = null;
            using (var conn = g.mysql.Ado.MasterPool.Get())
            {
                dplist1 = Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from song").ToList();
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {dplist1.Count}; ORM: Dapper");

            time.Restart();
            List<(int, string, string)> dplist2 = null;
            using (var conn = g.mysql.Ado.MasterPool.Get())
            {
                dplist2 = Dapper.SqlMapper.Query<(int, string, string)>(conn.Value, "select * from song").ToList();
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {dplist2.Count}; ORM: Dapper");

            time.Restart();
            List<dynamic> dplist3 = null;
            using (var conn = g.mysql.Ado.MasterPool.Get())
            {
                dplist3 = Dapper.SqlMapper.Query<dynamic>(conn.Value, "select * from song").ToList();
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Dynamic Counts: {dplist3.Count}; ORM: Dapper");




            var t31 = g.mysql.Ado.Query<xxx>("select * from song limit 1");

            time.Restart();
            var t3 = g.mysql.Ado.Query<xxx>("select * from song");
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {t3.Count}; ORM: FreeSql*");

            time.Restart();
            var t4 = g.mysql.Ado.Query<(int, string, string)>("select * from song");
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {t4.Count}; ORM: FreeSql*");

            time.Restart();
            var t41 = g.mysql.Select<xxx>().ToList<(int, string, string)>("id,title,url");
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query ToList<Tuple> Counts: {t41.Count}; ORM: FreeSql*");

            time.Restart();
            var t5 = g.mysql.Ado.Query<dynamic>("select * from song");
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Dynamic Counts: {t3.Count}; ORM: FreeSql*");

        }

        [Fact]
        public void QueryLimit10()
        {
            var sb = new StringBuilder();
            var time = new Stopwatch();

            time.Restart();
            List<xxx> dplist1 = new List<xxx>();
            for (var a = 0; a < 10000; a++)
            {
                using (var conn = g.mysql.Ado.MasterPool.Get())
                {
                    dplist1.AddRange(Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from song limit 10").ToList());
                }
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {dplist1.Count}; ORM: Dapper");

            time.Restart();
            List<(int, string, string)> dplist2 = new List<(int, string, string)>();
            for (var a = 0; a < 10000; a++)
            {
                using (var conn = g.mysql.Ado.MasterPool.Get())
                {
                    dplist2.AddRange(Dapper.SqlMapper.Query<(int, string, string)>(conn.Value, "select * from song limit 10").ToList());
                }
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {dplist2.Count}; ORM: Dapper");

            time.Restart();
            List<dynamic> dplist3 = new List<dynamic>();
            for (var a = 0; a < 10000; a++)
            {
                using (var conn = g.mysql.Ado.MasterPool.Get())
                {
                    dplist3.AddRange(Dapper.SqlMapper.Query<dynamic>(conn.Value, "select * from song limit 10").ToList());
                }
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Dynamic Counts: {dplist3.Count}; ORM: Dapper");




            time.Restart();
            List<xxx> t3 = new List<xxx>();
            for (var a = 0; a < 10000; a++)
            {
                t3.AddRange(g.mysql.Ado.Query<xxx>("select * from song limit 10"));
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {t3.Count}; ORM: FreeSql*");

            time.Restart();
            List<(int, string, string)> t4 = new List<(int, string, string)>();
            for (var a = 0; a < 10000; a++)
            {
                t4.AddRange(g.mysql.Ado.Query<(int, string, string)>("select * from song limit 10"));
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {t4.Count}; ORM: FreeSql*");

            time.Restart();
            List<dynamic> t5 = new List<dynamic>();
            for (var a = 0; a < 10000; a++)
            {
                t5.AddRange(g.mysql.Ado.Query<dynamic>("select * from song limit 10"));
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Dynamic Counts: {t3.Count}; ORM: FreeSql*");

        }

        [Fact]
        public void ToList()
        {
            var sb = new StringBuilder();
            var time = new Stopwatch();

            //var t31 = g.mysql.Select<xxx>().ToList();

            time.Restart();
            var t3 = g.mysql.Select<xxx>().ToList();
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; ToList Entity Counts: {t3.Count}; ORM: FreeSql*");

            time.Restart();
            List<xxx> dplist1 = null;
            using (var conn = g.mysql.Ado.MasterPool.Get())
            {
                dplist1 = Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from song").ToList();
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {dplist1.Count}; ORM: Dapper");
        }

        [Fact]
        public void ToListLimit10()
        {
            var sb = new StringBuilder();
            var time = new Stopwatch();

            time.Restart();
            var t3Count = 0;
            var p3 = Parallel.For(1, 50, b =>
            {
                List<xxx> t3 = new List<xxx>();
                for (var a = 0; a < 1000; a++)
                {
                    t3.AddRange(g.mysql.Select<xxx>().Limit(50).ToList());
                }
                Interlocked.Add(ref t3Count, t3.Count);
            });
            while (p3.IsCompleted == false) ;
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; ToList Entity Counts: {t3Count}; ORM: FreeSql*");

            time.Restart();
            var dplist1Count = 0;
            var p1 = Parallel.For(1, 50, b =>
            {
                List<xxx> dplist1 = new List<xxx>();
                for (var a = 0; a < 1000; a++)
                {
                    using (var conn = g.mysql.Ado.MasterPool.Get())
                    {
                        dplist1.AddRange(Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from song limit 50").ToList());
                    }
                }
                Interlocked.Add(ref dplist1Count, dplist1.Count);
            });
            while (p1.IsCompleted == false) ;
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {dplist1Count}; ORM: Dapper");
        }

        [Table(Name = "song")]
        class xxx
        {
            public int Id { get; set; }
            public string Title { get; set; }
            //public string Url { get; set; }
            public DateTime Create_time { get; set; }
            public bool Is_deleted { get; set; }
        }
    }
}
