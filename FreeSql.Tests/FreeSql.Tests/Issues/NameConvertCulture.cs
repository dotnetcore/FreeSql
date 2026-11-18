using System;
using FreeSql.Internal;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class NameConvertCulture
    {
        class NcLower { public int Id { get; set; } public string Image { get; set; } public string ImageUrl { get; set; } }
        class NcUnderscore { public int Id { get; set; } public string Image { get; set; } public string ImageUrl { get; set; } }
        class NcUpper { public int Id { get; set; } public string Title { get; set; } public string Image { get; set; } public string ImageUrl { get; set; } }
        class NcSync { public int Id { get; set; } public string Image { get; set; } public string ImageUrl { get; set; } }

        static void RunUnderTurkish(Action action)
        {
            var old = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                action();
            }
            finally { CultureInfo.CurrentCulture = old; }
        }

        static IFreeSql Create(Func<FreeSqlBuilder, FreeSqlBuilder> cfg) =>
            cfg(new FreeSqlBuilder().UseConnectionString(DataType.Sqlite, "Data Source=:memory:", typeof(FreeSql.Sqlite.SqliteProvider<>))).Build();

        [Fact]
        public void ToLowerNameConvert_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = Create(b => b.UseNameConvert(NameConvertType.ToLower));
                var table = fsql.CodeFirst.GetTableByEntity(typeof(NcLower));
                Assert.Equal("id", table.ColumnsByCs["Id"].Attribute.Name);
                Assert.Equal("image", table.ColumnsByCs["Image"].Attribute.Name);
                Assert.Equal("nclower", table.DbName);
            });
        }

        [Fact]
        public void PascalCaseToUnderscoreWithLower_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = Create(b => b.UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithLower));
                var table = fsql.CodeFirst.GetTableByEntity(typeof(NcUnderscore));
                Assert.Equal("id", table.ColumnsByCs["Id"].Attribute.Name);
                Assert.Equal("image_url", table.ColumnsByCs["ImageUrl"].Attribute.Name);
            });
        }

        [Fact]
        public void ToUpperNameConvert_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = Create(b => b.UseNameConvert(NameConvertType.ToUpper));
                var table = fsql.CodeFirst.GetTableByEntity(typeof(NcUpper));
                Assert.Equal("ID", table.ColumnsByCs["Id"].Attribute.Name);
                Assert.Equal("TITLE", table.ColumnsByCs["Title"].Attribute.Name);
                Assert.Equal("NCUPPER", table.DbName);
            });
        }

        [Fact]
        public void SyncStructureToLower_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = Create(b => b);
                fsql.CodeFirst.IsSyncStructureToLower = true;
                var table = fsql.CodeFirst.GetTableByEntity(typeof(NcSync));
                Assert.Equal("id", table.ColumnsByCs["Id"].Attribute.Name);
                Assert.Equal("image", table.ColumnsByCs["Image"].Attribute.Name);
            });
        }

        class Invoice { [FreeSql.DataAnnotations.Column(IsIdentity = true)] public int Id { get; set; } public string Title { get; set; } }

        [Fact]
        public void SqliteDbFirstExistsTable_IgnoreCase_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = Create(b => b.UseNameConvert(NameConvertType.ToLower));
                fsql.CodeFirst.SyncStructure<Invoice>();
                Assert.True(fsql.DbFirst.ExistsTable("invoice", false));
                Assert.True(fsql.DbFirst.ExistsTable("Invoice", true));
                Assert.True(fsql.DbFirst.ExistsTable("INVOICE", true));
                Assert.NotNull(fsql.DbFirst.GetTableByName("Invoice", true));
            });
        }

        [Fact]
        public void ZeroEntityTableName_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = Create(b => b);
                fsql.CodeFirst.IsSyncStructureToLower = true;
                var schema = new FreeSql.Extensions.ZeroEntity.TableDescriptor { Name = "Invoice" };
                schema.Columns.Add(new FreeSql.Extensions.ZeroEntity.TableDescriptor.ColumnDescriptor { Name = "Id", MapType = typeof(int), IsPrimary = true, IsIdentity = true });
                var ctx = new FreeSql.Extensions.ZeroEntity.ZeroDbContext(fsql, new[] { schema }, true);
                Assert.Equal("invoice", ctx.GetTableInfo("Invoice").DbName);
                Assert.True(fsql.DbFirst.ExistsTable("Invoice", true));
                Assert.Same(ctx.GetTableInfo("Invoice"), ctx.GetTableInfo("INVOICE"));
            });
        }

        // A connection which never reaches a server: every query finds nothing, so the CodeFirst provider
        // writes the DDL for a new table and the statements can be read without a database.
        class OfflineConnection : DbConnection
        {
            ConnectionState _state = ConnectionState.Closed;
            public override string ConnectionString { get; set; } = "Host=localhost;User Id=scott;Password=x;Database=d";
            public override string Database => "offline";
            public override string DataSource => "offline";
            public override string ServerVersion => "1.0";
            public override ConnectionState State => _state;
            public override void ChangeDatabase(string databaseName) { }
            public override void Close() => _state = ConnectionState.Closed;
            public override void Open() => _state = ConnectionState.Open;
            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotSupportedException();
            protected override DbCommand CreateDbCommand() => new OfflineCommand { Connection = this };
        }
        class OfflineCommand : DbCommand
        {
            public override string CommandText { get; set; } = "";
            public override int CommandTimeout { get; set; }
            public override CommandType CommandType { get; set; }
            public override bool DesignTimeVisible { get; set; }
            public override UpdateRowSource UpdatedRowSource { get; set; }
            protected override DbConnection DbConnection { get; set; }
            protected override DbParameterCollection DbParameterCollection { get; } = new OfflineParameters();
            protected override DbTransaction DbTransaction { get; set; }
            public override void Cancel() { }
            public override int ExecuteNonQuery() => 0;
            public override object ExecuteScalar() => null;
            public override void Prepare() { }
            protected override DbParameter CreateDbParameter() => new OfflineParameter();
            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => new DataTable().CreateDataReader();
        }
        class OfflineParameter : DbParameter
        {
            public override DbType DbType { get; set; }
            public override ParameterDirection Direction { get; set; }
            public override bool IsNullable { get; set; }
            public override string ParameterName { get; set; }
            public override int Size { get; set; }
            public override string SourceColumn { get; set; }
            public override bool SourceColumnNullMapping { get; set; }
            public override object Value { get; set; }
            public override void ResetDbType() { }
        }
        class OfflineParameters : DbParameterCollection
        {
            readonly System.Collections.Generic.List<DbParameter> _list = new System.Collections.Generic.List<DbParameter>();
            public override int Count => _list.Count;
            public override object SyncRoot => _list;
            public override int Add(object value) { _list.Add((DbParameter)value); return _list.Count - 1; }
            public override void AddRange(Array values) { foreach (var v in values) Add(v); }
            public override void Clear() => _list.Clear();
            public override bool Contains(object value) => _list.Contains((DbParameter)value);
            public override bool Contains(string value) => _list.Exists(a => a.ParameterName == value);
            public override void CopyTo(Array array, int index) => ((System.Collections.ICollection)_list).CopyTo(array, index);
            public override System.Collections.IEnumerator GetEnumerator() => _list.GetEnumerator();
            public override int IndexOf(object value) => _list.IndexOf((DbParameter)value);
            public override int IndexOf(string parameterName) => _list.FindIndex(a => a.ParameterName == parameterName);
            public override void Insert(int index, object value) => _list.Insert(index, (DbParameter)value);
            public override void Remove(object value) => _list.Remove((DbParameter)value);
            public override void RemoveAt(int index) => _list.RemoveAt(index);
            public override void RemoveAt(string parameterName) => _list.RemoveAt(IndexOf(parameterName));
            protected override DbParameter GetParameter(int index) => _list[index];
            protected override DbParameter GetParameter(string parameterName) => _list[IndexOf(parameterName)];
            protected override void SetParameter(int index, DbParameter value) => _list[index] = value;
            protected override void SetParameter(string parameterName, DbParameter value) => _list[IndexOf(parameterName)] = value;
        }

        class OfflineFactory : DbProviderFactory
        {
            public override DbConnection CreateConnection() => new OfflineConnection();
            public override DbCommand CreateCommand() => new OfflineCommand();
            public override DbParameter CreateParameter() => new OfflineParameter();
        }

        static string OfflineDdl(DataType dataType, Type providerType)
        {
            using var fsql = new FreeSqlBuilder().UseConnectionFactory(dataType, () => new OfflineConnection(), providerType).Build();
            fsql.SetDbProviderFactory(new OfflineFactory());
            return fsql.CodeFirst.GetComparisonDDLStatements<Invoice>();
        }

        [Fact]
        public void CustomOracleSequenceAndTrigger_AreCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                var ddl = OfflineDdl(DataType.CustomOracle, typeof(FreeSql.Custom.Oracle.CustomOracleProvider<>));
                Assert.Contains("INVOICE_SEQ_ID", ddl);
                Assert.Contains("INVOICE_SEQ_IDTI", ddl);
                Assert.DoesNotContain("İ", ddl);
            });
        }

        [Fact]
        public void CustomPostgreSQLSequence_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                var ddl = OfflineDdl(DataType.CustomPostgreSQL, typeof(FreeSql.Custom.PostgreSQL.CustomPostgreSQLProvider<>));
                Assert.Contains("invoice_id_sequence_name", ddl);
                Assert.DoesNotContain("ı", ddl);
            });
        }
    }
}
