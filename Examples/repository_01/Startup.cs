using Autofac;
using Autofac.Extensions.DependencyInjection;
using FreeSql.DataAnnotations;
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

	public interface IBaseModel<TKey> {

		TKey Id { get; set; }
	}

	/// <summary>
	/// 用户密码信息
	/// </summary>
	public class SysUserLogOn : IBaseModel<Guid> {
		[Column(IsPrimary = true)]
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid SysUserId { get; set; }

		public string UserPassword { get; set; }
		[Column(DbType = "varchar(100)")]
		public string UserSecretkey { get; set; }
		public DateTime PreviousVisitTime { get; set; } = DateTime.Now;
		public DateTime LastVisitTime { get; set; } = DateTime.Now;
		public int LogOnCount { get; set; }


		public virtual SysUser SysUser { get; set; }
	}
	public class SysUser : IBaseModel<Guid> {
		[Column(IsPrimary = true)]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Column(DbType = "varchar(50)")]
		public string AccountName { get; set; }
		[Column(DbType = "varchar(50)")]
		public string Name { get; set; }
		public string HeadIcon { get; set; }
		public Gender Gender { get; set; } = Gender.Man;
		public DateTime Birthday { get; set; } = DateTime.MinValue;
		[Column(DbType = "varchar(100)")]
		public string MobilePhone { get; set; }
		public string Email { get; set; }
		public string WeChat { get; set; }
		public string Description { get; set; }
		public DateTime CreationTime { get; set; } = DateTime.Now;
		public Guid? CreateUserId { get; set; }
		public DateTime LastModifyTime { get; set; } = DateTime.Now;
		public Guid? LastModifyUserId { get; set; }




		public AccountState State { get; set; } = AccountState.Normal;

	}
	public enum Gender {
		Man = 1,
		Woman = 2,
	}

	public enum AccountState {
		/// <summary>
		/// 正常
		/// </summary>
		Normal = 1,
		/// <summary>
		/// 被禁用
		/// </summary>
		Disabled = 2,
		/// <summary>
		/// 已注销
		/// </summary>
		Closed = 3
	}
	public class Startup {
		public Startup(IConfiguration configuration, ILoggerFactory loggerFactory) {
			Configuration = configuration;

			Fsql = new FreeSql.FreeSqlBuilder()
				.UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Pooling=true;Max Pool Size=10")
				.UseLogger(loggerFactory.CreateLogger<IFreeSql>())
				.UseAutoSyncStructure(true)
				.UseLazyLoading(true)
				.Build();

			var sysu = new SysUser { };
			Fsql.Insert<SysUser>().AppendData(sysu).ExecuteAffrows();
			Fsql.Insert<SysUserLogOn>().AppendData(new SysUserLogOn { SysUserId = sysu.Id }).ExecuteAffrows();
			var a = Fsql.Select<SysUserLogOn>().ToList();
			var b = Fsql.Select<SysUserLogOn>().Any();
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

			builder.RegisterFreeRepositoryAddFilter<Song>(() => a => a.Title == DateTime.Now.ToString() + System.Threading.Thread.CurrentThread.ManagedThreadId);
			//builder.RegisterFreeGuidRepository<Song>(a => a.Id == 1, oldname => $"{oldname}_{DateTime.Now.Year}");

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
