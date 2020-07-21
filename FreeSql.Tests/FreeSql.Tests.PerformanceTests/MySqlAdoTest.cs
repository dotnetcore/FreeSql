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
using FreeSql.Internal;

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
                dplist1 = Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from freesql_song").ToList();
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {dplist1.Count}; ORM: Dapper");

            time.Restart();
            List<(int, string, string)> dplist2 = null;
            using (var conn = g.mysql.Ado.MasterPool.Get())
            {
                dplist2 = Dapper.SqlMapper.Query<(int, string, string)>(conn.Value, "select * from freesql_song").ToList();
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {dplist2.Count}; ORM: Dapper");

            time.Restart();
            List<dynamic> dplist3 = null;
            using (var conn = g.mysql.Ado.MasterPool.Get())
            {
                dplist3 = Dapper.SqlMapper.Query<dynamic>(conn.Value, "select * from freesql_song").ToList();
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Dynamic Counts: {dplist3.Count}; ORM: Dapper");




            var t31 = g.mysql.Ado.Query<xxx>("select * from freesql_song limit 1");

            time.Restart();
            var t3 = g.mysql.Ado.Query<xxx>("select * from freesql_song");
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {t3.Count}; ORM: FreeSql*");

            time.Restart();
            var t4 = g.mysql.Ado.Query<(int, string, string)>("select * from freesql_song");
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {t4.Count}; ORM: FreeSql*");

            time.Restart();
            var t41 = g.mysql.Select<xxx>().ToList<(int, string, string)>("id,title,url");
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query ToList<Tuple> Counts: {t41.Count}; ORM: FreeSql*");

            time.Restart();
            var t5 = g.mysql.Ado.Query<dynamic>("select * from freesql_song");
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
                    dplist1.AddRange(Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from freesql_song limit 10").ToList());
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
                    dplist2.AddRange(Dapper.SqlMapper.Query<(int, string, string)>(conn.Value, "select * from freesql_song limit 10").ToList());
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
                    dplist3.AddRange(Dapper.SqlMapper.Query<dynamic>(conn.Value, "select * from freesql_song limit 10").ToList());
                }
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Dynamic Counts: {dplist3.Count}; ORM: Dapper");




            time.Restart();
            List<xxx> t3 = new List<xxx>();
            for (var a = 0; a < 10000; a++)
            {
                t3.AddRange(g.mysql.Ado.Query<xxx>("select * from freesql_song limit 10"));
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {t3.Count}; ORM: FreeSql*");

            time.Restart();
            List<(int, string, string)> t4 = new List<(int, string, string)>();
            for (var a = 0; a < 10000; a++)
            {
                t4.AddRange(g.mysql.Ado.Query<(int, string, string)>("select * from freesql_song limit 10"));
            }
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {t4.Count}; ORM: FreeSql*");

            time.Restart();
            List<dynamic> t5 = new List<dynamic>();
            for (var a = 0; a < 10000; a++)
            {
                t5.AddRange(g.mysql.Ado.Query<dynamic>("select * from freesql_song limit 10"));
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
            g.mysql.Select<xxx>().First();

            time.Restart();
            var t3 = g.mysql.Select<xxx>().ToList();
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; ToList Entity Counts: {t3.Count}; ORM: FreeSql*");

            time.Restart();
            var adoarr1 = g.mysql.Ado.ExecuteArray("select * from freesql_song");
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteArray Entity Counts: {adoarr1.Length}; ORM: FreeSql ExecuteArray*");

            time.Restart();
            var adolist1 = new List<xxx>();
            g.mysql.Ado.ExecuteReader(fetch =>
            {
                var xim = new xxx();
                fetch.Object.GetValue(0);
                fetch.Object.GetValue(1);
                fetch.Object.GetValue(2);
                fetch.Object.GetValue(3);
                fetch.Object.GetValue(4);
                adolist1.Add(xim);
            }, "select * from freesql_song");
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReader Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReader*");

            time.Restart();
            adolist1 = new List<xxx>();
            g.mysql.Ado.ExecuteReader(fetch =>
            {
                var xim = new xxx();
                fetch.Object.GetFieldValue<int>(0);
                fetch.Object.GetFieldValue<DateTime>(1);
                fetch.Object.GetFieldValue<bool>(2);
                fetch.Object.GetFieldValue<string>(3);
                fetch.Object.GetFieldValue<string>(4);
                adolist1.Add(xim);
            }, "select * from freesql_song");
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReader0000 Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReader0000*");

            //time.Restart();
            //adolist1 = new List<xxx>();
            //g.mysql.Ado.ExecuteReader(dr =>
            //{
            //    var xim = new xxx();
            //    Utils.GetDataReaderValue(typeof(int), dr.GetValue(0));
            //    Utils.GetDataReaderValue(typeof(DateTime), dr.GetValue(1));
            //    Utils.GetDataReaderValue(typeof(bool), dr.GetValue(2));
            //    Utils.GetDataReaderValue(typeof(string), dr.GetValue(3));
            //    Utils.GetDataReaderValue(typeof(string), dr.GetValue(4));
            //    adolist1.Add(xim);
            //}, "select * from freesql_song");
            //time.Stop();
            //sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReader1111 Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReader1111*");


            time.Restart();
            List<xxx> dplist1 = null;
            using (var conn = g.mysql.Ado.MasterPool.Get())
            {
                dplist1 = Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from freesql_song").ToList();
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
                        dplist1.AddRange(Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from freesql_song limit 50").ToList());
                    }
                }
                Interlocked.Add(ref dplist1Count, dplist1.Count);
            });
            while (p1.IsCompleted == false) ;
            time.Stop();
            sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {dplist1Count}; ORM: Dapper");
        }

        [Table(Name = "freesql_song")]
        class xxx
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
            public DateTime Create_time { get; set; }
            public bool Is_deleted { get; set; }
        }
    }
}
