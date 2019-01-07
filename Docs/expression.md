# 表达式函数
| 表达式 | MySql | SqlServer | PostgreSQL | Oracle | 功能说明 |
| - | - | - | - | - |
| a ? b : c | case when a then b else c end | case when a then b else c end | case when a then b else c end | case when a then b else c end | a成立时取b值，否则取c值 |
| a ?? b | ifnull(a, b) | isnull(a, b) | coalesce(a, b) | nvl(a, b) | 当a为null时，取b值 |
| 数字 + 数字 | a + b | a + b | a + b | a + b | 数字相加 |
| 数字 + 字符串 | concat(a, b) | cast(a as varchar) + cast(b as varchar) | case(a as varchar)\|\| b | a\|\| b | 字符串相加，a或b任意一个为字符串时 |
| a - b | a - b | a - b | a - b | a - b | 减
| a * b | a * b | a * b | a * b | a * b | 乘
| a / b | a / b | a / b | a / b | a / b | 除
| a % b | a % b | a % b | a % b | mod(a,b) | 模

> 等等...

### 数组
| 表达式 | MySql | SqlServer | PostgreSQL | Oracle | 功能说明 |
| - | - | - | - | - |
| a.Length | - | - | case when a is null then 0 else array_length(a,1) end | - | 数组长度 |
| 常量数组.Length | - | - | array_length(array[常量数组元素逗号分割],1) | - | 数组长度 |
| a.Any() | - | - | case when a is null then 0 else array_length(a,1) end > 0 | - | 数组是否为空 |
| 常量数组.Contains(b) | b in (常量数组元素逗号分割) | b in (常量数组元素逗号分割) | b in (常量数组元素逗号分割) | b in (常量数组元素逗号分割) | IN查询 |
| a.Contains(b) | - | - | a @> array[b] | - | a数组是否包含b元素 |
| a.Concat(b) | - | - | a \|\| b | - | 数组相连 |
| a.Count() | - | - | 同 Length | - | 数组长度 |

### 字典 Dictionary<string, string>
| 表达式 | MySql | SqlServer | PostgreSQL | Oracle | 功能说明 |
| - | - | - | - | - |
| a.Count | - | - | case when a is null then 0 else array_length(akeys(a),1) end | - | 字典长度 |
| a.Keys | - | - | akeys(a) | - | 返回字典所有key数组 |
| a.Values | - | - | avals(a) | - | 返回字典所有value数组 |
| a.Contains(b) | - | - | a @> b | - | 字典是否包含b
| a.ContainsKey(b) | - | - | a? b | - | 字典是否包含key
| a.Concat(b) | - | - | a \|\| b | - | 字典相连 |
| a.Count() | - | - | 同 Count | - | 字典长度 |

