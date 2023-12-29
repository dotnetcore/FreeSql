
using FreeSql;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace FreeSql.Extensions.JoinTable
{

	public static class JoinTableExtension
    {
        public static SelectContext<T1> JoinTables<T1>(this ISelect<T1> select) where T1 : class
        {
            return new SelectContext<T1>(select);
        }
    }

    public enum JoinType
    {
        LeftJoin,
        RightJoin,
        InnerJoin,
    }

    public class ExpressionConvert
    {
        public static LambdaExpression ConvertToMoreParameters(LambdaExpression expression, params Type[] additionalParameterTypes)
        {
            var currentParameters = expression.Parameters;
            var newParameters = currentParameters.ToList();

            foreach (var paramType in additionalParameterTypes)
            {
                newParameters.Add(Expression.Parameter(paramType, "param" + newParameters.Count));
            }

            return Expression.Lambda(expression.Body, newParameters);
        }
    }

    public class SelectContext<T1> where T1 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public SelectContext(ISelect<T1> select)
        {
            this.SelectProvider = select;
        }

        public SelectContext(SelectContext<T1> ctx)
        {
            this.SelectProvider = ctx.SelectProvider;
        }

        public virtual SelectContext<T1, T2> LeftJoin<T2>(Expression<Func<T1, T2, bool>> exp = null) where T2 : class
        {
            return new SelectContext<T1, T2>(this, exp, JoinType.LeftJoin);
        }

        public virtual SelectContext<T1, T2> RightJoin<T2>(Expression<Func<T1, T2, bool>> exp = null) where T2 : class
        {
            return new SelectContext<T1, T2>(this, exp, JoinType.RightJoin);
        }

        public virtual SelectContext<T1, T2> InnerJoin<T2>(Expression<Func<T1, T2, bool>> exp = null) where T2 : class
        {
            return new SelectContext<T1, T2>(this, exp, JoinType.InnerJoin);
        }
    }

    public class SelectContext<T1, T2>
        where T1 : class 
        where T2 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;

        public readonly JoinType JoinTypeT2;

        private ISelect<T1, T2> SelectCore;

        public SelectContext(SelectContext<T1> ctx, Expression<Func<T1, T2, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2>();

            

            

            this.ExpressionT2 = expression;
            this.JoinTypeT2 = joinType;
        }

                public SelectContext<T1, T2, T3> LeftJoin<T3>(Expression<Func<T1, T2, T3, bool>> exp = null) where T3 : class
        {
            return new SelectContext<T1, T2, T3>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3> InnerJoin<T3>(Expression<Func<T1, T2, T3, bool>> exp = null) where T3 : class
        {
            return new SelectContext<T1, T2, T3>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3> RightJoin<T3>(Expression<Func<T1, T2, T3, bool>> exp = null) where T3 : class
        {
            return new SelectContext<T1, T2, T3>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2> JoinTable(Expression<Func<T1, T2, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2> EndJoin()
        {
                JoinTable((Expression<Func<T1, T2, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2), JoinTypeT2); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3>
        where T1 : class 
        where T2 : class
        where T3 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;

        private ISelect<T1, T2, T3> SelectCore;

        public SelectContext(SelectContext<T1, T2> ctx, Expression<Func<T1, T2, T3, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3>();

            this.ExpressionT2 = ctx.ExpressionT2;

            this.JoinTypeT2 = ctx.JoinTypeT2;

            this.ExpressionT3 = expression;
            this.JoinTypeT3 = joinType;
        }

                public SelectContext<T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<T1, T2, T3, T4, bool>> exp = null) where T4 : class
        {
            return new SelectContext<T1, T2, T3, T4>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<T1, T2, T3, T4, bool>> exp = null) where T4 : class
        {
            return new SelectContext<T1, T2, T3, T4>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4> RightJoin<T4>(Expression<Func<T1, T2, T3, T4, bool>> exp = null) where T4 : class
        {
            return new SelectContext<T1, T2, T3, T4>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3> JoinTable(Expression<Func<T1, T2, T3, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3)), JoinTypeT2); 
               JoinTable((Expression<Func<T1, T2, T3, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3), JoinTypeT3); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;

        private ISelect<T1, T2, T3, T4> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3> ctx, Expression<Func<T1, T2, T3, T4, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;

            this.ExpressionT4 = expression;
            this.JoinTypeT4 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> exp = null) where T5 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> exp = null) where T5 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5> RightJoin<T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> exp = null) where T5 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4> JoinTable(Expression<Func<T1, T2, T3, T4, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4)), JoinTypeT3); 
               JoinTable((Expression<Func<T1, T2, T3, T4, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4), JoinTypeT4); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;

        private ISelect<T1, T2, T3, T4, T5> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4> ctx, Expression<Func<T1, T2, T3, T4, T5, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;

            this.ExpressionT5 = expression;
            this.JoinTypeT5 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6> LeftJoin<T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp = null) where T6 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6> InnerJoin<T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp = null) where T6 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6> RightJoin<T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp = null) where T6 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5> JoinTable(Expression<Func<T1, T2, T3, T4, T5, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5)), JoinTypeT4); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5), JoinTypeT5); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;

        private ISelect<T1, T2, T3, T4, T5, T6> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;

            this.ExpressionT6 = expression;
            this.JoinTypeT6 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6, T7> LeftJoin<T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp = null) where T7 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7> InnerJoin<T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp = null) where T7 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7> RightJoin<T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp = null) where T7 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5, T6> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6)), JoinTypeT5); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6), JoinTypeT6); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6, T7>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
        where T7 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ExpressionT7;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;
        public readonly JoinType JoinTypeT7;

        private ISelect<T1, T2, T3, T4, T5, T6, T7> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5, T6> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6, T7>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;
            this.ExpressionT6 = ctx.ExpressionT6;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;
            this.JoinTypeT6 = ctx.JoinTypeT6;

            this.ExpressionT7 = expression;
            this.JoinTypeT7 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin<T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp = null) where T8 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin<T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp = null) where T8 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin<T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp = null) where T8 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5, T6, T7> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6, T7> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6), typeof(T7)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6), typeof(T7)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6), typeof(T7)), JoinTypeT5); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6, typeof(T7)), JoinTypeT6); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT7), JoinTypeT7); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6, T7, T8>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
        where T7 : class
        where T8 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ExpressionT7;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ExpressionT8;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;
        public readonly JoinType JoinTypeT7;
        public readonly JoinType JoinTypeT8;

        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5, T6, T7> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6, T7, T8>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;
            this.ExpressionT6 = ctx.ExpressionT6;
            this.ExpressionT7 = ctx.ExpressionT7;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;
            this.JoinTypeT6 = ctx.JoinTypeT6;
            this.JoinTypeT7 = ctx.JoinTypeT7;

            this.ExpressionT8 = expression;
            this.JoinTypeT8 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp = null) where T9 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp = null) where T9 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp = null) where T9 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6, T7, T8> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6), typeof(T7), typeof(T8)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6), typeof(T7), typeof(T8)), JoinTypeT5); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6, typeof(T7), typeof(T8)), JoinTypeT6); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT7, typeof(T8)), JoinTypeT7); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT8), JoinTypeT8); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
        where T7 : class
        where T8 : class
        where T9 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ExpressionT7;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ExpressionT8;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ExpressionT9;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;
        public readonly JoinType JoinTypeT7;
        public readonly JoinType JoinTypeT8;
        public readonly JoinType JoinTypeT9;

        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5, T6, T7, T8> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6, T7, T8, T9>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;
            this.ExpressionT6 = ctx.ExpressionT6;
            this.ExpressionT7 = ctx.ExpressionT7;
            this.ExpressionT8 = ctx.ExpressionT8;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;
            this.JoinTypeT6 = ctx.JoinTypeT6;
            this.JoinTypeT7 = ctx.JoinTypeT7;
            this.JoinTypeT8 = ctx.JoinTypeT8;

            this.ExpressionT9 = expression;
            this.JoinTypeT9 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp = null) where T10 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp = null) where T10 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp = null) where T10 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6), typeof(T7), typeof(T8), typeof(T9)), JoinTypeT5); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6, typeof(T7), typeof(T8), typeof(T9)), JoinTypeT6); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT7, typeof(T8), typeof(T9)), JoinTypeT7); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT8, typeof(T9)), JoinTypeT8); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT9), JoinTypeT9); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
        where T7 : class
        where T8 : class
        where T9 : class
        where T10 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ExpressionT7;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ExpressionT8;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ExpressionT9;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ExpressionT10;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;
        public readonly JoinType JoinTypeT7;
        public readonly JoinType JoinTypeT8;
        public readonly JoinType JoinTypeT9;
        public readonly JoinType JoinTypeT10;

        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6, T7, T8, T9, T10>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;
            this.ExpressionT6 = ctx.ExpressionT6;
            this.ExpressionT7 = ctx.ExpressionT7;
            this.ExpressionT8 = ctx.ExpressionT8;
            this.ExpressionT9 = ctx.ExpressionT9;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;
            this.JoinTypeT6 = ctx.JoinTypeT6;
            this.JoinTypeT7 = ctx.JoinTypeT7;
            this.JoinTypeT8 = ctx.JoinTypeT8;
            this.JoinTypeT9 = ctx.JoinTypeT9;

            this.ExpressionT10 = expression;
            this.JoinTypeT10 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<T11>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp = null) where T11 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<T11>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp = null) where T11 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<T11>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp = null) where T11 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10)), JoinTypeT5); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6, typeof(T7), typeof(T8), typeof(T9), typeof(T10)), JoinTypeT6); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT7, typeof(T8), typeof(T9), typeof(T10)), JoinTypeT7); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT8, typeof(T9), typeof(T10)), JoinTypeT8); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT9, typeof(T10)), JoinTypeT9); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT10), JoinTypeT10); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
        where T7 : class
        where T8 : class
        where T9 : class
        where T10 : class
        where T11 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ExpressionT7;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ExpressionT8;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ExpressionT9;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ExpressionT10;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ExpressionT11;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;
        public readonly JoinType JoinTypeT7;
        public readonly JoinType JoinTypeT8;
        public readonly JoinType JoinTypeT9;
        public readonly JoinType JoinTypeT10;
        public readonly JoinType JoinTypeT11;

        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;
            this.ExpressionT6 = ctx.ExpressionT6;
            this.ExpressionT7 = ctx.ExpressionT7;
            this.ExpressionT8 = ctx.ExpressionT8;
            this.ExpressionT9 = ctx.ExpressionT9;
            this.ExpressionT10 = ctx.ExpressionT10;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;
            this.JoinTypeT6 = ctx.JoinTypeT6;
            this.JoinTypeT7 = ctx.JoinTypeT7;
            this.JoinTypeT8 = ctx.JoinTypeT8;
            this.JoinTypeT9 = ctx.JoinTypeT9;
            this.JoinTypeT10 = ctx.JoinTypeT10;

            this.ExpressionT11 = expression;
            this.JoinTypeT11 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<T12>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp = null) where T12 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<T12>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp = null) where T12 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<T12>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp = null) where T12 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11)), JoinTypeT5); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6, typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11)), JoinTypeT6); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT7, typeof(T8), typeof(T9), typeof(T10), typeof(T11)), JoinTypeT7); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT8, typeof(T9), typeof(T10), typeof(T11)), JoinTypeT8); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT9, typeof(T10), typeof(T11)), JoinTypeT9); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT10, typeof(T11)), JoinTypeT10); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT11), JoinTypeT11); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
        where T7 : class
        where T8 : class
        where T9 : class
        where T10 : class
        where T11 : class
        where T12 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ExpressionT7;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ExpressionT8;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ExpressionT9;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ExpressionT10;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ExpressionT11;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ExpressionT12;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;
        public readonly JoinType JoinTypeT7;
        public readonly JoinType JoinTypeT8;
        public readonly JoinType JoinTypeT9;
        public readonly JoinType JoinTypeT10;
        public readonly JoinType JoinTypeT11;
        public readonly JoinType JoinTypeT12;

        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;
            this.ExpressionT6 = ctx.ExpressionT6;
            this.ExpressionT7 = ctx.ExpressionT7;
            this.ExpressionT8 = ctx.ExpressionT8;
            this.ExpressionT9 = ctx.ExpressionT9;
            this.ExpressionT10 = ctx.ExpressionT10;
            this.ExpressionT11 = ctx.ExpressionT11;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;
            this.JoinTypeT6 = ctx.JoinTypeT6;
            this.JoinTypeT7 = ctx.JoinTypeT7;
            this.JoinTypeT8 = ctx.JoinTypeT8;
            this.JoinTypeT9 = ctx.JoinTypeT9;
            this.JoinTypeT10 = ctx.JoinTypeT10;
            this.JoinTypeT11 = ctx.JoinTypeT11;

            this.ExpressionT12 = expression;
            this.JoinTypeT12 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin<T13>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp = null) where T13 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin<T13>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp = null) where T13 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin<T13>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp = null) where T13 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12)), JoinTypeT5); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6, typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12)), JoinTypeT6); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT7, typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12)), JoinTypeT7); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT8, typeof(T9), typeof(T10), typeof(T11), typeof(T12)), JoinTypeT8); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT9, typeof(T10), typeof(T11), typeof(T12)), JoinTypeT9); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT10, typeof(T11), typeof(T12)), JoinTypeT10); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT11, typeof(T12)), JoinTypeT11); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT12), JoinTypeT12); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
        where T7 : class
        where T8 : class
        where T9 : class
        where T10 : class
        where T11 : class
        where T12 : class
        where T13 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ExpressionT7;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ExpressionT8;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ExpressionT9;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ExpressionT10;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ExpressionT11;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ExpressionT12;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ExpressionT13;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;
        public readonly JoinType JoinTypeT7;
        public readonly JoinType JoinTypeT8;
        public readonly JoinType JoinTypeT9;
        public readonly JoinType JoinTypeT10;
        public readonly JoinType JoinTypeT11;
        public readonly JoinType JoinTypeT12;
        public readonly JoinType JoinTypeT13;

        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;
            this.ExpressionT6 = ctx.ExpressionT6;
            this.ExpressionT7 = ctx.ExpressionT7;
            this.ExpressionT8 = ctx.ExpressionT8;
            this.ExpressionT9 = ctx.ExpressionT9;
            this.ExpressionT10 = ctx.ExpressionT10;
            this.ExpressionT11 = ctx.ExpressionT11;
            this.ExpressionT12 = ctx.ExpressionT12;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;
            this.JoinTypeT6 = ctx.JoinTypeT6;
            this.JoinTypeT7 = ctx.JoinTypeT7;
            this.JoinTypeT8 = ctx.JoinTypeT8;
            this.JoinTypeT9 = ctx.JoinTypeT9;
            this.JoinTypeT10 = ctx.JoinTypeT10;
            this.JoinTypeT11 = ctx.JoinTypeT11;
            this.JoinTypeT12 = ctx.JoinTypeT12;

            this.ExpressionT13 = expression;
            this.JoinTypeT13 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin<T14>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp = null) where T14 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin<T14>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp = null) where T14 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin<T14>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp = null) where T14 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13)), JoinTypeT5); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6, typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13)), JoinTypeT6); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT7, typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13)), JoinTypeT7); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT8, typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13)), JoinTypeT8); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT9, typeof(T10), typeof(T11), typeof(T12), typeof(T13)), JoinTypeT9); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT10, typeof(T11), typeof(T12), typeof(T13)), JoinTypeT10); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT11, typeof(T12), typeof(T13)), JoinTypeT11); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT12, typeof(T13)), JoinTypeT12); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT13), JoinTypeT13); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
        where T7 : class
        where T8 : class
        where T9 : class
        where T10 : class
        where T11 : class
        where T12 : class
        where T13 : class
        where T14 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ExpressionT7;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ExpressionT8;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ExpressionT9;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ExpressionT10;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ExpressionT11;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ExpressionT12;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ExpressionT13;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ExpressionT14;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;
        public readonly JoinType JoinTypeT7;
        public readonly JoinType JoinTypeT8;
        public readonly JoinType JoinTypeT9;
        public readonly JoinType JoinTypeT10;
        public readonly JoinType JoinTypeT11;
        public readonly JoinType JoinTypeT12;
        public readonly JoinType JoinTypeT13;
        public readonly JoinType JoinTypeT14;

        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;
            this.ExpressionT6 = ctx.ExpressionT6;
            this.ExpressionT7 = ctx.ExpressionT7;
            this.ExpressionT8 = ctx.ExpressionT8;
            this.ExpressionT9 = ctx.ExpressionT9;
            this.ExpressionT10 = ctx.ExpressionT10;
            this.ExpressionT11 = ctx.ExpressionT11;
            this.ExpressionT12 = ctx.ExpressionT12;
            this.ExpressionT13 = ctx.ExpressionT13;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;
            this.JoinTypeT6 = ctx.JoinTypeT6;
            this.JoinTypeT7 = ctx.JoinTypeT7;
            this.JoinTypeT8 = ctx.JoinTypeT8;
            this.JoinTypeT9 = ctx.JoinTypeT9;
            this.JoinTypeT10 = ctx.JoinTypeT10;
            this.JoinTypeT11 = ctx.JoinTypeT11;
            this.JoinTypeT12 = ctx.JoinTypeT12;
            this.JoinTypeT13 = ctx.JoinTypeT13;

            this.ExpressionT14 = expression;
            this.JoinTypeT14 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin<T15>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp = null) where T15 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin<T15>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp = null) where T15 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin<T15>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp = null) where T15 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14)), JoinTypeT5); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6, typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14)), JoinTypeT6); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT7, typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14)), JoinTypeT7); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT8, typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14)), JoinTypeT8); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT9, typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14)), JoinTypeT9); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT10, typeof(T11), typeof(T12), typeof(T13), typeof(T14)), JoinTypeT10); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT11, typeof(T12), typeof(T13), typeof(T14)), JoinTypeT11); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT12, typeof(T13), typeof(T14)), JoinTypeT12); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT13, typeof(T14)), JoinTypeT13); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT14), JoinTypeT14); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
        where T7 : class
        where T8 : class
        where T9 : class
        where T10 : class
        where T11 : class
        where T12 : class
        where T13 : class
        where T14 : class
        where T15 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ExpressionT7;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ExpressionT8;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ExpressionT9;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ExpressionT10;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ExpressionT11;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ExpressionT12;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ExpressionT13;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ExpressionT14;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ExpressionT15;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;
        public readonly JoinType JoinTypeT7;
        public readonly JoinType JoinTypeT8;
        public readonly JoinType JoinTypeT9;
        public readonly JoinType JoinTypeT10;
        public readonly JoinType JoinTypeT11;
        public readonly JoinType JoinTypeT12;
        public readonly JoinType JoinTypeT13;
        public readonly JoinType JoinTypeT14;
        public readonly JoinType JoinTypeT15;

        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;
            this.ExpressionT6 = ctx.ExpressionT6;
            this.ExpressionT7 = ctx.ExpressionT7;
            this.ExpressionT8 = ctx.ExpressionT8;
            this.ExpressionT9 = ctx.ExpressionT9;
            this.ExpressionT10 = ctx.ExpressionT10;
            this.ExpressionT11 = ctx.ExpressionT11;
            this.ExpressionT12 = ctx.ExpressionT12;
            this.ExpressionT13 = ctx.ExpressionT13;
            this.ExpressionT14 = ctx.ExpressionT14;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;
            this.JoinTypeT6 = ctx.JoinTypeT6;
            this.JoinTypeT7 = ctx.JoinTypeT7;
            this.JoinTypeT8 = ctx.JoinTypeT8;
            this.JoinTypeT9 = ctx.JoinTypeT9;
            this.JoinTypeT10 = ctx.JoinTypeT10;
            this.JoinTypeT11 = ctx.JoinTypeT11;
            this.JoinTypeT12 = ctx.JoinTypeT12;
            this.JoinTypeT13 = ctx.JoinTypeT13;
            this.JoinTypeT14 = ctx.JoinTypeT14;

            this.ExpressionT15 = expression;
            this.JoinTypeT15 = joinType;
        }

                public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin<T16>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp = null) where T16 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, exp, JoinType.LeftJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin<T16>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp = null) where T16 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, exp, JoinType.InnerJoin);
        }

        public SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin<T16>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp = null) where T16 : class
        {
            return new SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, exp, JoinType.RightJoin);
        }
        
        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15)), JoinTypeT5); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6, typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15)), JoinTypeT6); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT7, typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15)), JoinTypeT7); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT8, typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15)), JoinTypeT8); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT9, typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15)), JoinTypeT9); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT10, typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15)), JoinTypeT10); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT11, typeof(T12), typeof(T13), typeof(T14), typeof(T15)), JoinTypeT11); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT12, typeof(T13), typeof(T14), typeof(T15)), JoinTypeT12); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT13, typeof(T14), typeof(T15)), JoinTypeT13); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT14, typeof(T15)), JoinTypeT14); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT15), JoinTypeT15); 

            return SelectCore;
        }
    }

    public class SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        where T1 : class 
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
        where T6 : class
        where T7 : class
        where T8 : class
        where T9 : class
        where T10 : class
        where T11 : class
        where T12 : class
        where T13 : class
        where T14 : class
        where T15 : class
        where T16 : class
    {
        public readonly ISelect<T1> SelectProvider;

        public readonly Expression<Func<T1, T2, bool>> ExpressionT2;
        public readonly Expression<Func<T1, T2, T3, bool>> ExpressionT3;
        public readonly Expression<Func<T1, T2, T3, T4, bool>> ExpressionT4;
        public readonly Expression<Func<T1, T2, T3, T4, T5, bool>> ExpressionT5;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ExpressionT6;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ExpressionT7;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ExpressionT8;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ExpressionT9;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ExpressionT10;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ExpressionT11;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ExpressionT12;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ExpressionT13;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ExpressionT14;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ExpressionT15;
        public readonly Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ExpressionT16;

        public readonly JoinType JoinTypeT2;
        public readonly JoinType JoinTypeT3;
        public readonly JoinType JoinTypeT4;
        public readonly JoinType JoinTypeT5;
        public readonly JoinType JoinTypeT6;
        public readonly JoinType JoinTypeT7;
        public readonly JoinType JoinTypeT8;
        public readonly JoinType JoinTypeT9;
        public readonly JoinType JoinTypeT10;
        public readonly JoinType JoinTypeT11;
        public readonly JoinType JoinTypeT12;
        public readonly JoinType JoinTypeT13;
        public readonly JoinType JoinTypeT14;
        public readonly JoinType JoinTypeT15;
        public readonly JoinType JoinTypeT16;

        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> SelectCore;

        public SelectContext(SelectContext<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ctx, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression = null, JoinType joinType = JoinType.InnerJoin)
        {
            this.SelectProvider = ctx.SelectProvider;
			this.SelectCore = SelectProvider.From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>();

            this.ExpressionT2 = ctx.ExpressionT2;
            this.ExpressionT3 = ctx.ExpressionT3;
            this.ExpressionT4 = ctx.ExpressionT4;
            this.ExpressionT5 = ctx.ExpressionT5;
            this.ExpressionT6 = ctx.ExpressionT6;
            this.ExpressionT7 = ctx.ExpressionT7;
            this.ExpressionT8 = ctx.ExpressionT8;
            this.ExpressionT9 = ctx.ExpressionT9;
            this.ExpressionT10 = ctx.ExpressionT10;
            this.ExpressionT11 = ctx.ExpressionT11;
            this.ExpressionT12 = ctx.ExpressionT12;
            this.ExpressionT13 = ctx.ExpressionT13;
            this.ExpressionT14 = ctx.ExpressionT14;
            this.ExpressionT15 = ctx.ExpressionT15;

            this.JoinTypeT2 = ctx.JoinTypeT2;
            this.JoinTypeT3 = ctx.JoinTypeT3;
            this.JoinTypeT4 = ctx.JoinTypeT4;
            this.JoinTypeT5 = ctx.JoinTypeT5;
            this.JoinTypeT6 = ctx.JoinTypeT6;
            this.JoinTypeT7 = ctx.JoinTypeT7;
            this.JoinTypeT8 = ctx.JoinTypeT8;
            this.JoinTypeT9 = ctx.JoinTypeT9;
            this.JoinTypeT10 = ctx.JoinTypeT10;
            this.JoinTypeT11 = ctx.JoinTypeT11;
            this.JoinTypeT12 = ctx.JoinTypeT12;
            this.JoinTypeT13 = ctx.JoinTypeT13;
            this.JoinTypeT14 = ctx.JoinTypeT14;
            this.JoinTypeT15 = ctx.JoinTypeT15;

            this.ExpressionT16 = expression;
            this.JoinTypeT16 = joinType;
        }

        
        private ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> JoinTable(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp, JoinType type)
        {
            if (type == JoinType.LeftJoin)
            {
                return SelectCore.LeftJoin(exp);
            }
            else if (type == JoinType.InnerJoin)
            {
                return SelectCore.InnerJoin(exp);
            }
            else if (type == JoinType.RightJoin)
            {
                return SelectCore.RightJoin(exp);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> EndJoin()
        {
                    JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT2, typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT2); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT3, typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT3); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT4, typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT4); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT5, typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT5); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT6, typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT6); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT7, typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT7); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT8, typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT8); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT9, typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT9); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT10, typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT10); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT11, typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT11); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT12, typeof(T13), typeof(T14), typeof(T15), typeof(T16)), JoinTypeT12); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT13, typeof(T14), typeof(T15), typeof(T16)), JoinTypeT13); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT14, typeof(T15), typeof(T16)), JoinTypeT14); 
                   JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT15, typeof(T16)), JoinTypeT15); 
               JoinTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)ExpressionConvert.ConvertToMoreParameters(ExpressionT16), JoinTypeT16); 

            return SelectCore;
        }
    }

}
