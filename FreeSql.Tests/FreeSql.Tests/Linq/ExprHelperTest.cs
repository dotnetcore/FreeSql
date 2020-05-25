using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Npgsql.LegacyPostgis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Data.SqlClient;
using kwlib;
using System.Diagnostics;
using System.IO;
using System.Text;
using FreeSql.Extensions.Linq;

namespace FreeSql.Tests.Linq
{
    public class ExprHelperTest
    {
        
        [Fact]
        public void GetConstExprValue()
        {
            Assert.Equal(-1, ExprHelper.GetConstExprValue(Expression.Constant(-1)));
            Assert.Equal(-2, ExprHelper.GetConstExprValue(Expression.Constant(-2)));
            Assert.Equal(0, ExprHelper.GetConstExprValue(Expression.Constant(0)));
            Assert.Equal(1, ExprHelper.GetConstExprValue(Expression.Constant(1)));
            Assert.Equal(2, ExprHelper.GetConstExprValue(Expression.Constant(2)));

            var arr = new[] { -1, -2, 0, 1, 2 };
            for (var a = 0; a < arr.Length; a++)
            {
                Assert.Equal(arr[a], ExprHelper.GetConstExprValue(Expression.Constant(arr[a])));
            }

            var arritems = new[]
            {
                new ArrItem { Prop = -1, Field = -1 },
                new ArrItem { Prop = -2, Field = -2 },
                new ArrItem { Prop = 0, Field = 0 },
                new ArrItem { Prop = 1, Field = 1 },
                new ArrItem { Prop = 2, Field = 2 },
            }; 
            for (var a = 0; a < arr.Length; a++)
            {
                Assert.Equal(arritems[a].Prop, ExprHelper.GetConstExprValue(Expression.Constant(arritems[a].Prop)));
                Assert.Equal(arritems[a].Field, ExprHelper.GetConstExprValue(Expression.Constant(arritems[a].Field)));
            }
        }
        
        class ArrItem
        {
            public int Prop { get; set; }
            public int Field { get; set; }
        }
    }

}
