using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.Internal.CommonProvider;

namespace FreeSql.Provider.TDengine
{
    internal class TDengineProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere)
        {
            throw new NotImplementedException();
        }

        public override IInsert<T1> CreateInsertProvider<T1>()
        {
            throw new NotImplementedException();
        }

        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere)
        {
            throw new NotImplementedException();
        }

        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere)
        {
            throw new NotImplementedException();
        }

        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}