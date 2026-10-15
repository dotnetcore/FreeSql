using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using System;
using System.Data;
using System.IO;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Provider.SonnetDB
{
    public class SonnetDBCodeFirstMigrationTests
    {
        [Fact]
        public void RelationshipTable_SyncStructure_MigratesSupportedDifferences()
        {
            using var fsql = CreateFreeSql();
            fsql.Ado.ExecuteNonQuery(CommandType.Text, @"
CREATE TABLE ""sonnet_migration_old"" (
  ""Id"" INT AUTO_INCREMENT,
  ""LegacyName"" STRING NULL,
  ""Temperature"" INT NULL,
  ""LegacyIndexValue"" INT NULL,
  PRIMARY KEY (""Id"")
);");
            fsql.Ado.ExecuteNonQuery(CommandType.Text,
                "CREATE INDEX \"ix_sonnet_migration_value\" ON \"sonnet_migration_old\" (\"LegacyIndexValue\");");

            var ddl = fsql.CodeFirst.GetComparisonDDLStatements<SonnetMigrationModel>();

            Assert.Contains("ALTER TABLE \"sonnet_migration_old\" RENAME TO \"sonnet_migration_current\"", ddl);
            Assert.Contains("RENAME COLUMN \"LegacyName\" TO \"Name\"", ddl);
            Assert.Contains("ALTER COLUMN \"Temperature\" TYPE FLOAT NOT NULL SET DEFAULT 0", ddl);
            Assert.Contains("ADD COLUMN \"Enabled\" BOOL NULL DEFAULT TRUE", ddl);
            Assert.Contains("DROP INDEX \"ix_sonnet_migration_value\" ON \"sonnet_migration_current\"", ddl);
            Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS \"ix_sonnet_migration_value\"", ddl);

            fsql.CodeFirst.SyncStructure<SonnetMigrationModel>();

            Assert.Contains(fsql.Ado.Query<string>("SHOW TABLES"),
                name => string.Equals(name, "sonnet_migration_current", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(fsql.Ado.Query<string>("SHOW TABLES"),
                name => string.Equals(name, "sonnet_migration_old", StringComparison.OrdinalIgnoreCase));

            var columns = fsql.Ado.ExecuteDataTable("DESCRIBE TABLE \"sonnet_migration_current\"");
            Assert.Equal("float64", Convert.ToString(FindRow(columns, "Temperature")["data_type"]));
            Assert.False(Convert.ToBoolean(FindRow(columns, "Temperature")["is_nullable"]));
            Assert.True(Convert.ToBoolean(FindRow(columns, "Enabled")["is_nullable"]));
            Assert.Contains("Name", columns.Rows.Cast<DataRow>().Select(row => Convert.ToString(row["column_name"])));
            Assert.Contains("IndexValue", columns.Rows.Cast<DataRow>().Select(row => Convert.ToString(row["column_name"])));

            var indexes = fsql.Ado.ExecuteDataTable("SHOW INDEXES ON \"sonnet_migration_current\"");
            Assert.Single(indexes.Rows.Cast<DataRow>());
            Assert.True(Convert.ToBoolean(indexes.Rows[0]["is_unique"]));
            Assert.Equal("IndexValue", Convert.ToString(indexes.Rows[0]["columns"]));
            Assert.Null(fsql.CodeFirst.GetComparisonDDLStatements<SonnetMigrationModel>());
        }

        [Fact]
        public void RelationshipTable_AddRequiredColumnWithoutDefault_ThrowsChineseError()
        {
            using var fsql = CreateFreeSql();
            fsql.Ado.ExecuteNonQuery(CommandType.Text,
                "CREATE TABLE \"sonnet_migration_required\" (\"Id\" INT AUTO_INCREMENT, PRIMARY KEY (\"Id\"));");

            var exception = Assert.Throws<InvalidOperationException>(
                () => fsql.CodeFirst.GetComparisonDDLStatements<SonnetMigrationRequiredColumnModel>());

            Assert.Contains("DEFAULT", exception.Message);
            Assert.Contains("新增", exception.Message);
        }

        [Fact]
        public void RelationshipTable_DefaultStringCaseChange_IsDetected()
        {
            using var fsql = CreateFreeSql();
            fsql.Ado.ExecuteNonQuery(CommandType.Text,
                "CREATE TABLE \"sonnet_default_case\" (\"Id\" INT AUTO_INCREMENT, \"Value\" STRING NOT NULL DEFAULT 'a', PRIMARY KEY (\"Id\"));");

            var ddl = fsql.CodeFirst.GetComparisonDDLStatements<SonnetDefaultCaseModel>();

            // SQL keyword casing is immaterial, but quoted string contents are data.
            Assert.Contains("ALTER COLUMN \"Value\" TYPE STRING NOT NULL SET DEFAULT 'A'", ddl);
        }

        static DataRow FindRow(DataTable table, string name) => table.Rows.Cast<DataRow>()
            .Single(row => string.Equals(Convert.ToString(row["column_name"]), name, StringComparison.OrdinalIgnoreCase));

        static IFreeSql CreateFreeSql()
        {
            var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-Tests", Guid.NewGuid().ToString("N"));
            return new FreeSqlBuilder()
                .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
                .Build();
        }

        [SonnetDBTable]
        [Table(Name = "sonnet_migration_current", OldName = "sonnet_migration_old")]
        [Index("ix_sonnet_migration_value", nameof(IndexValue), true)]
        sealed class SonnetMigrationModel
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public long Id { get; set; }

            [Column(Name = "Name", OldName = "LegacyName", DbType = "STRING NOT NULL DEFAULT '未命名'")]
            public string Name { get; set; } = "";

            [Column(DbType = "FLOAT NOT NULL DEFAULT 0")]
            public double Temperature { get; set; }

            [Column(Name = "IndexValue", OldName = "LegacyIndexValue")]
            public long? IndexValue { get; set; }

            [Column(DbType = "BOOL NULL DEFAULT TRUE")]
            public bool? Enabled { get; set; }
        }

        [SonnetDBTable]
        [Table(Name = "sonnet_migration_required")]
        sealed class SonnetMigrationRequiredColumnModel
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public long Id { get; set; }

            [Column(IsNullable = false)]
            public string RequiredValue { get; set; } = "";
        }

        [SonnetDBTable]
        [Table(Name = "sonnet_default_case")]
        sealed class SonnetDefaultCaseModel
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public long Id { get; set; }

            [Column(DbType = "STRING NOT NULL DEFAULT 'A'")]
            public string Value { get; set; } = "";
        }
    }
}
