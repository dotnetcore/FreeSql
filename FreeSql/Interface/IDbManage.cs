//using FreeSql.DatabaseModel;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace FreeSql {
//	public interface IDbManage {

//		/// <summary>
//		/// 判断表是否存在
//		/// </summary>
//		/// <param name="name">表名</param>
//		/// <returns></returns>
//		bool ExistsTable(string name);
//		/// <summary>
//		/// 判断列是否存在
//		/// </summary>
//		/// <param name="table">表名</param>
//		/// <param name="column">列名</param>
//		/// <returns></returns>
//		bool ExistsColumn(string table, string column);

//		/// <summary>
//		/// 判断视图是否存在
//		/// </summary>
//		/// <param name="name">视图名</param>
//		/// <returns></returns>
//		bool ExistsView(string name);
//		/// <summary>
//		/// 判断列是否存在
//		/// </summary>
//		/// <param name="view">视图名</param>
//		/// <param name="column">列名</param>
//		/// <returns></returns>
//		bool ExistsColumnByView(string view, string column);

//		/// <summary>
//		/// 获取表信息，包括表、列详情、主键、唯一键、索引、备注（注意：本方法不返回外键）
//		/// </summary>
//		/// <param name="name">表名</param>
//		/// <returns></returns>
//		DbTableInfo GetTableInfo(string name);
//		/// <summary>
//		/// 获取视图信息，包括表、列详情
//		/// </summary>
//		/// <param name="name">视图名</param>
//		/// <returns></returns>
//		DbTableInfo GetViewInfo(string name);

//		/// <summary>
//		/// 获取指定数据库的表信息，包括表、列详情、主键、唯一键、索引、外键、备注
//		/// </summary>
//		/// <param name="database"></param>
//		/// <returns></returns>
//		List<DbTableInfo> GetTablesByDatabase(params string[] database);
//	}
//}
