using FreeSql.DatabaseModel;
using System;
using System.Collections.Generic;

namespace FreeSql
{
    public interface IDbFirst
    {

        /// <summary>
        /// 获取所有数据库
        /// </summary>
        /// <returns></returns>
        List<string> GetDatabases();
        /// <summary>
        /// 获取指定数据库的表信息，包括表、列详情、主键、唯一键、索引、外键、备注
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        List<DbTableInfo> GetTablesByDatabase(params string[] database);

        /// <summary>
        /// 获取数据库枚举类型int值
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        int GetDbType(DbColumnInfo column);

        /// <summary>
        /// 获取c#转换，(int)、(long)
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        string GetCsConvert(DbColumnInfo column);
        /// <summary>
        /// 获取c#值
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        string GetCsTypeValue(DbColumnInfo column);
        /// <summary>
        /// 获取c#类型，int、long
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        string GetCsType(DbColumnInfo column);
        /// <summary>
        /// 获取c#类型对象
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        Type GetCsTypeInfo(DbColumnInfo column);
        /// <summary>
        /// 获取ado.net读取方法, GetBoolean、GetInt64
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        string GetDataReaderMethod(DbColumnInfo column);
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        string GetCsStringify(DbColumnInfo column);
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        string GetCsParse(DbColumnInfo column);

        /// <summary>
        /// 获取数据库枚举类型，适用 PostgreSQL
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        List<DbEnumInfo> GetEnumsByDatabase(params string[] database);
    }
}
