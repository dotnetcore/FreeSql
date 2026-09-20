using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBDbFirstQualifiedNameTests
{
    [Fact]
    public void DbFirst_QuotedDatabaseNameContainingDot_FindsTable()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "FreeSql-SonnetDB.QualifiedNameTests",
            Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={path}")
            .Build();

        fsql.CodeFirst.SyncStructure<QualifiedNameSource>();

        var databases = fsql.DbFirst.GetDatabases();
        Assert.NotEmpty(databases);
        var database = databases[0];
        Assert.Contains(".", database);

        var escapedDatabase = database.Replace("\"", "\"\"");
        var table = fsql.DbFirst.GetTableByName(
            $"\"{escapedDatabase}\".\"sonnet_qualified_name_source\"");

        Assert.NotNull(table);
        Assert.Equal("sonnet_qualified_name_source", table!.Name);

        var quotedDatabaseTables = fsql.DbFirst.GetTablesByDatabase($"\"{escapedDatabase}\"");
        Assert.Contains(quotedDatabaseTables, item =>
            string.Equals(item.Name, "sonnet_qualified_name_source", StringComparison.OrdinalIgnoreCase));

        Assert.True(fsql.DbFirst.ExistsTable(
            $"\"{escapedDatabase}\".\"sonnet_qualified_name_source\"", true));
        Assert.False(fsql.DbFirst.ExistsTable(
            "\"other.database.with.dot\".\"sonnet_qualified_name_source\"", true));
    }

    [Fact]
    public void DbFirst_QuotedObjectNameContainingDot_IsNotSplit()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "FreeSql-SonnetDB.QualifiedObjectTests",
            Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={path}")
            .Build();

        fsql.Ado.ExecuteNonQuery(
            "CREATE TABLE \"sonnet.literal.table\" (\"Id\" INT, \"Name\" STRING, PRIMARY KEY (\"Id\"))");

        var table = fsql.DbFirst.GetTableByName("\"sonnet.literal.table\"");

        Assert.NotNull(table);
        Assert.Equal("sonnet.literal.table", table!.Name);
    }

    [Fact]
    public void DbFirst_QuotedViewNameContainingDot_IsDiscoverable()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "FreeSql-SonnetDB.QualifiedViewTests",
            Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={path}")
            .Build();

        fsql.Ado.ExecuteNonQuery(
            "CREATE TABLE \"sonnet_view_source\" (\"Id\" INT, \"Name\" STRING, PRIMARY KEY (\"Id\"))");
        fsql.Ado.ExecuteNonQuery(
            "CREATE VIEW \"sonnet.literal.view\" AS SELECT \"Id\", \"Name\" FROM \"sonnet_view_source\"");

        var view = fsql.DbFirst.GetTableByName("\"sonnet.literal.view\"");

        Assert.NotNull(view);
        Assert.Equal("sonnet.literal.view", view!.Name);
        Assert.Equal(DbTableType.VIEW, view.Type);
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_qualified_name_source")]
    private sealed class QualifiedNameSource
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
