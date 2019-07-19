using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    public abstract class Select3Provider<T1, T2, T3> : Select0Provider<ISelect<T1, T2, T3>, T1>, ISelect<T1, T2, T3>
            where T1 : class
            where T2 : class
            where T3 : class
    {

        public Select3Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
        }

        TMember ISelect<T1, T2, T3>.Avg<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg<TMember>(column?.Body);
        }

        Task<TMember> ISelect<T1, T2, T3>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync<TMember>(column?.Body);
        }

        ISelectGrouping<TKey, (T1, T2, T3)> ISelect<T1, T2, T3>.GroupBy<TKey>(Expression<Func<T1, T2, T3, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, (T1, T2, T3)>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, (T1, T2, T3)>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3>.Max<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        Task<TMember> ISelect<T1, T2, T3>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3>.Min<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        Task<TMember> ISelect<T1, T2, T3>.MinAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.OrderBy<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        TMember ISelect<T1, T2, T3>.Sum<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum<TMember>(column?.Body);
        }

        Task<TMember> ISelect<T1, T2, T3>.SumAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync<TMember>(column?.Body);
        }

        TReturn ISelect<T1, T2, T3>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }

        Task<TReturn> ISelect<T1, T2, T3>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, TReturn>> select)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body);
        }

        List<TReturn> ISelect<T1, T2, T3>.ToList<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }
        Task<List<TReturn>> ISelect<T1, T2, T3>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body);
        }
        List<TDto> ISelect<T1, T2, T3>.ToList<TDto>() => (this as ISelect<T1, T2, T3>).ToList(GetToListDtoSelector<TDto>());
        Task<List<TDto>> ISelect<T1, T2, T3>.ToListAsync<TDto>() => (this as ISelect<T1, T2, T3>).ToListAsync(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, TDto>> GetToListDtoSelector<TDto>()
        {
            var ctor = typeof(TDto).GetConstructor(new Type[0]);
            return Expression.Lambda<Func<T1, T2, T3, TDto>>(Expression.New(ctor),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"),
                Expression.Parameter(typeof(T2), "b"),
                Expression.Parameter(typeof(T3), "c"));
        }

        DataTable ISelect<T1, T2, T3>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        Task<DataTable> ISelect<T1, T2, T3>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body);
        }

        string ISelect<T1, T2, T3>.ToSql<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.Where(Expression<Func<T1, T2, T3, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereCascadeExpression));
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.WhereIf(bool condition, Expression<Func<T1, T2, T3, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereCascadeExpression));
        }

        bool ISelect<T1, T2, T3>.Any(Expression<Func<T1, T2, T3, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereCascadeExpression)).Any();
        }

        Task<bool> ISelect<T1, T2, T3>.AnyAsync(Expression<Func<T1, T2, T3, bool>> exp)
        {
            if (exp == null) return this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereCascadeExpression)).AnyAsync();
        }
    }
}