
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Internal.CommonProvider
{
    public class AopProvider : IAop
    {
        public EventHandler<Aop.ToListEventArgs> ToList { get; set; }
        public EventHandler<Aop.WhereEventArgs> Where { get; set; }
        public EventHandler<Aop.ParseExpressionEventArgs> ParseExpression { get; set; }
        public EventHandler<Aop.ConfigEntityEventArgs> ConfigEntity { get; set; }
        public EventHandler<Aop.ConfigEntityPropertyEventArgs> ConfigEntityProperty { get; set; }
        public EventHandler<Aop.CurdBeforeEventArgs> CurdBefore { get; set; }
        public EventHandler<Aop.CurdAfterEventArgs> CurdAfter { get; set; }
        public EventHandler<Aop.SyncStructureBeforeEventArgs> SyncStructureBefore { get; set; }
        public EventHandler<Aop.SyncStructureAfterEventArgs> SyncStructureAfter { get; set; }
    }
}
