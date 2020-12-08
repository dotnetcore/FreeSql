using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using SqlSugar;

namespace FreeSql.Bechmarker
{

    public class Program
    {
        public static void Main(string[] args)
        {
            //var summaryInsert = BenchmarkRunner.Run<OrmVsInsert>();
            var summarySelect = BenchmarkRunner.Run<OrmVsSelect>();
            //var summaryUpdate = BenchmarkRunner.Run<OrmVsUpdate>();

            Console.ReadKey();
            Console.ReadKey();
        }
    }

    public class Orm
    {
        public static IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.SqlServer, 
                    "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=20",
                    typeof(FreeSql.SqlServer.SqlServerProvider<>))
                //.UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=20")
                .UseAutoSyncStructure(false)
                .UseNoneCommandParameter(true)
                //.UseConfigEntityFromDbFirst(true)
                .Build();

        public static SqlSugarClient sugar
        {
            get => new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=20;Max Pool Size=20",
                DbType = DbType.SqlServer,
                //ConnectionString = "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Min Pool Size=20;Max Pool Size=20",
                //DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }
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

    [RPlotExporter, RankColumn]
    public class OrmVsInsert
    {
        public IEnumerable<Song> songs;

        [Params(1, 500)]
        public int size;

        [GlobalSetup]
        public void Setup()
        {
            Orm.fsql.CodeFirst.SyncStructure(typeof(Song), "freesql_song");
            Orm.fsql.CodeFirst.SyncStructure(typeof(Song), "sugar_song");
            Orm.fsql.CodeFirst.SyncStructure(typeof(Song), "efcore_song");
            Orm.fsql.CodeFirst.SyncStructure(typeof(Song), "dapper_song");

            //测试前清空数据
            Orm.fsql.Delete<Song>().Where(a => a.Id > 0).ExecuteAffrows();
            Orm.sugar.Deleteable<Song>().Where(a => a.Id > 0).ExecuteCommand();
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
            Orm.sugar.Insertable(songs.First()).ExecuteCommand();
            using (var db = new SongContext())
            {
                //db.Configuration.AutoDetectChangesEnabled = false;
                db.Songs.AddRange(songs.First());
                db.SaveChanges();
            }
        }

        [Benchmark]
        public int FreeSqlInsert() => Orm.fsql.Insert(songs).ExecuteAffrows();

        [Benchmark]
        public int SqlSugarInsert() => Orm.sugar.Insertable(songs.ToArray()).ExecuteCommand();

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

        [Benchmark]
        public int DapperInsert()
        {
            using (var conn = new SqlConnection("Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=21;Max Pool Size=31"))
            {
                foreach (var song in songs)
                {
                    Dapper.SqlMapper.Execute(conn, @$"insert into dapper_song(Create_time,Is_deleted,Title,Url) 
values('{song.Create_time.Value.ToString("yyyy-MM-dd HH:mm:ss")}',{(song.Is_deleted == true ? 1 : 0)},'{song.Title}','{song.Url}')");
                }
            }
            return songs.Count();
        }
    }

    [RPlotExporter, RankColumn]
    public class OrmVsUpdate
    {
        public List<Song> songs;

        [Params(1, 500)]
        public int size;

        [GlobalSetup]
        public void Setup()
        {
            songs = Orm.fsql.Select<Song>().Limit(size).ToList();
        }

        [Benchmark]
        public int FreeSqlUpdate() => Orm.fsql.Update<Song>().SetSource(songs).ExecuteAffrows();

        [Benchmark]
        public int SqlSugarUpdate() => Orm.sugar.Updateable(songs).ExecuteCommand();

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

        [Benchmark]
        public int DapperUpdate()
        {
            using (var conn = new SqlConnection("Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=21;Max Pool Size=31"))
            {
                foreach (var song in songs)
                {
                    Dapper.SqlMapper.Execute(conn, @$"update dapper_song set
Create_time = '{song.Create_time.Value.ToString("yyyy-MM-dd HH:mm:ss")}',
Is_deleted = {(song.Is_deleted == true ? 1 : 0)},
Title = '{song.Title}',
Url = '{song.Url}'
where id = {song.Id}");
                }
            }
            return songs.Count();
        }
    }

    [RPlotExporter, RankColumn]
    public class OrmVsSelect
    {

        [Params(1, 500)]
        public int size;

        [IterationSetup]
        public void Setup2()
        {
            Orm.fsql.Select<Song>().Limit(1).ToList();
            Orm.sugar.Queryable<Song>().Take(1).ToList();
            using (var db = new SongContext())
            {
                db.Songs.Take(1).AsNoTracking().ToList();
            }
            using (var conn = new SqlConnection("Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=21;Max Pool Size=31"))
            {
                Dapper.SqlMapper.Query<Song>(conn, $"select top {1} Id,Create_time,Is_deleted,Title,Url from dapper_song").ToList();
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            Orm.fsql.CodeFirst.SyncStructure(typeof(Song), "freesql_song");
            Orm.fsql.CodeFirst.SyncStructure(typeof(Song), "sugar_song");
            Orm.fsql.CodeFirst.SyncStructure(typeof(Song), "efcore_song");
            Orm.fsql.CodeFirst.SyncStructure(typeof(Song), "dapper_song");

            //测试前清空数据
            Orm.fsql.Delete<Song>().Where(a => a.Id > 0).ExecuteAffrows();
            Orm.sugar.Deleteable<Song>().Where(a => a.Id > 0).ExecuteCommand();
            Orm.fsql.Ado.ExecuteNonQuery("delete from efcore_song");
            Orm.fsql.Ado.ExecuteNonQuery("delete from dapper_song");

            var songs = Enumerable.Range(0, size).Select(a => new Song
            {
                Create_time = DateTime.Now,
                Is_deleted = false,
                Title = $"Insert_{a}",
                Url = $"Url_{a}"
            });

            //预热
            Orm.fsql.Insert(songs).ExecuteAffrows();
            Orm.sugar.Insertable(songs.ToArray()).ExecuteCommand();
            using (var db = new SongContext())
            {
                //db.Configuration.AutoDetectChangesEnabled = false;
                db.Songs.AddRange(songs);
                db.SaveChanges();
            }
            using (var conn = new SqlConnection("Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=21;Max Pool Size=31"))
            {
                foreach (var song in songs)
                {
                    Dapper.SqlMapper.Execute(conn, @$"insert into dapper_song(Create_time,Is_deleted,Title,Url) 
values('{song.Create_time.Value.ToString("yyyy-MM-dd HH:mm:ss")}',{(song.Is_deleted == true ? 1 : 0)},'{song.Title}','{song.Url}')");
                }
            }
        }

        [Benchmark]
        public List<Song> FreeSqlSelect() => Orm.fsql.Select<Song>().Limit(size).ToList();

        [Benchmark]
        public List<Song> SqlSugarSelect() => Orm.sugar.Queryable<Song>().Take(size).ToList();

        [Benchmark]
        public List<Song> EfCoreSelect()
        {
            using (var db = new SongContext())
            {
                return db.Songs.Take(size).AsNoTracking().ToList();
            }
        }

        [Benchmark]
        public List<Song> DapperSelete()
        {
            using (var conn = new SqlConnection("Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=21;Max Pool Size=31"))
            {
                return Dapper.SqlMapper.Query<Song>(conn, $"select top {size} Id,Create_time,Is_deleted,Title,Url from dapper_song").ToList();
            }
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
    }
}

