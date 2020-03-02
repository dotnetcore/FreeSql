
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Internal.CommonProvider
{
    public class AopProvider : IAop
    {
        public event EventHandler<Aop.ParseExpressionEventArgs> ParseExpression;
        public event EventHandler<Aop.ConfigEntityEventArgs> ConfigEntity;
        public event EventHandler<Aop.ConfigEntityPropertyEventArgs> ConfigEntityProperty;

        public event EventHandler<Aop.CurdBeforeEventArgs> CurdBefore;
        public event EventHandler<Aop.CurdAfterEventArgs> CurdAfter;
        public event EventHandler<Aop.SyncStructureBeforeEventArgs> SyncStructureBefore;
        public event EventHandler<Aop.SyncStructureAfterEventArgs> SyncStructureAfter;

        public event EventHandler<Aop.AuditValueEventArgs> AuditValue;

        public event EventHandler<Aop.CommandBeforeEventArgs> CommandBefore;
        public event EventHandler<Aop.CommandAfterEventArgs> CommandAfter;
        public event EventHandler<Aop.TraceBeforeEventArgs> TraceBefore;
        public event EventHandler<Aop.TraceAfterEventArgs> TraceAfter;

        //------------- Handler

        public EventHandler<Aop.ParseExpressionEventArgs> ParseExpressionHandler => ParseExpression;
        public EventHandler<Aop.ConfigEntityEventArgs> ConfigEntityHandler => ConfigEntity;
        public EventHandler<Aop.ConfigEntityPropertyEventArgs> ConfigEntityPropertyHandler => ConfigEntityProperty;

        public EventHandler<Aop.CurdBeforeEventArgs> CurdBeforeHandler => CurdBefore;
        public EventHandler<Aop.CurdAfterEventArgs> CurdAfterHandler => CurdAfter;
        public EventHandler<Aop.SyncStructureBeforeEventArgs> SyncStructureBeforeHandler => SyncStructureBefore;
        public EventHandler<Aop.SyncStructureAfterEventArgs> SyncStructureAfterHandler => SyncStructureAfter;

        public EventHandler<Aop.AuditValueEventArgs> AuditValueHandler => AuditValue;

        public EventHandler<Aop.CommandBeforeEventArgs> CommandBeforeHandler => CommandBefore;
        public EventHandler<Aop.CommandAfterEventArgs> CommandAfterHandler => CommandAfter;
        public EventHandler<Aop.TraceBeforeEventArgs> TraceBeforeHandler => TraceBefore;
        public EventHandler<Aop.TraceAfterEventArgs> TraceAfterHandler => TraceAfter;
    }
}
