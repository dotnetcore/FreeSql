using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Internal.Model
{
    public class SelectColumnInfo
    {
        public ColumnInfo Column { get; set; }
        public SelectTableInfo Table { get; set; }
    }
}
