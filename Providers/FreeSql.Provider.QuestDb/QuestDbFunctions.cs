using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Internal.CommonProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading;

/// <summary>
/// QuestDB lambda 表达式树扩展解析<para></para>
/// https://questdb.io/docs/reference/function/aggregation/
/// </summary>
[ExpressionCall]
public static class QuestFunc
{
    public static ThreadLocal<ExpressionCallContext> expContext = new ThreadLocal<ExpressionCallContext>();
    static ExpressionCallContext ec => expContext.Value;
    static T call<T>(string rt)
    {
        ec.Result = rt;
        return default(T);
    }

    public static decimal avg(object value) => call<decimal>($"avg({ec.ParsedContent["value"]})");
    public static long count() => call<long>($"count(*)");
    public static long count(object column_name) => call<long>($"count({ec.ParsedContent["column_name"]})");
    public static long count_distinct(object column_name) => call<long>($"count_distinct({ec.ParsedContent["column_name"]})");
    public static string first(object column_name) => call<string>($"first({ec.ParsedContent["column_name"]})");
    public static string last(object column_name) => call<string>($"last({ec.ParsedContent["column_name"]})");
    public static decimal haversine_dist_deg(decimal lat, decimal lon, DateTime ts) => call<decimal>($"haversine_dist_deg({ec.ParsedContent["lat"]},{ec.ParsedContent["lon"]},{ec.ParsedContent["ts"]})");
    public static decimal ksum(object value) => call<decimal>($"ksum({ec.ParsedContent["value"]})");
    public static T max<T>(T value) => call<T>($"max({ec.ParsedContent["value"]})");
    public static T min<T>(T value) => call<T>($"min({ec.ParsedContent["value"]})");
    public static decimal nsum(object value) => call<decimal>($"nsum({ec.ParsedContent["value"]})");
    public static decimal stddev_samp(object value) => call<decimal>($"stddev_samp({ec.ParsedContent["value"]})");
    public static decimal sum(object value) => call<decimal>($"sum({ec.ParsedContent["value"]})");

