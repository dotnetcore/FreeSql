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
				.UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Pooling=true;Max Pool Size=10")
				//.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=10")
				.UseLogger(loggerFactory.CreateLogger<IFreeSql>())
				.UseAutoSyncStructure(true)
				.UseLazyLoading(true)
				.UseNoneCommandParameter(true)

				.UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText),
					(cmd, log) => Trace.WriteLine(log)
				)
				.Build();

			Fsql2 = new FreeSql.FreeSqlBuilder()
				.UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document222.db;Pooling=true;Max Pool Size=10")
				//.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=10")
				.UseLogger(loggerFactory.CreateLogger<IFreeSql>())
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
			services.AddSwaggerGen(options => {
				options.SwaggerDoc("v1", new Info {
					Version = "v1",
					Title = "FreeSql.DbContext API"
				});
				//options.IncludeXmlComments(xmlPath);
			});



			services.AddSingleton<IFreeSql>(Fsql);
			services.AddSingleton<IFreeSql<long>>(Fsql2);
			services.AddFreeDbContext<SongContext>(options => options.UseFreeSql(Fsql));
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			Console.OutputEncoding = Encoding.GetEncoding("GB2312");
			Console.InputEncoding = Encoding.GetEncoding("GB2312");

			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			app.UseHttpMethodOverride(new HttpMethodOverrideOptions { FormFieldName = "X-Http-Method-Override" });
			app.UseDeveloperExceptionPage();
			app.UseMvc();

			app.UseSwagger();
			app.UseSwaggerUI(c => {
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreeSql.RESTful API V1");
			});
		}
	}
}
