# 表达式函数
| 表达式 | MySql | SqlServer | PostgreSQL | 功能说明 |
| - | - | - | - | - |
| (a ?? b) | ifnull(a, b) | isnull(a, b) | coalesce(a, b) | 当a为null时，取b值 |
| 数字 + 数字 | a + b | a + b | a + b | 数字相加 |
| 数字 + 字符串 | concat(a, b) | cast(a as varchar) + cast(b as varchar) | case(a as varchar) \|\| b | 字符串相加，a或b任意一个为字符串时 |
| a - b | a - b | a - b | a - b | 减
| a * b | a * b | a * b | a * b | 乘
| a / b | a / b | a / b | a / b | 乘
| a % b | a mod b | a mod b | a mod b | 模

> 等等...

### 字符串对象
| 表达式 | MySql | SqlServer | PostgreSQL | 功能说明 |
| - | - | - | - | - |
| string.Empty | '' | '' | '' | 空字符串表示 |
| a.CompareTo(b) | strcmp(a, b) | - | - | 比较a和b大小 |
| a.Contains('b') | a like '%b%' | a like '%b%' | - | a是否包含b |
| a.EndsWith('b') | a like '%b' | a like '%b' | - | a尾部是否包含b |
| a.IndexOf(b) | locate(a, b) - 1 | locate(a, b) - 1 | - | 查找a中出现b的位置 |
| a.Length | char_length(a) | len(a) | - | 返回a的字符串长度 |
| a.PadLeft(b, c) | lpad(a, b, c) | - | - | 在a的左侧充字符c，直到字符串长度大于b |
| a.PadRight(b, c) | rpad(a, b, c) | - | - | 在a的右侧充字符c，直到字符串长度大于b |
| a.Replace(b, c) | replace(a, b, c) | replace(a, b, c) | - | 将a中字符串b，替换成c |
| a.StartsWith('b') | a like 'b%' | a like 'b%' | - | a头部是否包含b |
| a.Substring(b, c) | substr(a, b, c) | substring(a, b, c) | - | 截取a中位置b到c的内容 |
| a.ToLower | lower(a) | lower(a) | - | 转小写 |
| a.ToUpper | upper(a) | upper(a) | - | 转大写 |
| a.Trim | trim(a) | trim(a) | - | 移除两边字符 |
| a.TrimEnd | rtrim(a) | rtrim(a) | - | 移除左侧指定字符 |
| a.TrimStart | ltrim(a) | ltrim(a) | - | 移除右侧指定字符 |

