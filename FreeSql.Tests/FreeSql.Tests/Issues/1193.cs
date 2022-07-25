using FreeSql.DataAnnotations;
using System;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1193
    {
        [Table(Name = "MyData_1193")]
        public class MyDataEntity
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        public class MyDataDto : MyDataDtoError
        {
            public MyDataDto() : base(0) { }
            public MyDataDto(int id) : base(id) { }
        }

        public class MyDataDtoError
        {
            public MyDataDtoError(int id)
            {
                Id = id;
            }

            public MyDataDtoError(int id, string title)
            {
                Id = id;
                Title = title;
            }
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Title { get; }
        }

        [Fact]
        public void TestYear()
        {
            var fsql = g.sqlite;

            fsql.GetRepository<MyDataEntity>().Insert(new MyDataEntity { Name = "my name" + DateTime.Now.ToString("yyyyMMddTHHmmss") });

            var uowm = new UnitOfWorkManager(fsql);

            using var uow = uowm.Begin();

            var repository = uow.Orm.GetRepository<MyDataEntity, int>();

            var firstData = repository.Select.First((o) => new MyDataDto(o.Id) { Name = o.Name });
            var errorData = repository.Select.First((o) => new MyDataDtoError(o.Id, o.Name) { Name = o.Name });
        }
    }
}
