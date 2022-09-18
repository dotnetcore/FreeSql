using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using aspnetcore_transaction.Controllers;
using FreeSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace aspnetcore_transaction
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Fsql = new FreeSqlBuilder()
                 .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\test_trans.db")
                 .UseAutoSyncStructure(true)
                 .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
                 .UseNoneCommandParameter(true)
                 .Build();

            Fsql.Aop.TraceBefore += (_, e) => Trace.WriteLine($"----TraceBefore---{e.Identifier} {e.Operation}");
            Fsql.Aop.TraceAfter += (_, e) => Trace.WriteLine($"----TraceAfter---{e.Identifier} {e.Operation} {e.Remark} {e.Exception?.Message} {e.ElapsedMilliseconds}ms\r\n");
        }

        public IConfiguration Configuration { get; }
        public static IFreeSql Fsql { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton<IFreeSql>(Fsql);
            services.AddScoped<UnitOfWorkManager>();

            //批量注入
            foreach (var repo in typeof(Startup).Assembly.GetTypes()
                .Where(a => a.IsAbstract == false && typeof(IBaseRepository).IsAssignableFrom(a)))
                services.AddScoped(repo);
            services.AddScoped<SongService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.GetEncoding("GB2312");
            Console.InputEncoding = Encoding.GetEncoding("GB2312");

            app.Use(async (context, next) =>
            {
                TransactionalAttribute.SetServiceProvider(context.RequestServices);
                await next();
            });

            app.UseHttpMethodOverride(new HttpMethodOverrideOptions { FormFieldName = "X-Http-Method-Override" });
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseEndpoints(a => a.MapControllers());
        }
    }
}
