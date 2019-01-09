using FreeSql.DataAnnotations;
using FreeSql.Generator;
using System;
using Xunit;

namespace FreeSql.Tests.Generator {
	public class PostgreSQLTemplateGeneratorTest {

		[Fact]
		public void BuildSimpleEntity() {
			var gen = new TemplateGenerator();
			gen.Build(g.pgsql.DbFirst, @"C:\Users\28810\Desktop\github\FreeSql\Templates\PostgreSQL\simple-entity", @"C:\Users\28810\Desktop\新建文件夹 (9)", "tedb");
		}

		[Fact]
		public void BuildSimpleEntityNavigationObject () {
			var gen = new TemplateGenerator();
			gen.Build(g.pgsql.DbFirst, @"C:\Users\28810\Desktop\github\FreeSql\Templates\PostgreSQL\simple-entity-navigation-object", @"C:\Users\28810\Desktop\新建文件夹 (9)", "tedb");
		}

		[Fact]
		public void BuildRichEntityNavigationObject() {
			var gen = new TemplateGenerator();
			gen.Build(g.pgsql.DbFirst, @"C:\Users\28810\Desktop\github\FreeSql\Templates\PostgreSQL\rich-entity-navigation-object", @"C:\Users\28810\Desktop\新建文件夹 (9)", "tedb");
		}
	}
}