### JSON JToken/JObject/JArray
| 表达式 | MySql | SqlServer | PostgreSQL | Oracle | 功能说明 |
| - | - | - | - | - |
| a.Count | - | - | jsonb_array_length(coalesce(a, '[])) | - | json数组类型的长度 |
| a.Any() | - | - | jsonb_array_length(coalesce(a, '[])) > 0 | - | json数组类型，是否为空 |
| a.Contains(b) | - | - | coalesce(a, '{}') @> b::jsonb | - | json中是否包含b |
| a.ContainsKey(b) | - | - | coalesce(a, '{}') ? b | - | json中是否包含键b |
| a.Concat(b) | - | - | coalesce(a, '{}') || b::jsonb | - | 连接两个json |
| Parse(a) | - | - | a::jsonb | - | 转化字符串为json类型 |

### 字符串对象
| 表达式 | MySql | SqlServer | PostgreSQL | Oracle | 功能说明 |
| - | - | - | - | - |
| string.Empty | '' | '' | '' | 空字符串表示 |
| string.IsNullOrEmpty(a) | (a is null or a = '') | (a is null or a = '') | (a is null or a = '') | (a is null or a = '') | 空字符串表示 |
| a.CompareTo(b) | strcmp(a, b) | - | case when a = b then 0 when a > b then 1 else -1 end | case when a = b then 0 when a > b then 1 else -1 end | 比较a和b大小 |
| a.Contains('b') | a like '%b%' | a like '%b%' | a ilike'%b%' | a like '%b%' | a是否包含b |
| a.EndsWith('b') | a like '%b' | a like '%b' | a ilike'%b' | a like '%b' | a尾部是否包含b |
| a.IndexOf(b) | locate(a, b) - 1 | locate(a, b) - 1 | strpos(a, b) - 1 | instr(a, b, 1, 1) - 1 | 查找a中出现b的位置 |
| a.Length | char_length(a) | len(a) | char_length(a) | length(a) | 返回a的字符串长度 |
| a.PadLeft(b, c) | lpad(a, b, c) | - | lpad(a, b, c) | lpad(a, b, c) | 在a的左侧充字符c，直到字符串长度大于b |
| a.PadRight(b, c) | rpad(a, b, c) | - | rpad(a, b, c) | rpad(a, b, c) | 在a的右侧充字符c，直到字符串长度大于b |
| a.Replace(b, c) | replace(a, b, c) | replace(a, b, c) | replace(a, b, c) | replace(a, b, c) | 将a中字符串b，替换成c |
| a.StartsWith('b') | a like 'b%' | a like 'b%' | a ilike'b%' | a like 'b%' | a头部是否包含b |
| a.Substring(b, c) | substr(a, b, c + 1) | substring(a, b, c + 1) | substr(a, b, c + 1) | substr(a, b, c + 1) | 截取a中位置b到c的内容 |
| a.ToLower | lower(a) | lower(a) | lower(a) | lower(a) | 转小写 |
| a.ToUpper | upper(a) | upper(a) | upper(a) | upper(a) | 转大写 |
| a.Trim | trim(a) | trim(a) | trim(a) | trim(a) | 移除两边字符 |
| a.TrimEnd | rtrim(a) | rtrim(a) | rtrim(a) | rtrim(a) | 移除左侧指定字符 |
| a.TrimStart | ltrim(a) | ltrim(a) | ltrim(a) | ltrim(a) | 移除右侧指定字符 |

### 日期对象
| 表达式 | MySql | SqlServer | PostgreSQL | Oracle |
| - | - | - | - |
| DateTime.Now | now() | getdate() | current_timestamp | systimestamp |
| DateTime.UtcNow | utc_timestamp() | getutcdate() | (current_timestamp at time zone 'UTC') | sys_extract_utc(systimestamp) |
| DateTime.Today | curdate | convert(char(10),getdate(),120) | current_date | trunc(systimestamp) |
| DateTime.MaxValue | cast('9999/12/31 23:59:59' as datetime) | '9999/12/31 23:59:59' | '9999/12/31 23:59:59'::timestamp | to_timestamp('9999-12-31 23:59:59','YYYY-MM-DD HH24:MI:SS.FF6') |
| DateTime.MinValue | cast('0001/1/1 0:00:00' as datetime) | '1753/1/1 0:00:00' | '0001/1/1 0:00:00'::timestamp | to_timestamp('0001-01-01 00:00:00','YYYY-MM-DD HH24:MI:SS.FF6') |
| DateTime.Compare(a, b) | a - b | a - b | extract(epoch from a::timestamp-b::timestamp) | extract(day from (a-b)) |
| DateTime.DaysInMonth(a, b) | dayofmonth(last_day(concat(a, '-', b, '-1'))) | datepart(day, dateadd(day, -1, dateadd(month, 1, cast(a as varchar) + '-' + cast(b as varchar) + '-1'))) | extract(day from (a || '-' || b || '-01')::timestamp+'1 month'::interval-'1 day'::interval) | cast(to_char(last_day(a||'-'||b||'-01'),'DD') as number) |
| DateTime.Equals(a, b) | a = b | a = b | a = b | a = b |
| DateTime.IsLeapYear(a) | a%4=0 and a%100<>0 or a%400=0 | a%4=0 and a%100<>0 or a%400=0 | a%4=0 and a%100<>0 or a%400=0 | mod(a,4)=0 AND mod(a,100)<>0 OR mod(a,400)=0 |
| DateTime.Parse(a) | cast(a as datetime) | cast(a as datetime) | a::timestamp | to_timestamp(a,'YYYY-MM-DD HH24:MI:SS.FF6') |
| a.Add(b) | date_add(a, interval b microsecond) | dateadd(millisecond, b / 1000, a) | a::timestamp+(b||' microseconds')::interval | 增加TimeSpan值 | a + b |
| a.AddDays(b) | date_add(a, interval b day) | dateadd(day, b, a) | a::timestamp+(b||' day')::interval | a + b |
| a.AddHours(b) | date_add(a, interval b hour) | dateadd(hour, b, a) | a::timestamp+(b||' hour')::interval | a + b/24 |
| a.AddMilliseconds(b) | date_add(a, interval b*1000 microsecond) | dateadd(millisecond, b, a) | a::timestamp+(b||' milliseconds')::interval | a + b/86400000 |
| a.AddMinutes(b) | date_add(a, interval b minute) | dateadd(minute, b, a) | a::timestamp+(b||' minute')::interval | a + b/1440 |
| a.AddMonths(b) | date_add(a, interval b month) | dateadd(month, b, a) | a::timestamp+(b||' month')::interval | add_months(a,b) |
| a.AddSeconds(b) | date_add(a, interval b second) | dateadd(second, b, a) | a::timestamp+(b||' second')::interval | a + b/86400 |
| a.AddTicks(b) | date_add(a, interval b/10 microsecond) | dateadd(millisecond, b / 10000, a) | a::timestamp+(b||' microseconds')::interval | a + b/86400000000 |
| a.AddYears(b) | date_add(a, interval b year) | dateadd(year, b, a) | a::timestamp+(b||' year')::interval | add_months(a,b*12) |
| a.Date | cast(date_format(a, '%Y-%m-%d') as datetime) | convert(char(10),a,120) | a::date | trunc(a) |
| a.Day | dayofmonth(a) | datepart(day, a) | extract(day from a::timestamp) | cast(to_char(a,'DD') as number) |
| a.DayOfWeek | dayofweek(a) | datepart(weekday, a) - 1 | extract(dow from a::timestamp) | case when to_char(a)='7' then 0 else cast(to_char(a) as number) end |
| a.DayOfYear | dayofyear(a) | datepart(dayofyear, a) | extract(doy from a::timestamp) | cast(to_char(a,'DDD') as number) |
| a.Hour | hour(a) | datepart(hour, a) | extract(hour from a::timestamp) | cast(to_char(a,'HH24') as number) |
| a.Millisecond | floor(microsecond(a) / 1000) | datepart(millisecond, a) | extract(milliseconds from a::timestamp)-extract(second from a::timestamp)*1000 | cast(to_char(a,'FF3') as number) |
| a.Minute | minute(a) | datepart(minute, a) | extract(minute from a::timestamp) | cast(to_char(a,'MI') as number) |
| a.Month | month(a) | datepart(month, a) | extract(month from a::timestamp) | cast(to_char(a,'FF3') as number) |
| a.Second | second(a) | datepart(second, a) | extract(second from a::timestamp) | cast(to_char(a,'SS') as number) |
| a.Subtract(b) | timestampdiff(microsecond, b, a) | datediff(millisecond, b, a) * 1000 | (extract(epoch from a::timestamp-b::timestamp)*1000000) | a - b |
| a.Ticks | timestampdiff(microsecond, '0001-1-1', a) * 10 | datediff(millisecond, '1970-1-1', a) * 10000 + 621355968000000000 | extract(epoch from a::timestamp)*10000000+621355968000000000 | cast(to_char(a,'FF7') as number) |
| a.TimeOfDay | timestampdiff(microsecond, date_format(a, '%Y-%m-%d'), a) | '1970-1-1 ' + convert(varchar, a, 14) | extract(epoch from a::time)*1000000 | a - trunc(a) |
| a.Year | year(a) | datepart(year, a) | extract(year from a::timestamp) | 年 | cast(to_char(a,'YYYY') as number) |
| a.Equals(b) | a = b | a = b | a = b | a = b |
| a.CompareTo(b) | a - b | a - b | a - b | a - b |
| a.ToString() | date_format(a, '%Y-%m-%d %H:%i:%s.%f') | convert(varchar, a, 121) | to_char(a, 'YYYY-MM-DD HH24:MI:SS.US') | to_char(a,'YYYY-MM-DD HH24:MI:SS.FF6') |

### 时间对象
| 表达式 | MySql(微秒) | SqlServer(秒) | PostgreSQL(微秒) | Oracle(Interval day(9) to second(7)) |
| - | - | - | - |
| TimeSpan.Zero | 0 | 0 | - | 0微秒 | numtodsinterval(0,'second') |
| TimeSpan.MaxValue | 922337203685477580 | 922337203685477580 | - | numtodsinterval(233720368.5477580,'second') |
| TimeSpan.MinValue | -922337203685477580 | -922337203685477580 | - | numtodsinterval(-233720368.5477580,'second') |
| TimeSpan.Compare(a, b) | a - b | a - b | - | extract(day from (a-b)) |
| TimeSpan.Equals(a, b) | a = b | a = b | - | a = b |
| TimeSpan.FromDays(a) | a * 1000000 * 60 * 60 * 24 | a * 1000000 * 60 * 60 * 24 | - | numtodsinterval(a*86400,'second') |
| TimeSpan.FromHours(a) | a * 1000000 * 60 * 60 | a * 1000000 * 60 * 60 | - | numtodsinterval(a*3600,'second') |
| TimeSpan.FromMilliseconds(a) | a * 1000 | a * 1000 | - | numtodsinterval(a/1000,'second') |
| TimeSpan.FromMinutes(a) | a * 1000000 * 60 | a * 1000000 * 60 | - | numtodsinterval(a*60,'second') |
| TimeSpan.FromSeconds(a) | a * 1000000 | a * 1000000 | - | numtodsinterval(a,'second') |
| TimeSpan.FromTicks(a) | a / 10 | a / 10 | - | numtodsinterval(a/10000000,'second') |
| a.Add(b) | a + b | a + b | - | a + b |
| a.Subtract(b) | a - b | a - b | - | a - b |
| a.CompareTo(b) | a - b | a - b | - | extract(day from (a-b)) |
| a.Days | a div (1000000 * 60 * 60 * 24) | a div (1000000 * 60 * 60 * 24) | - | extract(day from a) |
| a.Hours | a div (1000000 * 60 * 60) mod 24 | a div (1000000 * 60 * 60) mod 24 | - | extract(hour from a) |
| a.Milliseconds | a div 1000 mod 1000 | a div 1000 mod 1000 | - | cast(substr(extract(second from a)-floor(extract(second from a)),2,3) as number) |
| a.Seconds | a div 1000000 mod 60 | a div 1000000 mod 60 | - | extract(second from a) |
| a.Ticks | a * 10 | a * 10 | - | (extract(day from a)*86400+extract(hour from a)*3600+extract(minute from a)*60+extract(second from a))*10000000 |
| a.TotalDays | a / (1000000 * 60 * 60 * 24) | a / (1000000 * 60 * 60 * 24) | - | extract(day from a) |
| a.TotalHours | a / (1000000 * 60 * 60) | a / (1000000 * 60 * 60) | - | (extract(day from a)*24+extract(hour from a)) |
| a.TotalMilliseconds | a / 1000 | a / 1000 | - | (extract(day from a)*86400+extract(hour from a)*3600+extract(minute from a)*60+extract(second from a))*1000 |
| a.TotalMinutes | a / (1000000 * 60) | a / (1000000 * 60) | - | | (extract(day from a)*1440+extract(hour from a)*60+extract(minute from a)) |
| a.TotalSeconds | a / 1000000 | a / 1000000 | - | (extract(day from a)*86400+extract(hour from a)*3600+extract(minute from a)*60+extract(second from a)) |
| a.Equals(b) | a = b | a = b | - | a = b |
| a.ToString() | cast(a as varchar) | cast(a as varchar) | - | to_char(a) |

### 数学函数
| 表达式 | MySql | SqlServer | PostgreSQL |
| - | - | - | - |
| Math.Abs(a) | abs(a) | abs(a) | abs(a) |
| Math.Acos(a) | acos(a) | acos(a) | acos(a) | acos(a) |
| Math.Asin(a) | asin(a) | asin(a) | asin(a) | asin(a) |
| Math.Atan(a) | atan(a) | atan(a) | atan(a) | atan(a) |
| Math.Atan2(a, b) | atan2(a, b) | atan2(a, b) | atan2(a, b) | - |
| Math.Ceiling(a) | ceiling(a) | ceiling(a) | ceiling(a) | ceil(a) |
| Math.Cos(a) | cos(a) | cos(a) | cos(a) | cos(a) |
| Math.Exp(a) | exp(a) | exp(a) | exp(a) | exp(a) |
| Math.Floor(a) | floor(a) | floor(a) | floor(a) | floor(a) |
| Math.Log(a) | log(a) | log(a) | log(a) | log(e,a) |
| Math.Log10(a) | log10(a) | log10(a) | log10(a) | log(10,a) |
| Math.PI(a) | 3.1415926535897931 | 3.1415926535897931 | 3.1415926535897931 | 3.1415926535897931 |
| Math.Pow(a, b) | pow(a, b) | power(a, b) | pow(a, b) | power(a, b) |
| Math.Round(a, b) | round(a, b) | round(a, b) | round(a, b) | round(a, b) |
| Math.Sign(a) | sign(a) | sign(a) | sign(a) | sign(a) |
| Math.Sin(a) | sin(a) | sin(a) | sin(a) | sin(a) |
| Math.Sqrt(a) | sqrt(a) | sqrt(a) | sqrt(a) | sqrt(a) |
| Math.Tan(a) | tan(a) | tan(a) | tan(a) | tan(a) |
| Math.Truncate(a) | truncate(a, 0) | floor(a) | trunc(a, 0) | trunc(a, 0) |

### 类型转换
| 表达式 | MySql | SqlServer | PostgreSQL | Oracle |
| - | - | - | - |
| Convert.ToBoolean(a) | a not in ('0','false') | a not in ('0','false') | a::varchar not in ('0','false','f','no') | - |
| Convert.ToByte(a) | cast(a as unsigned) | cast(a as tinyint) | a::int2 | cast(a as number) |
| Convert.ToChar(a) | substr(cast(a as char),1,1) | substring(cast(a as nvarchar),1,1) | substr(a::char,1,1) | substr(to_char(a),1,1) |
| Convert.ToDateTime(a) | cast(a as datetime) | cast(a as datetime) | a::timestamp | to_timestamp(a,'YYYY-MM-DD HH24:MI:SS.FF6') |
| Convert.ToDecimal(a) | cast(a as decimal(36,18)) | cast(a as decimal(36,19)) | a::numeric | cast(a as number) |
| Convert.ToDouble(a) | cast(a as decimal(32,16)) | cast(a as decimal(32,16)) | a::float8 | cast(a as number) |
| Convert.ToInt16(a) | cast(a as signed) | cast(a as smallint) | a::int2 | cast(a as number) |
| Convert.ToInt32(a) | cast(a as signed) | cast(a as int) | a::int4 | cast(a as number) |
| Convert.ToInt64(a) | cast(a as signed) | cast(a as bigint) | a::int8 | cast(a as number) |
| Convert.ToSByte(a) | cast(a as signed) | cast(a as tinyint) | a::int2 | cast(a as number) |
| Convert.ToString(a) | cast(a as decimal(14,7)) | cast(a as decimal(14,7)) | a::float4 | to_char(a) |
| Convert.ToSingle(a) | cast(a as char) | cast(a as nvarchar) | a::varchar | cast(a as number) |
| Convert.ToUInt16(a) | cast(a as unsigned) | cast(a as smallint) | a::int2 | cast(a as number) |
| Convert.ToUInt32(a) | cast(a as unsigned) | cast(a as int) | a::int4 | cast(a as number) |
| Convert.ToUInt64(a) | cast(a as unsigned) | cast(a as bigint) | a::int8 | cast(a as number) |
