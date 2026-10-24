using FreeSql.Internal;
using System.Linq.Expressions;
using System.Reflection;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SharedCoreCompatibilityTests
{
    [Fact]
    public void DataReaderValueConversion_PreservesPublicTwoParameterSignature()
    {
        // Existing compiled extensions bind to the original two-parameter method.
        var method = typeof(Utils).GetMethod(nameof(Utils.GetDataReaderValueBlockExpression),
            BindingFlags.Public | BindingFlags.Static, null,
            new[] { typeof(Type), typeof(Expression) }, null);

        Assert.NotNull(method);
        var conversion = Assert.IsAssignableFrom<Expression>(
            method!.Invoke(null, new object[] { typeof(int), Expression.Constant("42") }));
        Assert.Equal(42, Expression.Lambda<Func<object>>(conversion).Compile()());
    }
}
