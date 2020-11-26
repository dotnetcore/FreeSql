using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{

    public interface ISelect<T1, T2> : ISelect0<ISelect<T1, T2>, T1> where T2 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, TReturn>> select);
        ISelect<T1, T2> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, TMember>> column);

        ISelect<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> exp);
        ISelect<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> exp);
        ISelect<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> exp);

        ISelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp);
        ISelect<T1, T2> WhereIf(bool condition, Expression<Func<T1, T2, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2>> GroupBy<TKey>(Expression<Func<T1, T2, TKey>> exp);

        ISelect<T1, T2> OrderBy<TMember>(Expression<Func<T1, T2, TMember>> column);
        ISelect<T1, T2> OrderByDescending<TMember>(Expression<Func<T1, T2, TMember>> column);
        ISelect<T1, T2> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, TMember>> column, bool descending = false);

        ISelect<T1, T2> WithSql(string sqlT1, string sqlT2, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column);

        ISelect<T1, T2> LeftJoin(Expression<Func<HzyTuple<T1, T2>, bool>> exp);
        ISelect<T1, T2> InnerJoin(Expression<Func<HzyTuple<T1, T2>, bool>> exp);
        ISelect<T1, T2> RightJoin(Expression<Func<HzyTuple<T1, T2>, bool>> exp);

        ISelect<T1, T2> Where(Expression<Func<HzyTuple<T1, T2>, bool>> exp);
        ISelect<T1, T2> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2>, TKey>> exp);

        ISelect<T1, T2> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column);
        ISelect<T1, T2> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column);
        ISelect<T1, T2> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3> : ISelect0<ISelect<T1, T2, T3>, T1> where T2 : class where T3 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, TReturn>> select);
        ISelect<T1, T2, T3> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, TMember>> column);

        ISelect<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> exp);
        ISelect<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> exp);
        ISelect<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> exp);

        ISelect<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> exp);
        ISelect<T1, T2, T3> WhereIf(bool condition, Expression<Func<T1, T2, T3, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3>> GroupBy<TKey>(Expression<Func<T1, T2, T3, TKey>> exp);

        ISelect<T1, T2, T3> OrderBy<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        ISelect<T1, T2, T3> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, TMember>> column);
        ISelect<T1, T2, T3> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3> WithSql(string sqlT1, string sqlT2, string sqlT3, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column);

        ISelect<T1, T2, T3> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp);
        ISelect<T1, T2, T3> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp);
        ISelect<T1, T2, T3> RightJoin(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp);

        ISelect<T1, T2, T3> Where(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp);
        ISelect<T1, T2, T3> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3>, TKey>> exp);

        ISelect<T1, T2, T3> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column);
        ISelect<T1, T2, T3> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column);
        ISelect<T1, T2, T3> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4> : ISelect0<ISelect<T1, T2, T3, T4>, T1> where T2 : class where T3 : class where T4 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, TReturn>> select);
        ISelect<T1, T2, T3, T4> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);

        ISelect<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> exp);
        ISelect<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> exp);
        ISelect<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> exp);

        ISelect<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> exp);
        ISelect<T1, T2, T3, T4> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, TKey>> exp);

        ISelect<T1, T2, T3, T4> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        ISelect<T1, T2, T3, T4> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column);
        ISelect<T1, T2, T3, T4> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column);

        ISelect<T1, T2, T3, T4> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp);
        ISelect<T1, T2, T3, T4> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp);
        ISelect<T1, T2, T3, T4> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp);

        ISelect<T1, T2, T3, T4> Where(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp);
        ISelect<T1, T2, T3, T4> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TKey>> exp);

        ISelect<T1, T2, T3, T4> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column);
        ISelect<T1, T2, T3, T4> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column);
        ISelect<T1, T2, T3, T4> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5> : ISelect0<ISelect<T1, T2, T3, T4, T5>, T1> where T2 : class where T3 : class where T4 : class where T5 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);

        ISelect<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> exp);
        ISelect<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> exp);
        ISelect<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> exp);

        ISelect<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> exp);
        ISelect<T1, T2, T3, T4, T5> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);
        ISelect<T1, T2, T3, T4, T5> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);
        ISelect<T1, T2, T3, T4, T5> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5, T6> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5, T6, T7> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6, T7> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6, T7> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column, CancellationToken cancellationToken = default);

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





    public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, string sqlT12, string sqlT13, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, string sqlT12, string sqlT13, string sqlT14, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, string sqlT12, string sqlT13, string sqlT14, string sqlT15, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column, bool descending = false);

        #endregion

    }





    public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, T1> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class where T16 : class
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);

        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, ISelectGroupingAggregate<T16>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column, CancellationToken cancellationToken = default);

        #region HzyTuple 元组

        Task<bool> AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp, CancellationToken cancellationToken = default);
        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, CancellationToken cancellationToken = default);

        Task<decimal> SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column, CancellationToken cancellationToken = default);

        #endregion

#endif

        bool Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select);
        List<TDto> ToList<TDto>();
        void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select);
        TDto First<TDto>();

        string ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, ISelectGroupingAggregate<T16>, TReturn>> select);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, ISelectGroupingAggregate<T16>, TReturn>> select, out TReturn result);
        decimal Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column);
        TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column);
        TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column);
        double Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column, bool descending = false);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, string sqlT12, string sqlT13, string sqlT14, string sqlT15, string sqlT16, object parms = null);

        #region HzyTuple 元组

        bool Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp);
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TTargetEntity>> select) where TTargetEntity : class;
        DataTable ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select);
        List<TReturn> ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select);
        void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        TReturn ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select);
        TReturn First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select);

        string ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        decimal Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column);
        TMember Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column);
        TMember Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column);
        double Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp);

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TKey>> exp);

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column);
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column, bool descending = false);

        #endregion

    }

}
