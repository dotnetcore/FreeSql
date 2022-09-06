using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Rougamo.Context;

namespace FreeSql
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TransactionalAttribute : Rougamo.MoAttribute
    {
        public Propagation Propagation { get; set; } = Propagation.Required;
        public IsolationLevel IsolationLevel { get => m_IsolationLevel.Value; set => m_IsolationLevel = value; }
        IsolationLevel? m_IsolationLevel;

        static AsyncLocal<IServiceProvider> m_ServiceProvider = new AsyncLocal<IServiceProvider>();
        public static void SetServiceProvider(IServiceProvider serviceProvider) => m_ServiceProvider.Value = serviceProvider;

        IUnitOfWork _uow;
        public override void OnEntry(MethodContext context)
        {
            var uowManager = m_ServiceProvider.Value.GetService(typeof(UnitOfWorkManager)) as UnitOfWorkManager;
            _uow = uowManager.Begin(this.Propagation, this.m_IsolationLevel);
        }
        public override void OnExit(MethodContext context)
        {
            if (typeof(Task).IsAssignableFrom(context.RealReturnType))
            {
                ((Task)context.ReturnValue).ContinueWith(t => _OnExit());
                return;
            }
            _OnExit();

            void _OnExit()
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
}
