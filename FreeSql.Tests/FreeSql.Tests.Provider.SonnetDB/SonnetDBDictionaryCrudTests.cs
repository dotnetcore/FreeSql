namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBDictionaryCrudTests
{
    [Fact]
    public void DictionaryCrud_UsesRelationshipTableSql()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-DictionaryTests", Guid.NewGuid().ToString("N"))}")
            .Build();

        var row = new Dictionary<string, object>
        {
            ["Id"] = 7L,
            ["Name"] = "动态表"
        };

        var insertSql = fsql.InsertDict(row).AsTable("sonnet_dictionary_row").ToSql();
        Assert.Contains("INSERT INTO \"sonnet_dictionary_row\"", insertSql, StringComparison.OrdinalIgnoreCase);

        var updateSql = fsql.UpdateDict(row).AsTable("sonnet_dictionary_row").WherePrimary("Id").ToSql();
        Assert.Contains("UPDATE \"sonnet_dictionary_row\"", updateSql, StringComparison.OrdinalIgnoreCase);

        var deleteSql = fsql.DeleteDict(row).AsTable("sonnet_dictionary_row").ToSql();
        Assert.Contains("DELETE FROM \"sonnet_dictionary_row\"", deleteSql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DictionaryUpsert_RemainsRejectedWithChineseMessage()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-DictionaryTests", Guid.NewGuid().ToString("N"))}")
            .Build();

        var row = new Dictionary<string, object>
        {
            ["Id"] = 7L,
            ["Name"] = "动态表"
        };

        var exception = Assert.Throws<NotSupportedException>(() => fsql.InsertOrUpdateDict(row));
        Assert.Contains("原子", exception.Message, StringComparison.Ordinal);
        Assert.Contains("InsertOrUpdate", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DictionaryCrud_ExecutesAgainstRelationshipTable()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-DictionaryExecutionTests", Guid.NewGuid().ToString("N"))}")
            .Build();

        fsql.Ado.ExecuteNonQuery(
            "CREATE TABLE \"sonnet_dictionary_exec\" (\"Id\" INT, \"Name\" STRING, PRIMARY KEY (\"Id\"))");

        var inserted = new Dictionary<string, object>
        {
            ["Id"] = 7L,
            ["Name"] = "初始"
        };
        Assert.Equal(1, fsql.InsertDict(inserted).AsTable("sonnet_dictionary_exec").ExecuteAffrows());

        var updated = new Dictionary<string, object>
        {
            ["Id"] = 7L,
            ["Name"] = "更新"
        };
        Assert.Equal(1, fsql.UpdateDict(updated)
            .AsTable("sonnet_dictionary_exec")
            .WherePrimary("Id")
            .ExecuteAffrows());

        var name = fsql.Ado.ExecuteScalar(
            "SELECT \"Name\" FROM \"sonnet_dictionary_exec\" WHERE \"Id\" = 7");
        Assert.Equal("更新", name?.ToString());

        Assert.Equal(1, fsql.DeleteDict(updated)
            .AsTable("sonnet_dictionary_exec")
            .ExecuteAffrows());
        Assert.Equal(0, Convert.ToInt32(fsql.Ado.ExecuteScalar(
            "SELECT count(1) FROM \"sonnet_dictionary_exec\"")));
    }
}
