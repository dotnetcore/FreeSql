using FreeSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Text;

namespace domain_01 {
	public class Startup {
		public Startup(IConfiguration configuration, ILoggerFactory loggerFactory) {
			Configuration = configuration;

			g.orm = new FreeSql.FreeSqlBuilder()
				.UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Pooling=true;Max Pool Size=10")
				.UseLogger(loggerFactory.CreateLogger<IFreeSql>())
				.UseAutoSyncStructure(true)
				.Build();
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services) {
			services.AddSingleton<IFreeSql>(g.orm);

			services.AddMvc();
			services.AddSwaggerGen(options => {
				options.SwaggerDoc("v1", new Info {
					Version = "v1",
					Title = "FreeSql.domain_01 API"
				});
				//options.IncludeXmlComments(xmlPath);
			});
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
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreeSql.domain_01 API V1");
			});
		}
	}
}

public static class g {
	public static IFreeSql orm { get; set; }
}