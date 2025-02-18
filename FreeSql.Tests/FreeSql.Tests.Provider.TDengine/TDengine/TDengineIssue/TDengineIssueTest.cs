using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace FreeSql.Tests.Provider.TDengine.TDengine.TDengineIssue
{
    public class TDengineIssueTest
    {
        private IFreeSql _fsql;
        private ITestOutputHelper _output;

        public TDengineIssueTest(ITestOutputHelper output)
        {
            _fsql = g.tdengine;
            _output = output;
        }

        [Fact]
        void CodeFirst1977()
        {
            _fsql.CodeFirst.SyncStructure(typeof(TDengineProcessMetrics1977));
        }


        [Fact]
        void SelectTest1977()
        {
            var data = _fsql.Select<TDengineProcessMetrics1977>()
                .ToList();
            _output.WriteLine(JsonConvert.SerializeObject(data));
        }

        [Fact]
        void InsertTest1977()
        {
            var insertAffrows = _fsql.Insert(new TDengineProcessMetrics1977()
                {
                    Timestamp = DateTime.Now,
                    HostName = "host6"
                }
            ).ExecuteAffrows();
            Assert.Equal(1, insertAffrows);
        }

    }

    public class TDengineProcessMetrics1977
    {
        /// <summary>
        /// 数据时间戳
        /// </summary>
        [Column(Name = "ts")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 主机名
        /// </summary>
        [Column(Name = "host_name")]
        public string HostName { get; set; }

        /// <summary>
        /// 进程启动时间
        /// </summary>
        [Column(Name = "start_time")]
        public DateTime StartTime { get; set; }
    }
}