using Microsoft.EntityFrameworkCore;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace orm_vs
{
    class Program
    {
        static IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                //.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=20")
                .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=20")
                .UseAutoSyncStructure(false)
                .UseNoneCommandParameter(true)
                //.UseConfigEntityFromDbFirst(true)
                .Build();

        static SqlSugarClient sugar
        {
            get => new SqlSugarClient(new ConnectionConfig()
            {
                //不欺负，让连接池100个最小
                //ConnectionString = "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=20;Max Pool Size=20",
                //DbType = DbType.SqlServer,
                ConnectionString = "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Min Pool Size=20;Max Pool Size=20",
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        class SongContext : DbContext
        {
            public DbSet<Song> Songs { get; set; }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                //optionsBuilder.UseSqlServer(@"Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=21;Max Pool Size=21");
                optionsBuilder.UseMySql("Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Min Pool Size=21;Max Pool Size=21");
            }
        }

        static void Main(string[] args)
        {

            fsql.CodeFirst.SyncStructure(typeof(Song), typeof(Song_tag), typeof(Tag));
            //sugar.CodeFirst.InitTables(typeof(Song), typeof(Song_tag), typeof(Tag));
            //sugar创建表失败：SqlSugar.SqlSugarException: Sequence contains no elements

            sugar.Aop.OnLogExecuted = (s, e) =>
            {
                Trace.WriteLine(s);
            };
            //测试前清空数据
            fsql.Delete<Song>().Where(a => a.Id > 0).ExecuteAffrows();
            sugar.Deleteable<Song>().Where(a => a.Id > 0).ExecuteCommand();
            fsql.Ado.ExecuteNonQuery("delete from efcore_song");

            var sb = new StringBuilder();
            Console.WriteLine("插入性能：");
            Insert(sb, 1000, 1);
            Console.Write(sb.ToString());
            sb.Clear();
            Insert(sb, 1000, 10);
            Console.Write(sb.ToString());
            sb.Clear();

            Insert(sb, 1, 1000);
            Console.Write(sb.ToString());
            sb.Clear();
            Insert(sb, 1, 10000);
            Console.Write(sb.ToString());
            sb.Clear();
            Insert(sb, 1, 50000);
            Console.Write(sb.ToString());
            sb.Clear();
            Insert(sb, 1, 100000);
            Console.Write(sb.ToString());
            sb.Clear();

            Console.WriteLine("查询性能：");
            Select(sb, 1000, 1);
            Console.Write(sb.ToString());
            sb.Clear();
            Select(sb, 1000, 10);
            Console.Write(sb.ToString());
            sb.Clear();

            Select(sb, 1, 1000);
            Console.Write(sb.ToString());
            sb.Clear();
            Select(sb, 1, 10000);
            Console.Write(sb.ToString());
            sb.Clear();
            Select(sb, 1, 50000);
            Console.Write(sb.ToString());
            sb.Clear();
            Select(sb, 1, 100000);
            Console.Write(sb.ToString());
            sb.Clear();

            Console.WriteLine("更新：");
            Update(sb, 1000, 1);
            Console.Write(sb.ToString());
            sb.Clear();
            Update(sb, 1000, 10);
            Console.Write(sb.ToString());
            sb.Clear();

            Update(sb, 1, 1000);
            Console.Write(sb.ToString());
            sb.Clear();
            Update(sb, 1, 10000);
            Console.Write(sb.ToString());
            sb.Clear();
            Update(sb, 1, 50000);
            Console.Write(sb.ToString());
            sb.Clear();
            Update(sb, 1, 100000);
            Console.Write(sb.ToString());
            sb.Clear();

            Console.WriteLine("测试结束，按任意键退出...");
            Console.ReadKey();
        }

        static void Select(StringBuilder sb, int forTime, int size)
        {
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
            sb.AppendLine($"SqlSugar Select {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms");

            sw.Restart();
            for (var a = 0; a < forTime; a++)
            {
                using (var db = new SongContext())
                {
                    db.Songs.Take(size).AsNoTracking().ToList();
                }
            }
            sw.Stop();
            sb.AppendLine($"EFCore Select {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms\r\n");
        }

        static void Insert(StringBuilder sb, int forTime, int size)
        {
            var songs = Enumerable.Range(0, size).Select(a => new Song
            {
                Create_time = DateTime.Now,
                Is_deleted = false,
                Title = $"Insert_{a}",
                Url = $"Url_{a}"
            });

            //预热
            fsql.Insert(songs.First()).ExecuteAffrows();
            sugar.Insertable(songs.First()).ExecuteCommand();
            using (var db = new SongContext())
            {
                //db.Configuration.AutoDetectChangesEnabled = false;
                db.Songs.AddRange(songs.First());
                db.SaveChanges();
            }
            Stopwatch sw = new Stopwatch();

            sw.Restart();
            for (var a = 0; a < forTime; a++)
            {
                fsql.Insert(songs).ExecuteAffrows();
                //using (var db = new FreeSongContext()) {
                //	//db.Configuration.AutoDetectChangesEnabled = false;
                //	db.Songs.AddRange(songs.ToArray());
                //	db.SaveChanges();
                //}
            }
            sw.Stop();
            sb.AppendLine($"FreeSql Insert {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms");

            sw.Restart();
            Exception sugarEx = null;
            try
            {
                for (var a = 0; a < forTime; a++)
                    sugar.Insertable(songs.ToArray()).ExecuteCommand();
            }
            catch (Exception ex)
            {
                sugarEx = ex;
            }
            sw.Stop();
            sb.AppendLine($"SqlSugar Insert {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms" + (sugarEx != null ? $"成绩无效，错误：{sugarEx.Message}" : ""));

            sw.Restart();
            for (var a = 0; a < forTime; a++)
            {

                using (var db = new SongContext())
                {
                    //db.Configuration.AutoDetectChangesEnabled = false;
                    db.Songs.AddRange(songs.ToArray());
                    db.SaveChanges();
                }
            }
            sw.Stop();
            sb.AppendLine($"EFCore Insert {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms\r\n");
        }

        static void Update(StringBuilder sb, int forTime, int size)
        {
            Stopwatch sw = new Stopwatch();

            var songs = fsql.Select<Song>().Limit(size).ToList();
            sw.Restart();
            for (var a = 0; a < forTime; a++)
            {
                fsql.Update<Song>().SetSource(songs).ExecuteAffrows();
            }
            sw.Stop();
            sb.AppendLine($"FreeSql Update {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms");

            songs = sugar.Queryable<Song>().Take(size).ToList();
            sw.Restart();
            Exception sugarEx = null;
            try
            {
                for (var a = 0; a < forTime; a++)
                    sugar.Updateable(songs).ExecuteCommand();
            }
            catch (Exception ex)
            {
                sugarEx = ex;
            }
            sw.Stop();
            sb.AppendLine($"SqlSugar Update {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms" + (sugarEx != null ? $"成绩无效，错误：{sugarEx.Message}" : ""));

            using (var db = new SongContext())
            {
                songs = db.Songs.Take(size).AsNoTracking().ToList();
            }
            sw.Restart();
            for (var a = 0; a < forTime; a++)
            {

                using (var db = new SongContext())
                {
                    //db.Configuration.AutoDetectChangesEnabled = false;
                    db.Songs.UpdateRange(songs.ToArray());
                    db.SaveChanges();
                }
            }
            sw.Stop();
            sb.AppendLine($"EFCore Update {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms\r\n");
        }
    }

    [FreeSql.DataAnnotations.Table(Name = "freesql_song")]
    [SugarTable("sugar_song")]
    [Table("efcore_song")]
    public class Song
    {
        [FreeSql.DataAnnotations.Column(IsIdentity = true)]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime? Create_time { get; set; }
        public bool? Is_deleted { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }

        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual ICollection<Tag> Tags { get; set; }
    }
    [FreeSql.DataAnnotations.Table(Name = "freesql_song_tag")]
    [SugarTable("sugar_song_tag")]
    [Table("efcore_song_tag")]
    public class Song_tag
    {
        public int Song_id { get; set; }
        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual Song Song { get; set; }

        public int Tag_id { get; set; }
        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual Tag Tag { get; set; }
    }
    [FreeSql.DataAnnotations.Table(Name = "freesql_tag")]
    [SugarTable("sugar_tag")]
    [Table("efcore_tag")]
    public class Tag
    {
        [FreeSql.DataAnnotations.Column(IsIdentity = true)]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? Parent_id { get; set; }
        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual Tag Parent { get; set; }

        public decimal? Ddd { get; set; }
        public string Name { get; set; }

        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual ICollection<Song> Songs { get; set; }
        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual ICollection<Tag> Tags { get; set; }
    }
}
