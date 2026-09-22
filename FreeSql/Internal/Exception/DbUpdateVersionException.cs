using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace FreeSql.Internal
{
    public class DbUpdateVersionException : Exception
    {
        public DbUpdateVersionException(string message, TableInfo table, string sql, DbParameter[] dbParms, int affrows, IEnumerable<object> source) : 
            base(message)
        {
            this.Table = table;
            this.Sql = sql;
            this.DbParams = DbParams;
            this.Affrows = affrows;
            this.EntitySource = source;
            this.EntitySourceCount = source.Count();
        }

        /// <summary>
        /// 更新实体的元数据
        /// </summary>
        public TableInfo Table { get; }
        /// <summary>
        /// 执行更新的 SQL
        /// </summary>
        public string Sql { get; }
        /// <summary>
        /// 执行更新命令的参数
        /// </summary>
        public DbParameter[] DbParams { get; }

        /// <summary>
        /// 执行更新命令影响的行
        /// </summary>
        public int Affrows { get; }
        /// <summary>
        /// 更新的实体数量
        /// </summary>
        public int EntitySourceCount { get; }

        /// <summary>
        /// 更新的实体
        /// </summary>
        public IEnumerable<object> EntitySource { get; }
    }
}
