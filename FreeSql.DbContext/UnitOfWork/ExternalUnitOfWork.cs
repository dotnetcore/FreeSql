using System;
using System.Data.Common;
using System.Linq;

namespace FreeSql
{
    /// <summary>
    /// 外部事务适配的工作单元
    /// <para>用于适配第三方事务（如 EFCore、ADO.NET 原生事务）给 FreeSql 使用</para>
    /// </summary>
    public class ExternalUnitOfWork : UnitOfWork, IUnitOfWork
    {
        private readonly DbTransaction _externalTran;

        public ExternalUnitOfWork(IFreeSql fsql, DbTransaction externalTran) : base(fsql)
        {
            if (externalTran == null) throw new ArgumentNullException(nameof(externalTran));
            _externalTran = externalTran;
            this._tran = externalTran;
        }

        #region IUnitOfWork 显式接口实现 (拦截基类行为)

        DbTransaction IUnitOfWork.GetOrBeginTransaction(bool isCreate)
        {
            return _externalTran;
        }

        void IUnitOfWork.Commit()
        {
            try
            {
                _fsql?.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(_tranBefore, "提交(外部)", null));

                if (EntityChangeReport != null && EntityChangeReport.OnChange != null && EntityChangeReport.Report.Any())
                    EntityChangeReport.OnChange.Invoke(EntityChangeReport.Report);
            }
            finally
            {
                EntityChangeReport?.Report.Clear();
            }
        }

        void IUnitOfWork.Rollback()
        {
            try
            {
                _fsql?.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(_tranBefore, "回滚(外部)", null));
            }
            finally
            {
                EntityChangeReport?.Report.Clear();
            }
        }

        void IDisposable.Dispose()
        {
            // 不调用 _externalTran.Dispose()，因为生命周期归外部管理
            EntityChangeReport?.Report.Clear();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}