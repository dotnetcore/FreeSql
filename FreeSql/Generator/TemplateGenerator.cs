using FreeSql.DatabaseModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Generator {
	public class TemplateGenerator {

		public void Build(IDbFirst dbfirst, string templateDirectory, string outputDirectory, params string[] database) {
			if (dbfirst == null) throw new ArgumentException("dbfirst 参数不能为 null");
			if (string.IsNullOrEmpty(templateDirectory) || Directory.Exists(templateDirectory) == false) throw new ArgumentException("templateDirectory 目录不存在");
			if (string.IsNullOrEmpty(templateDirectory)) throw new ArgumentException("outputDirectory 不能为 null");
			if (database == null || database.Any() == false) throw new ArgumentException("database 参数不能为空");
			if (Directory.Exists(outputDirectory) == false) Directory.CreateDirectory(outputDirectory);
			templateDirectory = new DirectoryInfo(templateDirectory).FullName;
			outputDirectory = new DirectoryInfo(outputDirectory).FullName;
			if (templateDirectory.IndexOf(outputDirectory, StringComparison.CurrentCultureIgnoreCase) != -1) throw new ArgumentException("outputDirectory 目录不能设置在 templateDirectory 目录内");
			var tables = dbfirst.GetTablesByDatabase(database);
			var tpl = new TemplateEngin(templateDirectory, "FreeSql", "FreeSql.DatabaseModel");
			BuildEachDirectory(templateDirectory, outputDirectory, tpl, dbfirst, tables);
			tpl.Dispose();
		}

		void BuildEachDirectory(string templateDirectory, string outputDirectory, TemplateEngin tpl, IDbFirst dbfirst, List<DbTableInfo> tables) {
			if (Directory.Exists(outputDirectory) == false) Directory.CreateDirectory(outputDirectory);
			var files = Directory.GetFiles(templateDirectory);
			foreach (var file in files) {
				var fi = new FileInfo(file);
				if (string.Compare(fi.Extension, ".FreeSql", true) == 0) {
					var outputExtension = "." + fi.Name.Split('.')[1];
					if (fi.Name.StartsWith("for-table.")) {
						foreach (var table in tables) {
							var result = tpl.RenderFile(file, new Dictionary<string, object>() { { "table", table }, { "dbfirst", dbfirst } });
							if (result.EndsWith("return;")) continue;
							var outputName = table.Name + outputExtension;
							var mcls = Regex.Match(result, @"\s+class\s+(\w+)");
							if (mcls.Success) outputName = mcls.Groups[1].Value + outputExtension;
							var outputStream = Encoding.UTF8.GetBytes(result);
							var fullname = outputDirectory + "/" + outputName;
							if (File.Exists(fullname)) File.Delete(fullname);
							using (var outfs = File.Open(fullname, FileMode.OpenOrCreate, FileAccess.Write)) {
								outfs.Write(outputStream, 0, outputStream.Length);
								outfs.Close();
							}
						}
						continue;
					} else {
						var result = tpl.RenderFile(file, new Dictionary<string, object>() { { "tables", tables }, { "dbfirst", dbfirst } });
						var outputName = fi.Name;
						var mcls = Regex.Match(result, @"\s+class\s+(\w+)");
						if (mcls.Success) outputName = mcls.Groups[1].Value + outputExtension;
						var outputStream = Encoding.UTF8.GetBytes(result);
						var fullname = outputDirectory + "/" + outputName;
						if (File.Exists(fullname)) File.Delete(fullname);
						using (var outfs = File.Open(fullname, FileMode.OpenOrCreate, FileAccess.Write)) {
							outfs.Write(outputStream, 0, outputStream.Length);
							outfs.Close();
						}
					}
				}
				File.Copy(file, outputDirectory + file.Replace(templateDirectory, ""), true);
			}
			var dirs = Directory.GetDirectories(templateDirectory);
			foreach(var dir in dirs) {
				BuildEachDirectory(dir, outputDirectory +  dir.Replace(templateDirectory, ""), tpl, dbfirst, tables);
			}
		}
	}
}
