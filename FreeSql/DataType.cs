using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql
{
    public enum DataType {

        MySql, SqlServer, PostgreSQL, Oracle, Sqlite,

        OdbcOracle, OdbcSqlServer, OdbcMySql, OdbcPostgreSQL,

        /// <summary>
        /// 通用的 Odbc 访问数据库 https://freesql.net/guide/freesql-provider-odbc.html
        /// </summary>
        Odbc = 9,

        //OdbcDameng,

        /// <summary>
        /// Microsoft Office Access 是由微软发布的关联式数据库管理系统
        /// </summary>
        MsAccess = 11,

        /// <summary>
        /// 武汉达梦数据库有限公司，基于 DmProvider.dll 的实现
        /// </summary>
        Dameng,

        //OdbcKingbaseES,

        /// <summary>
        ///  天津神舟通用数据技术有限公司，基于 System.Data.OscarClient.dll 的实现
        /// </summary>
        ShenTong = 14,

        /// <summary>
        /// 北京人大金仓信息技术股份有限公司，基于 Kdbndp.dll 的实现
        /// </summary>
        KingbaseES,

        /// <summary>
        /// Firebird 是一个跨平台的关系数据库，能作为多用户环境下的数据库服务器运行，也提供嵌入式数据库的实现
        /// </summary>
        Firebird,

        /// <summary>
        /// 自定义适配器，访问任何数据库 https://freesql.net/guide/freesql-provider-custom.html
        /// </summary>
        Custom,

        ClickHouse,

        /// <summary>
        /// 天津南大通用数据技术股份有限公司成立于2004年,是国产数据库、大数据领域的知名企业，基于 Odbc 的实现
        /// </summary>
        GBase,

        QuestDb,

        /// <summary>
        /// 虚谷
        /// </summary>
        Xugu,

        CustomOracle, CustomSqlServer, CustomMySql, CustomPostgreSQL,

        DuckDB,

        TDengine
    }
}
