using FreeSql;
using Microsoft.AspNetCore.Mvc.Filters;
using Rougamo.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TransactionalAttribute : Rougamo.MoAttribute
    {
        public Propagation Propagation { get; set; } = Propagation.Required;
        public IsolationLevel IsolationLevel { get => m_IsolationLevel.Value; set => m_IsolationLevel = value; }
        IsolationLevel? m_IsolationLevel;

        static AsyncLocal<IServiceProvider> m_ServiceProvider = new AsyncLocal<IServiceProvider>();
        public static void SetServiceProvider(IServiceProvider serviceProvider) => 
            m_ServiceProvider.Value = serviceProvider;

        IUnitOfWork _uow;
        public override void OnEntry(MethodContext context)
        {
            var uowManager = m_ServiceProvider.Value.GetService(typeof(UnitOfWorkManager)) as UnitOfWorkManager;
            _uow = uowManager.Begin(this.Propagation, this.m_IsolationLevel);
        }
        public override void OnExit(MethodContext context)
        {
            try
            {
                if (context.Exception == null) _uow.Commit();
                else _uow.Rollback();
            }
            finally
            {
                _uow.Dispose();
            }
        }
    }
}
