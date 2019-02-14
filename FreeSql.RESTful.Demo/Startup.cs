using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace FreeSql.RESTful.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;

			Fsql = new FreeSql.FreeSqlBuilder()
				.UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Attachs=xxxtb.db;Pooling=true;Max Pool Size=10")
				.UseLogger(loggerFactory.CreateLogger<IFreeSql>())
				.UseAutoSyncStructure(true)
				.Build();
		}

        public IConfiguration Configuration { get; }
		public IFreeSql Fsql { get; }

        public void ConfigureServices(IServiceCollection services)
        {
			services.AddSingleton<IFreeSql>(Fsql);

            services.AddMvc();
			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new Info {
					Version = "v1",
					Title = "FreeSql.RESTful API"
				});
				//options.IncludeXmlComments(xmlPath);
			});
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
