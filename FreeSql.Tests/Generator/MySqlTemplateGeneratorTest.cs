using FreeSql.DataAnnotations;
using FreeSql.Generator;
using System;
using Xunit;

namespace FreeSql.Tests.Generator {
	public class MySqlTemplateGeneratorTest {

		[Fact]
		public void BuildSimpleEntity() {
			var gen = new TemplateGenerator();
			gen.Build(g.mysql.DbFirst, @"C:\Users\28810\Desktop\github\FreeSql\Templates\MySql\simple-entity", @"C:\Users\28810\Desktop\新建文件夹 (9)", "cccddd");
		}

		[Fact]
		public void BuildSimpleEntityNavigationObject () {
			var gen = new TemplateGenerator();
			gen.Build(g.mysql.DbFirst, @"C:\Users\28810\Desktop\github\FreeSql\Templates\MySql\simple-entity-navigation-object", @"C:\Users\28810\Desktop\新建文件夹 (9)", "cccddd");
		}

		[Fact]
		public void BuildRichEntityNavigationObject() {
			var gen = new TemplateGenerator();
			gen.Build(g.mysql.DbFirst, @"C:\Users\28810\Desktop\github\FreeSql\Templates\MySql\rich-entity-navigation-object", @"C:\Users\28810\Desktop\新建文件夹 (9)", "cccddd");
		}
	}
}
