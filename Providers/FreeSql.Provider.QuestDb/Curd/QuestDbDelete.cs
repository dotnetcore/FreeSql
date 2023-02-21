using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.PostgreSQL.Curd
{

    class QuestDbDelete<T1> : Internal.CommonProvider.DeleteProvider<T1>
    {
        public QuestDbDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
            
        }

        public override List<T1> ExecuteDeleted() => throw new NotImplementedException("QuestDb 不支持删除数据.");

        public override int ExecuteAffrows() => throw new NotImplementedException("QuestDb 不支持删除数据.");

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException("QuestDb 不支持删除数据.");

        public override Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException("QuestDb 不支持删除数据.");
#endif
    }
}
