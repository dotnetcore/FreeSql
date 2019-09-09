using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Npgsql.LegacyPostgis;
using FreeSql.Internal;
using System.Linq.Expressions;
using FreeSql.Internal.Model;

namespace FreeSql.ExpressionTree
{
    public class GetAllTableRuleTest
    {

        [Fact]
        public void Test()
        {
            //var _tables = new List<SelectTableInfo>
            //{
            //    [0] = new SelectTableInfo {  }
            //};
            //var tableRuleInvoke = new Func<Type, string, string[]>((type, oldname) =>
            //{
            //    return new[] { oldname };
            //});

            //CommonUtils.GetAllTableRule(_tables, tableRuleInvoke);
        }
    }
}
