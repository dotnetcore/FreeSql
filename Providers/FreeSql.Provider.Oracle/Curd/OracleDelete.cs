using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Oracle.Curd
{

    class OracleDelete<T1> : Internal.CommonProvider.DeleteProvider<T1> where T1 : class
    {
        public OracleDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override List<T1> ExecuteDeleted()
        {
            throw new NotImplementedException();
        }
        public override Task<List<T1>> ExecuteDeletedAsync()
        {
            throw new NotImplementedException();
        }
    }
}
