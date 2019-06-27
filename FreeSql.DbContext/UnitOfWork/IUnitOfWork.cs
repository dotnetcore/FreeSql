using System;
using System.Data;
using System.Data.Common;

namespace FreeSql
{
    public interface IUnitOfWork : IDisposable
    {

        DbTransaction GetOrBeginTransaction(bool isCreate = true);

        IsolationLevel? IsolationLevel { get; set; }

        /// <summary>
        /// 是否启用工作单元
        /// </summary>
        bool Enable { get; }

        void Commit();

        void Rollback();

        /// <summary>
        /// 禁用工作单元
        /// <exception cref="Exception"></exception>
        /// <para></para>
        /// 若已开启事务（已有Insert/Update/Delete操作），调用此方法将发生异常，建议在执行逻辑前调用
        /// </summary>
        void Close();

        /// <summary>
        /// 开启工作单元
        /// </summary>
        void Open();
    }
}
