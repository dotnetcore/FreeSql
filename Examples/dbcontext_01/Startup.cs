using FreeSql;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace dbcontext_01
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;

            Fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document2.db;Pooling=true;Max Pool Size=10")
                //.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=10")

                //.UseConnectionString(FreeSql.DataType.Oracle, "user id=user1;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=10")
                //.UseSyncStructureToUpper(true)

                .UseAutoSyncStructure(true)
                .UseLazyLoading(true)
                .UseNoneCommandParameter(true)

                .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText),
                    (cmd, log) => Trace.WriteLine(log)
                )
                .Build();
            Fsql.Aop.SyncStructureBefore = (s, e) =>
            {
                Console.WriteLine(e.Identifier + ": " + string.Join(", ", e.EntityTypes.Select(a => a.FullName)));
            };
            Fsql.Aop.SyncStructureAfter = (s, e) =>
            {
                Console.WriteLine(e.Identifier + ": " + string.Join(", ", e.EntityTypes.Select(a => a.FullName)) + " " + e.ElapsedMilliseconds + "ms\r\n" + e.Exception?.Message + e.Sql);
            };

            Fsql.Aop.CurdBefore = (s, e) =>
            {
                Console.WriteLine(e.Identifier + ": " + e.EntityType.FullName + ", " + e.Sql);
            };
            Fsql.Aop.CurdAfter = (s, e) =>
            {
                Console.WriteLine(e.Identifier + ": " + e.EntityType.FullName + " " + e.ElapsedMilliseconds + "ms, " + e.Sql);
            };

            Fsql2 = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document222.db;Pooling=true;Max Pool Size=10")
                //.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=10")
                .UseAutoSyncStructure(true)
                .UseLazyLoading(true)

                .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText),
                    (cmd, log) => Trace.WriteLine(log)
                )
                .Build<long>();
        }

        enum MySql { }
        enum PgSql { }

        public IConfiguration Configuration { get; }
        public static IFreeSql Fsql { get; private set; }
        public static IFreeSql<long> Fsql2 { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "FreeSql.DbContext API"
                });
                //options.IncludeXmlComments(xmlPath);
            });



            services.AddSingleton<IFreeSql>(Fsql);
            services.AddSingleton<IFreeSql<long>>(Fsql2);
            services.AddFreeDbContext<SongContext>(options => options.UseFreeSql(Fsql));


            var sql1 = Fsql.Update<Song>(1).Set(a => a.Id + 10).ToSql();
            var sql2 = Fsql.Update<Song>(1).Set(a => a.Title + 10).ToSql();
            var sql3 = Fsql.Update<Song>(1).Set(a => a.Create_time.Value.AddHours(1)).ToSql();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.GetEncoding("GB2312");
            Console.InputEncoding = Encoding.GetEncoding("GB2312");

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseHttpMethodOverride(new HttpMethodOverrideOptions { FormFieldName = "X-Http-Method-Override" });
            app.UseDeveloperExceptionPage();
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreeSql.RESTful API V1");
            });
        }
    }
}
