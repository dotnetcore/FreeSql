

namespace FreeSql
{
    public class DbContextOptionsBuilder
    {

        internal IFreeSql _fsql;

        public DbContextOptionsBuilder UseFreeSql(IFreeSql orm)
        {
            _fsql = orm;
            return this;
        }
    }
}
