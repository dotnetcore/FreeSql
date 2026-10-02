using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBParameterTypeTests
{
    [Fact]
    public void RelationshipTable_CharParameter_RoundTripsAsString()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-CharParameterTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .UseNoneCommandParameter(false)
            .UseGenerateCommandParameterWithLambda(true)
            .Build();

        var commands = new List<Aop.CurdBeforeEventArgs>();
        fsql.Aop.CurdBefore += (_, e) => commands.Add(e);
        var source = new CharParameterRow { Id = 1, Code = 'A', Name = "参数化字符" };

        Assert.Equal(1, fsql.Insert(source).ExecuteAffrows());
        var loaded = fsql.Select<CharParameterRow>()
            .Where(row => row.Code == source.Code)
            .ToOne();

        Assert.NotNull(loaded);
        Assert.Equal(source.Code, loaded!.Code);
        Assert.Equal(source.Name, loaded.Name);

        var charParameters = commands
            .SelectMany(command => command.DbParms ?? Array.Empty<System.Data.Common.DbParameter>())
            .Where(parameter => parameter.Value is string text && text == "A")
            .ToList();
        Assert.NotEmpty(charParameters);
        Assert.All(charParameters, parameter => Assert.Equal(System.Data.DbType.String, parameter.DbType));
    }

    [Fact]
    public void RelationshipTable_FloatArrayInParameter_IsNotTreatedAsVector()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-FloatInParameterTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .UseNoneCommandParameter(false)
            .UseGenerateCommandParameterWithLambda(true)
            .Build();

        fsql.Insert(new FloatParameterRow { Id = 1, Value = 1.5f }).ExecuteAffrows();
        fsql.Insert(new FloatParameterRow { Id = 2, Value = 3.5f }).ExecuteAffrows();
        var values = new[] { 1.5f, 3.5f };

        var rows = fsql.Select<FloatParameterRow>()
            .Where(row => values.Contains(row.Value))
            .ToList();

        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void RawAdo_FloatArrayParameter_IsRejectedBeforeDriverBinding()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-RawVectorParameterTests", Guid.NewGuid().ToString("N"))}")
            .Build();

        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.Ado.ExecuteNonQuery("SELECT @vector", new { vector = new[] { 1f, 2f } }));

        Assert.Contains("VECTOR", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("参数绑定", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UInt64_WithinInt64Range_IsBoundAsInt64()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-UInt64ParameterTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .UseNoneCommandParameter(false)
            .UseGenerateCommandParameterWithLambda(true)
            .Build();

        var commands = new List<Aop.CurdBeforeEventArgs>();
        fsql.Aop.CurdBefore += (_, e) => commands.Add(e);

        Assert.Equal(1, fsql.Insert(new UInt64ParameterRow
        {
            Id = 1,
            Value = (ulong)long.MaxValue
        }).ExecuteAffrows());

        var valueParameter = commands
            .SelectMany(command => command.DbParms ?? Array.Empty<System.Data.Common.DbParameter>())
            .Single(parameter => parameter.Value is long value && value == long.MaxValue);
        Assert.Equal(System.Data.DbType.Int64, valueParameter.DbType);
    }

    [Fact]
    public void UInt64_OutsideInt64Range_IsRejectedBeforeParameterBinding()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-UInt64OverflowParameterTests", Guid.NewGuid().ToString("N"))}")
            .Build();

        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.Ado.ExecuteNonQuery("SELECT @value", new { value = ulong.MaxValue }));

        Assert.Contains("有符号 int64", exception.Message, StringComparison.Ordinal);
        Assert.Contains("long.MaxValue", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UInt64_OutsideInt64Range_IsRejectedForSqlLiterals()
    {
        var formattedException = Assert.Throws<NotSupportedException>(() =>
            "SELECT {0}".FormatSonnetDB(ulong.MaxValue));
        Assert.Contains("有符号 int64", formattedException.Message, StringComparison.Ordinal);

        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-UInt64OverflowLiteralTests", Guid.NewGuid().ToString("N"))}")
            .UseNoneCommandParameter(true)
            .Build();

        var insertException = Assert.Throws<NotSupportedException>(() =>
            fsql.Insert(new UInt64ParameterRow { Id = 1, Value = ulong.MaxValue }).ToSql());
        Assert.Contains("long.MaxValue", insertException.Message, StringComparison.Ordinal);
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_char_parameter_row")]
    private sealed class CharParameterRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public char Code { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_float_parameter_row")]
    private sealed class FloatParameterRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public float Value { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_uint64_parameter_row")]
    private sealed class UInt64ParameterRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public ulong Value { get; set; }
    }
}
