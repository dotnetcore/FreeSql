using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FreeSql;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace dbcontext_01
{
    public class Program
    {

        public class Song
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string BigNumber { get; set; }

            [Column(IsVersion = true)]//使用简单
            public long versionRow { get; set; }
        }

        public class SongContext : DbContext
        {

            public DbSet<Song> Songs { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder builder)
            {
                builder.UseFreeSql(fsql);
            }
        }
        static IFreeSql fsql;
        public static void Main(string[] args)
        {
            fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\dd2.db;Pooling=true;Max Pool Size=10")
                .UseAutoSyncStructure(true)
                .UseLazyLoading(true)
                .UseNoneCommandParameter(true)

                .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
                .Build();


            using (var ctx = new SongContext())
            {
                var song = new Song { BigNumber = "1000000000000000000" };
                ctx.Songs.Add(song);

                ctx.Songs.Update(song);

                song.BigNumber = (BigInteger.Parse(song.BigNumber) + 1).ToString();
                ctx.Songs.Update(song);

                ctx.SaveChanges();

                var sql = fsql.Update<Song>().SetSource(song).ToSql();
            }

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
