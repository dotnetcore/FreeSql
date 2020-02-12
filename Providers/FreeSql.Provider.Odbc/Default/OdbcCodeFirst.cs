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
        Dictionary<string, (OdbcType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)> _dicCsToDb;

        public override (int type, string dbtype, string dbtypeFull, bool? isnullable, object defaultValue)? GetDbInfo(Type type)
        {
            if (_dicCsToDb == null)
            {
                lock (_dicCsToDbLock)
                {
                    if (_dicCsToDb == null)
                    {
                        var reg = new Regex(@"\([^\)]+\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        Func<string, string> deleteBrackets = str => reg.Replace(str, "");

                        _dicCsToDb = new Dictionary<string, (OdbcType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)>() {
                            { typeof(bool).FullName,  (OdbcType.Bit, _utils.Adapter.MappingOdbcTypeBit,$"{_utils.Adapter.MappingOdbcTypeBit} NOT NULL", null, false, false) },{ typeof(bool?).FullName,  (OdbcType.Bit, _utils.Adapter.MappingOdbcTypeBit,_utils.Adapter.MappingOdbcTypeBit, null, true, null) },

                            { typeof(sbyte).FullName,  (OdbcType.SmallInt, _utils.Adapter.MappingOdbcTypeSmallInt, $"{_utils.Adapter.MappingOdbcTypeSmallInt} NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName,  (OdbcType.SmallInt, _utils.Adapter.MappingOdbcTypeSmallInt, _utils.Adapter.MappingOdbcTypeSmallInt, false, true, null) },
                            { typeof(short).FullName,  (OdbcType.SmallInt, _utils.Adapter.MappingOdbcTypeSmallInt,$"{_utils.Adapter.MappingOdbcTypeSmallInt} NOT NULL", false, false, 0) },{ typeof(short?).FullName,  (OdbcType.SmallInt, _utils.Adapter.MappingOdbcTypeSmallInt, _utils.Adapter.MappingOdbcTypeSmallInt, false, true, null) },
                            { typeof(int).FullName,  (OdbcType.Int, _utils.Adapter.MappingOdbcTypeInt, $"{_utils.Adapter.MappingOdbcTypeInt} NOT NULL", false, false, 0) },{ typeof(int?).FullName,  (OdbcType.Int, _utils.Adapter.MappingOdbcTypeInt, _utils.Adapter.MappingOdbcTypeInt, false, true, null) },
                            { typeof(long).FullName,  (OdbcType.BigInt, _utils.Adapter.MappingOdbcTypeBigInt,$"{_utils.Adapter.MappingOdbcTypeBigInt} NOT NULL", false, false, 0) },{ typeof(long?).FullName,  (OdbcType.BigInt, _utils.Adapter.MappingOdbcTypeBigInt,_utils.Adapter.MappingOdbcTypeBigInt, false, true, null) },

                            { typeof(byte).FullName,  (OdbcType.TinyInt, _utils.Adapter.MappingOdbcTypeTinyInt,$"{_utils.Adapter.MappingOdbcTypeTinyInt} NOT NULL", true, false, 0) },{ typeof(byte?).FullName,  (OdbcType.TinyInt, _utils.Adapter.MappingOdbcTypeTinyInt,_utils.Adapter.MappingOdbcTypeTinyInt, true, true, null) },
                            { typeof(ushort).FullName,  (OdbcType.Int, _utils.Adapter.MappingOdbcTypeInt,$"{_utils.Adapter.MappingOdbcTypeInt} NOT NULL", true, false, 0) },{ typeof(ushort?).FullName,  (OdbcType.Int, _utils.Adapter.MappingOdbcTypeInt, _utils.Adapter.MappingOdbcTypeInt, true, true, null) },
                            { typeof(uint).FullName,  (OdbcType.BigInt, _utils.Adapter.MappingOdbcTypeBigInt, $"{_utils.Adapter.MappingOdbcTypeBigInt} NOT NULL", true, false, 0) },{ typeof(uint?).FullName,  (OdbcType.BigInt, _utils.Adapter.MappingOdbcTypeBigInt, _utils.Adapter.MappingOdbcTypeBigInt, true, true, null) },
                            { typeof(ulong).FullName,  (OdbcType.Decimal, _utils.Adapter.MappingOdbcTypeDecimal, $"{_utils.Adapter.MappingOdbcTypeDecimal}(20,0) NOT NULL", true, false, 0) },{ typeof(ulong?).FullName,  (OdbcType.Decimal, _utils.Adapter.MappingOdbcTypeDecimal, $"{_utils.Adapter.MappingOdbcTypeDecimal}(20,0)", true, true, null) },

                            { typeof(double).FullName,  (OdbcType.Double, _utils.Adapter.MappingOdbcTypeDouble, $"{_utils.Adapter.MappingOdbcTypeDouble} NOT NULL", false, false, 0) },{ typeof(double?).FullName,  (OdbcType.Double, _utils.Adapter.MappingOdbcTypeDouble, _utils.Adapter.MappingOdbcTypeDouble, false, true, null) },
                            { typeof(float).FullName,  (OdbcType.Real, _utils.Adapter.MappingOdbcTypeReal,$"{_utils.Adapter.MappingOdbcTypeReal} NOT NULL", false, false, 0) },{ typeof(float?).FullName,  (OdbcType.Real, _utils.Adapter.MappingOdbcTypeReal,_utils.Adapter.MappingOdbcTypeReal, false, true, null) },
                            { typeof(decimal).FullName,  (OdbcType.Decimal, _utils.Adapter.MappingOdbcTypeDecimal, $"{_utils.Adapter.MappingOdbcTypeDecimal}(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName,  (OdbcType.Decimal, _utils.Adapter.MappingOdbcTypeDecimal, $"{_utils.Adapter.MappingOdbcTypeDecimal}(10,2)", false, true, null) },

                            { typeof(DateTime).FullName,  (OdbcType.DateTime, _utils.Adapter.MappingOdbcTypeDateTime, $"{_utils.Adapter.MappingOdbcTypeDateTime} NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName,  (OdbcType.DateTime, _utils.Adapter.MappingOdbcTypeDateTime, _utils.Adapter.MappingOdbcTypeDateTime, false, true, null) },

                            { typeof(byte[]).FullName,  (OdbcType.VarBinary, _utils.Adapter.MappingOdbcTypeVarBinary, $"{_utils.Adapter.MappingOdbcTypeVarBinary}(255)", false, null, new byte[0]) },
                            { typeof(string).FullName,  (OdbcType.VarChar, _utils.Adapter.MappingOdbcTypeVarChar, $"{_utils.Adapter.MappingOdbcTypeVarChar}(255)", false, null, "") },

                            { typeof(Guid).FullName,  (OdbcType.UniqueIdentifier, deleteBrackets(_utils.Adapter.MappingOdbcTypeUniqueIdentifier), $"{_utils.Adapter.MappingOdbcTypeUniqueIdentifier} NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName,  (OdbcType.UniqueIdentifier, deleteBrackets(_utils.Adapter.MappingOdbcTypeUniqueIdentifier), _utils.Adapter.MappingOdbcTypeUniqueIdentifier, false, true, null) },
                        };
                    }
                }
            }

            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new (int, string, string, bool?, object)?(((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable, trydc.defaultValue));
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
                    (OdbcType.BigInt, _utils.Adapter.MappingOdbcTypeBigInt, $"{_utils.Adapter.MappingOdbcTypeBigInt}{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0)) :
                    (OdbcType.Int, _utils.Adapter.MappingOdbcTypeInt, $"{_utils.Adapter.MappingOdbcTypeInt}{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0));
                if (_dicCsToDb.ContainsKey(type.FullName) == false)
                {
                    lock (_dicCsToDbLock)
                    {
                        if (_dicCsToDb.ContainsKey(type.FullName) == false)
                            _dicCsToDb.Add(type.FullName, newItem);
                    }
                }
                return ((int)newItem.Item1, newItem.Item2, newItem.Item3, newItem.Item5, newItem.Item6);
            }
            return null;
        }

        protected override string GetComparisonDDLStatements(params (Type entityType, string tableName)[] objects) => throw new NotImplementedException("FreeSql.Odbc.Default 未实现该功能");
    }
}