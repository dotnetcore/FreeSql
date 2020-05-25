using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Internal.Model
{
    public class DbToCs
    {
        public string csConvert { get; }
        public string csParse { get; }
        public string csStringify { get; }
        public string csType { get; }
        public Type csTypeInfo { get; }
        public Type csNullableTypeInfo { get; }
        public string csTypeValue { get; }
        public string dataReaderMethod { get; }
        public DbToCs(string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)
        {
            this.csConvert = csConvert;
            this.csParse = csParse;
            this.csStringify = csStringify;
            this.csType = csType;
            this.csTypeInfo = csTypeInfo;
            this.csNullableTypeInfo = csNullableTypeInfo;
            this.csTypeValue = csTypeValue;
            this.dataReaderMethod = dataReaderMethod;
        }
    }

    public static class CsToDb
    {
        public static CsToDb<T> New<T>(T type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue) =>
            new CsToDb<T>(type, dbtype, dbtypeFull, isUnsigned, isnullable, defaultValue);
    }
    public class CsToDb<T>
    {
        public T type { get; }
        public string dbtype { get; }
        public string dbtypeFull { get; }
        public bool? isUnsigned { get; }
        public bool? isnullable { get; }
        public object defaultValue { get; }
        public CsToDb(T type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)
        {
            this.type = type;
            this.dbtype = dbtype;
            this.dbtypeFull = dbtypeFull;
            this.isUnsigned = isUnsigned;
            this.isnullable = isnullable;
            this.defaultValue = defaultValue;
        }
    }


    public class DbInfoResult
    {
        public int type { get; }
        public string dbtype { get; }
        public string dbtypeFull { get; }
        public bool? isnullable { get; }
        public object defaultValue { get; }
        public DbInfoResult(int type, string dbtype, string dbtypeFull, bool? isnullable, object defaultValue)
        {
            this.type = type;
            this.dbtype = dbtype;
            this.dbtypeFull = dbtypeFull;
            this.isnullable = isnullable;
            this.defaultValue = defaultValue;
        }
    }
}
