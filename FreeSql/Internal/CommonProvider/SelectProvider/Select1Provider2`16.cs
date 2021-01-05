
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    public abstract class Select2Provider<T1, T2> : Select0Provider<ISelect<T1, T2>, T1>, ISelect<T1, T2>
            where T2 : class
    {

        public Select2Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2> ISelect<T1, T2>.WithSql(string sqlT1, string sqlT2, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2}", parms));
            return this;
        }

        double ISelect<T1, T2>.Avg<TMember>(Expression<Func<T1, T2, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2>> ISelect<T1, T2>.GroupBy<TKey>(Expression<Func<T1, T2, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2>>(exp?.Body);
        }

        TMember ISelect<T1, T2>.Max<TMember>(Expression<Func<T1, T2, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2>.Min<TMember>(Expression<Func<T1, T2, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2> ISelect<T1, T2>.OrderBy<TMember>(Expression<Func<T1, T2, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2> ISelect<T1, T2>.OrderByDescending<TMember>(Expression<Func<T1, T2, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2> ISelect<T1, T2>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2>.Sum<TMember>(Expression<Func<T1, T2, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2> ISelect<T1, T2>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2>.ToList<TReturn>(Expression<Func<T1, T2, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2>.ToList<TDto>() => (this as ISelect<T1, T2>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2>.ToDataTable<TReturn>(Expression<Func<T1, T2, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2>.ToSql<TReturn>(Expression<Func<T1, T2, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2> ISelect<T1, T2>.LeftJoin(Expression<Func<T1, T2, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2> ISelect<T1, T2>.InnerJoin(Expression<Func<T1, T2, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2> ISelect<T1, T2>.RightJoin(Expression<Func<T1, T2, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2> ISelect<T1, T2>.Where(Expression<Func<T1, T2, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2> ISelect<T1, T2>.WhereIf(bool condition, Expression<Func<T1, T2, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2>.Any(Expression<Func<T1, T2, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2>.ToOne<TReturn>(Expression<Func<T1, T2, TReturn>> select) => (this as ISelect<T1, T2>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2>.First<TReturn>(Expression<Func<T1, T2, TReturn>> select) => (this as ISelect<T1, T2>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2>.First<TDto>() => (this as ISelect<T1, T2>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).Avg((Expression<Func<T1, T2, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2>> ISelect<T1, T2>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2>).GroupBy((Expression<Func<T1, T2, TKey>>)expModify);
        }

        TMember ISelect<T1, T2>.Max<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).Max((Expression<Func<T1, T2, TMember>>)expModify);
        }

        TMember ISelect<T1, T2>.Min<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).Min((Expression<Func<T1, T2, TMember>>)expModify);
        }

        ISelect<T1, T2> ISelect<T1, T2>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).OrderBy((Expression<Func<T1, T2, TMember>>)expModify);
        }

        ISelect<T1, T2> ISelect<T1, T2>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).OrderByDescending((Expression<Func<T1, T2, TMember>>)expModify);
        }

        ISelect<T1, T2> ISelect<T1, T2>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).OrderByIf(condition, (Expression<Func<T1, T2, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).Sum((Expression<Func<T1, T2, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2>).ToList((Expression<Func<T1, T2, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2>).ToChunk((Expression<Func<T1, T2, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2>).ToDataTable((Expression<Func<T1, T2, TReturn>>)expModify);
        }

        int ISelect<T1, T2>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2>).InsertInto(tableName, (Expression<Func<T1, T2, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2>).ToSql((Expression<Func<T1, T2, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2> ISelect<T1, T2>.LeftJoin(Expression<Func<HzyTuple<T1, T2>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2>).LeftJoin((Expression<Func<T1, T2, bool>>)expModify);
        }

        ISelect<T1, T2> ISelect<T1, T2>.InnerJoin(Expression<Func<HzyTuple<T1, T2>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2>).InnerJoin((Expression<Func<T1, T2, bool>>)expModify);
        }

        ISelect<T1, T2> ISelect<T1, T2>.RightJoin(Expression<Func<HzyTuple<T1, T2>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2>).RightJoin((Expression<Func<T1, T2, bool>>)expModify);
        }

        ISelect<T1, T2> ISelect<T1, T2>.Where(Expression<Func<HzyTuple<T1, T2>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2>).Where((Expression<Func<T1, T2, bool>>)expModify);
        }

        ISelect<T1, T2> ISelect<T1, T2>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2>).WhereIf(condition, (Expression<Func<T1, T2, bool>>)expModify);
        }

        bool ISelect<T1, T2>.Any(Expression<Func<HzyTuple<T1, T2>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2>).Any((Expression<Func<T1, T2, bool>>)expModify);
        }

        TReturn ISelect<T1, T2>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select)
            => (this as ISelect<T1, T2>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2>.First<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select)
            => (this as ISelect<T1, T2>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2>.AvgAsync<TMember>(Expression<Func<T1, T2, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2>.MaxAsync<TMember>(Expression<Func<T1, T2, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2>.MinAsync<TMember>(Expression<Func<T1, T2, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2>.SumAsync<TMember>(Expression<Func<T1, T2, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2>.ToListAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2>.AnyAsync(Expression<Func<T1, T2, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2>.ToOneAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2>.FirstAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).AvgAsync((Expression<Func<T1, T2, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).MaxAsync((Expression<Func<T1, T2, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).MinAsync((Expression<Func<T1, T2, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).SumAsync((Expression<Func<T1, T2, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2>).ToListAsync((Expression<Func<T1, T2, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2>).ToDataTableAsync((Expression<Func<T1, T2, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2>.AnyAsync(Expression<Func<HzyTuple<T1, T2>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2>).AnyAsync((Expression<Func<T1, T2, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select3Provider<T1, T2, T3> : Select0Provider<ISelect<T1, T2, T3>, T1>, ISelect<T1, T2, T3>
            where T2 : class where T3 : class
    {

        public Select3Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.WithSql(string sqlT1, string sqlT2, string sqlT3, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3}", parms));
            return this;
        }

        double ISelect<T1, T2, T3>.Avg<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3>> ISelect<T1, T2, T3>.GroupBy<TKey>(Expression<Func<T1, T2, T3, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3>.Max<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3>.Min<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
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

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3>.Sum<TMember>(Expression<Func<T1, T2, T3, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3>.ToList<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3>.ToList<TDto>() => (this as ISelect<T1, T2, T3>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3>.ToSql<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.LeftJoin(Expression<Func<T1, T2, T3, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.InnerJoin(Expression<Func<T1, T2, T3, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.RightJoin(Expression<Func<T1, T2, T3, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.Where(Expression<Func<T1, T2, T3, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.WhereIf(bool condition, Expression<Func<T1, T2, T3, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3>.Any(Expression<Func<T1, T2, T3, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3>.ToOne<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select) => (this as ISelect<T1, T2, T3>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3>.First<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select) => (this as ISelect<T1, T2, T3>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3>.First<TDto>() => (this as ISelect<T1, T2, T3>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).Avg((Expression<Func<T1, T2, T3, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3>> ISelect<T1, T2, T3>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3>).GroupBy((Expression<Func<T1, T2, T3, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).Max((Expression<Func<T1, T2, T3, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).Min((Expression<Func<T1, T2, T3, TMember>>)expModify);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).OrderBy((Expression<Func<T1, T2, T3, TMember>>)expModify);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).OrderByDescending((Expression<Func<T1, T2, T3, TMember>>)expModify);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).OrderByIf(condition, (Expression<Func<T1, T2, T3, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).Sum((Expression<Func<T1, T2, T3, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3>).ToList((Expression<Func<T1, T2, T3, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3>).ToChunk((Expression<Func<T1, T2, T3, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3>).ToDataTable((Expression<Func<T1, T2, T3, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3>).InsertInto(tableName, (Expression<Func<T1, T2, T3, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3>).ToSql((Expression<Func<T1, T2, T3, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3>).LeftJoin((Expression<Func<T1, T2, T3, bool>>)expModify);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3>).InnerJoin((Expression<Func<T1, T2, T3, bool>>)expModify);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3>).RightJoin((Expression<Func<T1, T2, T3, bool>>)expModify);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.Where(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3>).Where((Expression<Func<T1, T2, T3, bool>>)expModify);
        }

        ISelect<T1, T2, T3> ISelect<T1, T2, T3>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3>).WhereIf(condition, (Expression<Func<T1, T2, T3, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3>.Any(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3>).Any((Expression<Func<T1, T2, T3, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select)
            => (this as ISelect<T1, T2, T3>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select)
            => (this as ISelect<T1, T2, T3>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3>.MinAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3>.SumAsync<TMember>(Expression<Func<T1, T2, T3, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3>.AnyAsync(Expression<Func<T1, T2, T3, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).AvgAsync((Expression<Func<T1, T2, T3, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).MaxAsync((Expression<Func<T1, T2, T3, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).MinAsync((Expression<Func<T1, T2, T3, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3>).SumAsync((Expression<Func<T1, T2, T3, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3>).ToListAsync((Expression<Func<T1, T2, T3, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3>).ToDataTableAsync((Expression<Func<T1, T2, T3, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3>).AnyAsync((Expression<Func<T1, T2, T3, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select4Provider<T1, T2, T3, T4> : Select0Provider<ISelect<T1, T2, T3, T4>, T1>, ISelect<T1, T2, T3, T4>
            where T2 : class where T3 : class where T4 : class
    {

        public Select4Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4>> ISelect<T1, T2, T3, T4>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4>.Max<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4>.Min<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.RightJoin(Expression<Func<T1, T2, T3, T4, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.Where(Expression<Func<T1, T2, T3, T4, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4>.Any(Expression<Func<T1, T2, T3, T4, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select) => (this as ISelect<T1, T2, T3, T4>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4>.First<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select) => (this as ISelect<T1, T2, T3, T4>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4>.First<TDto>() => (this as ISelect<T1, T2, T3, T4>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).Avg((Expression<Func<T1, T2, T3, T4, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4>> ISelect<T1, T2, T3, T4>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4>).GroupBy((Expression<Func<T1, T2, T3, T4, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).Max((Expression<Func<T1, T2, T3, T4, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).Min((Expression<Func<T1, T2, T3, T4, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).OrderBy((Expression<Func<T1, T2, T3, T4, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).OrderByDescending((Expression<Func<T1, T2, T3, T4, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).Sum((Expression<Func<T1, T2, T3, T4, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4>).ToList((Expression<Func<T1, T2, T3, T4, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4>).ToChunk((Expression<Func<T1, T2, T3, T4, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4>).ToDataTable((Expression<Func<T1, T2, T3, T4, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4>).ToSql((Expression<Func<T1, T2, T3, T4, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4>).LeftJoin((Expression<Func<T1, T2, T3, T4, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4>).InnerJoin((Expression<Func<T1, T2, T3, T4, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4>).RightJoin((Expression<Func<T1, T2, T3, T4, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4>).Where((Expression<Func<T1, T2, T3, T4, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4> ISelect<T1, T2, T3, T4>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4>).Any((Expression<Func<T1, T2, T3, T4, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4>.AnyAsync(Expression<Func<T1, T2, T3, T4, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).AvgAsync((Expression<Func<T1, T2, T3, T4, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).MaxAsync((Expression<Func<T1, T2, T3, T4, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).MinAsync((Expression<Func<T1, T2, T3, T4, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4>).SumAsync((Expression<Func<T1, T2, T3, T4, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4>).ToListAsync((Expression<Func<T1, T2, T3, T4, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4>).AnyAsync((Expression<Func<T1, T2, T3, T4, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select5Provider<T1, T2, T3, T4, T5> : Select0Provider<ISelect<T1, T2, T3, T4, T5>, T1>, ISelect<T1, T2, T3, T4, T5>
            where T2 : class where T3 : class where T4 : class where T5 : class
    {

        public Select5Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5>> ISelect<T1, T2, T3, T4, T5>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.Where(Expression<Func<T1, T2, T3, T4, T5, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5>.Any(Expression<Func<T1, T2, T3, T4, T5, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).Avg((Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5>> ISelect<T1, T2, T3, T4, T5>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).Max((Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).Min((Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).Sum((Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).ToList((Expression<Func<T1, T2, T3, T4, T5, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).ToSql((Expression<Func<T1, T2, T3, T4, T5, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).Where((Expression<Func<T1, T2, T3, T4, T5, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).Any((Expression<Func<T1, T2, T3, T4, T5, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select6Provider<T1, T2, T3, T4, T5, T6> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6>, T1>, ISelect<T1, T2, T3, T4, T5, T6>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class
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
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";

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
            if (condition == false || column == null) return this;
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
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"));
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

        int ISelect<T1, T2, T3, T4, T5, T6>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
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
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6>> ISelect<T1, T2, T3, T4, T5, T6>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6> ISelect<T1, T2, T3, T4, T5, T6>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select7Provider<T1, T2, T3, T4, T5, T6, T7> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class
    {

        public Select7Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T7)), Alias = $"SP10g", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";
                if (type == _tables[6].Table?.Type && string.IsNullOrEmpty(sqlT7) == false) return $"({sqlT7})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6};\r\n{sqlT7}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6, T7>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7>> ISelect<T1, T2, T3, T4, T5, T6, T7>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"), Expression.Parameter(typeof(T7), "g"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6, T7>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6, T7>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7>> ISelect<T1, T2, T3, T4, T5, T6, T7>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select8Provider<T1, T2, T3, T4, T5, T6, T7, T8> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7, T8>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class
    {

        public Select8Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T7)), Alias = $"SP10g", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T8)), Alias = $"SP10h", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";
                if (type == _tables[6].Table?.Type && string.IsNullOrEmpty(sqlT7) == false) return $"({sqlT7})";
                if (type == _tables[7].Table?.Type && string.IsNullOrEmpty(sqlT8) == false) return $"({sqlT8})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6};\r\n{sqlT7};\r\n{sqlT8}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"), Expression.Parameter(typeof(T7), "g"), Expression.Parameter(typeof(T8), "h"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select9Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class
    {

        public Select9Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T7)), Alias = $"SP10g", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T8)), Alias = $"SP10h", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T9)), Alias = $"SP10i", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";
                if (type == _tables[6].Table?.Type && string.IsNullOrEmpty(sqlT7) == false) return $"({sqlT7})";
                if (type == _tables[7].Table?.Type && string.IsNullOrEmpty(sqlT8) == false) return $"({sqlT8})";
                if (type == _tables[8].Table?.Type && string.IsNullOrEmpty(sqlT9) == false) return $"({sqlT9})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6};\r\n{sqlT7};\r\n{sqlT8};\r\n{sqlT9}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"), Expression.Parameter(typeof(T7), "g"), Expression.Parameter(typeof(T8), "h"), Expression.Parameter(typeof(T9), "i"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select10Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class
    {

        public Select10Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T7)), Alias = $"SP10g", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T8)), Alias = $"SP10h", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T9)), Alias = $"SP10i", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T10)), Alias = $"SP10j", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";
                if (type == _tables[6].Table?.Type && string.IsNullOrEmpty(sqlT7) == false) return $"({sqlT7})";
                if (type == _tables[7].Table?.Type && string.IsNullOrEmpty(sqlT8) == false) return $"({sqlT8})";
                if (type == _tables[8].Table?.Type && string.IsNullOrEmpty(sqlT9) == false) return $"({sqlT9})";
                if (type == _tables[9].Table?.Type && string.IsNullOrEmpty(sqlT10) == false) return $"({sqlT10})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6};\r\n{sqlT7};\r\n{sqlT8};\r\n{sqlT9};\r\n{sqlT10}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"), Expression.Parameter(typeof(T7), "g"), Expression.Parameter(typeof(T8), "h"), Expression.Parameter(typeof(T9), "i"), Expression.Parameter(typeof(T10), "j"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select11Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class
    {

        public Select11Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T7)), Alias = $"SP10g", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T8)), Alias = $"SP10h", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T9)), Alias = $"SP10i", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T10)), Alias = $"SP10j", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T11)), Alias = $"SP10k", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";
                if (type == _tables[6].Table?.Type && string.IsNullOrEmpty(sqlT7) == false) return $"({sqlT7})";
                if (type == _tables[7].Table?.Type && string.IsNullOrEmpty(sqlT8) == false) return $"({sqlT8})";
                if (type == _tables[8].Table?.Type && string.IsNullOrEmpty(sqlT9) == false) return $"({sqlT9})";
                if (type == _tables[9].Table?.Type && string.IsNullOrEmpty(sqlT10) == false) return $"({sqlT10})";
                if (type == _tables[10].Table?.Type && string.IsNullOrEmpty(sqlT11) == false) return $"({sqlT11})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6};\r\n{sqlT7};\r\n{sqlT8};\r\n{sqlT9};\r\n{sqlT10};\r\n{sqlT11}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"), Expression.Parameter(typeof(T7), "g"), Expression.Parameter(typeof(T8), "h"), Expression.Parameter(typeof(T9), "i"), Expression.Parameter(typeof(T10), "j"), Expression.Parameter(typeof(T11), "k"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select12Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class
    {

        public Select12Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T7)), Alias = $"SP10g", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T8)), Alias = $"SP10h", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T9)), Alias = $"SP10i", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T10)), Alias = $"SP10j", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T11)), Alias = $"SP10k", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T12)), Alias = $"SP10l", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, string sqlT12, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";
                if (type == _tables[6].Table?.Type && string.IsNullOrEmpty(sqlT7) == false) return $"({sqlT7})";
                if (type == _tables[7].Table?.Type && string.IsNullOrEmpty(sqlT8) == false) return $"({sqlT8})";
                if (type == _tables[8].Table?.Type && string.IsNullOrEmpty(sqlT9) == false) return $"({sqlT9})";
                if (type == _tables[9].Table?.Type && string.IsNullOrEmpty(sqlT10) == false) return $"({sqlT10})";
                if (type == _tables[10].Table?.Type && string.IsNullOrEmpty(sqlT11) == false) return $"({sqlT11})";
                if (type == _tables[11].Table?.Type && string.IsNullOrEmpty(sqlT12) == false) return $"({sqlT12})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6};\r\n{sqlT7};\r\n{sqlT8};\r\n{sqlT9};\r\n{sqlT10};\r\n{sqlT11};\r\n{sqlT12}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"), Expression.Parameter(typeof(T7), "g"), Expression.Parameter(typeof(T8), "h"), Expression.Parameter(typeof(T9), "i"), Expression.Parameter(typeof(T10), "j"), Expression.Parameter(typeof(T11), "k"), Expression.Parameter(typeof(T12), "l"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select13Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class
    {

        public Select13Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T7)), Alias = $"SP10g", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T8)), Alias = $"SP10h", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T9)), Alias = $"SP10i", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T10)), Alias = $"SP10j", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T11)), Alias = $"SP10k", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T12)), Alias = $"SP10l", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T13)), Alias = $"SP10m", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, string sqlT12, string sqlT13, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";
                if (type == _tables[6].Table?.Type && string.IsNullOrEmpty(sqlT7) == false) return $"({sqlT7})";
                if (type == _tables[7].Table?.Type && string.IsNullOrEmpty(sqlT8) == false) return $"({sqlT8})";
                if (type == _tables[8].Table?.Type && string.IsNullOrEmpty(sqlT9) == false) return $"({sqlT9})";
                if (type == _tables[9].Table?.Type && string.IsNullOrEmpty(sqlT10) == false) return $"({sqlT10})";
                if (type == _tables[10].Table?.Type && string.IsNullOrEmpty(sqlT11) == false) return $"({sqlT11})";
                if (type == _tables[11].Table?.Type && string.IsNullOrEmpty(sqlT12) == false) return $"({sqlT12})";
                if (type == _tables[12].Table?.Type && string.IsNullOrEmpty(sqlT13) == false) return $"({sqlT13})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6};\r\n{sqlT7};\r\n{sqlT8};\r\n{sqlT9};\r\n{sqlT10};\r\n{sqlT11};\r\n{sqlT12};\r\n{sqlT13}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"), Expression.Parameter(typeof(T7), "g"), Expression.Parameter(typeof(T8), "h"), Expression.Parameter(typeof(T9), "i"), Expression.Parameter(typeof(T10), "j"), Expression.Parameter(typeof(T11), "k"), Expression.Parameter(typeof(T12), "l"), Expression.Parameter(typeof(T13), "m"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select14Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class
    {

        public Select14Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T7)), Alias = $"SP10g", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T8)), Alias = $"SP10h", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T9)), Alias = $"SP10i", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T10)), Alias = $"SP10j", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T11)), Alias = $"SP10k", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T12)), Alias = $"SP10l", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T13)), Alias = $"SP10m", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T14)), Alias = $"SP10n", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, string sqlT12, string sqlT13, string sqlT14, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";
                if (type == _tables[6].Table?.Type && string.IsNullOrEmpty(sqlT7) == false) return $"({sqlT7})";
                if (type == _tables[7].Table?.Type && string.IsNullOrEmpty(sqlT8) == false) return $"({sqlT8})";
                if (type == _tables[8].Table?.Type && string.IsNullOrEmpty(sqlT9) == false) return $"({sqlT9})";
                if (type == _tables[9].Table?.Type && string.IsNullOrEmpty(sqlT10) == false) return $"({sqlT10})";
                if (type == _tables[10].Table?.Type && string.IsNullOrEmpty(sqlT11) == false) return $"({sqlT11})";
                if (type == _tables[11].Table?.Type && string.IsNullOrEmpty(sqlT12) == false) return $"({sqlT12})";
                if (type == _tables[12].Table?.Type && string.IsNullOrEmpty(sqlT13) == false) return $"({sqlT13})";
                if (type == _tables[13].Table?.Type && string.IsNullOrEmpty(sqlT14) == false) return $"({sqlT14})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6};\r\n{sqlT7};\r\n{sqlT8};\r\n{sqlT9};\r\n{sqlT10};\r\n{sqlT11};\r\n{sqlT12};\r\n{sqlT13};\r\n{sqlT14}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"), Expression.Parameter(typeof(T7), "g"), Expression.Parameter(typeof(T8), "h"), Expression.Parameter(typeof(T9), "i"), Expression.Parameter(typeof(T10), "j"), Expression.Parameter(typeof(T11), "k"), Expression.Parameter(typeof(T12), "l"), Expression.Parameter(typeof(T13), "m"), Expression.Parameter(typeof(T14), "n"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select15Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class
    {

        public Select15Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T7)), Alias = $"SP10g", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T8)), Alias = $"SP10h", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T9)), Alias = $"SP10i", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T10)), Alias = $"SP10j", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T11)), Alias = $"SP10k", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T12)), Alias = $"SP10l", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T13)), Alias = $"SP10m", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T14)), Alias = $"SP10n", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T15)), Alias = $"SP10o", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, string sqlT12, string sqlT13, string sqlT14, string sqlT15, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";
                if (type == _tables[6].Table?.Type && string.IsNullOrEmpty(sqlT7) == false) return $"({sqlT7})";
                if (type == _tables[7].Table?.Type && string.IsNullOrEmpty(sqlT8) == false) return $"({sqlT8})";
                if (type == _tables[8].Table?.Type && string.IsNullOrEmpty(sqlT9) == false) return $"({sqlT9})";
                if (type == _tables[9].Table?.Type && string.IsNullOrEmpty(sqlT10) == false) return $"({sqlT10})";
                if (type == _tables[10].Table?.Type && string.IsNullOrEmpty(sqlT11) == false) return $"({sqlT11})";
                if (type == _tables[11].Table?.Type && string.IsNullOrEmpty(sqlT12) == false) return $"({sqlT12})";
                if (type == _tables[12].Table?.Type && string.IsNullOrEmpty(sqlT13) == false) return $"({sqlT13})";
                if (type == _tables[13].Table?.Type && string.IsNullOrEmpty(sqlT14) == false) return $"({sqlT14})";
                if (type == _tables[14].Table?.Type && string.IsNullOrEmpty(sqlT15) == false) return $"({sqlT15})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6};\r\n{sqlT7};\r\n{sqlT8};\r\n{sqlT9};\r\n{sqlT10};\r\n{sqlT11};\r\n{sqlT12};\r\n{sqlT13};\r\n{sqlT14};\r\n{sqlT15}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"), Expression.Parameter(typeof(T7), "g"), Expression.Parameter(typeof(T8), "h"), Expression.Parameter(typeof(T9), "i"), Expression.Parameter(typeof(T10), "j"), Expression.Parameter(typeof(T11), "k"), Expression.Parameter(typeof(T12), "l"), Expression.Parameter(typeof(T13), "m"), Expression.Parameter(typeof(T14), "n"), Expression.Parameter(typeof(T15), "o"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }



    public abstract class Select16Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
            where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class where T16 : class
    {

        public Select16Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16));
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T2)), Alias = $"SP10b", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T3)), Alias = $"SP10c", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T4)), Alias = $"SP10d", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T5)), Alias = $"SP10e", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T6)), Alias = $"SP10f", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T7)), Alias = $"SP10g", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T8)), Alias = $"SP10h", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T9)), Alias = $"SP10i", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T10)), Alias = $"SP10j", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T11)), Alias = $"SP10k", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T12)), Alias = $"SP10l", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T13)), Alias = $"SP10m", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T14)), Alias = $"SP10n", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T15)), Alias = $"SP10o", On = null, Type = SelectTableInfoType.From });
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T16)), Alias = $"SP10p", On = null, Type = SelectTableInfoType.From });

        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.WithSql(string sqlT1, string sqlT2, string sqlT3, string sqlT4, string sqlT5, string sqlT6, string sqlT7, string sqlT8, string sqlT9, string sqlT10, string sqlT11, string sqlT12, string sqlT13, string sqlT14, string sqlT15, string sqlT16, object parms)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sqlT1) == false) return $"({sqlT1})";
                if (type == _tables[1].Table?.Type && string.IsNullOrEmpty(sqlT2) == false) return $"({sqlT2})";
                if (type == _tables[2].Table?.Type && string.IsNullOrEmpty(sqlT3) == false) return $"({sqlT3})";
                if (type == _tables[3].Table?.Type && string.IsNullOrEmpty(sqlT4) == false) return $"({sqlT4})";
                if (type == _tables[4].Table?.Type && string.IsNullOrEmpty(sqlT5) == false) return $"({sqlT5})";
                if (type == _tables[5].Table?.Type && string.IsNullOrEmpty(sqlT6) == false) return $"({sqlT6})";
                if (type == _tables[6].Table?.Type && string.IsNullOrEmpty(sqlT7) == false) return $"({sqlT7})";
                if (type == _tables[7].Table?.Type && string.IsNullOrEmpty(sqlT8) == false) return $"({sqlT8})";
                if (type == _tables[8].Table?.Type && string.IsNullOrEmpty(sqlT9) == false) return $"({sqlT9})";
                if (type == _tables[9].Table?.Type && string.IsNullOrEmpty(sqlT10) == false) return $"({sqlT10})";
                if (type == _tables[10].Table?.Type && string.IsNullOrEmpty(sqlT11) == false) return $"({sqlT11})";
                if (type == _tables[11].Table?.Type && string.IsNullOrEmpty(sqlT12) == false) return $"({sqlT12})";
                if (type == _tables[12].Table?.Type && string.IsNullOrEmpty(sqlT13) == false) return $"({sqlT13})";
                if (type == _tables[13].Table?.Type && string.IsNullOrEmpty(sqlT14) == false) return $"({sqlT14})";
                if (type == _tables[14].Table?.Type && string.IsNullOrEmpty(sqlT15) == false) return $"({sqlT15})";
                if (type == _tables[15].Table?.Type && string.IsNullOrEmpty(sqlT16) == false) return $"({sqlT16})";

                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject($"{sqlT1};\r\n{sqlT2};\r\n{sqlT3};\r\n{sqlT4};\r\n{sqlT5};\r\n{sqlT6};\r\n{sqlT7};\r\n{sqlT8};\r\n{sqlT9};\r\n{sqlT10};\r\n{sqlT11};\r\n{sqlT12};\r\n{sqlT13};\r\n{sqlT14};\r\n{sqlT15};\r\n{sqlT16}", parms));
            return this;
        }

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column)
        {
            if (column == null) return default(double);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvg(column?.Body);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TKey>> exp)
        {
            if (exp == null) return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>(exp?.Body);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalGroupBy<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>(exp?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMax<TMember>(column?.Body);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column)
        {
            if (column == null) return default(TMember);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMin<TMember>(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderBy(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalOrderByDescending(column?.Body);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.OrderByIf<TMember>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column, bool descending)
        {
            if (condition == false || column == null) return this;
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return descending ? this.InternalOrderByDescending(column?.Body) : this.InternalOrderBy(column?.Body);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSum(column?.Body);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, ISelectGroupingAggregate<T16>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, ISelectGroupingAggregate<T16>, TReturn>> select, out TReturn result)
        {
            result = (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).ToAggregate(select);
            return this;
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToList<TReturn>(select?.Body);
        }

        List<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToList<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).ToList(GetToListDtoSelector<TDto>());
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDto>> GetToListDtoSelector<TDto>()
        {
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDto>>(
                typeof(TDto).InternalNewExpression(),
                _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a"), Expression.Parameter(typeof(T2), "b"), Expression.Parameter(typeof(T3), "c"), Expression.Parameter(typeof(T4), "d"), Expression.Parameter(typeof(T5), "e"), Expression.Parameter(typeof(T6), "f"), Expression.Parameter(typeof(T7), "g"), Expression.Parameter(typeof(T8), "h"), Expression.Parameter(typeof(T9), "i"), Expression.Parameter(typeof(T10), "j"), Expression.Parameter(typeof(T11), "k"), Expression.Parameter(typeof(T12), "l"), Expression.Parameter(typeof(T13), "m"), Expression.Parameter(typeof(T14), "n"), Expression.Parameter(typeof(T15), "o"), Expression.Parameter(typeof(T16), "p"));
        }

        public void ToChunk<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToDataTable<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTable(select?.Body);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertInto<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertInto<TTargetEntity>(tableName, select?.Body);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp)
        {
            if (exp == null) return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp)
        {
            if (exp == null) return this.Where(null);
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            return this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params));
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Any(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp)
        {
            if (exp == null) return this.Any();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToOne<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.First<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Limit(1).ToList(select).FirstOrDefault();
        TDto ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.First<TDto>() => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Limit(1).ToList<TDto>().FirstOrDefault();





        #region HzyTuple 元组

        double ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Avg<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Avg((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify);
        }

        ISelectGrouping<TKey, NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.GroupBy<TKey>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TKey>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).GroupBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TKey>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Max<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Max((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify);
        }

        TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Min<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Min((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.OrderBy<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).OrderBy((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.OrderByDescending<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).OrderByDescending((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.OrderByIf<TMember>(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column, bool descending)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).OrderByIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify, descending);
        }

        decimal ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Sum<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Sum((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify);
        }

        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToList<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).ToList((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>>)expModify);
        }

        public void ToChunk<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).ToChunk((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>>)expModify, size, done);
        }

        DataTable ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToDataTable<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).ToDataTable((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>>)expModify);
        }

        int ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.InsertInto<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).InsertInto(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTargetEntity>>)expModify);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToSql<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).ToSql((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>>)expModify, fieldAlias);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.LeftJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).LeftJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.InnerJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).InnerJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.RightJoin(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).RightJoin((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Where(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Where((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)expModify);
        }

        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.WhereIf(bool condition, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).WhereIf(condition, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)expModify);
        }

        bool ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Any(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Any((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)expModify);
        }

        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToOne<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Limit(1).ToList(select).FirstOrDefault();
        TReturn ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.First<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select)
            => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Limit(1).ToList(select).FirstOrDefault();

        #endregion



#if net40
#else
        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.AvgAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.MaxAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.MinAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.SumAsync<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> column, CancellationToken cancellationToken)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }

        Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, ISelectGroupingAggregate<T3>, ISelectGroupingAggregate<T4>, ISelectGroupingAggregate<T5>, ISelectGroupingAggregate<T6>, ISelectGroupingAggregate<T7>, ISelectGroupingAggregate<T8>, ISelectGroupingAggregate<T9>, ISelectGroupingAggregate<T10>, ISelectGroupingAggregate<T11>, ISelectGroupingAggregate<T12>, ISelectGroupingAggregate<T13>, ISelectGroupingAggregate<T14>, ISelectGroupingAggregate<T15>, ISelectGroupingAggregate<T16>, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToListAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }
        Task<List<TDto>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToListAsync<TDto>(CancellationToken cancellationToken) => (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.AnyAsync(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> exp, CancellationToken cancellationToken)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToOneAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.FirstAsync<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.FirstAsync<TDto>(CancellationToken cancellationToken) => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).AvgAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).MaxAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify, cancellationToken);
        }

        Task<TMember> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).MinAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify, cancellationToken);
        }

        Task<decimal> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TMember>> column, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).SumAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>>)expModify, cancellationToken);
        }

        Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).ToListAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>>)expModify, cancellationToken);
        }

        Task<DataTable> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).ToDataTableAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>>)expModify, cancellationToken);
        }

        Task<int> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TTargetEntity>> select, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTargetEntity>>)expModify, cancellationToken);
        }

        async Task<bool> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.AnyAsync(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp, CancellationToken cancellationToken)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).AnyAsync((Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>)expModify, cancellationToken);
        }

        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>> select, CancellationToken cancellationToken)
            => (await (this as ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>).Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();


        #endregion

#endif
    }


}


