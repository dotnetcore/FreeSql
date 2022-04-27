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
            Assert.Equal(-1, Expression.Constant(-1).GetConstExprValue());
            Assert.Equal(-2, Expression.Constant(-2).GetConstExprValue());
            Assert.Equal(0, Expression.Constant(0).GetConstExprValue());
            Assert.Equal(1, Expression.Constant(1).GetConstExprValue());
            Assert.Equal(2, Expression.Constant(2).GetConstExprValue());

            var arr = new[] { -1, -2, 0, 1, 2 };
            for (var a = 0; a < arr.Length; a++)
            {
                Assert.Equal(arr[a], Expression.Constant(arr[a]).GetConstExprValue());
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
                Assert.Equal(arritems[a].Prop, Expression.Constant(arritems[a].Prop).GetConstExprValue());
                Assert.Equal(arritems[a].Field, Expression.Constant(arritems[a].Field).GetConstExprValue());
            }
        }
        
        class ArrItem
        {
            public int Prop { get; set; }
            public int Field { get; set; }
        }
    }

}
