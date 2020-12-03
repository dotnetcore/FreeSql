using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace FreeSql
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 该对象 Select/Delete/Insert/Update/InsertOrUpdate 与工作单元事务保持一致，可省略传递 WithTransaction
        /// </summary>
        IFreeSql Orm { get; }

        /// <summary>
        /// 开启事务，或者返回已开启的事务
        /// </summary>
        /// <param name="isCreate">若未开启事务，则开启</param>
        /// <returns></returns>
        DbTransaction GetOrBeginTransaction(bool isCreate = true);

        IsolationLevel? IsolationLevel { get; set; }

        void Commit();

        void Rollback();

        /// <summary>
        /// 工作单元内的实体变化跟踪
        /// </summary>
        DbContext.EntityChangeReport EntityChangeReport { get; }
    }
}
