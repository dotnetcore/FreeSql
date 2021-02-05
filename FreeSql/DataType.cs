using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql
{
    public enum DataType {

        MySql, SqlServer, PostgreSQL, Oracle, Sqlite,

        OdbcOracle, OdbcSqlServer, OdbcMySql, OdbcPostgreSQL, 

        /// <summary>
        /// 通用的 Odbc 实现，只能做基本的 Crud 操作 <para></para>
        /// 不支持实体结构迁移、不支持分页（只能 Take 查询） <para></para>
        /// 
        /// 通用实现为了让用户自己适配更多的数据库，比如连接 mssql 2000、db2 等数据库<para></para>
        /// 默认适配 SqlServer，可以继承后重新适配 FreeSql.Odbc.Default.OdbcAdapter，最好去看下代码 <para></para>
        /// 
        /// 适配新的 OdbcAdapter，请在 FreeSqlBuilder.Build 之后调用 IFreeSql.SetOdbcAdapter 方法设置
        /// </summary>
        Odbc,

        /// <summary>
        /// 武汉达梦数据库有限公司，基于 Odbc 的实现
        /// </summary>
        OdbcDameng,

        /// <summary>
        /// Microsoft Office Access 是由微软发布的关联式数据库管理系统
        /// </summary>
        MsAccess,

        /// <summary>
        /// 武汉达梦数据库有限公司，基于 DmProvider.dll 的实现
        /// </summary>
        Dameng,

        /// <summary>
        /// 北京人大金仓信息技术股份有限公司，基于 Odbc 的实现
        /// </summary>
        OdbcKingbaseES,

        /// <summary>
        ///  天津神舟通用数据技术有限公司，基于 System.Data.OscarClient.dll 的实现
        /// </summary>
        ShenTong,

        /// <summary>
        /// 北京人大金仓信息技术股份有限公司，基于 Kdbndp.dll 的实现
        /// </summary>
        KingbaseES,

        /// <summary>
        /// Firebird 是一个跨平台的关系数据库，能作为多用户环境下的数据库服务器运行，也提供嵌入式数据库的实现
        /// </summary>
        Firebird,

        /// <summary>
        /// 自定义适配器，访问任何数据库<para></para>
        /// 注意：该类型不提供 DbFirst/CodeFirst 功能
        /// </summary>
        Custom
    }
}
