using FreeSql.DataAnnotations;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace orm_vs
{
    class Program
    {
		static IFreeSql fsql = new FreeSql.FreeSqlBuilder()
				.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=20")
				//.UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=20")
				.UseAutoSyncStructure(false)
				.UseNoneCommandParameter(true)
				//.UseConfigEntityFromDbFirst(true)
				.Build();

		static SqlSugarClient sugar {
			get => new SqlSugarClient(new ConnectionConfig() {
				//不欺负，让连接池100个最小
				ConnectionString = "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=20;Max Pool Size=20",
				DbType = DbType.SqlServer,
				//ConnectionString = "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Min Pool Size=20;Max Pool Size=20",
				//DbType = DbType.MySql,
				IsAutoCloseConnection = true,
				InitKeyType = InitKeyType.Attribute
			});
		}

		static StringBuilder sb = new StringBuilder();

		static void Main(string[] args) {

			//fsql.CodeFirst.SyncStructure(typeof(Song), typeof(Song_tag), typeof(Tag));
			//sugar.CodeFirst.InitTables(typeof(Song), typeof(Song_tag), typeof(Tag));
			//sugar创建表失败：SqlSugar.SqlSugarException: Sequence contains no elements

			//测试前清空数据
			fsql.Delete<Song>().Where(a => a.Id > 0).ExecuteAffrows();
			sugar.Deleteable<Song>().Where(a => a.Id > 0).ExecuteCommand();

			Console.WriteLine("插入性能：");
			Insert(1000, 1);
			Console.Write(sb.ToString());
			sb.Clear();
			Insert(1000, 10);
			Console.Write(sb.ToString());
			sb.Clear();

			Insert(1, 1000);
			Console.Write(sb.ToString());
			sb.Clear();
			Insert(1, 10000);
			Console.Write(sb.ToString());
			sb.Clear();
			Insert(1, 50000);
			Console.Write(sb.ToString());
			sb.Clear();

			Console.WriteLine("查询性能：");
			Select(1000, 1);
			Console.Write(sb.ToString());
			sb.Clear();
			Select(1000, 10);
			Console.Write(sb.ToString());
			sb.Clear();

			Select(1, 1000);
			Console.Write(sb.ToString());
			sb.Clear();
			Select(1, 10000);
			Console.Write(sb.ToString());
			sb.Clear();
			Select(1, 50000);
			Console.Write(sb.ToString());
			sb.Clear();
			Select(1, 100000);
			Console.Write(sb.ToString());
			sb.Clear();

			Console.Write(sb.ToString());
			Console.WriteLine("测试结束，按任意键退出...");
			Console.ReadKey();
		}

		static void Select(int forTime, int size) {
			Stopwatch sw = new Stopwatch();

			sw.Restart();
			for (var a = 0; a < forTime; a++)
				fsql.Select<Song>().Limit(size).ToList();
			sw.Stop();
			sb.AppendLine($"FreeSql Select {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms");

			sw.Restart();
			for (var a = 0; a < forTime; a++)
				sugar.Queryable<Song>().Take(size).ToList();
			sw.Stop();
			sb.AppendLine($"SqlSugar Select {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms\r\n");
		}

		static void Insert(int forTime, int size) {
			var songs = Enumerable.Range(0, size).Select(a => new Song {
				Create_time = DateTime.Now,
				Is_deleted = false,
				Title = $"Insert_{a}",
				Url = $"Url_{a}"
			}).ToArray();

			//预热
			fsql.Insert(songs.First()).ExecuteAffrows();
			sugar.Insertable(songs.First()).ExecuteCommand();
			Stopwatch sw = new Stopwatch();

			sw.Restart();
			for (var a = 0; a < forTime; a++)
				fsql.Insert(songs).ExecuteAffrows();
			sw.Stop();
			sb.AppendLine($"FreeSql Insert {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms");

			sw.Restart();
			for (var a = 0; a < forTime; a++)
				sugar.Insertable(songs).ExecuteCommand();
			sw.Stop();

			sb.AppendLine($"SqlSugar Insert {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms\r\n");
		}
    }

	[Table(Name = "freesql_song")]
	[SugarTable("sugar_song")]
	public class Song {
		[Column(IsIdentity = true)]
		[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
		public int Id { get; set; }
		public DateTime? Create_time { get; set; }
		public bool? Is_deleted { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }

		[SugarColumn(IsIgnore = true)]
		public virtual ICollection<Tag> Tags { get; set; }
	}
	[Table(Name = "freesql_song_tag")]
	[SugarTable("sugar_song_tag")]
	public class Song_tag {
		public int Song_id { get; set; }
		[SugarColumn(IsIgnore = true)]
		public virtual Song Song { get; set; }

		public int Tag_id { get; set; }
		[SugarColumn(IsIgnore = true)]
		public virtual Tag Tag { get; set; }
	}
	[Table(Name = "freesql_tag")]
	[SugarTable("sugar_tag")]
	public class Tag {
		[Column(IsIdentity = true)]
		[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
		public int Id { get; set; }
		public int? Parent_id { get; set; }
		[SugarColumn(IsIgnore = true)]
		public virtual Tag Parent { get; set; }

		public decimal? Ddd { get; set; }
		public string Name { get; set; }

		[SugarColumn(IsIgnore = true)]
		public virtual ICollection<Song> Songs { get; set; }
		[SugarColumn(IsIgnore = true)]
		public virtual ICollection<Tag> Tags { get; set; }
	}
}
