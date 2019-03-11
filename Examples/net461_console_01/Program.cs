using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net46_console_01 {
	class Program {
		static void Main(string[] args) {

			var orm = new FreeSql.FreeSqlBuilder()
				.UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Pooling=true;Max Pool Size=10")
				//.UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10")
				.UseAutoSyncStructure(true)
				.UseConfigEntityFromDbFirst(true)
				.Build();

			var repos = orm.GetGuidRepository<Song22>();

			var item = repos.Insert(new Song22());
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

			item.Title = "xxx";
			repos.Update(item);
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

			Console.WriteLine(repos.UpdateDiy.Where(a => a.Id == item.Id).Set(a => a.Clicks + 1).ToSql());
			repos.UpdateDiy.Where(a => a.Id == item.Id).Set(a => a.Clicks + 1).ExecuteAffrows();

			item = repos.Find(item.Id);
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));
		}
	}

	public class Song22 {

		public Guid Id { get; set; }
		public string Title { get; set; }

		public int Clicks { get; set; } = 10;
	}
}
