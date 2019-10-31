using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
//using SqlSugar;

namespace FreeSql.Bechmarker
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var summaryInsert = BenchmarkRunner.Run<OrmVsInsert>();
            var summarySelect = BenchmarkRunner.Run<OrmVsSelect>();
            var summaryUpdate = BenchmarkRunner.Run<OrmVsUpdate>();
        }
    }

    public class Orm
    {
        public static IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=20")
                //.UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=20")
                .UseAutoSyncStructure(false)
                .UseNoneCommandParameter(true)
                //.UseConfigEntityFromDbFirst(true)
                .Build();

        //public static SqlSugarClient sugar {
        //	get => new SqlSugarClient(new ConnectionConfig() {
        //		//不欺负，让连接池100个最小
        //		ConnectionString = "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=20;Max Pool Size=20",
        //		DbType = DbType.SqlServer,
        //		//ConnectionString = "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Min Pool Size=20;Max Pool Size=20",
        //		//DbType = DbType.MySql,
        //		IsAutoCloseConnection = true,
        //		InitKeyType = InitKeyType.Attribute
        //	});
        //}
    }
    class SongContext : DbContext
    {
        public DbSet<Song> Songs { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=21;Max Pool Size=21");
            //optionsBuilder.UseMySql("Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Min Pool Size=21;Max Pool Size=21");
        }
    }

    [CoreJob]
    [RPlotExporter, RankColumn]
    public class OrmVsInsert
    {
        public IEnumerable<Song> songs;

        [Params(1, 500, 1000, 5000, 10000, 50000, 100000)]
        public int size;

        [GlobalSetup]
        public void Setup()
        {
            Orm.fsql.CodeFirst.SyncStructure(typeof(Song), typeof(Song_tag), typeof(Tag));
            //Orm.sugar.CodeFirst.InitTables(typeof(Song), typeof(Song_tag), typeof(Tag));
            //sugar创建表失败：SqlSugar.SqlSugarException: Sequence contains no elements

            //测试前清空数据
            Orm.fsql.Delete<Song>().Where(a => a.Id > 0).ExecuteAffrows();
            //Orm.sugar.Deleteable<Song>().Where(a => a.Id > 0).ExecuteCommand();
            Orm.fsql.Ado.ExecuteNonQuery("delete from efcore_song");

            songs = Enumerable.Range(0, size).Select(a => new Song
            {
                Create_time = DateTime.Now,
                Is_deleted = false,
                Title = $"Insert_{a}",
                Url = $"Url_{a}"
            });

            //预热
            Orm.fsql.Insert(songs.First()).ExecuteAffrows();
            //Orm.sugar.Insertable(songs.First()).ExecuteCommand();
            using (var db = new SongContext())
            {
                //db.Configuration.AutoDetectChangesEnabled = false;
                db.Songs.AddRange(songs.First());
                db.SaveChanges();
            }
        }

        [Benchmark]
        public int FreeSqlInsert() => Orm.fsql.Insert(songs).ExecuteAffrows();

        //[Benchmark]
        //public int SqlSugarInsert() => Orm.sugar.Insertable(songs.ToArray()).ExecuteCommand();

        [Benchmark]
        public int EfCoreInsert()
        {
            using (var db = new SongContext())
            {
                //db.Configuration.AutoDetectChangesEnabled = false;
                db.Songs.AddRange(songs.ToArray());
                return db.SaveChanges();
            }
        }
    }

    [CoreJob]
    [RPlotExporter, RankColumn]
    public class OrmVsUpdate
    {
        public List<Song> songs;

        [Params(1, 500, 1000, 5000, 10000, 50000, 100000)]
        public int size;

        [GlobalSetup]
        public void Setup()
        {
            songs = Orm.fsql.Select<Song>().Limit(size).ToList();
        }

        [Benchmark]
        public int FreeSqlUpdate() => Orm.fsql.Update<Song>().SetSource(songs).ExecuteAffrows();

        //[Benchmark]
        //public int SqlSugarUpdate() => Orm.sugar.Updateable(songs).ExecuteCommand();

        [Benchmark]
        public int EfCoreUpdate()
        {
            using (var db = new SongContext())
            {
                //db.Configuration.AutoDetectChangesEnabled = false;
                db.Songs.UpdateRange(songs.ToArray());
                return db.SaveChanges();
            }
        }
    }

    [CoreJob]
    [RPlotExporter, RankColumn]
    public class OrmVsSelect
    {

        [Params(1, 500, 1000, 5000, 10000, 50000, 100000)]
        public int size;

        [GlobalSetup]
        public void Setup()
        {

        }

        [Benchmark]
        public List<Song> FreeSqlSelect() => Orm.fsql.Select<Song>().Limit(size).ToList();

        //[Benchmark]
        //public List<Song> SqlSugarSelect() => Orm.sugar.Queryable<Song>().Take(size).ToList();

        [Benchmark]
        public List<Song> EfCoreSelect()
        {
            using (var db = new SongContext())
            {
                return db.Songs.Take(size).AsNoTracking().ToList();
            }
        }
    }

    [FreeSql.DataAnnotations.Table(Name = "freesql_song")]
    //[SugarTable("sugar_song")]
    [Table("efcore_song")]
    public class Song
    {
        [FreeSql.DataAnnotations.Column(IsIdentity = true)]
        //[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime? Create_time { get; set; }
        public bool? Is_deleted { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }

        //[SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual ICollection<Tag> Tags { get; set; }
    }
    [FreeSql.DataAnnotations.Table(Name = "freesql_song_tag")]
    //[SugarTable("sugar_song_tag")]
    [Table("efcore_song_tag")]
    public class Song_tag
    {
        public int Song_id { get; set; }
        //[SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual Song Song { get; set; }

        public int Tag_id { get; set; }
        //[SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual Tag Tag { get; set; }
    }
    [FreeSql.DataAnnotations.Table(Name = "freesql_tag")]
    //[SugarTable("sugar_tag")]
    [Table("efcore_tag")]
    public class Tag
    {
        [FreeSql.DataAnnotations.Column(IsIdentity = true)]
        //[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? Parent_id { get; set; }
        //[SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual Tag Parent { get; set; }

        public decimal? Ddd { get; set; }
        public string Name { get; set; }

        //[SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual ICollection<Song> Songs { get; set; }
        //[SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual ICollection<Tag> Tags { get; set; }
    }
}

