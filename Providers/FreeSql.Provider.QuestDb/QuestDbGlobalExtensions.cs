using CsvHelper;
using FreeSql;
using FreeSql.Provider.QuestDb;
using FreeSql.Provider.QuestDb.Models;
using FreeSql.QuestDb;
using FreeSql.QuestDb.Curd;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static partial class QuestDbGlobalExtensions
{
    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatQuestDb(this string that, params object[] args) =>
        _questDbAdo.Addslashes(that, args);

    private static readonly QuestDbAdo _questDbAdo = new QuestDbAdo();

    /// <summary>
    /// 启动QuestDb Http功能
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="host"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static FreeSqlBuilder UseQuestDbRestAPI(this FreeSqlBuilder builder, string host, string username = "",
        string password = "")
    {
        //初始化容器，添加HttpClient
        ServiceContainer.Initialize(service =>
        {
            var apiFeatures = new QuestResetApiFeatures
            {
                BaseAddress = host.StartsWith("http") ? host : $"http://{host}"
            };

            service.AddHttpClient("QuestDb", client => client.BaseAddress = new Uri(apiFeatures.BaseAddress))
                .ConfigurePrimaryHttpMessageHandler(handlerBuilder =>
                {
                    //忽略SSL验证
                    return new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual,
                        ServerCertificateCustomValidationCallback =
                            (httpRequestMessage, cert, certChain, policyErrors) => true
                    };
                });

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                apiFeatures.BasicToken = $"Basic {base64}";
            }

            service.AddSingleton(apiFeatures);
        });

        //RestApi需要无参数
        builder.UseNoneCommandParameter(true);

        return builder;
    }

    /// <summary>
    /// 对于多个时间序列存储在同一个表中的场景，根据时间戳检索给定键或键组合的最新项。
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="select"></param>
    /// <param name="timestamp">时间标识</param>
    /// <param name="partition">最新项的列</param>
    /// <returns></returns>
    public static ISelect<T1> LatestOn<T1, TKey>(this ISelect<T1> select, Expression<Func<T1, DateTime?>> timestamp,
        Expression<Func<T1, TKey>> partition)
    {
        LatestOnExtension.InternalImpl(timestamp, partition);
        return select;
    }

    /// <summary>
    /// 对于多个时间序列存储在同一个表中的场景，根据时间戳检索给定键或键组合的最新项。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="select"></param>
    /// <param name="timestamp">时间标识</param>
    /// <param name="partition">最新项的列</param>
    /// <returns></returns>
    public static ISelect<T1, T2> LatestOn<T1, T2, TKey>(this ISelect<T1, T2> select,
        Expression<Func<T1, DateTime?>> timestamp,
        Expression<Func<T1, TKey>> partition) where T2 : class
    {
        LatestOnExtension.InternalImpl(timestamp, partition);
        return select;
    }

    /// <summary>
    /// 对于多个时间序列存储在同一个表中的场景，根据时间戳检索给定键或键组合的最新项。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="select"></param>
    /// <param name="timestamp">时间标识</param>
    /// <param name="partition">最新项的列</param>
    /// <returns></returns>
    public static ISelect<T1, T2, T3> LatestOn<T1, T2, T3, TKey>(this ISelect<T1, T2, T3> select,
        Expression<Func<T1, DateTime?>> timestamp,
        Expression<Func<T1, TKey>> partition) where T2 : class where T3 : class
    {
        LatestOnExtension.InternalImpl(timestamp, partition);
        return select;
    }

    /// <summary>
    /// 对于多个时间序列存储在同一个表中的场景，根据时间戳检索给定键或键组合的最新项。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="select"></param>
    /// <param name="timestamp">时间标识</param>
    /// <param name="partition">最新项的列</param>
    /// <returns></returns>
    public static ISelect<T1, T2, T3, T4> LatestOn<T1, T2, T3, T4, TKey>(this ISelect<T1, T2, T3, T4> select,
        Expression<Func<T1, DateTime?>> timestamp,
        Expression<Func<T1, TKey>> partition) where T2 : class where T3 : class where T4 : class
    {
        LatestOnExtension.InternalImpl(timestamp, partition);
        return select;
    }

    /// <summary>
    /// SAMPLE BY用于时间序列数据，将大型数据集汇总为同质时间块的聚合，作为SELECT语句的一部分。对缺少数据的数据集执行查询的用户可以使用FILL关键字指定填充行为
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="select"></param>
    /// <param name="time">时长</param>
    /// <param name="unit">单位</param>
    /// <param name="alignToCalendar">对准日历</param>
    /// <returns></returns>
    public static ISelect<T> SampleBy<T>(this ISelect<T> select, double time, SampleUnit unit,
        bool alignToCalendar = false)
    {
        SampleByExtension.IsExistence.Value = true;
        var samoleByTemple = $"{Environment.NewLine}SAMPLE BY {{0}}{{1}} {{2}}";
        string alignToCalendarTemple = "";
        if (alignToCalendar) alignToCalendarTemple = "ALIGN TO CALENDAR ";
        SampleByExtension.SamoleByString.Value =
            string.Format(samoleByTemple, time.ToString(), (char)unit, alignToCalendarTemple);
        return select;
    }


    /// <summary>
    /// 逐行读取，包含空行
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static List<string> SplitByLine(string text)
    {
        List<string> lines = new List<string>();
        byte[] array = Encoding.UTF8.GetBytes(text);
        using (MemoryStream stream = new MemoryStream(array))
        {
            using (var sr = new StreamReader(stream))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    lines.Add(line);
                    line = sr.ReadLine();
                }
            }
        }

        return lines;
    }

    /// <summary>
    /// 批量快速插入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="dateFormat">导入时，时间格式 默认:yyyy/M/d H:mm:ss</param>
    /// <returns></returns>
    public static async Task<int> ExecuteQuestDbBulkCopyAsync<T>(this IInsert<T> that,
        string dateFormat = "yyyy/M/d H:mm:ss") where T : class
    {
        var features = ServiceContainer.GetService<QuestResetApiFeatures>();

        if (string.IsNullOrWhiteSpace(features.BaseAddress))
        {
            throw new Exception(
                @"BulkCopy功能需要启用RestAPI，启用方式：new FreeSqlBuilder().UseQuestDbRestAPI(""localhost:9000"", ""username"", ""password"")");
        }

        var result = 0;

        try
        {
            var boundary = $"---------------{DateTime.Now.Ticks:x}";
            var list = new List<Hashtable>();
            var insert = that as QuestDbInsert<T>;
            var name = insert.InternalTableRuleInvoke(); //获取表名
            insert.InternalOrm.DbFirst.GetTableByName(name).Columns.ForEach(d =>
            {
                if (d.DbTypeText == "TIMESTAMP")
                {
                    list.Add(new Hashtable()
                    {
                        { "name", d.Name },
                        { "type", d.DbTypeText },
                        { "pattern", dateFormat }
                    });
                }
                else
                {
                    list.Add(new Hashtable()
                    {
                        { "name", d.Name },
                        { "type", d.DbTypeText }
                    });
                }
            });
            var schema = JsonConvert.SerializeObject(list);
            using (var stream = new MemoryStream())
            {
                //写入CSV文件
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
                {
                    await csv.WriteRecordsAsync(insert._source);
                }

                var client = features.HttpClient;
                var httpContent = new MultipartFormDataContent(boundary);
                if (!string.IsNullOrWhiteSpace(features.BasicToken))
                    client.DefaultRequestHeaders.Add("Authorization", features.BasicToken);
                httpContent.Add(new StringContent(schema), "schema");
                httpContent.Add(new ByteArrayContent(stream.ToArray()), "data");
                httpContent.Headers.Remove("Content-Type");
                httpContent.Headers.TryAddWithoutValidation("Content-Type",
                    $"multipart/form-data; boundary={boundary}");
                var httpResponseMessage =
                    await client.PostAsync($"imp?name={name}", httpContent);
                var readAsStringAsync = await httpResponseMessage.Content.ReadAsStringAsync();
                var splitByLine = SplitByLine(readAsStringAsync);
                foreach (var strings in from s in splitByLine
                         where s.Contains("Rows")
                         select s.Split('|')
                         into strings
                         where strings[1].Trim() == "Rows imported"
                         select strings)
                {
                    result = Convert.ToInt32(strings[2].Trim());
                }
            }
        }
        catch (Exception e)
        {
            throw e;
        }

        return result;
    }

    /// <summary>
    /// 批量快速插入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="insert"></param>
    /// <param name="dateFormat">导入时，时间格式 默认:yyyy/M/d H:mm:ss</param>
    /// <returns></returns>
    public static int ExecuteQuestDbBulkCopy<T>(this IInsert<T> insert, string dateFormat = "yyyy/M/d H:mm:ss")
        where T : class
    {
        return ExecuteQuestDbBulkCopyAsync(insert, dateFormat).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}

