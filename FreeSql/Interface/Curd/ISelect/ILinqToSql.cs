using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql
{
    public interface ILinqToSql<T1> where T1 : class
    {
        /// <summary>
        /// 【linq to sql】专用方法，不建议直接使用
        /// </summary>
        ISelect<TReturn> Select<TReturn>(Expression<Func<T1, TReturn>> select) where TReturn : class;
        /// <summary>
        /// 【linq to sql】专用方法，不建议直接使用
        /// </summary>
        ISelect<TResult> Join<TInner, TKey, TResult>(ISelect<TInner> inner, Expression<Func<T1, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<T1, TInner, TResult>> resultSelector) where TInner : class where TResult : class;
        /// <summary>
        /// 【linq to sql】专用方法，不建议直接使用
        /// </summary>
        ISelect<TResult> GroupJoin<TInner, TKey, TResult>(ISelect<TInner> inner, Expression<Func<T1, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<T1, ISelect<TInner>, TResult>> resultSelector) where TInner : class where TResult : class;
        /// <summary>
        /// 【linq to sql】专用方法，不建议直接使用
        /// </summary>
        ISelect<T1> DefaultIfEmpty();
        /// <summary>
        /// 【linq to sql】专用方法，不建议直接使用
        /// </summary>
        ISelect<TResult> SelectMany<TCollection, TResult>(Expression<Func<T1, ISelect<TCollection>>> collectionSelector, Expression<Func<T1, TCollection, TResult>> resultSelector) where TCollection : class where TResult : class;
    }
}
