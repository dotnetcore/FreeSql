using System.Collections.Generic;

namespace FreeSql.DatabaseModel
{
    public class DbForeignInfo
    {
        public DbTableInfo Table { get; set; }
        public List<DbColumnInfo> Columns { get; set; } = new List<DbColumnInfo>();
        public DbTableInfo ReferencedTable { get; set; }
        public List<DbColumnInfo> ReferencedColumns { get; set; } = new List<DbColumnInfo>();

    }
}
