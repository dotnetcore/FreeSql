using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using restful.Entitys;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Text;

namespace repository_01 {
	public class Startup {
		public Startup(IConfiguration configuration, ILoggerFactory loggerFactory) {
			Configuration = configuration;

			Fsql = new FreeSql.FreeSqlBuilder()
				.UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Pooling=true;Max Pool Size=10")
				.UseLogger(loggerFactory.CreateLogger<IFreeSql>())
				.UseAutoSyncStructure(true)
				.Build();
		}

		public IConfiguration Configuration { get; }
		public IFreeSql Fsql { get; }

		public IServiceProvider ConfigureServices(IServiceCollection services) {

			services.AddSingleton<IFreeSql>(Fsql);
			//services.AddTransient(s => s.)

			services.AddMvc();
			services.AddSwaggerGen(options => {
				options.SwaggerDoc("v1", new Info {
					Version = "v1",
					Title = "FreeSql.RESTful API"
				});
				//options.IncludeXmlComments(xmlPath);
			});

			var builder = new ContainerBuilder();

			builder.RegisterFreeRepository<Song>(a => a.Id == 1);
			builder.RegisterFreeGuidRepository<Song>(a => a.Id == 1, oldname => $"{oldname}_{DateTime.Now.Year}");

			builder.Populate(services);
			var container = builder.Build();

			return new AutofacServiceProvider(container);
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
