using System.Linq.Expressions;
using Xunit;

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
