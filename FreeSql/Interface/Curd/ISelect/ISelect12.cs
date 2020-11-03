using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql
{

    public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTargetEntity>> select) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select);
        Task<List<TDto>> ToListAsync<TDto>();

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select);
        Task<TDto> FirstAsync<TDto>();

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, TReturn>> select);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TTargetEntity>> select) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, string sqlT12, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column, bool descending = false);

        #endregion

    }

}