

namespace FreeSql
{
    public class FreeContext : DbContext
    {

        public FreeContext(IFreeSql orm)
        {
            _ormScoped = DbContextScopedFreeSql.Create(orm, () => this, () => UnitOfWork);
        }
    }
}
