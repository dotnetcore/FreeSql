using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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
            if (condition == false || column == null) this.InternalOrderBy(column?.Body);
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
        Task<double> ISelect<T1, T2>.AvgAsync<TMember>(Expression<Func<T1, T2, TMember>> column)
        {
            if (column == null) return Task.FromResult(default(double));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalAvgAsync(column?.Body);
        }

        Task<TMember> ISelect<T1, T2>.MaxAsync<TMember>(Expression<Func<T1, T2, TMember>> column)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMaxAsync<TMember>(column?.Body);
        }

        Task<TMember> ISelect<T1, T2>.MinAsync<TMember>(Expression<Func<T1, T2, TMember>> column)
        {
            if (column == null) return Task.FromResult(default(TMember));
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalMinAsync<TMember>(column?.Body);
        }

        Task<decimal> ISelect<T1, T2>.SumAsync<TMember>(Expression<Func<T1, T2, TMember>> column)
        {
            if (column == null) this.InternalOrderBy(column?.Body);
            for (var a = 0; a < column.Parameters.Count; a++) _tables[a].Parameter = column.Parameters[a];
            return this.InternalSumAsync(column?.Body);
        }

        Task<TReturn> ISelect<T1, T2>.ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, ISelectGroupingAggregate<T2>, TReturn>> select)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToAggregateAsync<TReturn>(select?.Body);
        }

        Task<List<TReturn>> ISelect<T1, T2>.ToListAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select)
        {
            if (select == null) return this.InternalToListAsync<TReturn>(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToListAsync<TReturn>(select?.Body);
        }
        Task<List<TDto>> ISelect<T1, T2>.ToListAsync<TDto>() => (this as ISelect<T1, T2>).ToListAsync(GetToListDtoSelector<TDto>());

        Task<DataTable> ISelect<T1, T2>.ToDataTableAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalToDataTableAsync(select?.Body);
        }

        Task<int> ISelect<T1, T2>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, T2, TTargetEntity>> select)
        {
            if (select == null) return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select);
            for (var a = 0; a < select.Parameters.Count; a++) _tables[a].Parameter = select.Parameters[a];
            return this.InternalInsertIntoAsync<TTargetEntity>(tableName, select?.Body);
        }

        async Task<bool> ISelect<T1, T2>.AnyAsync(Expression<Func<T1, T2, bool>> exp)
        {
            if (exp == null) return await this.AnyAsync();
            for (var a = 0; a < exp.Parameters.Count; a++) _tables[a].Parameter = exp.Parameters[a];
            var oldwhere = _where.ToString();
            var ret = await this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null, _whereGlobalFilter, _params)).AnyAsync();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        async Task<TReturn> ISelect<T1, T2>.ToOneAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select) => (await (this as ISelect<T1, T2>).Limit(1).ToListAsync(select)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2>.FirstAsync<TReturn>(Expression<Func<T1, T2, TReturn>> select) => (await (this as ISelect<T1, T2>).Limit(1).ToListAsync(select)).FirstOrDefault();
        async Task<TDto> ISelect<T1, T2>.FirstAsync<TDto>() => (await (this as ISelect<T1, T2>).Limit(1).ToListAsync<TDto>()).FirstOrDefault();




        #region HzyTuple 元组

        Task<double> ISelect<T1, T2>.AvgAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).AvgAsync((Expression<Func<T1, T2, TMember>>)expModify);
        }

        Task<TMember> ISelect<T1, T2>.MaxAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).MaxAsync((Expression<Func<T1, T2, TMember>>)expModify);
        }

        Task<TMember> ISelect<T1, T2>.MinAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).MinAsync((Expression<Func<T1, T2, TMember>>)expModify);
        }

        Task<decimal> ISelect<T1, T2>.SumAsync<TMember>(Expression<Func<HzyTuple<T1, T2>, TMember>> column)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(column, _tables);
            return (this as ISelect<T1, T2>).SumAsync((Expression<Func<T1, T2, TMember>>)expModify);
        }

        Task<List<TReturn>> ISelect<T1, T2>.ToListAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2>).ToListAsync((Expression<Func<T1, T2, TReturn>>)expModify);
        }

        Task<DataTable> ISelect<T1, T2>.ToDataTableAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2>).ToDataTableAsync((Expression<Func<T1, T2, TReturn>>)expModify);
        }

        Task<int> ISelect<T1, T2>.InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<HzyTuple<T1, T2>, TTargetEntity>> select)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(select, _tables);
            return (this as ISelect<T1, T2>).InsertIntoAsync(tableName, (Expression<Func<T1, T2, TTargetEntity>>)expModify);
        }

        async Task<bool> ISelect<T1, T2>.AnyAsync(Expression<Func<HzyTuple<T1, T2>, bool>> exp)
        {
            var expModify = new CommonExpression.ReplaceHzyTupleToMultiParam().Modify(exp, _tables);
            return await (this as ISelect<T1, T2>).AnyAsync((Expression<Func<T1, T2, bool>>)expModify);
        }

        async Task<TReturn> ISelect<T1, T2>.ToOneAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select)
            => (await (this as ISelect<T1, T2>).Limit(1).ToListAsync(select)).FirstOrDefault();
        async Task<TReturn> ISelect<T1, T2>.FirstAsync<TReturn>(Expression<Func<HzyTuple<T1, T2>, TReturn>> select)
            => (await (this as ISelect<T1, T2>).Limit(1).ToListAsync(select)).FirstOrDefault();


        #endregion

#endif
    }


}