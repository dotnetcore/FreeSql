using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Xunit;

namespace FreeSql.Tests.AdoNetExtensions.SqlConnectionExtensions
{
    [Collection("AdoNetExtensions")]
	public class Methods : IDisposable
    {

		string _connectString = "Data Source=.;Integrated Security=True;Initial Catalog=issues684;Pooling=true;Max Pool Size=3;TrustServerCertificate=true";

		public Methods() {
			g.sqlserver.CodeFirst.SyncStructure<TestConnectionExt>();

            FreeSql.AdoNetExtensions.AdoNetFreeSqlCreated += AdoNetExtensions_AdoNetFreeSqlCreated;
        }

        public void Dispose()
        {
            FreeSql.AdoNetExtensions.AdoNetFreeSqlCreated -= AdoNetExtensions_AdoNetFreeSqlCreated;
        }

        private static int _adoNetFreeSqlCreatedCount;

        private static void AdoNetExtensions_AdoNetFreeSqlCreated(object sender, AdoNetFreeSqlCreatedEventArgs e)
		{
            Assert.True(sender is SqlConnection, sender.GetType().FullName);
			Assert.Contains("SqlServerProvider", e.FreeSql.GetType().FullName);

            _adoNetFreeSqlCreatedCount++;
            
            Assert.Equal(1, _adoNetFreeSqlCreatedCount);
		}

        [Fact]
		public void Insert() {
			var affrows = 0;
			using (var conn = new SqlConnection(_connectString)) {
				var item = new TestConnectionExt { title = "testinsert" };
				affrows = conn.Insert<TestConnectionExt>().AppendData(item).ExecuteAffrows();
				conn.Close();
			}
			Assert.Equal(1, affrows);
		}
		[Fact]
		public void InsertOrUpdate()
		{
			var affrows = 0;
			using (var conn = new SqlConnection(_connectString))
			{
				var item = new TestConnectionExt { title = "testinsert" };
				affrows = conn.Insert<TestConnectionExt>().AppendData(item).ExecuteAffrows();
				Assert.Equal(1, affrows);
				item.title = "testinsertorupdate";
				var affrows2 = conn.InsertOrUpdate<TestConnectionExt>().SetSource(item).ExecuteAffrows();
				conn.Close();
			}
			Assert.Equal(1, affrows);
		}
		[Fact]
		public void Update() {
			var affrows = 0;
			using (var conn = new SqlConnection(_connectString)) {
				var item = new TestConnectionExt { title = "testupdate" };
				affrows = conn.Insert<TestConnectionExt>().AppendData(item).ExecuteAffrows();
				Assert.Equal(1, affrows);
				item = conn.Select<TestConnectionExt>().First();
				affrows = conn.Update<TestConnectionExt>().SetSource(item).Set(a => a.title, "testupdated").ExecuteAffrows();
				conn.Close();
			}
			Assert.Equal(1, affrows);
		}
		[Fact]
		public void Delete() {
			var affrows = 0;
			using (var conn = new SqlConnection(_connectString)) {
				var item = new TestConnectionExt { title = "testdelete" };
				affrows = conn.Insert<TestConnectionExt>().AppendData(item).ExecuteAffrows();
				Assert.Equal(1, affrows);
				affrows = conn.Delete<TestConnectionExt>().Where(item).ExecuteAffrows();
				conn.Close();
			}
			Assert.Equal(1, affrows);
		}
		[Fact]
		public void Select() {
			var list = new List<TestConnectionExt>();
			var affrows = 0;
			using (var conn = new SqlConnection(_connectString)) {
				var item = new TestConnectionExt { title = "testselect" };
				affrows = conn.Insert<TestConnectionExt>().AppendData(item).ExecuteAffrows();
				Assert.Equal(1, affrows);
				list = conn.Select<TestConnectionExt>().Where(a => a.id == item.id).ToList();
				conn.Close();
			}
			Assert.Single(list);
		}

		class TestConnectionExt {
			public Guid id { get; set; }
			public string title { get; set; }
			public DateTime createTime { get; set; } = DateTime.Now;
		}
	}
}
