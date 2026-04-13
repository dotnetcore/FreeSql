using FreeSql.DataAnnotations;
using Xunit;

namespace FreeSql.Tests.Issues;

public class _2226 
{

    [Table(Name = "issue9999_order")]
    class TestOrder
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    /// UnitOfWork.Dispose 后，绑定了该 UoW 的 Repository 执行写操作
    /// 不应开启孤儿事务，应走 auto-commit 正常持久化
    /// </summary>
    [Fact]
    public void UnitOfWorkDispose_ShouldPreventOrphanTransaction()
    {
        var fsql = g.sqlite;
        fsql.Delete<TestOrder>().Where("1=1").ExecuteAffrows();

        var repo = fsql.GetRepository<TestOrder>();

        // 阶段一：在 UoW 内插入并提交
        using (var uow = fsql.CreateUnitOfWork())
        {
            repo.UnitOfWork = uow;
            repo.Insert(new TestOrder { Name = "test" });
            uow.Commit();
        }
        // UoW 已 Dispose，但 repo.UnitOfWork 仍指向它

        // 阶段二：通过残留 UoW 引用的 repo 执行更新
        // 修复前：GetOrBeginTransaction 在已 Dispose 的 UoW 上开启新事务，永无 Commit，成为孤儿
        // 修复后：Enable=false → GetOrBeginTransaction 返回 null → auto-commit
        var item = repo.Select.First();
        Assert.NotNull(item);

        item.Name = "test_updated";
        repo.Update(item);

        // 用独立查询验证更新已持久化（非孤儿事务中的不可见数据）
        var updated = fsql.Select<TestOrder>().Where(a => a.Id == item.Id).First();
        Assert.Equal("test_updated", updated.Name);
    }

    /// <summary>
    /// UnitOfWork.Dispose 后 Enable 应为 false，
    /// GetOrBeginTransaction 应返回 null 而非创建新事务
    /// </summary>
    [Fact]
    public void UnitOfWorkDispose_ShouldSetEnableFalse()
    {
        var fsql = g.sqlite;

        var uow = fsql.CreateUnitOfWork();
        var concreteUow = (UnitOfWork)uow;
        Assert.True(concreteUow.Enable);

        uow.Commit();
        uow.Dispose();

        // 修复后 Dispose 将 Enable 设为 false
        Assert.False(concreteUow.Enable);

        // Enable=false 时 GetOrBeginTransaction 应返回 null，不创建新事务
        var tran = uow.GetOrBeginTransaction();
        Assert.Null(tran);
    }
}