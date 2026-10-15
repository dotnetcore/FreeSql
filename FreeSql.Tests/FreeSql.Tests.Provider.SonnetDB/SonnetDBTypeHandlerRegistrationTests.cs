using FreeSql.Internal;
using System.Text.Json;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBTypeHandlerRegistrationTests
{
    [Fact]
    public void JsonDocumentHandler_IsIdempotentAndPreservesEarlierRegistration()
    {
        var hadEarlierHandler = Utils.TypeHandlers.TryGetValue(typeof(JsonDocument), out var earlierHandler);

        using (CreateFreeSql())
        {
            Assert.True(Utils.TypeHandlers.TryGetValue(typeof(JsonDocument), out var firstHandler));
            Assert.Equal(typeof(JsonDocument), firstHandler.Type);

            using (CreateFreeSql())
            {
                Assert.True(Utils.TypeHandlers.TryGetValue(typeof(JsonDocument), out var secondHandler));
                Assert.Same(firstHandler, secondHandler);
            }

            if (hadEarlierHandler)
                Assert.Same(earlierHandler, firstHandler);
        }
    }

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-TypeHandler-Tests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .Build();
    }
}
