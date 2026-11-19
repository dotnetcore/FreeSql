using FreeSql.DataAnnotations;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _2298
    {
        [Fact]
        public async Task WithTempQueryToDictionary()
        {
            var fsql = g.sqlite;
            fsql.Delete<GroupTest2298>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new[]
            {
                new GroupTest2298 { Sid = 1 },
                new GroupTest2298 { Sid = 1 },
                new GroupTest2298 { Sid = 2 },
            }).ExecuteAffrows();

            var query = fsql.Select<GroupTest2298>()
                .GroupBy(s => s.Sid)
                .WithTempQuery(g => new { Id = g.Key, Count = g.Count() });

            var dict = query.ToDictionary(p => p.Id, p => p.Count);
            Assert.Equal(2, dict.Count);
            Assert.Equal(2, dict[1]);
            Assert.Equal(1, dict[2]);

            var dictAsync = await fsql.Select<GroupTest2298>()
                .GroupBy(s => s.Sid)
                .WithTempQuery(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(p => p.Id, p => p.Count);
            Assert.Equal(2, dictAsync.Count);
            Assert.Equal(2, dictAsync[1]);
            Assert.Equal(1, dictAsync[2]);

            var dictOfItems = fsql.Select<GroupTest2298>()
                .GroupBy(s => s.Sid)
                .WithTempQuery(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionary(p => p.Id);
            Assert.Equal(2, dictOfItems[1].Count);
        }

        [Table(Name = "issues_2298_group_test")]
        class GroupTest2298
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int Sid { get; set; }
        }
    }
}
