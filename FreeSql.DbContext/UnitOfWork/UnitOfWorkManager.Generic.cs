namespace FreeSql
{
    /// <summary>
    /// Typed UnitOfWork manager for multi-database scenarios.
    /// </summary>
    public class UnitOfWorkManager<TMark> : UnitOfWorkManager
    {
        public UnitOfWorkManager(IFreeSql<TMark> fsql) : base(fsql) { }
    }
}
