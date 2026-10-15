using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBDbFirstViewTests
{
    [Fact]
    public void DbFirst_RelationshipView_IsDiscoverable()
    {
        var path = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-DbFirstViewTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={path}")
            .Build();

        fsql.CodeFirst.SyncStructure<ViewSource>();
        fsql.Ado.ExecuteNonQuery(
            "CREATE VIEW \"sonnet_dbfirst_view\" AS SELECT \"Id\", \"Name\" FROM \"sonnet_dbfirst_view_source\"");

        var views = fsql.Ado.Query<string>("SHOW VIEWS");
        Assert.Contains(views, name => string.Equals(name, "sonnet_dbfirst_view", StringComparison.OrdinalIgnoreCase));

        var view = fsql.DbFirst.GetTableByName("sonnet_dbfirst_view");
        Assert.NotNull(view);
        Assert.Equal(DbTableType.VIEW, view!.Type);
        Assert.Contains(view.Columns, column => string.Equals(column.Name, "Id", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(view.Columns, column => string.Equals(column.Name, "Name", StringComparison.OrdinalIgnoreCase));

        var allTables = fsql.DbFirst.GetTablesByDatabase();
        var listedView = Assert.Single(allTables.Where(table =>
            string.Equals(table.Name, "sonnet_dbfirst_view", StringComparison.OrdinalIgnoreCase)));
        Assert.Equal(DbTableType.VIEW, listedView.Type);

        var currentDatabases = fsql.DbFirst.GetDatabases();
        Assert.NotEmpty(currentDatabases);
        Assert.Contains(
            fsql.DbFirst.GetTablesByDatabase(currentDatabases[0]),
            table => string.Equals(table.Name, "sonnet_dbfirst_view", StringComparison.OrdinalIgnoreCase));

        var otherDatabaseTables = fsql.DbFirst.GetTablesByDatabase("sonnetdb_database_that_is_not_current");
        Assert.DoesNotContain(otherDatabaseTables, table =>
            string.Equals(table.Name, "sonnet_dbfirst_view", StringComparison.OrdinalIgnoreCase));

        var qualifiedOtherDatabase = fsql.DbFirst.GetTableByName(
            "sonnetdb_database_that_is_not_current.sonnet_dbfirst_view");
        Assert.Null(qualifiedOtherDatabase);
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_dbfirst_view_source")]
    private sealed class ViewSource
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
