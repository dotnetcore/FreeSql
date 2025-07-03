using FreeSql.DataAnnotations;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace base_entity
{
    partial class Program
    {
        public static void test_pgsql(IFreeSql fsql)
        {
            var ddl = fsql.CodeFirst.GetComparisonDDLStatements<gistIndex>();
        }
    }

    [Index("sidx_zjds_geom", nameof(Geom), IndexMethod = IndexMethod.GiST)]
    class gistIndex
    {
        public int bb { get; set; }
        public LineString Geom { get; set; }
    }
}
