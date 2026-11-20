using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Globalization;
using FreeSql.DatabaseModel;
using Xunit;

namespace FreeSql.Tests.Issues
{
    // DbFirst maps the column type a database reports by lower-casing it and comparing it with names such as
    // "integer" or "int32". Databases report upper-case or Pascal-case type names (SQLite "INTEGER", ClickHouse
    // "Int32", Oracle "TIMESTAMP(6)"), which under tr-TR lower-case to "ınteger", "ınt32" and "tımestamp(6)".
    public class DbFirstTypeCulture
    {
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

        static DbColumnInfo Column(string type, string full = null) =>
            new DbColumnInfo { DbTypeText = type, DbTypeTextFull = full ?? type };

        [Fact]
        public void Sqlite_UpperCaseTypes_AreMappedUnderTurkishCulture()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Sqlite, "Data Source=:memory:", typeof(FreeSql.Sqlite.SqliteProvider<>))
                    .Build();

                Assert.Equal((int)DbType.Int32, fsql.DbFirst.GetDbType(Column("INTEGER")));
                Assert.Equal((int)DbType.Int32, fsql.DbFirst.GetDbType(Column("INT", "INT")));
                Assert.Equal((int)DbType.DateTime, fsql.DbFirst.GetDbType(Column("DATETIME")));
            });
        }

        [Fact]
        public void ClickHouse_PascalCaseTypes_AreMappedUnderTurkishCulture()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.ClickHouse, "Host=127.0.0.1;Port=1;Database=test", typeof(FreeSql.ClickHouse.ClickHouseProvider<>))
                    .Build();

                Assert.Equal((int)DbType.Int32, fsql.DbFirst.GetDbType(Column("Int32")));
                Assert.Equal((int)DbType.Int64, fsql.DbFirst.GetDbType(Column("Nullable(Int64)")));
                Assert.Equal((int)DbType.UInt32, fsql.DbFirst.GetDbType(Column("UInt32")));
            });
        }

        [Fact]
        public void Oracle_UpperCaseTypes_AreMappedUnderTurkishCulture()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Oracle, "User Id=test;Password=test;Data Source=//127.0.0.1:1/test;Pooling=false", typeof(FreeSql.Oracle.OracleProvider<>))
                    .Build();

                Assert.Equal((int)OracleDbType.TimeStamp, fsql.DbFirst.GetDbType(Column("TIMESTAMP", "TIMESTAMP(6)")));
                Assert.Equal((int)OracleDbType.IntervalDS, fsql.DbFirst.GetDbType(Column("INTERVAL DAY TO SECOND", "INTERVAL DAY(2) TO SECOND(6)")));
            });
        }
    }
}
