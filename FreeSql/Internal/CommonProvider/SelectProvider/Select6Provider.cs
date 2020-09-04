using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    public abstract class Select6Provider<T1, T2, T3, T4, T5, T6> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6>, T1>, ISelect<T1, T2, T3, T4, T5, T6>
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
    {

        public Select6Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"( {sqlT1} )";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"( {sqlT2} )";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"( {sqlT3} )";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"( {sqlT4} )";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"( {sqlT5} )";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"( {sqlT6} )";
                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6>> ISelect<T1, T2, T3, T4, T5, T6>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column, bool descending)
        {
            if (condition == false || column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }
        List<TDto> ISelect<T1, T2, T3, T4, T5, T6>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"),
                Expression.Parameter(typeof(T2), "b"),
                Expression.Parameter(typeof(T3), "c"),
                Expression.Parameter(typeof(T4), "d"),
                Expression.Parameter(typeof(T5), "e"),
                Expression.Parameter(typeof(T6), "f"));
        }
        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereCascadeExpression, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereCascadeExpression, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereCascadeExpression, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToList<TDto>().FirstOrDefault();

#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, TReturn>> select)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6>.ToListAsync<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6>).ToListAsync(GetToListDtoSelector<TDto>());

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereCascadeExpression, _params)).AnyAsync();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select) => (await (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToListAsync(select)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select) => (await (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToListAsync(select)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6>.FirstAsync<TDto>() => (await (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToListAsync<TDto>()).FirstOrDefault();

#endif
    }
}