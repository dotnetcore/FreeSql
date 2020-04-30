using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aspnetcore_transaction.Controllers;
using FreeSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace aspnetcore_transaction
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Fsql = new FreeSql.FreeSqlBuilder()
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
            services.AddFreeRepository(null, typeof(Startup).Assembly);
            services.AddScoped<SongService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.GetEncoding("GB2312");
            Console.InputEncoding = Encoding.GetEncoding("GB2312");

            app.UseHttpMethodOverride(new HttpMethodOverrideOptions { FormFieldName = "X-Http-Method-Override" });
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseEndpoints(a => a.MapControllers());
        }
    }
}
