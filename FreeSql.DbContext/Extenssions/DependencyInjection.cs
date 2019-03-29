using Microsoft.Extensions.DependencyInjection;
using System;

namespace FreeSql {
	public static class DbContextDependencyInjection {

		public static IServiceCollection AddFreeDbContext<TDbContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> options) where TDbContext : DbContext {

			services.AddScoped<TDbContext>(sp => {
				var ctx = Activator.CreateInstance<TDbContext>();

				if (ctx._orm == null) {
					var builder = new DbContextOptionsBuilder();
					options(builder);
					ctx._orm = builder._fsql;
				}

				return ctx;
			});

			return services;
		}
	}
}
