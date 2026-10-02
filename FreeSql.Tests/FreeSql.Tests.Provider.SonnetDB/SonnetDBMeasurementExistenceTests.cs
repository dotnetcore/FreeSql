using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using System;
using System.Data;
using System.IO;
using Xunit;

namespace FreeSql.Tests.Provider.SonnetDB;

/// <summary>
/// 验证 CodeFirst 能区分关系表和时序 measurement 的同名对象。
/// </summary>
public sealed class SonnetDBMeasurementExistenceTests
{
    [Fact]
    public void CodeFirst_同名关系表不应被误判为Measurement()
    {
        using var fsql = CreateFreeSql();
        fsql.Ado.ExecuteNonQuery(CommandType.Text,
            "CREATE TABLE \"sonnet_same_name_object\" (\"Id\" INT AUTO_INCREMENT, PRIMARY KEY (\"Id\"));");

        var ddl = fsql.CodeFirst.GetComparisonDDLStatements<SameNameMeasurement>();

        Assert.Contains("CREATE MEASUREMENT \"sonnet_same_name_object\"", ddl,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CodeFirst_已存在Measurement不重复生成DDL()
    {
        using var fsql = CreateFreeSql();
        fsql.Ado.ExecuteNonQuery(CommandType.Text,
            "CREATE MEASUREMENT \"sonnet_existing_measurement\" (\"Host\" TAG, \"Value\" FIELD FLOAT);");

        var ddl = fsql.CodeFirst.GetComparisonDDLStatements<ExistingMeasurement>();

        Assert.Null(ddl);
    }

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-MeasurementExistenceTests",
            Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .Build();
    }

    [Table(Name = "sonnet_same_name_object")]
    sealed class SameNameMeasurement
    {
        public DateTime Time { get; set; }

        [SonnetDBTag]
        public string Host { get; set; } = "";

        [SonnetDBField]
        public double Value { get; set; }
    }

    [Table(Name = "sonnet_existing_measurement")]
    sealed class ExistingMeasurement
    {
        public DateTime Time { get; set; }

        [SonnetDBTag]
        public string Host { get; set; } = "";

        [SonnetDBField]
        public double Value { get; set; }
    }
}
