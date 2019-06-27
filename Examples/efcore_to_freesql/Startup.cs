using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using efcore_to_freesql.DBContexts;
using efcore_to_freesql.Entitys;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace efcore_to_freesql
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;

            Fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Pooling=true;Max Pool Size=10")
                .UseAutoSyncStructure(true)
                .Build();

            DBContexts.BaseDBContext.Fsql = Fsql;

            var sql11 = Fsql.Select<Topic1>().ToSql();
            //SELECT a."Id", a."Title", a."CreateTime" FROM "Topic1" a
            var sql12 = Fsql.Insert<Topic1>().AppendData(new Topic1()).ToSql();
            //INSERT INTO "Topic1"("Id", "Title", "CreateTime") VALUES(@Id0, @Title0, @CreateTime0)

            var sql21 = Fsql.Select<Topic2>().ToSql();
            //SELECT a."Id", a."Title", a."CreateTime" FROM "Topic2" a
            var sql22 = Fsql.Insert<Topic2>().AppendData(new Topic2()).ToSql();
            //INSERT INTO "Topic2"("Id", "Title", "CreateTime") VALUES(@Id0, @Title0, @CreateTime0)

            using (var db = new Topic1Context())
            {
                db.Topic1s.Add(new Topic1());
            }
            using (var db = new Topic2Context())
            {
                db.Topic2s.Add(new Topic2());
            }

            var sql13 = Fsql.Select<Topic1>().ToSql();
            //SELECT a."topic1_id", a."Title", a."CreateTime" FROM "topic1_sss" a
            var sql14 = Fsql.Insert<Topic1>().AppendData(new Topic1()).ToSql();
            //INSERT INTO "topic1_sss"("Title", "CreateTime") VALUES(@Title0, @CreateTime0)

            var sql23 = Fsql.Select<Topic2>().ToSql();
            //SELECT a."topic2_id", a."Title", a."CreateTime" FROM "topic2_sss" a
            var sql24 = Fsql.Insert<Topic2>().AppendData(new Topic2()).ToSql();
            //INSERT INTO "topic2_sss"("Title", "CreateTime") VALUES(@Title0, @CreateTime0)
        }

        public IConfiguration Configuration { get; }
        public IFreeSql Fsql { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFreeSql>(Fsql);
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();
            app.UseMvc();
        }
    }
}
