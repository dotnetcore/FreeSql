using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 包括 CreateTime/UpdateTime/IsDeleted 的实体基类
/// </summary>
[Table(DisableSyncStructure = true)]
public abstract class BaseEntity
{
    static IFreeSql _ormPriv;
    /// <summary>
    /// 全局 IFreeSql orm 对象
    /// </summary>
    public static IFreeSql Orm => _ormPriv ?? throw new Exception(@"使用前请初始化 BaseEntity.Initialization(new FreeSqlBuilder()
.UseAutoSyncStructure(true)
.UseConnectionString(DataType.Sqlite, ""data source=test.db;max pool size=5"")
.Build());");

    /// <summary>
	/// 初始化BaseEntity
	/// BaseEntity.Initialization(new FreeSqlBuilder()
    /// <para></para>
    /// .UseAutoSyncStructure(true)
    /// <para></para>
    /// .UseConnectionString(DataType.Sqlite, "data source=test.db;max pool size=5")
    /// <para></para>
    /// .Build());
	/// </summary>
	/// <param name="fsql">IFreeSql orm 对象</param>
	public static void Initialization(IFreeSql fsql)
    {
        _ormPriv = fsql;
        _ormPriv.Aop.CurdBefore += (s, e) => Trace.WriteLine(e.Sql + "\r\n");
    }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }
    /// <summary>
    /// 逻辑删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 开启工作单元事务
    /// </summary>
    /// <returns></returns>
    public static IUnitOfWork Begin() => Begin(null);
    /// <summary>
    /// 开启工作单元事务
    /// </summary>
    /// <param name="level">事务等级</param>
    /// <returns></returns>
    public static IUnitOfWork Begin(IsolationLevel? level)
    {
        var uow = Orm.CreateUnitOfWork();
        uow.IsolationLevel = level;
        return uow;
    }
}
