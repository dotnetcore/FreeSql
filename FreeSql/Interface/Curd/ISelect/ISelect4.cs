using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface ISelect<T1, T2, T3, T4> : ISelect0<ISelect<T1, T2, T3, T4>, T1> where T1 : class where T2 : class where T3 : class where T4 : class
    {

        bool Any(Expression<Func<T1, T2, T3, T4, bool>> exp);
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, bool>> exp);

        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select);
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select);

        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select);
        List<TDto> ToList<TDto>();
        Task<List<TDto>> ToListAsync<TDto>();
        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select);

        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, TReturn>> select);
        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, TReturn>> select);

        TMember Sum<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        Task<TMember> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        TMember Avg<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        Task<TMember> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);

        ISelect<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> exp);
        ISelect<T1, T2, T3, T4> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, bool>> exp);

        ISelectGrouping<TKey, (T1, T2, T3, T4)> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, TKey>> exp);

        ISelect<T1, T2, T3, T4> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        ISelect<T1, T2, T3, T4> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
    }
}