    public static bool isOrdered(object column) => call<bool>($"isOrdered({ec.ParsedContent["column"]})");
    public static T coalesce<T>(object value1, object value2) => call<T>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]})");
    public static T coalesce<T>(object value1, object value2, object value3) => call<T>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]})");
    public static T coalesce<T>(object value1, object value2, object value3, object value4) => call<T>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]},{ec.ParsedContent["value4"]})");
    public static T coalesce<T>(object value1, object value2, object value3, object value4, object value5) => call<T>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]},{ec.ParsedContent["value4"]},{ec.ParsedContent["value5"]})");
    public static T nullif<T>(object value1, object value2) => call<T>($"nullif({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]})");

    public static DateTime date_trunc([RawValue] date_trunc_unit unit, object timestamp) => call<DateTime>($"date_trunc('{unit.ToString()}',{ec.ParsedContent["timestamp"]})");
    public enum date_trunc_unit
    {
        millennium,
        decade,
        century,
        year,
        quarter,
        month,
        week,
        day,
        hour,
        minute,
        second,
        milliseconds,
        microseconds,
    }
    public static DateTime dateadd([RawValue] date_period period, [RawValue] long n, object startDate) => call<DateTime>($"dateadd('{(char)(int)period}',{n},{ec.ParsedContent["startDate"]})");
    public static long datediff([RawValue] date_period period, object date1, object date2) => call<long>($"datediff('{(char)period}',{ec.ParsedContent["date1"]},{ec.ParsedContent["date2"]})");
    public enum date_period
    {
        second = 's',
        minute = 'm',
        hour = 'h',
        day = 'd',
        week = 'w',
        month = 'M',
        year = 'y',
    }
    public static int day(object timestamp) => call<int>($"day({ec.ParsedContent["timestamp"]})");
    public static int day_of_week(object timestamp) => call<int>($"day_of_week({ec.ParsedContent["timestamp"]})");
    public static int day_of_week_sunday_first(object timestamp) => call<int>($"day_of_week_sunday_first({ec.ParsedContent["timestamp"]})");
    public static long extract([RawValue] extract_unit unit, object timestamp) => call<int>($"extract({unit.ToString()} from {ec.ParsedContent["timestamp"]})");
    public enum extract_unit
    {
        millennium,
        epoch,
        decade,
        century,
        year,
        isoyear,
        /// <summary>
        ///  day of year
        /// </summary>
        doy,
        quarter,
        month,
        week,
        /// <summary>
        /// day of week
        /// </summary>
        dow,
        isodow,
        day,
        hour,
        minute,
        second,
        microseconds,
        milliseconds,
    }
    public static int hour(object timestamp) => call<int>($"hour({ec.ParsedContent["timestamp"]})");
    public static bool is_leap_year(object timestamp) => call<bool>($"is_leap_year({ec.ParsedContent["timestamp"]})");
    public static bool days_in_month(object timestamp) => call<bool>($"days_in_month({ec.ParsedContent["timestamp"]})");
    public static int micros(object timestamp) => call<int>($"micros({ec.ParsedContent["timestamp"]})");
    public static int millis(object timestamp) => call<int>($"millis({ec.ParsedContent["timestamp"]})");
    public static int minute(object timestamp) => call<int>($"minute({ec.ParsedContent["timestamp"]})");
    public static int month(object timestamp) => call<int>($"month({ec.ParsedContent["timestamp"]})");
    public static DateTime now() => call<DateTime>($"now()");
    public static DateTime pg_postmaster_start_time() => call<DateTime>($"pg_postmaster_start_time()");
    public static int second(object timestamp) => call<int>($"second({ec.ParsedContent["timestamp"]})");
    /// <summary>
    /// Use now() with WHERE clause filter.
    /// </summary>
    /// <returns></returns>
    public static DateTime systimestamp() => call<DateTime>($"systimestamp()");
    /// <summary>
    /// Use now() with WHERE clause filter.
    /// </summary>
    /// <returns></returns>
    public static DateTime sysdate() => call<DateTime>($"sysdate()");

    public static DateTime timestamp_ceil([RawValue] timestamp_ceil_unit unit, object timestamp) => call<DateTime>($"timestamp_ceil({(char)unit},{ec.ParsedContent["timestamp"]})");
    public static DateTime timestamp_floor([RawValue] timestamp_ceil_unit unit, object timestamp) => call<DateTime>($"timestamp_floor({(char)unit},{ec.ParsedContent["timestamp"]})");
    public enum timestamp_ceil_unit
    {
        milliseconds = 'T',
        seconds = 's',
        minutes = 'm',
        hours = 'h',
        days = 'd',
        months = 'M',
        year = 'y',
    }
    public static DateTime timestamp_shuffle(object timestamp_1, object timestamp_2) => call<DateTime>($"timestamp_shuffle({ec.ParsedContent["timestamp_1"]},{ec.ParsedContent["timestamp_2"]})");
    public static DateTime to_date(string str, string format) => call<DateTime>($"to_date({ec.ParsedContent["str"]},{ec.ParsedContent["format"]})");
    public static string to_str(DateTime value, string format) => call<string>($"to_str({ec.ParsedContent["value"]},{ec.ParsedContent["format"]})");
    public static DateTime to_timestamp(string str, string format) => call<DateTime>($"to_timestamp({ec.ParsedContent["str"]},{ec.ParsedContent["format"]})");
    public static DateTime to_timezone(DateTime timestamp, string timezone) => call<DateTime>($"to_timezone({ec.ParsedContent["timestamp"]},{ec.ParsedContent["timezone"]})");
    public static DateTime to_utc(DateTime timestamp, string timezone) => call<DateTime>($"to_utc({ec.ParsedContent["timestamp"]},{ec.ParsedContent["timezone"]})");
    public static int week_of_year(object timestamp) => call<int>($"week_of_year({ec.ParsedContent["timestamp"]})");
    public static int year(object timestamp) => call<int>($"year({ec.ParsedContent["timestamp"]})");

    public static T abs<T>(T value) => call<T>($"abs({ec.ParsedContent["value"]})");
    public static decimal log(object value) => call<decimal>($"log({ec.ParsedContent["value"]})");
    public static decimal power(object _base, object exponent) => call<decimal>($"power({ec.ParsedContent["_base"]},{ec.ParsedContent["exponent"]})");
    public static T round<T>(T value, int scale) => call<T>($"round({ec.ParsedContent["value"]},{ec.ParsedContent["scale"]})");
    public static T round_down<T>(T value, int scale) => call<T>($"round_down({ec.ParsedContent["value"]},{ec.ParsedContent["scale"]})");
    public static T round_half_even<T>(T value, int scale) => call<T>($"round_half_even({ec.ParsedContent["value"]},{ec.ParsedContent["scale"]})");
    public static T round_up<T>(T value, int scale) => call<T>($"round_up({ec.ParsedContent["value"]},{ec.ParsedContent["scale"]})");
    public static string size_pretty(long value) => call<string>($"size_pretty({ec.ParsedContent["value"]})");
    public static decimal sqrt(object value) => call<decimal>($"sqrt({ec.ParsedContent["value"]})");

    public static bool rnd_boolean() => call<bool>($"rnd_boolean()");
    public static byte rnd_byte() => call<byte>($"rnd_byte()");
    public static byte rnd_byte(byte min, byte max) => call<byte>($"rnd_byte({ec.ParsedContent["min"]},{ec.ParsedContent["max"]})");
    public static short rnd_short() => call<short>($"rnd_short()");
    public static short rnd_short(short min, short max) => call<short>($"rnd_short({ec.ParsedContent["min"]},{ec.ParsedContent["max"]})");
    public static int rnd_int() => call<int>($"rnd_int()");
    public static int rnd_int(int min, int max) => call<int>($"rnd_int({ec.ParsedContent["min"]},{ec.ParsedContent["max"]})");
    public static int rnd_int(int min, int max, int nanRate) => call<int>($"rnd_int({ec.ParsedContent["min"]},{ec.ParsedContent["max"]},{ec.ParsedContent["nanRate"]})");
    public static long rnd_long() => call<long>($"rnd_long()");
    public static long rnd_long(long min, long max) => call<long>($"rnd_long({ec.ParsedContent["min"]},{ec.ParsedContent["max"]})");
    public static long rnd_long(long min, long max, int nanRate) => call<long>($"rnd_long({ec.ParsedContent["min"]},{ec.ParsedContent["max"]},{ec.ParsedContent["nanRate"]})");
    public static BigInteger rnd_long256() => call<BigInteger>($"rnd_long256()");
    public static BigInteger rnd_long256(BigInteger min, BigInteger max) => call<BigInteger>($"rnd_long256({ec.ParsedContent["min"]},{ec.ParsedContent["max"]})");
    public static BigInteger rnd_long256(BigInteger min, BigInteger max, int nanRate) => call<BigInteger>($"rnd_long256({ec.ParsedContent["min"]},{ec.ParsedContent["max"]},{ec.ParsedContent["nanRate"]})");
    public static float rnd_float() => call<float>($"rnd_float()");
    public static float rnd_float(int nanRate) => call<float>($"rnd_float({ec.ParsedContent["nanRate"]})");
    public static double rnd_double() => call<double>($"rnd_double()");
    public static double rnd_double(int nanRate) => call<double>($"rnd_double({ec.ParsedContent["nanRate"]})"); 
    public static DateTime rnd_date(DateTime min, DateTime max) => call<DateTime>($"rnd_date({ec.ParsedContent["min"]},{ec.ParsedContent["max"]},0)");
    public static DateTime rnd_date(DateTime min, DateTime max, int nanRate) => call<DateTime>($"rnd_date({ec.ParsedContent["min"]},{ec.ParsedContent["max"]},{ec.ParsedContent["nanRate"]})");
    public static DateTime rnd_timestamp(DateTime min, DateTime max) => call<DateTime>($"rnd_timestamp({ec.ParsedContent["min"]},{ec.ParsedContent["max"]},0)");
    public static DateTime rnd_timestamp(DateTime min, DateTime max, int nanRate) => call<DateTime>($"rnd_timestamp({ec.ParsedContent["min"]},{ec.ParsedContent["max"]},{ec.ParsedContent["nanRate"]})");
    public static char rnd_char() => call<char>($"rnd_char()");
    public static string rnd_symbol([RawValue] string[] symbolList) => call<string>($"rnd_symbol({string.Join(",", symbolList.Select(a => ec.FormatSql(a)))})");
    public static string rnd_symbol(int list_size, int minLength, int maxLength, int nullRate) => call<string>($"rnd_symbol({ec.ParsedContent["list_size"]},{ec.ParsedContent["minLength"]},{ec.ParsedContent["maxLength"]},{ec.ParsedContent["nullRate"]})");
    public static string rnd_str([RawValue] string[] stringList) => call<string>($"rnd_str({string.Join(",", stringList.Select(a => ec.FormatSql(a)))})");
    public static string rnd_str(int list_size, int minLength, int maxLength, int nullRate) => call<string>($"rnd_str({ec.ParsedContent["list_size"]},{ec.ParsedContent["minLength"]},{ec.ParsedContent["maxLength"]},{ec.ParsedContent["nullRate"]})");
    public static byte[] rnd_bin() => call<byte[]>($"rnd_bin()");
    public static byte[] rnd_bin(long minBytes, int maxBytes, int nullRate) => call<byte[]>($"rnd_bin({ec.ParsedContent["minBytes"]},{ec.ParsedContent["maxBytes"]},{ec.ParsedContent["nullRate"]})");
    public static Guid rnd_uuid4() => call<Guid>($"rnd_uuid4()");

    public static byte[] rnd_geohash(int bits) => call<byte[]>($"rnd_geohash({ec.ParsedContent["bits"]})");
    public static byte[] make_geohash(decimal lon, decimal lat, int bits) => call<byte[]>($"make_geohash({ec.ParsedContent["lon"]},{ec.ParsedContent["lat"]},{ec.ParsedContent["bits"]})");
    
    public static string concat(object value1, object value2) => call<string>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]})");
    public static string concat(object value1, object value2, object value3) => call<string>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]})");
    public static string concat(object value1, object value2, object value3, object value4) => call<string>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]},{ec.ParsedContent["value4"]})");
    public static string concat(object value1, object value2, object value3, object value4, object value5) => call<string>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]},{ec.ParsedContent["value4"]},{ec.ParsedContent["value5"]})");
    public static string concat(object value1, object value2, object value3, object value4, object value5, object value6) => call<string>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]},{ec.ParsedContent["value4"]},{ec.ParsedContent["value5"]},{ec.ParsedContent["value6"]})");
    public static string concat(object value1, object value2, object value3, object value4, object value5, object value6, object value7) => call<string>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]},{ec.ParsedContent["value4"]},{ec.ParsedContent["value5"]},{ec.ParsedContent["value6"]},{ec.ParsedContent["value7"]})");
    public static string concat(object value1, object value2, object value3, object value4, object value5, object value6, object value7, object value8) => call<string>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]},{ec.ParsedContent["value4"]},{ec.ParsedContent["value5"]},{ec.ParsedContent["value6"]},{ec.ParsedContent["value7"]},{ec.ParsedContent["value8"]})");
    public static string concat(object value1, object value2, object value3, object value4, object value5, object value6, object value7, object value8, object value9) => call<string>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]},{ec.ParsedContent["value4"]},{ec.ParsedContent["value5"]},{ec.ParsedContent["value6"]},{ec.ParsedContent["value7"]},{ec.ParsedContent["value8"]},{ec.ParsedContent["value9"]})");
    public static string concat(object value1, object value2, object value3, object value4, object value5, object value6, object value7, object value8, object value9, object value10) => call<string>($"coalesce({ec.ParsedContent["value1"]},{ec.ParsedContent["value2"]},{ec.ParsedContent["value3"]},{ec.ParsedContent["value4"]},{ec.ParsedContent["value5"]},{ec.ParsedContent["value6"]},{ec.ParsedContent["value7"]},{ec.ParsedContent["value8"]},{ec.ParsedContent["value9"]},{ec.ParsedContent["value10"]})");
    public static long length(object value) => call<long>($"length({ec.ParsedContent["value"]})");
    public static string left(string str, int count) => call<string>($"left({ec.ParsedContent["str"]},{ec.ParsedContent["count"]})");
    public static string right(string str, int count) => call<string>($"right({ec.ParsedContent["str"]},{ec.ParsedContent["count"]})");
    public static int strpos(object str, string substr) => call<int>($"strpos({ec.ParsedContent["str"]},{ec.ParsedContent["substr"]})");
    public static string substring(string str, int start, int length) => call<string>($"substring({ec.ParsedContent["str"]},{ec.ParsedContent["start"]},{ec.ParsedContent["length"]})");
    public static string lower(string str) => call<string>($"lower({ec.ParsedContent["str"]})");
    public static string upper(string str) => call<string>($"upper({ec.ParsedContent["str"]})");

    public static DateTime timestamp_sequence(DateTime startTimestamp, long step) => call<DateTime>($"timestamp_sequence({ec.ParsedContent["startTimestamp"]},{ec.ParsedContent["step"]})");
    public static string regexp_replace(string str1, string regex, string str2) => call<string>($"regexp_replace({ec.ParsedContent["str1"]},{ec.ParsedContent["regex"]},{ec.ParsedContent["str2"]})");
    public static bool regex_match(string str, string regex) => call<bool>($"{ec.ParsedContent["str"]} ~ {ec.ParsedContent["regex"]}");
    public static bool regex_not_match(string str, string regex) => call<bool>($"{ec.ParsedContent["str"]} !~ {ec.ParsedContent["regex"]}");
    public static bool like(string str, string pattern) => call<bool>($"{ec.ParsedContent["str"]} LIKE {ec.ParsedContent["pattern"]}");
    public static bool not_like(string str, string pattern) => call<bool>($"{ec.ParsedContent["str"]} NOT LIKE {ec.ParsedContent["pattern"]}");
    public static bool ilike(string str, string pattern) => call<bool>($"{ec.ParsedContent["str"]} ILIKE {ec.ParsedContent["pattern"]}");
    public static bool not_ilike(string str, string pattern) => call<bool>($"{ec.ParsedContent["str"]} NOT ILIKE {ec.ParsedContent["pattern"]}");

    public static bool within(object geo, object geohash1) => call<bool>($"{ec.ParsedContent["str"]} within({ec.ParsedContent["geohash1"]})");
    public static bool within(object geo, object geohash1, object geohash2) => call<bool>($"{ec.ParsedContent["str"]} within({ec.ParsedContent["geohash1"]},{ec.ParsedContent["geohash2"]})");
    public static bool within(object geo, object geohash1, object geohash2, object geohash3) => call<bool>($"{ec.ParsedContent["str"]} within({ec.ParsedContent["geohash1"]},{ec.ParsedContent["geohash2"]},{ec.ParsedContent["geohash3"]})");
    public static bool within(object geo, object geohash1, object geohash2, object geohash3, object geohash4) => call<bool>($"{ec.ParsedContent["str"]} within({ec.ParsedContent["geohash1"]},{ec.ParsedContent["geohash2"]},{ec.ParsedContent["geohash3"]},{ec.ParsedContent["geohash4"]})");
    public static bool within(object geo, object geohash1, object geohash2, object geohash3, object geohash4, object geohash5) => call<bool>($"{ec.ParsedContent["str"]} within({ec.ParsedContent["geohash1"]},{ec.ParsedContent["geohash2"]},{ec.ParsedContent["geohash3"]},{ec.ParsedContent["geohash4"]},{ec.ParsedContent["geohash5"]})");
}

