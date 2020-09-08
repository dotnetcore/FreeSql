using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Odbc.Default
{

    class OdbcCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public override bool IsAutoSyncStructure { get => false; set => base.IsAutoSyncStructure = false; }
        public override bool IsNoneCommandParameter { get => true; set => base.IsNoneCommandParameter = true; }
        public OdbcCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression)
        {
            _utils = _commonUtils as OdbcUtils;
        }

        OdbcUtils _utils;
        object _dicCsToDbLock = new object();
        Dictionary<string, CsToDb<OdbcType>> _dicCsToDb;

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

                        _dicCsToDb = new Dictionary<string, CsToDb<OdbcType>>() {
                            { typeof(bool).FullName, CsToDb.New(OdbcType.Bit, _utils.Adapter.MappingOdbcTypeBit,$"{_utils.Adapter.MappingOdbcTypeBit} NOT NULL", null, false, false) },{ typeof(bool?).FullName, CsToDb.New(OdbcType.Bit, _utils.Adapter.MappingOdbcTypeBit,_utils.Adapter.MappingOdbcTypeBit, null, true, null) },

                            { typeof(sbyte).FullName, CsToDb.New(OdbcType.SmallInt, _utils.Adapter.MappingOdbcTypeSmallInt, $"{_utils.Adapter.MappingOdbcTypeSmallInt} NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName, CsToDb.New(OdbcType.SmallInt, _utils.Adapter.MappingOdbcTypeSmallInt, _utils.Adapter.MappingOdbcTypeSmallInt, false, true, null) },
                            { typeof(short).FullName, CsToDb.New(OdbcType.SmallInt, _utils.Adapter.MappingOdbcTypeSmallInt,$"{_utils.Adapter.MappingOdbcTypeSmallInt} NOT NULL", false, false, 0) },{ typeof(short?).FullName, CsToDb.New(OdbcType.SmallInt, _utils.Adapter.MappingOdbcTypeSmallInt, _utils.Adapter.MappingOdbcTypeSmallInt, false, true, null) },
                            { typeof(int).FullName, CsToDb.New(OdbcType.Int, _utils.Adapter.MappingOdbcTypeInt, $"{_utils.Adapter.MappingOdbcTypeInt} NOT NULL", false, false, 0) },{ typeof(int?).FullName, CsToDb.New(OdbcType.Int, _utils.Adapter.MappingOdbcTypeInt, _utils.Adapter.MappingOdbcTypeInt, false, true, null) },
                            { typeof(long).FullName, CsToDb.New(OdbcType.BigInt, _utils.Adapter.MappingOdbcTypeBigInt,$"{_utils.Adapter.MappingOdbcTypeBigInt} NOT NULL", false, false, 0) },{ typeof(long?).FullName, CsToDb.New(OdbcType.BigInt, _utils.Adapter.MappingOdbcTypeBigInt,_utils.Adapter.MappingOdbcTypeBigInt, false, true, null) },

                            { typeof(byte).FullName, CsToDb.New(OdbcType.TinyInt, _utils.Adapter.MappingOdbcTypeTinyInt,$"{_utils.Adapter.MappingOdbcTypeTinyInt} NOT NULL", true, false, 0) },{ typeof(byte?).FullName, CsToDb.New(OdbcType.TinyInt, _utils.Adapter.MappingOdbcTypeTinyInt,_utils.Adapter.MappingOdbcTypeTinyInt, true, true, null) },
                            { typeof(ushort).FullName, CsToDb.New(OdbcType.Int, _utils.Adapter.MappingOdbcTypeInt,$"{_utils.Adapter.MappingOdbcTypeInt} NOT NULL", true, false, 0) },{ typeof(ushort?).FullName, CsToDb.New(OdbcType.Int, _utils.Adapter.MappingOdbcTypeInt, _utils.Adapter.MappingOdbcTypeInt, true, true, null) },
                            { typeof(uint).FullName, CsToDb.New(OdbcType.BigInt, _utils.Adapter.MappingOdbcTypeBigInt, $"{_utils.Adapter.MappingOdbcTypeBigInt} NOT NULL", true, false, 0) },{ typeof(uint?).FullName, CsToDb.New(OdbcType.BigInt, _utils.Adapter.MappingOdbcTypeBigInt, _utils.Adapter.MappingOdbcTypeBigInt, true, true, null) },
                            { typeof(ulong).FullName, CsToDb.New(OdbcType.Decimal, _utils.Adapter.MappingOdbcTypeDecimal, $"{_utils.Adapter.MappingOdbcTypeDecimal}(20,0) NOT NULL", true, false, 0) },{ typeof(ulong?).FullName, CsToDb.New(OdbcType.Decimal, _utils.Adapter.MappingOdbcTypeDecimal, $"{_utils.Adapter.MappingOdbcTypeDecimal}(20,0)", true, true, null) },

                            { typeof(double).FullName, CsToDb.New(OdbcType.Double, _utils.Adapter.MappingOdbcTypeDouble, $"{_utils.Adapter.MappingOdbcTypeDouble} NOT NULL", false, false, 0) },{ typeof(double?).FullName, CsToDb.New(OdbcType.Double, _utils.Adapter.MappingOdbcTypeDouble, _utils.Adapter.MappingOdbcTypeDouble, false, true, null) },
                            { typeof(float).FullName, CsToDb.New(OdbcType.Real, _utils.Adapter.MappingOdbcTypeReal,$"{_utils.Adapter.MappingOdbcTypeReal} NOT NULL", false, false, 0) },{ typeof(float?).FullName, CsToDb.New(OdbcType.Real, _utils.Adapter.MappingOdbcTypeReal,_utils.Adapter.MappingOdbcTypeReal, false, true, null) },
                            { typeof(decimal).FullName, CsToDb.New(OdbcType.Decimal, _utils.Adapter.MappingOdbcTypeDecimal, $"{_utils.Adapter.MappingOdbcTypeDecimal}(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName, CsToDb.New(OdbcType.Decimal, _utils.Adapter.MappingOdbcTypeDecimal, $"{_utils.Adapter.MappingOdbcTypeDecimal}(10,2)", false, true, null) },

                            { typeof(DateTime).FullName, CsToDb.New(OdbcType.DateTime, _utils.Adapter.MappingOdbcTypeDateTime, $"{_utils.Adapter.MappingOdbcTypeDateTime} NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName, CsToDb.New(OdbcType.DateTime, _utils.Adapter.MappingOdbcTypeDateTime, _utils.Adapter.MappingOdbcTypeDateTime, false, true, null) },

                            { typeof(byte[]).FullName, CsToDb.New(OdbcType.VarBinary, _utils.Adapter.MappingOdbcTypeVarBinary, $"{_utils.Adapter.MappingOdbcTypeVarBinary}(255)", false, null, new byte[0]) },
                            { typeof(string).FullName, CsToDb.New(OdbcType.VarChar, _utils.Adapter.MappingOdbcTypeVarChar, $"{_utils.Adapter.MappingOdbcTypeVarChar}(255)", false, null, "") },
                            { typeof(char).FullName, CsToDb.New(OdbcType.Char, _utils.Adapter.MappingOdbcTypeChar, $"{_utils.Adapter.MappingOdbcTypeChar}(1)", false, null, '\0') },

                            { typeof(Guid).FullName, CsToDb.New(OdbcType.UniqueIdentifier, deleteBrackets(_utils.Adapter.MappingOdbcTypeUniqueIdentifier), $"{_utils.Adapter.MappingOdbcTypeUniqueIdentifier} NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName, CsToDb.New(OdbcType.UniqueIdentifier, deleteBrackets(_utils.Adapter.MappingOdbcTypeUniqueIdentifier), _utils.Adapter.MappingOdbcTypeUniqueIdentifier, false, true, null) },
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
                    CsToDb.New(OdbcType.BigInt, _utils.Adapter.MappingOdbcTypeBigInt, $"{_utils.Adapter.MappingOdbcTypeBigInt}{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue()) :
                    CsToDb.New(OdbcType.Int, _utils.Adapter.MappingOdbcTypeInt, $"{_utils.Adapter.MappingOdbcTypeInt}{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue());
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

        protected override string GetComparisonDDLStatements(params TypeAndName[] objects) => throw new NotImplementedException("FreeSql.Odbc.Default 未实现该功能");
    }
}