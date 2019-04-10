using FreeSql.DataAnnotations;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace FreeSql.Tests.MySql {
	public class MySqlConnectionExtensions {

		string _connectString = "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10";

		[Fact]
		public void Insert() {
			var affrows = 0;
			using (var conn = new MySqlConnection(_connectString)) {
				var item = new TestConnectionExt { title = "testinsert" };
				affrows = conn.Insert<TestConnectionExt>().AppendData(item).ExecuteAffrows();
				conn.Close();
			}
			Assert.Equal(1, affrows);
		}
		[Fact]
		public void Update() {
			var affrows = 0;
			using (var conn = new MySqlConnection(_connectString)) {
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
			using (var conn = new MySqlConnection(_connectString)) {
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
			using (var conn = new MySqlConnection(_connectString)) {
				var item = new TestConnectionExt { title = "testselect" };
				affrows = conn.Insert<TestConnectionExt>().AppendData(item).ExecuteAffrows();
				Assert.Equal(1, affrows);
				list = conn.Select<TestConnectionExt>().Where(a => a.id == item.id).ToList();
				conn.Close();
			}
			Assert.Single(list);
		}

		[Fact]
		public void Query() {
			var list = new List<TestConnectionExt>();
			var affrows = 0;
			using (var conn = new MySqlConnection(_connectString)) {
				var item = new TestConnectionExt { title = "testquery" };
				affrows = conn.Insert<TestConnectionExt>().AppendData(item).ExecuteAffrows();
				Assert.Equal(1, affrows);
				list = conn.Query<TestConnectionExt>("select * from TestConnectionExt where id = ?id", new { id = item.id });
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
