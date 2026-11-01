using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using FreeSql.SonnetDB;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBModbusFunctionTests
{
    [Fact]
    public void ModbusScalarFunctions_TranslateToSonnetDbNames()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-ModbusTests", Guid.NewGuid().ToString("N"))}")
            .Build();

        var sql = fsql.Select<ModbusRow>().ToSql(row => new
        {
            Signed = SonnetDBFunctions.ModbusInt32(row.High, row.Low, "ABCD"),
            Unsigned = SonnetDBFunctions.ModbusUInt32(row.High, row.Low, "CDAB"),
            Floating = SonnetDBFunctions.ModbusFloat32(row.High, row.Low, "DCBA")
        });

        Assert.Contains("modbus_int32(a.\"High\", a.\"Low\", 'ABCD')", sql,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("modbus_uint32(a.\"High\", a.\"Low\", 'CDAB')", sql,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("modbus_float32(a.\"High\", a.\"Low\", 'DCBA')", sql,
            StringComparison.OrdinalIgnoreCase);
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_modbus_row")]
    private sealed class ModbusRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public long High { get; set; }

        public long Low { get; set; }
    }
}
