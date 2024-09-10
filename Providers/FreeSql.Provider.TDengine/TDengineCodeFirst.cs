using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FreeSql.TDengine
{
    internal class TDengineCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public TDengineCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm,
            commonUtils, commonExpression)
        {
        }

        static Dictionary<string, CsToDb<DbType>> _dicCsToDb = new Dictionary<string, CsToDb<DbType>>()
        {
            { typeof(bool).FullName, CsToDb.New(DbType.Boolean, "BOOL", "BOOL", null, false, null) },
            { typeof(bool?).FullName, CsToDb.New(DbType.Boolean, "BOOL", "BOOL", null, true, null) },

            { typeof(DateTime).FullName, CsToDb.New(DbType.DateTime, "TIMESTAMP", "TIMESTAMP", null, false, null) },
            { typeof(DateTime?).FullName, CsToDb.New(DbType.DateTime, "TIMESTAMP", "TIMESTAMP", null, true, null) },


            { typeof(TimeSpan).FullName, CsToDb.New(DbType.DateTime, "TIMESTAMP", "TIMESTAMP", null, false, null) },
            { typeof(TimeSpan?).FullName, CsToDb.New(DbType.DateTime, "TIMESTAMP", "TIMESTAMP", null, true, null) },

            { typeof(short).FullName, CsToDb.New(DbType.Int16, "SMALLINT", "SMALLINT", null, false, 0) },
            { typeof(short?).FullName, CsToDb.New(DbType.Int16, "SMALLINT", "SMALLINT", null, true, null) },

            { typeof(int).FullName, CsToDb.New(DbType.Int32, "INT", "INT", null, false, 0) },
            { typeof(int?).FullName, CsToDb.New(DbType.Int32, "INT", "INT", null, true, null) },

            { typeof(sbyte).FullName, CsToDb.New(DbType.SByte, "TINYINT", "TINYINT", null, false, 0) },
            { typeof(sbyte?).FullName, CsToDb.New(DbType.SByte, "TINYINT", "TINYINT", null, true, null) },

            { typeof(long).FullName, CsToDb.New(DbType.Int64, "BIGINT", "BIGINT", null, false, 0) },
            { typeof(long?).FullName, CsToDb.New(DbType.Int64, "BIGINT", "BIGINT", null, true, null) },

            { typeof(byte).FullName, CsToDb.New(DbType.Byte, "TINYINT UNSIGNED", "TINYINT UNSIGNED", null, false, 0) },
            {
                typeof(byte?).FullName,
                CsToDb.New(DbType.Byte, "TINYINT UNSIGNED", "TINYINT UNSIGNED", null, true, null)
            },

            {
                typeof(ushort).FullName,
                CsToDb.New(DbType.UInt16, "SMALLINT UNSIGNED", "SMALLINT UNSIGNED", null, false, 0)
            },
            {
                typeof(ushort?).FullName,
                CsToDb.New(DbType.UInt16, "SMALLINT UNSIGNED", "SMALLINT UNSIGNED", null, true, null)
            },

            { typeof(uint).FullName, CsToDb.New(DbType.UInt32, "INT UNSIGNED", "INT UNSIGNED", null, false, 0) },
            { typeof(uint?).FullName, CsToDb.New(DbType.UInt32, "INT UNSIGNED", "INT UNSIGNED", null, true, null) },

            { typeof(ulong).FullName, CsToDb.New(DbType.UInt64, "BIGINT UNSIGNED", "BIGINT UNSIGNED", null, false, 0) },
            {
                typeof(ulong?).FullName,
                CsToDb.New(DbType.UInt64, "BIGINT UNSIGNED", "BIGINT UNSIGNED", null, true, null)
            },

            { typeof(float).FullName, CsToDb.New(DbType.Single, "FLOAT", "FLOAT", null, false, 0) },
            { typeof(float?).FullName, CsToDb.New(DbType.Single, "FLOAT", "FLOAT", null, true, null) },

            { typeof(double).FullName, CsToDb.New(DbType.Double, "DOUBLE", "DOUBLE", null, false, 0) },
            { typeof(double?).FullName, CsToDb.New(DbType.Double, "DOUBLE", "DOUBLE", null, true, null) },

            { typeof(string).FullName, CsToDb.New(DbType.String, "NCHAR", "NCHAR", null, false, 0) },
        };

        public override DbInfoResult GetDbInfo(Type type)
        {
            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new DbInfoResult((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable, trydc.defaultValue);
            if (type.IsArray) return null;
            return null;
        }

        protected override string GetComparisonDDLStatements(params TypeSchemaAndName[] objects)
        {
            throw new NotImplementedException();
        }
    }
}