using System;

namespace FreeSql {
	public interface ICodeFirst {

		/// <summary>
		/// 【开发环境必备】自动同步实体结构到数据库，程序运行中检查实体表是否存在，然后创建或修改
		/// </summary>
		bool IsAutoSyncStructure { get; set; }

		/// <summary>
		/// 转小写同步结构
		/// </summary>
		bool IsSyncStructureToLower { get; set; }
		/// <summary>
		/// 延时加载导航属性对象，导航属性需要声明 virtual
		/// </summary>
		bool IsLazyLoading { get; set; }

		/// <summary>
		/// 将实体类型与数据库对比，返回DDL语句
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <returns></returns>
		string GetComparisonDDLStatements<TEntity>();
		/// <summary>
		/// 将实体类型集合与数据库对比，返回DDL语句
		/// </summary>
		/// <param name="entityTypes"></param>
		/// <returns></returns>
		string GetComparisonDDLStatements(params Type[] entityTypes);
		/// <summary>
		/// 同步实体类型到数据库
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <returns></returns>
		bool SyncStructure<TEntity>();
		/// <summary>
		/// 同步实体类型集合到数据库
		/// </summary>
		/// <param name="entityTypes"></param>
		/// <returns></returns>
		bool SyncStructure(params Type[] entityTypes);

		/// <summary>
		/// 根据 System.Type 获取数据库信息
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		(int type, string dbtype, string dbtypeFull, bool? isnullable, object defaultValue)? GetDbInfo(Type type);
	}
}
