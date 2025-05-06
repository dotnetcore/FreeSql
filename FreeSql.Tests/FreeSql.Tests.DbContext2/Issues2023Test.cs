using FreeSql.DataAnnotations;
using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.DbContext2
{
    public class Issues2023Test
    {

        [Fact]
        public void Test2023()
        {
            var fsql = g.firebird;
            var list = new List<TestCategory>
                {
                    new TestCategory
                    {
                        Name = "c1",
                        Articles = new List<TestArticle>
                        {
                            new TestArticle
                            {
                                Title = "t1a",
                            },
                            new TestArticle
                            {
                                Title = "t1b",
                            }
                        }
                    },
                    new TestCategory
                    {
                        Name = "c2",
                        Articles = new List<TestArticle>
                        {
                            new TestArticle
                            {
                                Title = "t2a",
                            },
                            new TestArticle
                            {
                                Title = "t2b",
                            }
                        }
                    }
                };

            var repo = fsql.GetRepository<TestCategory>();
            repo.DbContextOptions.EnableCascadeSave = true;
            repo.Insert(list); // 这里报 System.NullReferenceException
        }

        [Table(Name = "test_category")]
        public class TestCategory
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public long Id { get; set; }

            [Required]
            public string Name { get; set; }

            [Navigate(nameof(TestArticle.CategoryId))]
            public List<TestArticle> Articles { get; set; } = new List<TestArticle>();
        }

        [Table(Name = "test_article")]
        public class TestArticle
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public long Id { get; set; }

            [Required]
            public string Title { get; set; }

            public long CategoryId { get; set; }

            [Navigate(nameof(CategoryId))]
            public TestCategory Category { get; set; }

            [Column(ServerTime = DateTimeKind.Local)] // 移除此特性就可以成功插入！
            public DateTime CreatedTime { get; set; }
        }
    }
}
