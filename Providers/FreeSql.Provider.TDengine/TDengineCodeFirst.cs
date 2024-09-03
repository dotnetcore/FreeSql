using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Provider.TDengine
{
    internal class TDengineCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public TDengineCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression)
        {

        }

        public override DbInfoResult GetDbInfo(Type type)
        {
            throw new NotImplementedException();
        }

        protected override string GetComparisonDDLStatements(params TypeSchemaAndName[] objects)
        {
            throw new NotImplementedException();
        }
    }
}
