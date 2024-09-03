using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.Internal;

namespace FreeSql.TDengine.Curd
{
    internal class TDengineSelect<T1> : FreeSql.Internal.CommonProvider.Select1Provider<T1>
    {
        public TDengineSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {

        }

        public override ISelect<T1, T2> From<T2>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3> From<T2, T3>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4> From<T2, T3, T4>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5> From<T2, T3, T4, T5>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6> From<T2, T3, T4, T5, T6>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6, T7> From<T2, T3, T4, T5, T6, T7>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8> From<T2, T3, T4, T5, T6, T7, T8>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T2, T3, T4, T5, T6, T7, T8, T9>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T2, T3, T4, T5, T6, T7, T8, T9, T10>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(System.Linq.Expressions.Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, ISelectFromExpression<T1>>> exp = null)
        {
            throw new NotImplementedException();
        }

        public override string ToSql(string field = null)
        {
            throw new NotImplementedException();
        }
    }
}
