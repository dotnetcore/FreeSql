using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface ISelectedQuery<TOut>
    {
        Select0Provider SelectOwner { get; }

#if net40
#else
        Task<List<TOut>> ToListAsync(CancellationToken cancellationToken = default);
        Task<TOut> ToOneAsync(CancellationToken cancellationToken = default);
        Task<TOut> FirstAsync(CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(CancellationToken cancellationToken = default);
        Task<long> CountAsync(CancellationToken cancellationToken = default);
#endif

        List<TOut> ToList();
        TOut ToOne();
        TOut First();

        string ToSql();
        bool Any();

        long Count();
        ISelectedQuery<TOut> Count(out long count);

        ISelectedQuery<TOut> Skip(int offset);
        ISelectedQuery<TOut> Offset(int offset);
        ISelectedQuery<TOut> Limit(int limit);
        ISelectedQuery<TOut> Take(int limit);
        ISelectedQuery<TOut> Page(int pageNumber, int pageSize);

        ISelectedQuery<TOut> Where(Expression<Func<TOut, bool>> exp);
        ISelectedQuery<TOut> WhereIf(bool condition, Expression<Func<TOut, bool>> exp);

        ISelectedQuery<TOut> OrderBy<TMember>(Expression<Func<TOut, TMember>> column);
        ISelectedQuery<TOut> OrderByIf<TMember>(bool condition, Expression<Func<TOut, TMember>> column, bool descending = false);
        ISelectedQuery<TOut> OrderByDescending<TMember>(Expression<Func<TOut, TMember>> column);
    }
}

namespace FreeSql.Internal.CommonProvider
{
    public class SelectedQueryProvider<TOut> : BaseDiyMemberExpression, ISelectedQuery<TOut>
    {
        public Select0Provider _select;
        public CommonExpression _comonExp;
        public  SelectedQueryProvider(Select0Provider select, Expression selector)
        {
            _select = select;
            _comonExp = _select._commonExpression;
            _map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = -10000; //临时规则，不返回 as1

            if (selector != null) 
                _comonExp.ReadAnonymousField(_select._tables, field, _map, ref index, selector, null, null, _select._whereGlobalFilter, null, false); //不走 DTO 映射，不处理 IncludeMany
            _field = field.ToString();
        }

        public override string ParseExp(Expression[] members)
        {
            if (members.Any() == false) return _map.DbField;
            var read = _map;
            for (var a = 0; a < members.Length; a++)
            {
                read = read.Childs.Where(z => z.CsName == (members[a] as MemberExpression)?.Member.Name).FirstOrDefault();
                if (read == null) return null;
            }
            return read.DbField;
        }

        public Select0Provider SelectOwner => _select;

#if net40
#else
        public Task<List<TOut>> ToListAsync(CancellationToken cancellationToken = default)
        {
            var method = _select.GetType().GetMethod("ToListMapReaderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(typeof(TOut));
            return method.Invoke(_select, new object[] { new ReadAnonymousTypeAfInfo(_map, _field.Length > 0 ? _field.Remove(0, 2).ToString() : null), cancellationToken }) as Task<List<TOut>>;
        }
        async public Task<TOut> ToOneAsync(CancellationToken cancellationToken = default) => (await ToListAsync(cancellationToken)).FirstOrDefault();
        public Task<TOut> FirstAsync(CancellationToken cancellationToken = default) => ToOneAsync(cancellationToken);

        public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            var method = _select.GetType().GetMethod("AnyAsync", new Type[] { typeof(CancellationToken) });
            return method.Invoke(_select, new object[] { cancellationToken }) as Task<bool>;
        }
        public Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            var method = _select.GetType().GetMethod("CountAsync", new Type[] { typeof(CancellationToken) });
            return method.Invoke(_select, new object[] { cancellationToken }) as Task<long>;
        }
#endif

        public List<TOut> ToList()
        {
            var method = _select.GetType().GetMethod("ToListMapReader", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(typeof(TOut));
            return method.Invoke(_select, new object[] { new ReadAnonymousTypeAfInfo(_map, _field.Length > 0 ? _field.Remove(0, 2).ToString() : null) }) as List<TOut>;
        }
        public TOut ToOne() => ToList().FirstOrDefault();
        public TOut First() => ToOne();

        public string ToSql()
        {
            var method = _select.GetType().GetMethod("ToSql", new[] { typeof(string) });
            return method.Invoke(_select, new object[] { _field.Length > 0 ? _field.Remove(0, 2).ToString() : null }) as string;
        }

        public bool Any()
        {
            var method = _select.GetType().GetMethod("Any", new Type[0]);
            return (bool)method.Invoke(_select, new object[0]);
        }
        public long Count()
        {
            var method = _select.GetType().GetMethod("Count", new Type[0]);
            return (long)method.Invoke(_select, new object[0]);
        }
        public ISelectedQuery<TOut> Count(out long count)
        {
            count = this.Count();
            return this;
        }

        public ISelectedQuery<TOut> Skip(int offset)
        {
            _select._skip = offset;
            return this;
        }
        public ISelectedQuery<TOut> Offset(int offset) => Skip(offset);
        public ISelectedQuery<TOut> Limit(int limit) => Take(limit);
        public ISelectedQuery<TOut> Take(int limit)
        {
            _select._limit = limit;
            return this;
        }
        public ISelectedQuery<TOut> Page(int pageNumber, int pageSize)
        {
            this.Skip(Math.Max(0, pageNumber - 1) * pageSize);
            return this.Limit(pageSize);
        }

        public ISelectedQuery<TOut> OrderBy<TMember>(Expression<Func<TOut, TMember>> column) => OrderByIf(true, column);
        public ISelectedQuery<TOut> OrderByIf<TMember>(bool condition, Expression<Func<TOut, TMember>> column, bool descending = false)
        {
            if (condition == false) return this;
            _lambdaParameter = column?.Parameters[0];
            var sql = _comonExp.ExpressionWhereLambda(null, column, this, null, null);
            var method = _select.GetType().GetMethod("OrderBy", new[] { typeof(string), typeof(object) });
            method.Invoke(_select, new object[] { descending ? $"{sql} DESC" : sql, null });
            return this;
        }
        public ISelectedQuery<TOut> OrderByDescending<TMember>(Expression<Func<TOut, TMember>> column) => OrderByIf(true, column, true);

        public ISelectedQuery<TOut> Where(Expression<Func<TOut, bool>> exp) => WhereIf(true, exp);
        public ISelectedQuery<TOut> WhereIf(bool condition, Expression<Func<TOut, bool>> exp)
        {
            if (condition == false) return this;
            _lambdaParameter = exp?.Parameters[0];
            var sql = _comonExp.ExpressionWhereLambda(null, exp, this, null, null);
            var method = _select.GetType().GetMethod("Where", new[] { typeof(string), typeof(object) });
            method.Invoke(_select, new object[] { sql, null });
            return this;
        }
    }
}
