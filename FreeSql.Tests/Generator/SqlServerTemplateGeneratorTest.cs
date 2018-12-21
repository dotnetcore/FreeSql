using FreeSql.DataAnnotations;
using FreeSql.Generator;
using System;
using Xunit;

namespace FreeSql.Tests.Generator {
	public class SqlServerTemplateGeneratorTest {

		[Fact]
		public void BuildSimpleEntity() {
			var gen = new TemplateGenerator();
			gen.Build(g.sqlserver.DbFirst, @"C:\Users\28810\Desktop\github\FreeSql\Templates\SqlServer\simple-entity", @"C:\Users\28810\Desktop\新建文件夹 (9)", "shop");
		}

		[Fact]
		public void BuildSimpleEntityNavigationObject () {
			var gen = new TemplateGenerator();
			gen.Build(g.sqlserver.DbFirst, @"C:\Users\28810\Desktop\github\FreeSql\Templates\SqlServer\simple-entity-navigation-object", @"C:\Users\28810\Desktop\新建文件夹 (9)", "shop");
		}

		[Fact]
		public void BuildRichEntityNavigationObject() {
			var gen = new TemplateGenerator();
			gen.Build(g.sqlserver.DbFirst, @"C:\Users\28810\Desktop\github\FreeSql\Templates\SqlServer\rich-entity-navigation-object", @"C:\Users\28810\Desktop\新建文件夹 (9)", "shop");
		}
	}
}