static class SampleByExtension
{
    //是否使用该方法
    internal static AsyncLocal<bool> IsExistence = new AsyncLocal<bool>()
    {
        Value = false
    };

    internal static AsyncLocal<string> SamoleByString = new AsyncLocal<string>()
    {
        Value = string.Empty
    };

    internal static void Initialize()
    {
        IsExistence.Value = false;
        SamoleByString.Value = string.Empty;
    }
}

static class LatestOnExtension
{
    //是否使用该方法
    internal static AsyncLocal<bool> IsExistence = new AsyncLocal<bool>()
    {
        Value = false
    };

    internal static AsyncLocal<string> LatestOnString = new AsyncLocal<string>()
    {
        Value = string.Empty
    };

    internal static void Initialize()
    {
        IsExistence.Value = false;
        LatestOnString.Value = string.Empty;
    }

    internal static void InternalImpl<T1, TKey>(Expression<Func<T1, DateTime?>> timestamp,
        Expression<Func<T1, TKey>> partition)
    {
        IsExistence.Value = true;
        var latestOnTemple = $"{Environment.NewLine}LATEST ON {{0}} PARTITION BY {{1}} ";
        var expressionVisitor = new QuestDbExpressionVisitor();
        expressionVisitor.Visit(timestamp);
        var _timestamp = expressionVisitor.Fields();
        expressionVisitor.Visit(partition);
        var _partition = expressionVisitor.Fields();
        LatestOnString.Value = string.Format(latestOnTemple, _timestamp, _partition);
    }
}