using System.Data;
using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using SonnetDB.Tables;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBPublishedAdoContractTests
{
    const decimal ExactAmount = 123456789.1234567890123456789m;

    [Fact]
    public async Task Reader_EmptyResultsPreserveDeclaredColumnsWithoutReportingRows()
    {
        using var fsql = CreateFreeSql();
        fsql.CodeFirst.SyncStructure<AmountRow>();
        using var connection = fsql.Ado.MasterPool.Get();

        using (var command = connection.Value.CreateCommand())
        {
            command.CommandText = "SELECT \"Id\",\"Amount\" FROM \"sonnet_published_amount\" WHERE \"Id\"=-1";
            using var reader = command.ExecuteReader();
            Assert.False(reader.HasRows);
            Assert.Equal(2, reader.FieldCount);
            Assert.Equal(typeof(decimal), reader.GetFieldType(1));
            Assert.Equal(2, reader.GetSchemaTable()!.Rows.Count);
            Assert.False(reader.Read());
            Assert.False(reader.HasRows);
        }

        using (var command = connection.Value.CreateCommand())
        {
            command.CommandText = "SELECT \"Amount\" FROM \"sonnet_published_amount\" WHERE \"Id\"=-1";
            using var reader = await command.ExecuteReaderAsync();
            Assert.False(reader.HasRows);
            Assert.Equal(typeof(decimal), reader.GetFieldType(0));
            Assert.False(await reader.ReadAsync());
            Assert.False(reader.HasRows);
        }

        var emptyTable = await fsql.Ado.ExecuteDataTableAsync(
            "SELECT \"Id\",\"Amount\" FROM \"sonnet_published_amount\" WHERE \"Id\"=-1");
        Assert.Empty(emptyTable.Rows.Cast<DataRow>());
        Assert.Equal(typeof(decimal), emptyTable.Columns["Amount"]!.DataType);

        fsql.Insert(new AmountRow { Id = 1, Amount = ExactAmount }).ExecuteAffrows();
        using var nonemptyCommand = connection.Value.CreateCommand();
        nonemptyCommand.CommandText = "SELECT \"Amount\" FROM \"sonnet_published_amount\" WHERE \"Id\"=1";
        using var nonemptyReader = nonemptyCommand.ExecuteReader();
        Assert.True(nonemptyReader.HasRows);
        Assert.True(nonemptyReader.HasRows);
        Assert.True(nonemptyReader.Read());
        Assert.Equal(ExactAmount, nonemptyReader.GetDecimal(0));
        Assert.False(nonemptyReader.Read());
    }

    [Fact]
    public async Task RawAdo_DecimalParametersAndSqlLiteralsPreserveSignificantDigits()
    {
        using var fsql = CreateFreeSql();
        fsql.CodeFirst.SyncStructure<AmountRow>();

        var parameters = fsql.Ado.GetDbParamtersByObject(new { amount = ExactAmount });
        Assert.Equal(DbType.Decimal, Assert.Single(parameters).DbType);
        Assert.IsType<decimal>(parameters[0].Value);
        Assert.Equal(1, fsql.Ado.ExecuteNonQuery(
            "INSERT INTO \"sonnet_published_amount\"(\"Id\",\"Amount\") VALUES(1,@amount)",
            new { amount = ExactAmount }));
        Assert.Equal(1, await fsql.Ado.ExecuteNonQueryAsync(
            "INSERT INTO \"sonnet_published_amount\"(\"Id\",\"Amount\") VALUES(2,-123456789.1234567890123456789)"));

        Assert.Equal(ExactAmount, Assert.IsType<decimal>(fsql.Ado.ExecuteScalar(
            "SELECT \"Amount\" FROM \"sonnet_published_amount\" WHERE \"Amount\"=@amount",
            new { amount = ExactAmount })));
        Assert.Equal(-ExactAmount, Assert.Single(await fsql.Ado.QueryAsync<decimal>(
            "SELECT \"Amount\" FROM \"sonnet_published_amount\" WHERE \"Amount\"=-123456789.1234567890123456789")));
        Assert.Equal(0m, Assert.IsType<decimal>(fsql.Ado.ExecuteScalar(
            "SELECT SUM(\"Amount\") FROM \"sonnet_published_amount\"")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DecimalEntities_BatchWritesComparisonsAndUpdatesRemainExact(bool noneParameter)
    {
        using var fsql = CreateFreeSql(noneParameter);
        var source = new[]
        {
            new AmountRow { Id = 1, Amount = ExactAmount, OptionalAmount = -ExactAmount },
            new AmountRow { Id = 2, Amount = 9007199254740992.125m },
            new AmountRow { Id = 3, Amount = 9007199254740993.125m, OptionalAmount = 0m }
        };
        Assert.Equal(3, await fsql.Insert(source).ExecuteAffrowsAsync());
        var loaded = await fsql.Select<AmountRow>().OrderBy(row => row.Id).ToListAsync();
        Assert.Equal(source.Select(row => row.Amount), loaded.Select(row => row.Amount));
        Assert.Equal(source.Select(row => row.OptionalAmount), loaded.Select(row => row.OptionalAmount));

        var adjacentAmount = source[2].Amount;
        Assert.Equal(3, fsql.Select<AmountRow>()
            .Where(row => row.Amount == adjacentAmount).ToOne().Id);
        Assert.Equal(1, await fsql.Update<AmountRow>().Where(row => row.Id == 1)
            .Set(row => row.Amount, -ExactAmount).ExecuteAffrowsAsync());
        Assert.Equal(-ExactAmount, fsql.Select<AmountRow>().Where(row => row.Id == 1).ToOne().Amount);
        Assert.Equal(1, fsql.Delete<AmountRow>().Where(row => row.Amount == adjacentAmount).ExecuteAffrows());
        Assert.Equal(2, fsql.Select<AmountRow>().Count());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExplicitSerializableTransaction_RejectsChangedReadSetAndRollsBackItsWrites(bool useAsync)
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new[]
        {
            new IsolationRow { Id = 1, Value = 10 },
            new IsolationRow { Id = 2, Value = 30 }
        }).ExecuteAffrows();
        using var first = fsql.Ado.MasterPool.Get();
        using var second = fsql.Ado.MasterPool.Get();
        Assert.NotSame(first.Value, second.Value);
        using var transaction = first.Value.BeginTransaction(IsolationLevel.Serializable);
        Assert.Equal(IsolationLevel.Serializable, transaction.IsolationLevel);

        Assert.Equal(10, fsql.Select<IsolationRow>().WithTransaction(transaction)
            .Where(row => row.Id == 1).ToOne().Value);
        Assert.Equal(1, fsql.Update<IsolationRow>().WithConnection(second.Value)
            .Where(row => row.Id == 1).Set(row => row.Value, 20).ExecuteAffrows());
        Assert.Equal(1, fsql.Update<IsolationRow>().WithTransaction(transaction)
            .Where(row => row.Id == 2).Set(row => row.Value, 99).ExecuteAffrows());

        // Optimistic Serializable may observe a later value, but that history must not commit.
        var secondRead = fsql.Select<IsolationRow>().WithTransaction(transaction).Where(row => row.Id == 1);
        Assert.Equal(20, useAsync ? (await secondRead.ToOneAsync()).Value : secondRead.ToOne().Value);
        var conflict = useAsync
            ? await Assert.ThrowsAsync<TableConstraintException>(() => transaction.CommitAsync())
            : Assert.Throws<TableConstraintException>(() => transaction.Commit());
        Assert.Equal(TableConstraintException.SerializationConflict, conflict.ErrorCode);
        Assert.Equal(new long[] { 20, 30 }, fsql.Select<IsolationRow>()
            .WithConnection(second.Value).OrderBy(row => row.Id).ToList().Select(row => row.Value));

        using var retry = first.Value.BeginTransaction(IsolationLevel.Serializable);
        Assert.Equal(20, fsql.Select<IsolationRow>().WithTransaction(retry).Where(row => row.Id == 1).ToOne().Value);
        fsql.Update<IsolationRow>().WithTransaction(retry).Where(row => row.Id == 2)
            .Set(row => row.Value, 40).ExecuteAffrows();
        if (useAsync) await retry.CommitAsync();
        else retry.Commit();
        Assert.Equal(40, fsql.Select<IsolationRow>().WithConnection(second.Value).Where(row => row.Id == 2).ToOne().Value);
    }

    [Fact]
    public void AmbientSerializableTransaction_PropagatesCommitConflictToCaller()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new[]
        {
            new IsolationRow { Id = 1, Value = 10 },
            new IsolationRow { Id = 2, Value = 30 }
        }).ExecuteAffrows();
        using var competing = fsql.Ado.MasterPool.Get();

        var conflict = Assert.Throws<TableConstraintException>(() => fsql.Transaction(IsolationLevel.Serializable, () =>
        {
            Assert.Equal(10, fsql.Select<IsolationRow>().Where(row => row.Id == 1).ToOne().Value);
            fsql.Update<IsolationRow>().WithConnection(competing.Value)
                .Where(row => row.Id == 1).Set(row => row.Value, 20).ExecuteAffrows();
            fsql.Update<IsolationRow>().Where(row => row.Id == 2).Set(row => row.Value, 99).ExecuteAffrows();
        }));

        Assert.Equal(TableConstraintException.SerializationConflict, conflict.ErrorCode);
        Assert.Null(fsql.Ado.TransactionCurrentThread);
        Assert.Equal(new long[] { 20, 30 }, fsql.Select<IsolationRow>()
            .OrderBy(row => row.Id).ToList().Select(row => row.Value));
    }

    [Fact]
    public void AmbientTransaction_PropagatesDuplicateKeyFailureAndRollsBackAllWrites()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new IsolationRow { Id = 1, Value = 10 }).ExecuteAffrows();

        Assert.Throws<TableConstraintException>(() => fsql.Transaction(() =>
        {
            fsql.Insert(new IsolationRow { Id = 2, Value = 20 }).ExecuteAffrows();
            fsql.Insert(new IsolationRow { Id = 1, Value = 99 }).ExecuteAffrows();
        }));

        Assert.Null(fsql.Ado.TransactionCurrentThread);
        var retained = Assert.Single(fsql.Select<IsolationRow>().ToList());
        Assert.Equal(1, retained.Id);
        Assert.Equal(10, retained.Value);
        fsql.Transaction(() => fsql.Insert(new IsolationRow { Id = 2, Value = 20 }).ExecuteAffrows());
        Assert.Equal(2, fsql.Select<IsolationRow>().Count());
    }

    [Fact]
    public void AmbientTransaction_BusinessExceptionRemainsTheOriginalFailure()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new IsolationRow { Id = 1, Value = 10 }).ExecuteAffrows();
        var failure = new InvalidOperationException("Business operation aborted.");

        var observed = Assert.Throws<InvalidOperationException>(() => fsql.Transaction(IsolationLevel.Serializable, () =>
        {
            fsql.Update<IsolationRow>().Where(row => row.Id == 1).Set(row => row.Value, 99).ExecuteAffrows();
            throw failure;
        }));

        Assert.Same(failure, observed);
        Assert.Null(fsql.Ado.TransactionCurrentThread);
        Assert.Equal(10, fsql.Select<IsolationRow>().ToOne().Value);
    }

    [Fact]
    public void DecimalMigrations_GetSchemaAndDbFirstReportTheActualPrecisionAndScale()
    {
        using var fsql = CreateFreeSql();
        fsql.Ado.ExecuteNonQuery(
            "CREATE TABLE \"sonnet_published_metadata\" (\"Id\" INT, \"Amount\" DECIMAL(18,4) NULL, PRIMARY KEY(\"Id\"))");
        fsql.Ado.ExecuteNonQuery(
            "ALTER TABLE \"sonnet_published_metadata\" ADD COLUMN \"Fee\" DECIMAL(12,2) NULL");
        fsql.Ado.ExecuteNonQuery(
            "ALTER TABLE \"sonnet_published_metadata\" ALTER COLUMN \"Amount\" TYPE DECIMAL(15,3) NULL");

        using var connection = fsql.Ado.MasterPool.Get();
        var schema = connection.Value.GetSchema("Columns", [null, null, "sonnet_published_metadata", null]);
        var amount = Assert.Single(schema.Rows.Cast<DataRow>().Where(row => (string)row["COLUMN_NAME"] == "Amount"));
        var fee = Assert.Single(schema.Rows.Cast<DataRow>().Where(row => (string)row["COLUMN_NAME"] == "Fee"));
        Assert.Equal(15, Convert.ToInt32(amount["NUMERIC_PRECISION"]));
        Assert.Equal(3, Convert.ToInt32(amount["NUMERIC_SCALE"]));
        Assert.Equal(12, Convert.ToInt32(fee["NUMERIC_PRECISION"]));
        Assert.Equal(2, Convert.ToInt32(fee["NUMERIC_SCALE"]));

        var table = fsql.DbFirst.GetTableByName("sonnet_published_metadata");
        Assert.NotNull(table);
        var mappedAmount = Assert.Single(table.Columns.Where(column => column.Name == "Amount"));
        var mappedFee = Assert.Single(table.Columns.Where(column => column.Name == "Fee"));
        Assert.Equal(15, mappedAmount.Precision);
        Assert.Equal(3, mappedAmount.Scale);
        Assert.Equal(12, mappedFee.Precision);
        Assert.Equal(2, mappedFee.Scale);
    }

    [Fact]
    public async Task RawAdo_BinaryAndSmallIntegerParametersKeepTheirValues()
    {
        using var fsql = CreateFreeSql();
        fsql.Ado.ExecuteNonQuery(
            "CREATE TABLE \"sonnet_published_binary\" (\"Id\" INT,\"Payload\" BLOB,\"Signed\" INT,\"Unsigned\" INT,PRIMARY KEY(\"Id\"))");
        byte[] payload = [0, 1, 127, 128, 255];
        Assert.Equal(1, await fsql.Ado.ExecuteNonQueryAsync(
            "INSERT INTO \"sonnet_published_binary\"(\"Id\",\"Payload\",\"Signed\",\"Unsigned\") VALUES(1,@payload,@signed,@unsigned)",
            new { payload, signed = sbyte.MinValue, unsigned = uint.MaxValue }));
        var row = Assert.Single(fsql.Ado.ExecuteArray(
            "SELECT \"Payload\",\"Signed\",\"Unsigned\" FROM \"sonnet_published_binary\" WHERE \"Payload\"=@payload",
            new { payload }));
        Assert.Equal(payload, Assert.IsType<byte[]>(row[0]));
        Assert.Equal((long)sbyte.MinValue, Assert.IsType<long>(row[1]));
        Assert.Equal((long)uint.MaxValue, Assert.IsType<long>(row[2]));
    }

    static IFreeSql CreateFreeSql(bool noneParameter = false) => new FreeSqlBuilder()
        .UseConnectionString(DataType.SonnetDB,
            $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-PublishedAdo", Guid.NewGuid().ToString("N"))}")
        .UseAutoSyncStructure(true)
        .UseNoneCommandParameter(noneParameter)
        .UseGenerateCommandParameterWithLambda(!noneParameter)
        .Build();

    [SonnetDBTable, Table(Name = "sonnet_published_amount")]
    sealed class AmountRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public decimal Amount { get; set; }
        public decimal? OptionalAmount { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_published_isolation")]
    sealed class IsolationRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public long Value { get; set; }
    }
}
