using FreeSql.DataAnnotations;
using FreeSql.Internal.Model;
using System;

namespace FreeSql
{
    public interface ICodeFirst
    {

        /// <summary>
        /// 【开发环境必备】自动同步实体结构到数据库，程序运行中检查实体表是否存在，然后创建或修改
        /// </summary>
        bool IsAutoSyncStructure { get; set; }

        /// <summary>
        /// 转小写同步结构
        /// </summary>
        bool IsSyncStructureToLower { get; set; }
        /// <summary>
        /// 转大写同步结构
        /// </summary>
        bool IsSyncStructureToUpper { get; set; }
        /// <summary>
        /// 将数据库的主键、自增、索引设置导入，适用 DbFirst 模式，无须在实体类型上设置 [Column(IsPrimary)] 或者 ConfigEntity。此功能目前可用于 mysql/sqlserver/postgresql/oracle。<para></para>
        /// 本功能会影响 IFreeSql 首次访问的速度。<para></para>
        /// 若使用 CodeFirst 创建索引后，又直接在数据库上建了索引，若无本功能下一次 CodeFirst 迁移时数据库上创建的索引将被删除
        /// </summary>
        bool IsConfigEntityFromDbFirst { get; set; }
        /// <summary>
        /// 不使用命令参数化执行，针对 Insert/Update
        /// </summary>
        bool IsNoneCommandParameter { get; set; }
        /// <summary>
        /// 是否生成命令参数化执行，针对 lambda 表达式解析<para></para>
        /// 注意：常量不会参数化，变量才会做参数化<para></para>
        /// var id = 100;
        /// fsql.Select&lt;T&gt;().Where(a => a.id == id) 会参数化<para></para>
        /// fsql.Select&lt;T&gt;().Where(a => a.id == 100) 不会参数化
        /// </summary>
        bool IsGenerateCommandParameterWithLambda { get; set; }
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
        /// <param name="entityTypes">实体类型</param>
        /// <returns></returns>
        string GetComparisonDDLStatements(params Type[] entityTypes);
        /// <summary>
        /// 将实体类型与数据库对比，返回DDL语句（指定表名）
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="tableName">指定表名对比</param>
        /// <returns></returns>
        string GetComparisonDDLStatements(Type entityType, string tableName);
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
        /// 同步实体类型到数据库（指定表名）
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="tableName">指定表名对比</param>
        /// <returns></returns>
        bool SyncStructure(Type entityType, string tableName);

        /// <summary>
        /// 根据 System.Type 获取数据库信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        (int type, string dbtype, string dbtypeFull, bool? isnullable, object defaultValue)? GetDbInfo(Type type);
        /// <summary>
        /// 在外部配置实体的特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        ICodeFirst ConfigEntity<T>(Action<TableFluent<T>> entity);
        /// <summary>
        /// 在外部配置实体的特性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        ICodeFirst ConfigEntity(Type type, Action<TableFluent> entity);
        /// <summary>
        /// 获取在外部配置实体的特性
        /// </summary>
        /// <param name="type"></param>
        /// <returns>未使用ConfigEntity配置时，返回null</returns>
        TableAttribute GetConfigEntity(Type type);
        /// <summary>
        /// 获取实体类核心配置
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        TableInfo GetTableByEntity(Type type);
    }
}
