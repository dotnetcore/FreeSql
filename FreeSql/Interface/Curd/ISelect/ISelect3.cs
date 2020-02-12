using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface ISelect<T1, T2, T3> : ISelect0<ISelect<T1, T2, T3>, T1> where T1 : class where T2 : class where T3 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, bool>> exp);
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        Task<List<TDto>> ToListAsync<TDto>();

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        Task<TDto> FirstAsync<TDto>();

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, TReturn>> select);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
#endif

        bool Any(Expression<Func<T1, T2, T3, bool>> exp);
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        List<TDto> ToList<TDto>();

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, TReturn>> select);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, TMember>> column);

        ISelect<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> exp);
        ISelect<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> exp);
        ISelect<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> exp);

        ISelect<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> exp);
        ISelect<T1, T2, T3> WhereIf(bool condition, Expression<Func<T1, T2, T3, bool>> exp);

        ISelectGrouping<TKey, (T1, T2, T3)> GroupBy<TKey>(Expression<Func<T1, T2, T3, TKey>> exp);

        ISelect<T1, T2, T3> OrderBy<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        ISelect<T1, T2, T3> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
    }
}