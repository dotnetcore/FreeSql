namespace FreeSql
{
    /// <summary>
    /// Typed UnitOfWork manager for multi-database scenarios.
    /// </summary>
    public class UnitOfWorkManager<TMark> : UnitOfWorkManager, IUnitOfWorkManager<TMark>
    {
        public UnitOfWorkManager(IFreeSql<TMark> fsql) : base(fsql) { }
    }

    public interface IUnitOfWorkManager<TMark> : IUnitOfWorkManager
    {
    }
}
