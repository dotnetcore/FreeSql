using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Custom
{
    public class CustomAdapter
    {
        /// <summary>
        /// Select TOP 1，或 Limit 1 风格
        /// </summary>
        public virtual SelecTopStyle SelectTopStyle => SelecTopStyle.Top;
        public enum SelecTopStyle { Top, Limit }
        
        /// <summary>
        /// 插入成功后，获取自增值
        /// </summary>
        public virtual string InsertAfterGetIdentitySql => "SELECT SCOPE_IDENTITY()";
        /// <summary>
        /// 批量插入时，自动拆分的每次执行数量
        /// </summary>
        public virtual int InsertBatchSplitLimit => 255;
        /// <summary>
        /// 批量更新时，自动拆分的每次执行数量
        /// </summary>
        public virtual int UpdateBatchSplitLimit => 255;

        public virtual string MappingDbTypeBit => "int";
        public virtual string MappingDbTypeSmallInt => "smallint";
        public virtual string MappingDbTypeInt => "int";
        public virtual string MappingDbTypeBigInt => "bigint";
        public virtual string MappingDbTypeTinyInt => "tinyint";
        public virtual string MappingDbTypeDecimal => "decimal";
        public virtual string MappingDbTypeDouble => "float";
        public virtual string MappingDbTypeReal => "real";
        public virtual string MappingDbTypeDateTime => "datetime";
        public virtual string MappingDbTypeVarBinary => "varbinary";
        public virtual string MappingDbTypeVarChar => "nvarchar";
        public virtual string MappingDbTypeChar => "char";
        public virtual string MappingDbTypeText => "nvarchar(max)";
        public virtual string MappingDbTypeUniqueIdentifier => "uniqueidentifier";

        public virtual char QuoteSqlNameLeft => '[';
        public virtual char QuoteSqlNameRight => ']';

        public virtual string FieldSql(Type type, string columnName) => columnName;
        public virtual string UnicodeStringRawSql(object value, ColumnInfo mapColumn) => value == null ? "NULL" : string.Concat("N'", value.ToString().Replace("'", "''"), "'");
        public virtual string DateTimeRawSql(object value)
        {
            if (value == null) return "NULL";
            if (value.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
            return string.Concat("'", ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"), "'");
        }
        public virtual string TimeSpanRawSql(object value) => value == null ? "NULL" : ((TimeSpan)value).TotalSeconds.ToString();
        public virtual string ByteRawSql(object value)
        {
            if (value == null) return "NULL";
            return $"0x{CommonUtils.BytesSqlRaw(value as byte[])}";
        }

        public virtual string CastSql(string sql, string to) => $"cast({sql} as {to})";
        public virtual string IsNullSql(string sql, object value) => $"isnull({sql}, {value})";
        public virtual string ConcatSql(string[] objs, Type[] types)
        {
            var sb = new StringBuilder();
            var news = new string[objs.Length];
            for (var a = 0; a < objs.Length; a++)
            {
                if (types[a] == typeof(string)) news[a] = objs[a];
                else if (types[a].NullableTypeOrThis() == typeof(Guid)) news[a] = $"cast({objs[a]} as char(36))";
                else news[a] = $"cast({objs[a]} as nvarchar)";
            }
            return string.Join(" + ", news);
        }

        public virtual string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";
        public virtual string Div(string left, string right, Type leftType, Type rightType) => $"{left} / {right}";

        public virtual string LambdaConvert_ToBoolean(Type type, string operand) => $"(cast({operand} as varchar) not in ('0','false'))";
        public virtual string LambdaConvert_ToByte(Type type, string operand) => $"cast({operand} as tinyint)";
        public virtual string LambdaConvert_ToChar(Type type, string operand) => $"substring(cast({operand} as varchar),1,1)";
        public virtual string LambdaConvert_ToDateTime(Type type, string operand) => $"cast({operand} as datetime)";
        public virtual string LambdaConvert_ToDecimal(Type type, string operand) => $"cast({operand} as decimal(36,18))";
        public virtual string LambdaConvert_ToDouble(Type type, string operand) => $"cast({operand} as decimal(32,16))";
        public virtual string LambdaConvert_ToInt16(Type type, string operand) => $"cast({operand} as smallint)";
        public virtual string LambdaConvert_ToInt32(Type type, string operand) => $"cast({operand} as int)";
        public virtual string LambdaConvert_ToInt64(Type type, string operand) => $"cast({operand} as bigint)";
        public virtual string LambdaConvert_ToSByte(Type type, string operand) => $"cast({operand} as tinyint)";
        public virtual string LambdaConvert_ToSingle(Type type, string operand) => $"cast({operand} as decimal(14,7))";
        public virtual string LambdaConvert_ToString(Type type, string operand) => type.NullableTypeOrThis() == typeof(Guid) ? $"cast({operand} as varchar(36))" : $"cast({operand} as nvarchar)";
        public virtual string LambdaConvert_ToUInt16(Type type, string operand) => $"cast({operand} as smallint)";
        public virtual string LambdaConvert_ToUInt32(Type type, string operand) => $"cast({operand} as int)";
        public virtual string LambdaConvert_ToUInt64(Type type, string operand) => $"cast({operand} as bigint)";
        public virtual string LambdaConvert_ToGuid(Type type, string operand) => $"cast({operand} as uniqueidentifier)";

        public virtual string LambdaGuid_NewGuid => "newid()";
        public virtual string LambdaRandom_Next => "cast(rand()*1000000000 as int)";
        public virtual string LambdaRandom_NextDouble => "rand()";

        public virtual string LambdaString_IsNullOrEmpty(string operand) => $"({operand} is null or {operand} = '')";
        public virtual string LambdaString_IsNullOrWhiteSpace(string operand) => $"({operand} is null or {operand} = '' or ltrim({operand}) = '')";
        public virtual string LambdaString_Length(string operand) => $"len({operand})";

        public virtual string LambdaString_ToLower(string operand) => $"lower({operand})";
        public virtual string LambdaString_ToUpper(string operand) => $"upper({operand})";
        public virtual string LambdaString_Substring(string operand, string startIndex, string length) => string.IsNullOrEmpty(length) ? $"left({operand}, {startIndex})" : $"substring({operand}, {startIndex}, {length})";
        public virtual string LambdaString_IndexOf(string operand, string value, string startIndex) => string.IsNullOrEmpty(startIndex) ? $"(charindex({value}, {operand})-1)" : $"(charindex({value}, {operand}, {startIndex})-1)";
        public virtual string LambdaString_PadLeft(string operand, string length, string paddingChar) => string.IsNullOrEmpty(paddingChar) ? $"lpad({operand}, {length})" : $"lpad({operand}, {length}, {paddingChar})";
        public virtual string LambdaString_PadRight(string operand, string length, string paddingChar) => string.IsNullOrEmpty(paddingChar) ? $"rpad({operand}, {length})" : $"rpad({operand}, {length}, {paddingChar})";
        public virtual string LambdaString_Trim(string operand) => $"ltrim(rtrim({operand}))";
        public virtual string LambdaString_TrimStart(string operand) => $"ltrim({operand})";
        public virtual string LambdaString_TrimEnd(string operand) => $"rtrim({operand})";
        public virtual string LambdaString_Replace(string operand, string oldValue, string newValue) => $"replace({operand}, {oldValue}, {newValue})";
        public virtual string LambdaString_CompareTo(string operand, string value) => $"({operand} - {value})";
        public virtual string LambdaString_Equals(string operand, string value) => $"({operand} = {value})";

        public virtual string LambdaDateTime_Now => "getdate()";
        public virtual string LambdaDateTime_UtcNow => "getutcdate()";
        public virtual string LambdaDateTime_Today => "convert(char(10),getdate(),120)";
        public virtual string LambdaDateTime_MinValue => "'1753/1/1 0:00:00'";
        public virtual string LambdaDateTime_MaxValue => "'9999/12/31 23:59:59'";
        public virtual string LambdaDateTime_Date(string operand) => $"convert(char(10),{operand},120)";
        public virtual string LambdaDateTime_TimeOfDay(string operand) => $"datediff(second, convert(char(10),{operand},120), {operand})";
        public virtual string LambdaDateTime_DayOfWeek(string operand) => $"(datepart(weekday, {operand})-1)";
        public virtual string LambdaDateTime_Day(string operand) => $"datepart(day, {operand})";
        public virtual string LambdaDateTime_DayOfYear(string operand) => $"datepart(dayofyear, {operand})";
        public virtual string LambdaDateTime_Month(string operand) => $"datepart(month, {operand})";
        public virtual string LambdaDateTime_Year(string operand) => $"datepart(year, {operand})";
        public virtual string LambdaDateTime_Hour(string operand) => $"datepart(hour, {operand})";
        public virtual string LambdaDateTime_Minute(string operand) => $"datepart(minute, {operand})";
        public virtual string LambdaDateTime_Second(string operand) => $"datepart(second, {operand})";
        public virtual string LambdaDateTime_Millisecond(string operand) => $"(datepart(millisecond, {operand})/1000)";
        public virtual string LambdaDateTime_Ticks(string operand) => $"(cast(datediff(second, '1970-1-1', {operand}) as bigint)*10000000+621355968000000000)";

        public virtual string LambdaDateTime_DaysInMonth(string year, string month) => $"datepart(day, dateadd(day, -1, dateadd(month, 1, cast({year} as varchar) + '-' + cast({month} as varchar) + '-1')))";
        public virtual string LambdaDateTime_IsLeapYear(string year) => $"(({year})%4=0 AND ({year})%100<>0 OR ({year})%400=0)";
        public virtual string LambdaDateTime_Add(string operand, string value) => $"dateadd(second, {value}, {operand})";
        public virtual string LambdaDateTime_AddDays(string operand, string value) => $"dateadd(day, {value}, {operand})";
        public virtual string LambdaDateTime_AddHours(string operand, string value) => $"dateadd(hour, {value}, {operand})";
        public virtual string LambdaDateTime_AddMilliseconds(string operand, string value) => $"dateadd(second, ({value})/1000, {operand})";
        public virtual string LambdaDateTime_AddMinutes(string operand, string value) => $"dateadd(minute, {value}, {operand})";
        public virtual string LambdaDateTime_AddMonths(string operand, string value) => $"dateadd(month, {value}, {operand})";
        public virtual string LambdaDateTime_AddSeconds(string operand, string value) => $"dateadd(second, {value}, {operand})";
        public virtual string LambdaDateTime_AddTicks(string operand, string value) => $"dateadd(second, ({value})/10000000, {operand})";
        public virtual string LambdaDateTime_AddYears(string operand, string value) => $"dateadd(year, {value}, {operand})";
        public virtual string LambdaDateTime_Subtract(string operand, string value) => $"datediff(second, {value}, {operand})";
        public virtual string LambdaDateTime_SubtractTimeSpan(string operand, string value) => $"dateadd(second, ({value})*-1, {operand})";
        public virtual string LambdaDateTime_Equals(string operand, string value) => $"({operand} = {value})";
        public virtual string LambdaDateTime_CompareTo(string operand, string value) => $"datediff(second,{value},{operand})";
        public virtual string LambdaDateTime_ToString(string operand) => $"convert(varchar, {operand}, 121)";

        public virtual string LambdaMath_Abs(string operand) => $"abs({operand})";
        public virtual string LambdaMath_Sign(string operand) => $"sign({operand})";
        public virtual string LambdaMath_Floor(string operand) => $"floor({operand})";
        public virtual string LambdaMath_Ceiling(string operand) => $"ceiling({ operand})";
        public virtual string LambdaMath_Round(string operand, string decimals) => $"round({operand}, {decimals})";
        public virtual string LambdaMath_Exp(string operand) => $"exp({operand})";
        public virtual string LambdaMath_Log(string operand) => $"log({operand})";
        public virtual string LambdaMath_Log10(string operand) => $"log10({operand})";
        public virtual string LambdaMath_Pow(string operand, string y) => $"power({operand}, {y})";
        public virtual string LambdaMath_Sqrt(string operand) => $"sqrt({operand})";
        public virtual string LambdaMath_Cos(string operand) => $"cos({operand})";
        public virtual string LambdaMath_Sin(string operand) => $"sin({operand})";
        public virtual string LambdaMath_Tan(string operand) => $"tan({operand})";
        public virtual string LambdaMath_Acos(string operand) => $"acos({operand})";
        public virtual string LambdaMath_Asin(string operand) => $"asin({operand})";
        public virtual string LambdaMath_Atan(string operand) => $"atan({operand})";
        public virtual string LambdaMath_Atan2(string operand, string x) => $"atan2({operand}, {x})";
        public virtual string LambdaMath_Truncate(string operand) => $"floor({operand})";
    }
}