partial class QuestDbGlobalExtensions
{
    /// <summary>
    /// QuestDB lambda 表达式树扩展解析<para></para>
    /// fsql.SelectLongSequence(10, () => new { str1 = qdbfunc.rnd_str(10, 5, 8, 0), ... })...<para></para>
    /// SELECT rnd_str(10,5,8,0) FROM long_sequence(10)
    /// </summary>
    public static ISelect<T> SelectLongSequence<T>(this IFreeSql fsql, long iterations, Expression<Func<T>> selector)
    {
        var selector2 = Expression.Lambda<Func<object, T>>(selector.Body, Expression.Parameter(typeof(object), "a"));
        var tablename = $"(long_sequence ({iterations}))";
        return fsql.Select<object>().AsTable((t, old) => tablename).WithTempQuery(selector2);
    }

    /// <summary>
    /// QuestDB lambda 表达式树扩展解析<para></para>
    /// fsql.SelectTimestampSequence(10, () => new { str1 = qdbfunc.rnd_str(10, 5, 8, 0), ... })...<para></para>
    /// SELECT rnd_str(10,5,8,0) FROM long_sequence(10)
    /// </summary>
    public static ISelect<T> SelectTimestampSequence<T>(this IFreeSql fsql, DateTime startTimestamp, TimeSpan step, Expression<Func<T>> selector)
    {
        var selector2 = Expression.Lambda<Func<object, T>>(selector.Body, Expression.Parameter(typeof(object), "a"));
        var tablename = $"(timestamp_sequence (to_timestamp('{startTimestamp.ToString("yyyy-MM-dd HH:mm:ss")}', 'yyyy-MM-dd HH:mm:ss'), {Math.Ceiling(step.TotalMilliseconds / 1000)}))";
        return fsql.Select<object>().AsTable((t, old) => tablename).WithTempQuery(selector2);
    }
}