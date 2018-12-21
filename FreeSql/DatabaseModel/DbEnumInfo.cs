using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.DatabaseModel {
	public class DbEnumInfo {

		/// <summary>
		/// 枚举类型标识
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 枚举项
		/// </summary>
		public Dictionary<string, string> Labels { get; set; }
	}
}
