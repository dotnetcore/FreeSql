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
        /// 是否启用工作单元
        /// </summary>
        [Obsolete("即将删除（保留到2020-12-01），请改用 UnitOfWorkManager 的方式管理事务 https://github.com/dotnetcore/FreeSql/issues/289")] 
        bool Enable { get; }

        /// <summary>
        /// 禁用工作单元
        /// <exception cref="Exception"></exception>
        /// <para></para>
        /// 若已开启事务（已有Insert/Update/Delete操作），调用此方法将发生异常，建议在执行逻辑前调用
        /// </summary>
        [Obsolete("即将删除（保留到2020-12-01），请改用 UnitOfWorkManager 的方式管理事务 https://github.com/dotnetcore/FreeSql/issues/289")]
        void Close();

        /// <summary>
        /// 开启工作单元
        /// </summary>
        [Obsolete("即将删除（保留到2020-12-01），请改用 UnitOfWorkManager 的方式管理事务 https://github.com/dotnetcore/FreeSql/issues/289")]
        void Open();

        /// <summary>
        /// 工作单元内的实体变化跟踪
        /// </summary>
        DbContext.EntityChangeReport EntityChangeReport { get; }
    }
}