### 日期对象
| 表达式 | MySql | SqlServer | PostgreSQL | 功能说明 |
| - | - | - | - | - |
| DateTime.Now | now() | getdate() | - | 取本地时间 |
| DateTime.UtcNow | utc_timestamp() | getutcdate() | - | 取UTC时间 |
| DateTime.Today | curdate | convert(char(10),getdate(),120) | - | 取本地时间，日期部分 |
| DateTime.MaxValue | cast('9999/12/31 23:59:59' as datetime) | '1753/1/1 0:00:00' | - | 最大时间 |
| DateTime.MinValue | cast('0001/1/1 0:00:00' as datetime) | '9999/12/31 23:59:59' | - | 最小时间 |
| DateTime.Compare(a, b) | a - b | a - b | - | 比较a和b的大小 |
| DateTime.DaysInMonth(a, b) | dayofmonth(last_day(concat(a, '-', b, '-1'))) | datepart(day, dateadd(day, -1, dateadd(month, 1, cast(a as varchar) + '-' + cast(b as varchar) + '-1'))) | - | 取指定年月份的总天数 |
| DateTime.Equals(a, b) | a = b | a = b | - | 比较a和b相等 |
| DateTime.IsLeapYear(a) | a%4=0 and a%100<>0 or a%400=0 | a%4=0 and a%100<>0 or a%400=0 | - | 判断闰年 |
| DateTime.Parse(a) | cast(a as datetime) | cast(a as datetime) | - | 转换日期类型 |
| a.Add(b) | date_add(a, interval b microsecond) | dateadd(millisecond, b / 1000, a) | - | 增加TimeSpan值 |
| a.AddDays(b) | date_add(a, interval b day) | dateadd(day, b, a) | - | 增加天数 |
| a.AddHours(b) | date_add(a, interval b hour) | dateadd(hour, b, a) | - | 增加小时 |
| a.AddMilliseconds(b) | date_add(a, interval b*1000 microsecond) | dateadd(millisecond, b, a) | - | 增加毫秒 |
| a.AddMinutes(b) | date_add(a, interval b minute) | dateadd(minute, b, a) | - | 增加分钟 |
| a.AddMonths(b) | date_add(a, interval b month) | dateadd(month, b, a) | - | 增加月 |
| a.AddSeconds(b) | date_add(a, interval b second) | dateadd(second, b, a) | - | 增加秒 |
| a.AddTicks(b) | date_add(a, interval b/10 microsecond) | dateadd(millisecond, b / 10000, a) | - | 增加刻度，微秒的1/10 |
| a.AddYears(b) | date_add(a, interval b year) | dateadd(year, b, a) | - | 增加年 |
| a.Date | cast(date_format(a, '%Y-%m-%d') as datetime) | convert(char(10),a,120) | - | 获取a的日期部分 |
| a.Day | dayofmonth(a) | datepart(day, a) | - | 获取a在月的第几天 |
| a.DayOfWeek | dayofweek(a) | datepart(weekday, a) - 1 | - | 获取a在周的第几天 |
| a.DayOfYear | dayofyear(a) | datepart(dayofyear, a) | - | 获取a在年的第几天 |
| a.Hour | hour(a) | datepart(hour, a) | - | 小时 |
| a.Millisecond | floor(microsecond(a) / 1000) | datepart(millisecond, a) | - | 毫秒 |
| a.Minute | minute(a) | datepart(minute, a) | - | 分钟 |
| a.Month | month(a) | datepart(month, a) | - | 月 |
| a.Second | second(a) | datepart(second, a) | - | 秒 |
| a.Subtract(b) | timestampdiff(microsecond, b, a) | datediff(millisecond, b, a) * 1000 | - | 将a的值和b相减 |
| a.Ticks | timestampdiff(microsecond, '1970-1-1', a) * 10 + 621355968000000000 | datediff(millisecond, '1970-1-1', a) * 10000 + 621355968000000000 | - | 刻度总数 |
| a.TimeOfDay | timestampdiff(microsecond, date_format(a, '1970-1-1 %H:%i:%s.%f'), a) + 62135596800000000 | '1970-1-1 ' + convert(varchar, a, 14) | - | 获取a的时间部分 |
| a.Year | year(a) | datepart(year, a) | - | 年 |
| a.Equals(b) | a = b | a = b | - | 比较a和b相等 |
| a.CompareTo(b) | a - b | a - b | - | 比较a和b大小 |
| a.ToString() | date_format(a, '%Y-%m-%d %H:%i:%s.%f') | convert(varchar, a, 121) | - | 转换字符串 |

