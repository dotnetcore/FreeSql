using FreeSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

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
                //.UseConnectionString(DataType.MySql, "Data Source=192.168.164.10;Port=33061;User ID=root;Password=123456;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=5")
                //.UseSlave("Data Source=192.168.164.10;Port=33062;User ID=root;Password=123456;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=5")
                //.UseConnectionString(FreeSql.DataType.Oracle, "user id=user1;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=10")
                //.UseSyncStructureToUpper(true)
                .UseAutoSyncStructure(true)
                .UseLazyLoading(true)
                .UseNoneCommandParameter(true)
                .UseMonitorCommand(cmd => { }, (cmd, log) => Trace.WriteLine(log))
                .Build();

            Fsql.Aop.CurdAfter += (s, e) =>
            {
                Console.WriteLine(e.Identifier + ": " + e.EntityType.FullName + " " + e.ElapsedMilliseconds + "ms, " + e.Sql);
                CurdAfterLog.Current.Value?.Sb.AppendLine($"{Thread.CurrentThread.ManagedThreadId}: {e.EntityType.FullName} {e.ElapsedMilliseconds}ms, {e.Sql}");
            };

        }

        enum MySql { }
        enum PgSql { }

        public IConfiguration Configuration { get; }
        public static IFreeSql Fsql { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton<IFreeSql>(Fsql);
            services.AddFreeDbContext<SongContext>(options => options.UseFreeSql(Fsql));
            services.AddScoped<CurdAfterLog>();
        }

        public void Configure(IApplicationBuilder app)
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

    public class CurdAfterLog : IDisposable
    {
        public static AsyncLocal<CurdAfterLog> Current = new AsyncLocal<CurdAfterLog>();
        public StringBuilder Sb { get; } = new StringBuilder();

        public CurdAfterLog()
        {
            Current.Value = this;
        }
        public void Dispose()
        {
            Sb.Clear();
            Current.Value = null;
        }
    }
}
