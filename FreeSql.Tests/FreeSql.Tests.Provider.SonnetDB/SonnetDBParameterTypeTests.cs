using FreeSql.DataAnnotations;
using FreeSql.Internal.CommonProvider;
using FreeSql.Provider.SonnetDB.Attributes;
using System.Data;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBParameterTypeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RelationshipTable_OffsetParameterTypes_FollowTheColumnDefinition(bool useNull)
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-OffsetParameterTypes", Guid.NewGuid().ToString("N"))}")
            .UseNoneCommandParameter(false)
            .Build();
        var value = new DateTimeOffset(2026, 11, 2, 9, 10, 11, TimeSpan.FromHours(8)).AddTicks(1234);
        var insert = fsql.Insert(new OffsetParameterRow
        {
            Id = 1,
            Legacy = value,
            Precise = value,
            Offset = value,
            OptionalLegacy = useNull ? null : value,
            OptionalPrecise = useNull ? null : value,
            OptionalOffset = useNull ? null : value
        });

        _ = insert.ToSql();
        var parameters = ((InsertProvider<OffsetParameterRow>)insert)._params;
        foreach (var (name, dbType) in new[]
        {
            (nameof(OffsetParameterRow.Legacy), DbType.DateTime),
            (nameof(OffsetParameterRow.Precise), DbType.DateTime2),
            (nameof(OffsetParameterRow.Offset), DbType.DateTimeOffset),
            (nameof(OffsetParameterRow.OptionalLegacy), DbType.DateTime),
            (nameof(OffsetParameterRow.OptionalPrecise), DbType.DateTime2),
            (nameof(OffsetParameterRow.OptionalOffset), DbType.DateTimeOffset)
        })
        {
            var parameter = Assert.Single(parameters.Where(item => item.ParameterName == $"@{name}_0"));
            Assert.Equal(dbType, parameter.DbType);
            if (useNull && name.StartsWith("Optional", StringComparison.Ordinal)) Assert.Equal(DBNull.Value, parameter.Value);
            else Assert.True(value.EqualsExact(Assert.IsType<DateTimeOffset>(parameter.Value)));
        }
    }

    [Fact]
    public void RelationshipTable_DefaultOffsetParameters_FilterNonUtcAndNullableValues()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-OffsetParameterExecution", Guid.NewGuid().ToString("N"))}")
            .UseAutoSyncStructure(true)
            .UseNoneCommandParameter(false)
            .UseGenerateCommandParameterWithLambda(true)
            .Build();
        var value = new DateTimeOffset(2026, 11, 2, 9, 10, 11, TimeSpan.FromHours(8));
        var commands = new List<Aop.CurdBeforeEventArgs>();
        fsql.Aop.CurdBefore += (_, e) => commands.Add(e);
        Assert.Equal(2, fsql.Insert(new[]
        {
            new LegacyOffsetRow { Id = 1, Value = value, OptionalValue = value },
            new LegacyOffsetRow { Id = 2, Value = value }
        }).ExecuteAffrows());

        var matching = fsql.Select<LegacyOffsetRow>()
            .Where(row => row.Value == value && row.OptionalValue == value).ToList();
        Assert.Equal(1, Assert.Single(matching).Id);
        Assert.Equal(value, matching[0].Value);
        Assert.Equal(value, matching[0].OptionalValue);
        Assert.Equal(2, fsql.Select<LegacyOffsetRow>().Where(row => row.OptionalValue == null).ToOne().Id);
        var offsetParameters = commands.SelectMany(command => command.DbParms ?? []).Where(parameter => parameter.Value is DateTimeOffset).ToArray();
        Assert.NotEmpty(offsetParameters);
        Assert.All(offsetParameters, parameter =>
        {
            Assert.Equal(DbType.DateTime, parameter.DbType);
            Assert.True(value.EqualsExact((DateTimeOffset)parameter.Value));
        });
    }

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
    public void RawAdo_FloatArrayParameter_ExecutesMeasurementDistance()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-RawVectorParameterTests", Guid.NewGuid().ToString("N"))}")
            .UseAutoSyncStructure(true)
            .UseNoneCommandParameter(false)
            .Build();

        fsql.Insert(new VectorParameterRow
        {
            Time = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc),
            Embedding = [1f, 0f]
        }).ExecuteAffrows();
        var distance = fsql.Ado.ExecuteScalar(
            "SELECT cosine_distance(\"Embedding\", @vector) FROM \"sonnet_raw_vector_parameter\"",
            new { vector = new[] { 1f, 0f } });

        Assert.Equal(0d, Convert.ToDouble(distance));
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

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task NativeTemporal_InsertUpdateAndUpsert_PreserveTicksAndUtcInstant(bool noneParameter, bool upsert)
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-NativeTemporal", Guid.NewGuid().ToString("N"))}")
            .UseAutoSyncStructure(true)
            .UseNoneCommandParameter(noneParameter)
            .Build();
        var row = new NativeTemporalRow
        {
            Id = 1,
            At = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc).AddTicks(1234567),
            Offset = new DateTimeOffset(2026, 11, 2, 3, 0, 0, TimeSpan.FromHours(5.5)).AddTicks(7654321)
        };
        Assert.Equal(1, upsert ? await fsql.InsertOrUpdate<NativeTemporalRow>().SetSource(row).ExecuteAffrowsAsync()
            : await fsql.Insert(row).ExecuteAffrowsAsync());
        var inserted = fsql.Select<NativeTemporalRow>().ToOne();
        Assert.Equal(row.At.Ticks, inserted.At.Ticks);
        Assert.Equal(row.Offset.UtcTicks, inserted.Offset.UtcTicks);
        Assert.Equal(TimeSpan.Zero, inserted.Offset.Offset);
        row.At = row.At.AddTicks(1);
        row.Offset = row.Offset.AddTicks(2);
        Assert.Equal(1, upsert ? fsql.InsertOrUpdate<NativeTemporalRow>().SetSource(row).ExecuteAffrows()
            : fsql.Update<NativeTemporalRow>().SetSource(row).ExecuteAffrows());
        var updated = await fsql.Select<NativeTemporalRow>().ToOneAsync();
        Assert.Equal(row.At.Ticks, updated.At.Ticks);
        Assert.Equal(row.Offset.UtcTicks, updated.Offset.UtcTicks);
        Assert.Equal(TimeSpan.Zero, updated.Offset.Offset);
    }

    [SonnetDBTable, Table(Name = "sonnet_native_temporal_literals")]
    private sealed class NativeTemporalRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        [Column(DbType = "DATETIME2")] public DateTime At { get; set; }
        [Column(DbType = "DATETIMEOFFSET")] public DateTimeOffset Offset { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_offset_parameter_types")]
    private sealed class OffsetParameterRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public DateTimeOffset Legacy { get; set; }
        [Column(DbType = "DATETIME2")] public DateTimeOffset Precise { get; set; }
        [Column(DbType = "DATETIMEOFFSET")] public DateTimeOffset Offset { get; set; }
        public DateTimeOffset? OptionalLegacy { get; set; }
        [Column(DbType = "DATETIME2")] public DateTimeOffset? OptionalPrecise { get; set; }
        [Column(DbType = "DATETIMEOFFSET")] public DateTimeOffset? OptionalOffset { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_legacy_offset_parameter")]
    private sealed class LegacyOffsetRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public DateTimeOffset Value { get; set; }
        public DateTimeOffset? OptionalValue { get; set; }
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

    [Table(Name = "sonnet_raw_vector_parameter")]
    private sealed class VectorParameterRow
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [Column(DbType = "FIELD VECTOR(2)", MapType = typeof(float[]))]
        public float[] Embedding { get; set; } = [];
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
