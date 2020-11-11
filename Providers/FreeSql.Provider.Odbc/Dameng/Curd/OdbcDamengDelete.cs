using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Odbc.Dameng
{

    class OdbcDamengDelete<T1> : Internal.CommonProvider.DeleteProvider<T1>
    {
        public OdbcDamengDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override List<T1> ExecuteDeleted()
        {
            throw new NotImplementedException();
        }

#if net40
#else
        public override Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
