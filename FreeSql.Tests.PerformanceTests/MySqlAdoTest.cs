using FreeSql.DataAnnotations;
using System;
using System.Diagnostics;
using System.Text;
using Xunit;
using Dapper;
using System.Linq;
using System.Collections.Generic;

namespace FreeSql.Tests.PerformanceTest {
	public class MySqlAdoTest {

		[Fact]
		public void Query() {
			var sb = new StringBuilder();
			var time = new Stopwatch();

			time.Restart();
			List<xxx> dplist1 = null;
			using (var conn = g.mysql.Ado.MasterPool.Get()) {
				dplist1 = Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from song limit 1").ToList();
			}
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {dplist1.Count}; ORM: Dapper");

			time.Restart();
			List<(int, string, string)> dplist2 = null;
			using (var conn = g.mysql.Ado.MasterPool.Get()) {
				dplist2 = Dapper.SqlMapper.Query<(int, string, string)>(conn.Value, "select * from song").ToList();
			}
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {dplist2.Count}; ORM: Dapper");

			time.Restart();
			List<dynamic> dplist3 = null;
			using (var conn = g.mysql.Ado.MasterPool.Get()) {
				dplist3 = Dapper.SqlMapper.Query<dynamic>(conn.Value, "select * from song").ToList();
			}
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Dynamic Counts: {dplist3.Count}; ORM: Dapper");




			time.Restart();
			var t3 = g.mysql.Ado.Query<xxx>("select * from song");
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {t3.Count}; ORM: FreeSql*");

			time.Restart();
			var t4 = g.mysql.Ado.Query<(int, string, string)>("select * from song");
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {t4.Count}; ORM: FreeSql*");

			time.Restart();
			var t5 = g.mysql.Ado.Query<dynamic>("select * from song");
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Dynamic Counts: {t3.Count}; ORM: FreeSql*");

		}

		[Fact]
		public void QueryLimit10() {
			var sb = new StringBuilder();
			var time = new Stopwatch();

			time.Restart();
			List<xxx> dplist1 = new List<xxx>();
			for (var a = 0; a < 10000; a++) {
				using (var conn = g.mysql.Ado.MasterPool.Get()) {
					dplist1.AddRange(Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from song limit 10").ToList());
				}
			}
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {dplist1.Count}; ORM: Dapper");

			time.Restart();
			List<(int, string, string)> dplist2 = new List<(int, string, string)>();
			for (var a = 0; a < 10000; a++) {
				using (var conn = g.mysql.Ado.MasterPool.Get()) {
					dplist2.AddRange(Dapper.SqlMapper.Query<(int, string, string)>(conn.Value, "select * from song limit 10").ToList());
				}
			}
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {dplist2.Count}; ORM: Dapper");

			time.Restart();
			List<dynamic> dplist3 = new List<dynamic>();
			for (var a = 0; a < 10000; a++) {
				using (var conn = g.mysql.Ado.MasterPool.Get()) {
					dplist3.AddRange(Dapper.SqlMapper.Query<dynamic>(conn.Value, "select * from song limit 10").ToList());
				}
			}
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Dynamic Counts: {dplist3.Count}; ORM: Dapper");




			time.Restart();
			List<xxx> t3 = new List<xxx>();
			for (var a = 0; a < 10000; a++) {
				t3.AddRange(g.mysql.Ado.Query<xxx>("select * from song limit 10"));
			}
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {t3.Count}; ORM: FreeSql*");

			time.Restart();
			List<(int, string, string)> t4 = new List<(int, string, string)>();
			for (var a = 0; a < 10000; a++) {
				t4.AddRange(g.mysql.Ado.Query<(int, string, string)>("select * from song limit 10"));
			}
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Tuple Counts: {t4.Count}; ORM: FreeSql*");

			time.Restart();
			List<dynamic> t5 = new List<dynamic>();
			for (var a = 0; a < 10000; a++) {
				t5.AddRange(g.mysql.Ado.Query<dynamic>("select * from song limit 10"));
			}
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Dynamic Counts: {t3.Count}; ORM: FreeSql*");

		}

		[Fact]
		public void ToList() {
			var sb = new StringBuilder();
			var time = new Stopwatch();

			time.Restart();
			List<xxx> dplist1 = null;
			using (var conn = g.mysql.Ado.MasterPool.Get()) {
				dplist1 = Dapper.SqlMapper.Query<xxx>(conn.Value, "select * from song").ToList();
			}
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {dplist1.Count}; ORM: Dapper");


			time.Restart();
			var t3 = g.mysql.Select<xxx>().ToList();
			time.Stop();
			sb.AppendLine($"Elapsed: {time.Elapsed}; ToList Entity Counts: {t3.Count}; ORM: FreeSql*");
		}

		[Table(Name = "song")]
		class xxx {
			public int Id { get; set; }
			public string Title { get; set; }
			public string Url { get; set; }
			public DateTime Create_time { get; set; }
			public bool Is_deleted { get; set; }
		}
	}
}