### 时间对象
| 表达式 | MySql | SqlServer | PostgreSQL | 功能说明 |
| - | - | - | - | - |
| TimeSpan.Zero | 0 | 0 | - | 0微秒 |
| TimeSpan.MaxValue | 922337203685477580 | 922337203685477580 | - | 最大微秒时间 |
| TimeSpan.MinValue | -922337203685477580 | -922337203685477580 | - | 最小微秒时间 |
| TimeSpan.Compare(a, b) | a - b | a - b | - | 比较a和b的大小 |
| TimeSpan.Equals(a, b) | a = b | a = b | - | 比较a和b相等 |
| TimeSpan.FromDays(a) | a * 1000000 * 60 * 60 * 24 | a * 1000000 * 60 * 60 * 24 | - | a天的微秒值 |
| TimeSpan.FromHours(a) | a * 1000000 * 60 * 60 | a * 1000000 * 60 * 60 | - | a小时的微秒值 |
| TimeSpan.FromMilliseconds(a) | a * 1000 | a * 1000 | - | a毫秒的微秒值 |
| TimeSpan.FromMinutes(a) | a * 1000000 * 60 | a * 1000000 * 60 | - | a分钟的微秒值 |
| TimeSpan.FromSeconds(a) | a * 1000000 | a * 1000000 | - | a秒钟的微秒值 |
| TimeSpan.FromTicks(a) | a / 10 | a / 10 | - | a刻度的毫秒值 |
| a.Add(b) | a + b | a + b | - | 增加值 |
| a.Subtract(b) | a - b | a - b | - | 将a的值和b相减 |
| a.CompareTo(b) | a - b | a - b | - | 比较a和b大小 |
| a.Days | a div (1000000 * 60 * 60 * 24) | a div (1000000 * 60 * 60 * 24) | - | 天数部分 |
| a.Hours | a div (1000000 * 60 * 60) mod 24 | a div (1000000 * 60 * 60) mod 24 | - | 小时部分 |
| a.Milliseconds | a div 1000 mod 1000 | a div 1000 mod 1000 | - | 毫秒部分 |
| a.Seconds | a div 1000000 mod 60 | a div 1000000 mod 60 | - | 秒数部分 |
| a.Ticks | a * 10 | a * 10 | - | 刻度总数 |
| a.TotalDays | a / (1000000 * 60 * 60 * 24) | a / (1000000 * 60 * 60 * 24) | - | 总天数(含小数) |
| a.TotalHours | a / (1000000 * 60 * 60) | a / (1000000 * 60 * 60) | - | 总小时(含小数) |
| a.TotalMilliseconds | a / 1000 | a / 1000 | - | 总毫秒(含小数) |
| a.TotalMinutes | a / (1000000 * 60) | a / (1000000 * 60) | - | 总分钟(含小数) |
| a.TotalSeconds | a / 1000000 | a / 1000000 | - | 总秒数(含小数) |
| a.Equals(b) | a = b | a = b | - | 比较a和b相等 |
| a.ToString() | cast(a as varchar) | cast(a as varchar) | - | 转换字符串 |

### 数学函数
| 表达式 | MySql | SqlServer | PostgreSQL | 功能说明 |
| - | - | - | - | - |
| Math.Abs(a) | abs(a) | abs(a) | - | - |
| Math.Acos(a) | acos(a) | acos(a) | - | - |
| Math.Asin(a) | asin(a) | asin(a) | - | - |
| Math.Atan(a) | atan(a) | atan(a) | - | - |
| Math.Atan2(a, b) | atan2(a, b) | atan2(a, b) | - | - |
| Math.Ceiling(a) | ceiling(a) | ceiling(a) | - | - |
| Math.Cos(a) | cos(a) | cos(a) | - | - |
| Math.Exp(a) | exp(a) | exp(a) | - | - |
| Math.Floor(a) | floor(a) | floor(a) | - | - |
| Math.Log(a) | log(a) | log(a) | - | - |
| Math.Log10(a) | log10(a) | log10(a) | - | - |
| Math.PI(a) | 3.1415926535897931 | 3.1415926535897931 | - | - |
| Math.Pow(a, b) | pow(a, b) | power(a, b) | - | - |
| Math.Round(a, b) | round(a, b) | round(a, b) | - | - |
| Math.Sign(a) | sign(a) | sign(a) | - | - |
| Math.Sin(a) | sin(a) | sin(a) | - | - |
| Math.Sqrt(a) | sqrt(a) | sqrt(a) | - | - |
| Math.Tan(a) | tan(a) | tan(a) | - | - |
| Math.Truncate(a) | truncate(a, 0) | floor(a) | - | - |
