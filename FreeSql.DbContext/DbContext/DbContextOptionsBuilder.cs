

namespace FreeSql
{
    public class DbContextOptionsBuilder
    {

        internal IFreeSql _fsql;
        internal DbContextOptions _options;

        public DbContextOptionsBuilder UseFreeSql(IFreeSql orm)
        {
            _fsql = orm;
            return this;
        }
        public DbContextOptionsBuilder UseOptions(DbContextOptions options)
        {
            _options = options;
            return this;
        }
    }
}
