using FreeSql.Tests.Provider.TDengine.TDengine.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace FreeSql.Tests.Provider.TDengine.TDengine.TDengineCrud;

public class TDengineBatchTest
{
    private readonly ITestOutputHelper _output;

    // 通过构造函数注入
    public TDengineBatchTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void BatchInsertToSuperTable()
    {
        var fsql = g.tdengine;
        fsql.CodeFirst.SyncStructure<PointDataEntity>();

        var generateCount = 2_000; // 生成数据数量
        var dt = DateTime.Now;
        var data = Enumerable.Range(1, generateCount)
            .Select(t => new PointDataEntity
            {
                Ts = dt,
                PointNumber = $"001A{t:D5}",
                PointValue = 345
            })
            .ToList();
        // 生成第二批数据，时间戳加1秒，IsAlarm为true
        var data2 = Enumerable.Range(1, generateCount)
            .Select(t => new PointDataEntity
            {
                Ts = dt.AddSeconds(1),
                PointNumber = $"001A{t:D5}",
                IsAlarm = true,
                PointValue = 123.45
            })
            .ToList();
        data.AddRange(data2);

        var insertable = fsql.Insert(data)
            .AsTable(t => $"child_{t.PointNumber}") // 自定义子表名
            .BatchOptions(5000, 5000, false); // tdengine 关闭事务

        //_output.WriteLine($"集合中总数量：{data.Count}");
        //var sql = insertable.ToSql();
        //_output.WriteLine(sql);

        var affrows = insertable.ExecuteAffrows();

        Assert.NotEqual(0, affrows);
    }
}
