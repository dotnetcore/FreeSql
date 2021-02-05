using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Custom
{

    class CustomCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public override bool IsAutoSyncStructure { get => false; set => base.IsAutoSyncStructure = false; }
        public override bool IsNoneCommandParameter { get => true; set => base.IsNoneCommandParameter = true; }
        public CustomCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression)
        {
            _utils = _commonUtils as CustomUtils;
        }

        CustomUtils _utils;
        object _dicCsToDbLock = new object();
        Dictionary<string, CsToDb<DbType>> _dicCsToDb;

        public override DbInfoResult GetDbInfo(Type type)
        {
            if (_dicCsToDb == null)
            {
                lock (_dicCsToDbLock)
                {
                    if (_dicCsToDb == null)
                    {
                        var reg = new Regex(@"\([^\)]+\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        Func<string, string> deleteBrackets = str => reg.Replace(str, "");

                        _dicCsToDb = new Dictionary<string, CsToDb<DbType>>() {
                            { typeof(bool).FullName, CsToDb.New(DbType.Boolean, _utils.Adapter.MappingDbTypeBit,$"{_utils.Adapter.MappingDbTypeBit} NOT NULL", null, false, false) },{ typeof(bool?).FullName, CsToDb.New(DbType.Boolean, _utils.Adapter.MappingDbTypeBit,_utils.Adapter.MappingDbTypeBit, null, true, null) },

                            { typeof(sbyte).FullName, CsToDb.New(DbType.SByte, _utils.Adapter.MappingDbTypeSmallInt, $"{_utils.Adapter.MappingDbTypeSmallInt} NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName, CsToDb.New(DbType.SByte, _utils.Adapter.MappingDbTypeSmallInt, _utils.Adapter.MappingDbTypeSmallInt, false, true, null) },
                            { typeof(short).FullName, CsToDb.New(DbType.Int16, _utils.Adapter.MappingDbTypeSmallInt,$"{_utils.Adapter.MappingDbTypeSmallInt} NOT NULL", false, false, 0) },{ typeof(short?).FullName, CsToDb.New(DbType.Int16, _utils.Adapter.MappingDbTypeSmallInt, _utils.Adapter.MappingDbTypeSmallInt, false, true, null) },
                            { typeof(int).FullName, CsToDb.New(DbType.Int32, _utils.Adapter.MappingDbTypeInt, $"{_utils.Adapter.MappingDbTypeInt} NOT NULL", false, false, 0) },{ typeof(int?).FullName, CsToDb.New(DbType.Int32, _utils.Adapter.MappingDbTypeInt, _utils.Adapter.MappingDbTypeInt, false, true, null) },
                            { typeof(long).FullName, CsToDb.New(DbType.Int64, _utils.Adapter.MappingDbTypeBigInt,$"{_utils.Adapter.MappingDbTypeBigInt} NOT NULL", false, false, 0) },{ typeof(long?).FullName, CsToDb.New(DbType.Int64, _utils.Adapter.MappingDbTypeBigInt,_utils.Adapter.MappingDbTypeBigInt, false, true, null) },

                            { typeof(byte).FullName, CsToDb.New(DbType.Byte, _utils.Adapter.MappingDbTypeTinyInt,$"{_utils.Adapter.MappingDbTypeTinyInt} NOT NULL", true, false, 0) },{ typeof(byte?).FullName, CsToDb.New(DbType.Byte, _utils.Adapter.MappingDbTypeTinyInt,_utils.Adapter.MappingDbTypeTinyInt, true, true, null) },
                            { typeof(ushort).FullName, CsToDb.New(DbType.UInt16, _utils.Adapter.MappingDbTypeInt,$"{_utils.Adapter.MappingDbTypeInt} NOT NULL", true, false, 0) },{ typeof(ushort?).FullName, CsToDb.New(DbType.UInt16, _utils.Adapter.MappingDbTypeInt, _utils.Adapter.MappingDbTypeInt, true, true, null) },
                            { typeof(uint).FullName, CsToDb.New(DbType.UInt32, _utils.Adapter.MappingDbTypeBigInt, $"{_utils.Adapter.MappingDbTypeBigInt} NOT NULL", true, false, 0) },{ typeof(uint?).FullName, CsToDb.New(DbType.UInt32, _utils.Adapter.MappingDbTypeBigInt, _utils.Adapter.MappingDbTypeBigInt, true, true, null) },
                            { typeof(ulong).FullName, CsToDb.New(DbType.UInt64, _utils.Adapter.MappingDbTypeDecimal, $"{_utils.Adapter.MappingDbTypeDecimal}(20,0) NOT NULL", true, false, 0) },{ typeof(ulong?).FullName, CsToDb.New(DbType.UInt64, _utils.Adapter.MappingDbTypeDecimal, $"{_utils.Adapter.MappingDbTypeDecimal}(20,0)", true, true, null) },

                            { typeof(double).FullName, CsToDb.New(DbType.Double, _utils.Adapter.MappingDbTypeDouble, $"{_utils.Adapter.MappingDbTypeDouble} NOT NULL", false, false, 0) },{ typeof(double?).FullName, CsToDb.New(DbType.Double, _utils.Adapter.MappingDbTypeDouble, _utils.Adapter.MappingDbTypeDouble, false, true, null) },
                            { typeof(float).FullName, CsToDb.New(DbType.Single, _utils.Adapter.MappingDbTypeReal,$"{_utils.Adapter.MappingDbTypeReal} NOT NULL", false, false, 0) },{ typeof(float?).FullName, CsToDb.New(DbType.Single, _utils.Adapter.MappingDbTypeReal,_utils.Adapter.MappingDbTypeReal, false, true, null) },
                            { typeof(decimal).FullName, CsToDb.New(DbType.Decimal, _utils.Adapter.MappingDbTypeDecimal, $"{_utils.Adapter.MappingDbTypeDecimal}(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName, CsToDb.New(DbType.Decimal, _utils.Adapter.MappingDbTypeDecimal, $"{_utils.Adapter.MappingDbTypeDecimal}(10,2)", false, true, null) },

                            { typeof(DateTime).FullName, CsToDb.New(DbType.DateTime, _utils.Adapter.MappingDbTypeDateTime, $"{_utils.Adapter.MappingDbTypeDateTime} NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName, CsToDb.New(DbType.DateTime, _utils.Adapter.MappingDbTypeDateTime, _utils.Adapter.MappingDbTypeDateTime, false, true, null) },

                            { typeof(byte[]).FullName, CsToDb.New(DbType.Binary, _utils.Adapter.MappingDbTypeVarBinary, $"{_utils.Adapter.MappingDbTypeVarBinary}(255)", false, null, new byte[0]) },
                            { typeof(string).FullName, CsToDb.New(DbType.String, _utils.Adapter.MappingDbTypeVarChar, $"{_utils.Adapter.MappingDbTypeVarChar}(255)", false, null, "") },
                            { typeof(char).FullName, CsToDb.New(DbType.AnsiStringFixedLength, _utils.Adapter.MappingDbTypeChar, $"{_utils.Adapter.MappingDbTypeChar}(1)", false, null, '\0') },

                            { typeof(Guid).FullName, CsToDb.New(DbType.Guid, deleteBrackets(_utils.Adapter.MappingDbTypeUniqueIdentifier), $"{_utils.Adapter.MappingDbTypeUniqueIdentifier} NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName, CsToDb.New(DbType.Guid, deleteBrackets(_utils.Adapter.MappingDbTypeUniqueIdentifier), _utils.Adapter.MappingDbTypeUniqueIdentifier, false, true, null) },
                        };
                    }
                }
            }

            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new DbInfoResult((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable, trydc.defaultValue);
            if (type.IsArray) return null;
            var enumType = type.IsEnum ? type : null;
            if (enumType == null && type.IsNullableType())
            {
                var genericTypes = type.GetGenericArguments();
                if (genericTypes.Length == 1 && genericTypes.First().IsEnum) enumType = genericTypes.First();
            }
            if (enumType != null)
            {
                var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
                    CsToDb.New(DbType.Int64, _utils.Adapter.MappingDbTypeBigInt, $"{_utils.Adapter.MappingDbTypeBigInt}{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue()) :
                    CsToDb.New(DbType.Int32, _utils.Adapter.MappingDbTypeInt, $"{_utils.Adapter.MappingDbTypeInt}{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue());
                if (_dicCsToDb.ContainsKey(type.FullName) == false)
                {
                    lock (_dicCsToDbLock)
                    {
                        if (_dicCsToDb.ContainsKey(type.FullName) == false)
                            _dicCsToDb.Add(type.FullName, newItem);
                    }
                }
                return new DbInfoResult((int)newItem.type, newItem.dbtype, newItem.dbtypeFull, newItem.isnullable, newItem.defaultValue);
            }
            return null;
        }

        protected override string GetComparisonDDLStatements(params TypeAndName[] objects) => throw new NotImplementedException("FreeSql.Provider.Custom 未实现该功能");
    }
}