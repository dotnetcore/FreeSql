

namespace FreeSql
{
    public class FreeContext : DbContext
    {

        public FreeContext(IFreeSql orm)
        {
            _orm = orm;
        }
    }
}
