using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBProviderTests
{
    static readonly DateTime TestTime = new(2026, 4, 30, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Build_WithSonnetDBDataType_WiresProvider()
    {
        using var fsql = CreateFreeSql();

        Assert.Equal(DataType.SonnetDB, fsql.Ado.DataType);
        Assert.StartsWith("FreeSql.SonnetDB.SonnetDBProvider`1", fsql.GetType().FullName);
    }

    [Fact]
    public void Select_ToSql_PreservesSonnetDBAliasQualification()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<SonnetMetric>()
            .Where(a => a.Host == "server-01" && a.Time >= TestTime)
            .Limit(10)
            .ToSql();

        Assert.Equal(
            "SELECT a.time, a.\"Host\", a.\"Usage\", a.\"Ok\" \r\n" +
            "FROM \"sonnet_metric\" a \r\n" +
            "WHERE (a.\"Host\" = 'server-01' AND a.time >= 1777536000000) \r\n" +
            "limit 10",
            sql);
    }

    [Fact]
    public void Select_ToSql_PreservesAliasForDottedTableName()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<SonnetMetric>()
            .AsTable((_, _) => "metrics.sonnet_metric")
            .Where(a => a.Host == "server-01")
            .ToSql();

        Assert.Equal(
            "SELECT a.time, a.\"Host\", a.\"Usage\", a.\"Ok\" \r\n" +
            "FROM \"metrics\".\"sonnet_metric\" a \r\n" +
            "WHERE (a.\"Host\" = 'server-01')",
            sql);
    }

    [Fact]
    public void Insert_ToSql_GeneratesSonnetDBSql()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Insert(new SonnetMetric
        {
            Time = TestTime,
            Host = "server-01",
            Usage = 0.71,
            Ok = true
        }).ToSql();

        Assert.Equal(
            "INSERT INTO \"sonnet_metric\"(time, \"Host\", \"Usage\", \"Ok\") VALUES(1777536000000, 'server-01', 0.71, true)",
            sql);
    }

    [Fact]
    public void Delete_ToSql_GeneratesSonnetDBSql()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Delete<SonnetMetric>()
            .Where(a => a.Host == "server-01")
            .ToSql();

        Assert.Equal("DELETE FROM \"sonnet_metric\" WHERE (\"Host\" = 'server-01')", sql);
    }

    [Fact]
    public void MeasurementUpdate_ThrowsClearNotSupported()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() => fsql.Update<SonnetMetric>());

        Assert.Contains("UPDATE", ex.Message);
        Assert.Contains("关系表", ex.Message);
    }

    [Fact]
    public void Update_AsTypeMeasurement_IsRejectedUsingCurrentModel()
    {
        using var fsql = CreateFreeSql();

        // AsType 在 UpdateProvider 构造后替换内部表映射，必须按替换后的
        // 时序测量模型重新拒绝 UPDATE。
        var ex = Assert.Throws<NotSupportedException>(() => fsql.Update<SonnetDevice>()
            .Set(a => a.Name, "invalid")
            .Where("1 = 1")
            .AsType(typeof(SonnetMetric)));
        Assert.Contains("UPDATE", ex.Message);
        Assert.Contains("关系表", ex.Message);
    }

    [Fact]
    public void Update_ObjectAsTypeRelationship_RemainsSupported()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Update<object>()
            .AsType(typeof(SonnetDevice))
            .SetByPropertyName("Name", "dynamic-name")
            .Where("Id = 1")
            .ToSql();

        Assert.Contains("UPDATE \"sonnet_device\" SET \"Name\" = 'dynamic-name'", sql);
        Assert.Contains("WHERE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Id", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("= 1", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InsertOrUpdate_ThrowsClearNotSupported()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() => fsql.InsertOrUpdate<SonnetMetric>());

        Assert.Contains("原子", ex.Message);
        Assert.Contains("InsertOrUpdate", ex.Message);
    }

    [Fact]
    public void ExecuteIdentity_ReportsAffrowsToCurdAfter()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        object? executeResult = null;

        fsql.Aop.CurdAfter += (_, e) =>
        {
            if (e.CurdType == Aop.CurdType.Insert)
                executeResult = e.ExecuteResult;
        };

        var identity = fsql.Insert(new SonnetMetric
        {
            Time = TestTime,
            Host = "server-01",
            Usage = 0.71,
            Ok = true
        }).ExecuteIdentity();

        Assert.Equal(0, identity);
        var affrows = Assert.IsType<long>(executeResult);
        Assert.Equal(1, affrows);
    }

    [Fact]
    public async Task ExecuteIdentityAsync_ReportsAffrowsToCurdAfter()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        object? executeResult = null;

        fsql.Aop.CurdAfter += (_, e) =>
        {
            if (e.CurdType == Aop.CurdType.Insert)
                executeResult = e.ExecuteResult;
        };

        var identity = await fsql.Insert(new SonnetMetric
        {
            Time = TestTime,
            Host = "server-01",
            Usage = 0.71,
            Ok = true
        }).ExecuteIdentityAsync();

        Assert.Equal(0, identity);
        var affrows = Assert.IsType<long>(executeResult);
        Assert.Equal(1, affrows);
    }

    [Fact]
    public void CodeFirst_RelationshipTable_GeneratesSupportedDdl()
    {
        using var fsql = CreateFreeSql();

        var ddl = fsql.CodeFirst.GetComparisonDDLStatements<SonnetDevice>();

        Assert.Contains("CREATE TABLE IF NOT EXISTS \"sonnet_device\"", ddl);
        Assert.Contains("\"Id\" INT AUTO_INCREMENT", ddl);
        Assert.Contains("\"Name\" STRING NOT NULL", ddl);
        Assert.Contains("\"InstalledAt\" DATETIME NULL", ddl);
        Assert.Contains("\"Version\" INT ROWVERSION", ddl);
        Assert.Contains("\"Enabled\" BOOL NOT NULL DEFAULT TRUE", ddl);
        Assert.Contains("\"Metadata\" JSON NULL", ddl);
        Assert.Contains("\"Payload\" BLOB NULL", ddl);
        Assert.Contains("PRIMARY KEY (\"Id\")", ddl);
        Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS \"ux_sonnet_device_name\" ON \"sonnet_device\" (\"Name\")", ddl);
        Assert.DoesNotContain("FIELD", ddl);
        Assert.DoesNotContain(" TAG", ddl);
    }

    [Fact]
    public void CodeFirst_RelationshipTable_SyncsSchemaAndIndexes()
    {
        using var fsql = CreateFreeSql();

        fsql.CodeFirst.SyncStructure<SonnetDevice>();

        Assert.Contains(fsql.Ado.Query<string>("SHOW TABLES"),
            name => string.Equals(name, "sonnet_device", StringComparison.OrdinalIgnoreCase));

        var columns = fsql.Ado.ExecuteDataTable("DESCRIBE TABLE \"sonnet_device\"");
        Assert.Equal(7, columns.Rows.Count);
        Assert.Equal("datetime", Convert.ToString(columns.Rows.Cast<System.Data.DataRow>()
            .Single(row => string.Equals(Convert.ToString(row["column_name"]), "InstalledAt", StringComparison.OrdinalIgnoreCase))["data_type"]));
        Assert.True(Convert.ToBoolean(columns.Rows.Cast<System.Data.DataRow>()
            .Single(row => string.Equals(Convert.ToString(row["column_name"]), "Id", StringComparison.OrdinalIgnoreCase))["is_auto_increment"]));

        var indexes = fsql.Ado.ExecuteDataTable("SHOW INDEXES ON \"sonnet_device\"");
        Assert.Single(indexes.Rows.Cast<System.Data.DataRow>());
        Assert.Equal("ux_sonnet_device_name", Convert.ToString(indexes.Rows[0]["index_name"]));
        Assert.True(Convert.ToBoolean(indexes.Rows[0]["is_unique"]));
    }

    [Fact]
    public void DbFirst_RelationshipTable_ReturnsCompleteMetadata()
    {
        using var fsql = CreateFreeSql();

        fsql.CodeFirst.SyncStructure<SonnetDevice>();

        var table = fsql.DbFirst.GetTableByName("sonnet_device");
        Assert.NotNull(table);
        Assert.Equal("sonnet_device", table!.Name, ignoreCase: true);

        var id = Assert.Single(table.Columns.Where(a => string.Equals(a.Name, "Id", StringComparison.OrdinalIgnoreCase)));
        Assert.True(id.IsPrimary);
        Assert.True(id.IsIdentity);
        Assert.Equal(typeof(long), id.CsType);
        Assert.Contains(id, table.Primarys);
        Assert.Contains(id, table.Identitys);

        var installedAt = Assert.Single(table.Columns.Where(a => string.Equals(a.Name, "InstalledAt", StringComparison.OrdinalIgnoreCase)));
        Assert.True(installedAt.IsNullable);
        Assert.Equal(typeof(DateTime), installedAt.CsType);
        Assert.Contains("DATETIME", installedAt.DbTypeTextFull, StringComparison.OrdinalIgnoreCase);

        var payload = Assert.Single(table.Columns.Where(a => string.Equals(a.Name, "Payload", StringComparison.OrdinalIgnoreCase)));
        Assert.Equal(typeof(byte[]), payload.CsType);
        Assert.Contains("BLOB", payload.DbTypeTextFull, StringComparison.OrdinalIgnoreCase);

        var metadata = Assert.Single(table.Columns.Where(a => string.Equals(a.Name, "Metadata", StringComparison.OrdinalIgnoreCase)));
        Assert.Equal(typeof(string), metadata.CsType);
        Assert.Contains("JSON", metadata.DbTypeTextFull, StringComparison.OrdinalIgnoreCase);

        var version = Assert.Single(table.Columns.Where(a => string.Equals(a.Name, "Version", StringComparison.OrdinalIgnoreCase)));
        Assert.Equal(typeof(long), version.CsType);
        Assert.Contains("ROWVERSION", version.DbTypeTextFull, StringComparison.OrdinalIgnoreCase);

        var unique = Assert.Single(table.Uniques.Where(a => string.Equals(a.Name, "ux_sonnet_device_name", StringComparison.OrdinalIgnoreCase)));
        Assert.True(unique.IsUnique);
        var uniqueColumn = Assert.Single(unique.Columns);
        Assert.Equal("Name", uniqueColumn.Column.Name, ignoreCase: true);
    }

    [Fact]
    public void DbFirst_RelationshipTable_ReturnsForeignKeyMetadata()
    {
        using var fsql = CreateFreeSql();
        fsql.Ado.ExecuteNonQuery(System.Data.CommandType.Text,
            "CREATE TABLE \"sonnet_fk_parent\" (\"Id\" INT NOT NULL, PRIMARY KEY (\"Id\"))");
        fsql.Ado.ExecuteNonQuery(System.Data.CommandType.Text,
            "CREATE TABLE \"sonnet_fk_child\" (\"Id\" INT NOT NULL, \"ParentId\" INT NOT NULL, " +
            "PRIMARY KEY (\"Id\"), FOREIGN KEY (\"ParentId\") " +
            "REFERENCES \"sonnet_fk_parent\" (\"Id\") ON DELETE CASCADE)");

        var child = Assert.Single(fsql.DbFirst.GetTablesByDatabase()
            .Where(table => string.Equals(table.Name, "sonnet_fk_child", StringComparison.OrdinalIgnoreCase)));
        var foreign = Assert.Single(child.Foreigns);

        Assert.Equal("sonnet_fk_parent", foreign.ReferencedTable.Name, ignoreCase: true);
        Assert.Equal("ParentId", Assert.Single(foreign.Columns).Name, ignoreCase: true);
        Assert.Equal("Id", Assert.Single(foreign.ReferencedColumns).Name, ignoreCase: true);
    }

    [Fact]
    public void DbFirst_GetTableByName_ReturnsForeignKeyMetadata()
    {
        using var fsql = CreateFreeSql();
        fsql.Ado.ExecuteNonQuery(System.Data.CommandType.Text,
            "CREATE TABLE \"sonnet_fk_parent_single\" (\"Id\" INT NOT NULL, PRIMARY KEY (\"Id\"))");
        fsql.Ado.ExecuteNonQuery(System.Data.CommandType.Text,
            "CREATE TABLE \"sonnet_fk_child_single\" (\"Id\" INT NOT NULL, \"ParentId\" INT NOT NULL, " +
            "PRIMARY KEY (\"Id\"), FOREIGN KEY (\"ParentId\") " +
            "REFERENCES \"sonnet_fk_parent_single\" (\"Id\") ON DELETE CASCADE)");

        var child = fsql.DbFirst.GetTableByName("sonnet_fk_child_single");
        var foreign = Assert.Single(child!.Foreigns);

        Assert.Equal("sonnet_fk_parent_single", foreign.ReferencedTable.Name, ignoreCase: true);
        Assert.Equal("ParentId", Assert.Single(foreign.Columns).Name, ignoreCase: true);
        Assert.Equal("Id", Assert.Single(foreign.ReferencedColumns).Name, ignoreCase: true);
    }

    [Fact]
    public void CodeFirst_RelationshipTable_RequiresPrimaryKey()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<InvalidOperationException>(
            () => fsql.CodeFirst.GetComparisonDDLStatements<SonnetDeviceWithoutPrimaryKey>());

        Assert.Contains("主键", ex.Message);
    }

    [Fact]
    public void CodeFirst_NavigateIsNotPhysicalForeignKey()
    {
        using var fsql = CreateFreeSql();

        var ddl = fsql.CodeFirst.GetComparisonDDLStatements(
            typeof(SonnetNavigationParent), typeof(SonnetNavigationChild));

        Assert.DoesNotContain("FOREIGN KEY", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("REFERENCES", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE IF NOT EXISTS \"sonnet_navigation_parent\"", ddl);
        Assert.Contains("CREATE TABLE IF NOT EXISTS \"sonnet_navigation_child\"", ddl);
    }

    [Fact]
    public void RelationshipTable_CrudReturningAndRowVersion_Work()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var source = new SonnetDevice
        {
            Name = "device-" + Guid.NewGuid().ToString("N"),
            InstalledAt = TestTime,
            Enabled = true,
            Metadata = "{\"kind\":\"pump\"}",
            Payload = new byte[] { 1, 2, 3 }
        };

        source.Id = fsql.Insert(source).ExecuteIdentity();
        Assert.True(source.Id > 0);

        var inserted = fsql.Insert(new SonnetDevice
        {
            Name = "device-" + Guid.NewGuid().ToString("N"),
            InstalledAt = TestTime.AddDays(1),
            Enabled = false,
            Metadata = "{\"kind\":\"fan\"}",
            Payload = new byte[] { 4, 5 }
        }).ExecuteInserted().Single();
        Assert.True(inserted.Id > 0);
        Assert.Equal(1, inserted.Version);
        Assert.Equal(TestTime.AddDays(1), inserted.InstalledAt);
        Assert.Equal(new byte[] { 4, 5 }, inserted.Payload);

        var loaded = fsql.Select<SonnetDevice>().Where(a => a.Id == source.Id).ToOne();
        Assert.NotNull(loaded);
        var oldVersion = loaded!.Version;
        loaded.Name = loaded.Name + "-updated";
        Assert.Equal(1, fsql.Update<SonnetDevice>().SetSource(loaded).ExecuteAffrows());
        Assert.Equal(oldVersion + 1, loaded.Version);

        var refreshed = fsql.Select<SonnetDevice>().Where(a => a.Id == source.Id).ToOne();
        Assert.Equal(loaded.Name, refreshed!.Name);
        Assert.Equal(oldVersion + 1, refreshed.Version);
        Assert.Equal(1, fsql.Delete<SonnetDevice>().Where(a => a.Id == source.Id).ExecuteAffrows());
        Assert.Null(fsql.Select<SonnetDevice>().Where(a => a.Id == source.Id).ToOne());
    }

    [Fact]
    public void RelationshipTable_SelectToUpdate_ExecutesWithSinglePrimaryKey()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var name = "select-update-" + Guid.NewGuid().ToString("N");
        var device = new SonnetDevice { Name = name, Enabled = true };
        device.Id = fsql.Insert(device).ExecuteIdentity();

        var update = fsql.Select<SonnetDevice>()
            .Where(a => a.Name == name)
            .ToUpdate()
            .Set(a => a.Enabled, false);
        var sql = update.ToSql();

        Assert.DoesNotContain("select * from", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("select ftb_upd.\"Id\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, update.ExecuteAffrows());
        Assert.False(fsql.Select<SonnetDevice>().Where(a => a.Id == device.Id).ToOne()!.Enabled);
    }

    [Fact]
    public void RelationshipTable_SelectToDelete_ExecutesWithSinglePrimaryKey()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var name = "select-delete-" + Guid.NewGuid().ToString("N");
        var device = new SonnetDevice { Name = name, Enabled = true };
        device.Id = fsql.Insert(device).ExecuteIdentity();

        var delete = fsql.Select<SonnetDevice>()
            .Where(a => a.Name == name)
            .ToDelete();
        var sql = delete.ToSql();

        Assert.DoesNotContain("select * from", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("select ftb_del.\"Id\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, delete.ExecuteAffrows());
        Assert.Null(fsql.Select<SonnetDevice>().Where(a => a.Id == device.Id).ToOne());
    }

    [Fact]
    public async Task RelationshipTable_SelectToDelete_ExecutesAsyncWithSinglePrimaryKey()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var name = "select-delete-async-" + Guid.NewGuid().ToString("N");
        var device = new SonnetDevice { Name = name, Enabled = true };
        device.Id = await fsql.Insert(device).ExecuteIdentityAsync();

        var delete = fsql.Select<SonnetDevice>()
            .Where(a => a.Name == name)
            .ToDelete();
        var sql = delete.ToSql();

        Assert.DoesNotContain("select * from", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("select ftb_del.\"Id\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, await delete.ExecuteAffrowsAsync());
        Assert.Null(await fsql.Select<SonnetDevice>().Where(a => a.Id == device.Id).ToOneAsync());
    }

    [Fact]
    public void RelationshipTable_SelectToUpdate_UsesSingleProjectionForCompositePrimaryKey()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var name = "select-update-composite-" + Guid.NewGuid().ToString("N");
        var row = new SonnetCompositeDevice { TenantId = 7, DeviceId = 11, Name = name, Enabled = true };
        Assert.Equal(1, fsql.Insert(row).ExecuteAffrows());

        var update = fsql.Select<SonnetCompositeDevice>()
            .Where(a => a.Name == name)
            .ToUpdate()
            .Set(a => a.Enabled, false);
        var sql = update.ToSql();

        Assert.DoesNotContain("select * from", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("select ftb_upd.as1", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, update.ExecuteAffrows());
        Assert.False(fsql.Select<SonnetCompositeDevice>()
            .Where(a => a.TenantId == row.TenantId && a.DeviceId == row.DeviceId)
            .ToOne()!.Enabled);
    }

    [Fact]
    public void RelationshipTable_TransactionRollbackAndCommit_Work()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        // SonnetDB 不允许事务内执行 DDL，先完成 CodeFirst 建表。
        fsql.CodeFirst.SyncStructure<SonnetDevice>();
        var rollbackName = "rollback-" + Guid.NewGuid().ToString("N");
        Assert.Throws<InvalidOperationException>(() => fsql.Ado.Transaction(() =>
        {
            fsql.Insert(new SonnetDevice { Name = rollbackName }).ExecuteAffrows();
            throw new InvalidOperationException("测试回滚");
        }));
        Assert.Null(fsql.Select<SonnetDevice>().Where(a => a.Name == rollbackName).ToOne());

        var commitName = "commit-" + Guid.NewGuid().ToString("N");
        fsql.Ado.Transaction(() => fsql.Insert(new SonnetDevice { Name = commitName }).ExecuteAffrows());
        Assert.NotNull(fsql.Select<SonnetDevice>().Where(a => a.Name == commitName).ToOne());
    }

    [Fact]
    public void MeasurementWriteInsideTransaction_IsRejected()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        fsql.CodeFirst.SyncStructure<SonnetMetric>();
        var ex = Assert.Throws<NotSupportedException>(() => fsql.Ado.Transaction(() =>
            fsql.Insert(new SonnetMetric
            {
                Time = TestTime,
                Host = "transaction-rejected",
                Usage = 1,
                Ok = true
            }).ExecuteAffrows()));
        Assert.Contains("事务", ex.Message);
    }

    [Fact]
    public void MeasurementDeleteInsideTransaction_IsRejected()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        fsql.CodeFirst.SyncStructure<SonnetMetric>();
        var host = "delete-transaction-rejected-" + Guid.NewGuid().ToString("N");
        fsql.Insert(new SonnetMetric
        {
            Time = TestTime,
            Host = host,
            Usage = 1,
            Ok = true
        }).ExecuteAffrows();

        var ex = Assert.Throws<NotSupportedException>(() => fsql.Ado.Transaction(() =>
            fsql.Delete<SonnetMetric>().Where(a => a.Host == host).ExecuteAffrows()));

        Assert.Contains("删除", ex.Message);
        Assert.NotNull(fsql.Select<SonnetMetric>().Where(a => a.Host == host).ToOne());
    }

    [Fact]
    public void MeasurementDelete_ReturnsDeletedSeriesCount()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var host = "delete-count-" + Guid.NewGuid().ToString("N");
        fsql.Insert(new SonnetMetric
        {
            Time = TestTime,
            Host = host,
            Usage = 1,
            Ok = true
        }).ExecuteAffrows();

        // SonnetDB's low-level DELETE result counts tombstones for each field;
        // FreeSql reports the number of deleted measurement rows.
        Assert.Equal(1, fsql.Delete<SonnetMetric>().Where(a => a.Host == host).ExecuteAffrows());
    }

    [Fact]
    public void RelationshipTable_ParameterizedCrud_Works()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true, noneCommandParameter: false,
            generateCommandParameterWithLambda: true);
        var name = "parameterized-" + Guid.NewGuid().ToString("N");
        var source = new SonnetDevice
        {
            Name = name,
            InstalledAt = TestTime,
            Enabled = true,
            Metadata = "{\"source\":\"parameter\"}",
            Payload = new byte[] { 9, 8, 7 }
        };
        var commands = new List<Aop.CurdBeforeEventArgs>();
        fsql.Aop.CurdBefore += (_, e) => commands.Add(e);

        source.Id = fsql.Insert(source).ExecuteIdentity();
        Assert.True(source.Id > 0);

        var loaded = fsql.Select<SonnetDevice>().Where(a => a.Name == name).ToOne();
        Assert.NotNull(loaded);
        Assert.Equal(TestTime, loaded!.InstalledAt);
        Assert.Equal(source.Metadata, loaded.Metadata);
        Assert.Equal(source.Payload, loaded.Payload);

        loaded.Enabled = false;
        loaded.Metadata = "{\"source\":\"updated\"}";
        Assert.Equal(1, fsql.Update<SonnetDevice>().SetSource(loaded).ExecuteAffrows());
        var updated = fsql.Select<SonnetDevice>().Where(a => a.Id == source.Id && a.Enabled == false).ToOne();
        Assert.NotNull(updated);
        Assert.Equal(loaded.Metadata, updated!.Metadata);

        Assert.Equal(1, fsql.Delete<SonnetDevice>().Where(a => a.Id == source.Id).ExecuteAffrows());
        foreach (var curdType in new[] { Aop.CurdType.Insert, Aop.CurdType.Select, Aop.CurdType.Update, Aop.CurdType.Delete })
        {
            Assert.Contains(commands, command =>
                command.CurdType == curdType &&
                command.DbParms?.Length > 0 &&
                command.Sql.Contains("@", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Measurement_ParameterizedWriteAndRead_Works()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true, noneCommandParameter: false);
        var host = "parameterized-measurement-" + Guid.NewGuid().ToString("N");
        var metric = new SonnetMetric
        {
            Time = TestTime,
            Host = host,
            Usage = 0.37,
            Ok = true
        };

        Assert.Equal(1, fsql.Insert(metric).ExecuteAffrows());
        var loaded = fsql.Select<SonnetMetric>()
            .Where(a => a.Host == host && a.Time == TestTime)
            .ToOne();

        Assert.NotNull(loaded);
        Assert.Equal(metric.Usage, loaded!.Usage);
        Assert.Equal(metric.Ok, loaded.Ok);
    }

    [Fact]
    public void MeasurementJoins_ThrowChineseNotSupported()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() => fsql.Select<SonnetMetric>()
            .InnerJoin<SonnetDevice>((a, b) => a.Host == b.Name)
            .ToSql());

        Assert.Contains("参数绑定", ex.Message);
        Assert.Contains("JOIN", ex.Message);
        Assert.Contains("拒绝", ex.Message);

        var reverse = Assert.Throws<NotSupportedException>(() => fsql.Select<SonnetDevice, SonnetMetric>()
            .LeftJoin((a, b) => a.Name == b.Host)
            .ToSql());
        Assert.Contains("时序测量", reverse.Message);
        Assert.Contains("参数绑定", reverse.Message);
    }

    [Fact]
    public void UnsupportedJoinShapes_ThrowChineseNotSupported()
    {
        using var fsql = CreateFreeSql();

        var rightJoin = Assert.Throws<NotSupportedException>(() => fsql.Select<SonnetDevice, SonnetDevicePeer>()
            .RightJoin((a, b) => a.Id == b.Id)
            .ToSql());
        Assert.Contains("RIGHT JOIN", rightJoin.Message);
        Assert.Contains("仅支持", rightJoin.Message);

        var rawOuterJoin = Assert.Throws<NotSupportedException>(() => fsql.Select<SonnetDevice, SonnetDevicePeer>()
            .RawJoin("FULL JOIN \"sonnet_device_peer\" b ON a.\"Id\" = b.\"Id\"")
            .ToSql());
        Assert.Contains("拒绝", rawOuterJoin.Message);

        var multipleRoots = Assert.Throws<NotSupportedException>(() => fsql.Select<SonnetDevice>()
            .From<SonnetDevicePeer>((a, b) => a)
            .ToSql());
        Assert.Contains("多根 FROM", multipleRoots.Message);
        Assert.Contains("笛卡尔积", multipleRoots.Message);
    }

    [Fact]
    public void RelationshipTableJoins_ExecuteWithParameters()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true, noneCommandParameter: false,
            generateCommandParameterWithLambda: true);
        var suffix = Guid.NewGuid().ToString("N");
        var deviceName = "join-device-" + suffix;
        var peerName = "join-peer-" + suffix;
        var groupName = "join-group-" + suffix;
        var commands = new List<Aop.CurdBeforeEventArgs>();
        fsql.Aop.CurdBefore += (_, e) => commands.Add(e);
        var device = new SonnetDevice { Name = deviceName, Enabled = true };
        device.Id = fsql.Insert(device).ExecuteIdentity();
        fsql.Insert(new SonnetDevicePeer { Id = device.Id, Name = peerName }).ExecuteAffrows();
        fsql.Insert(new SonnetDeviceGroup { Id = device.Id, DeviceId = device.Id, Name = groupName }).ExecuteAffrows();

        var innerRows = fsql.Select<SonnetDevice, SonnetDevicePeer>()
            .InnerJoin((a, b) => a.Id == b.Id)
            .Where((a, b) => a.Name == deviceName && b.Name == peerName)
            .ToList((a, b) => new { DeviceId = a.Id, PeerName = b.Name });
        var innerRow = Assert.Single(innerRows);
        Assert.Equal(device.Id, innerRow.DeviceId);
        Assert.Equal(peerName, innerRow.PeerName);

        var leftCount = fsql.Select<SonnetDevice, SonnetDevicePeer>()
            .LeftJoin((a, b) => a.Id == b.Id)
            .Where((a, b) => a.Id == device.Id)
            .Count();
        Assert.Equal(1, leftCount);

        var chainedCount = fsql.Select<SonnetDevice, SonnetDevicePeer, SonnetDeviceGroup>()
            .InnerJoin((a, b, c) => a.Id == b.Id)
            .InnerJoin((a, b, c) => b.Id == c.DeviceId)
            .Where((a, b, c) => a.Id == device.Id && c.Name == groupName)
            .Count();
        Assert.Equal(1, chainedCount);

        var chainedSql = fsql.Select<SonnetDevice, SonnetDevicePeer, SonnetDeviceGroup>()
            .InnerJoin((a, b, c) => a.Id == b.Id)
            .LeftJoin((a, b, c) => b.Id == c.DeviceId)
            .ToSql();
        Assert.Contains("INNER JOIN", chainedSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LEFT JOIN", chainedSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(commands, command =>
            command.CurdType == Aop.CurdType.Select &&
            command.DbParms?.Length > 0 &&
            command.Sql.Contains("JOIN", StringComparison.OrdinalIgnoreCase) &&
            command.Sql.Contains("@", StringComparison.Ordinal));
    }

    static IFreeSql CreateFreeSql(bool autoSyncStructure = false, bool noneCommandParameter = true,
        bool generateCommandParameterWithLambda = false)
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-Tests", Guid.NewGuid().ToString("N"));
        var builder = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseNoneCommandParameter(noneCommandParameter)
            .UseGenerateCommandParameterWithLambda(generateCommandParameterWithLambda);
        if (autoSyncStructure) builder.UseAutoSyncStructure(true);
        return builder.Build();
    }

    [Table(Name = "sonnet_metric")]
    sealed class SonnetMetric
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [SonnetDBTag]
        public string Host { get; set; } = "";

        [SonnetDBField]
        public double Usage { get; set; }

        [SonnetDBField]
        public bool Ok { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_device")]
    [Index("ux_sonnet_device_name", nameof(Name), true)]
    sealed class SonnetDevice
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        [Column(IsNullable = false)]
        public string Name { get; set; } = "";

        public DateTime? InstalledAt { get; set; }

        [Column(IsVersion = true)]
        public long Version { get; set; }

        [Column(DbType = "BOOL NOT NULL DEFAULT TRUE")]
        public bool Enabled { get; set; }

        [Column(DbType = "JSON NULL")]
        public string? Metadata { get; set; }

        [Column(DbType = "BLOB NULL")]
        public byte[]? Payload { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_composite_device")]
    sealed class SonnetCompositeDevice
    {
        [Column(IsPrimary = true)]
        public long TenantId { get; set; }

        [Column(IsPrimary = true)]
        public long DeviceId { get; set; }

        public string Name { get; set; } = "";

        public bool Enabled { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_device_without_primary_key")]
    sealed class SonnetDeviceWithoutPrimaryKey
    {
        public string Value { get; set; } = "";
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_navigation_parent")]
    sealed class SonnetNavigationParent
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_navigation_child")]
    sealed class SonnetNavigationChild
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public long ParentId { get; set; }

        [Navigate(nameof(ParentId))]
        public SonnetNavigationParent Parent { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_device_peer")]
    sealed class SonnetDevicePeer
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public string Name { get; set; } = "";
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_device_group")]
    sealed class SonnetDeviceGroup
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public long DeviceId { get; set; }

        public string Name { get; set; } = "";
    }
}
