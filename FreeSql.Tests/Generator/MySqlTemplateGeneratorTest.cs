using FreeSql.DataAnnotations;
using FreeSql.Generator;
using System;
using Xunit;

namespace FreeSql.Tests.Generator {
	public class MySqlTemplateGeneratorTest {

		[Fact]
		public void BuildSimpleEntity() {
			var gen = new TemplateGenerator();
			gen.Build(g.mysql.DbFirst, @"E:\GitHub\temp\FreeSql\Templates\MySql\simple-entity", @"E:\GitHub\temp\FreeSql2", "cccddd");
		}

		[Fact]
		public void BuildSimpleEntityNavigationObject () {
			var gen = new TemplateGenerator();
			gen.Build(g.mysql.DbFirst, @"E:\GitHub\temp\FreeSql\Templates\MySql\simple-entity-navigation-object", @"E:\GitHub\temp\FreeSql2", "cccddd");
		}

		[Fact]
		public void BuildRichEntityNavigationObject() {
			var gen = new TemplateGenerator();
			gen.Build(g.mysql.DbFirst, @"E:\GitHub\temp\FreeSql\Templates\MySql\rich-entity-navigation-object", @"E:\GitHub\temp\FreeSql2", "cccddd");
		}
	}
}
