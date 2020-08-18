using FreeSql.DataAnnotations;
using FreeSql.Extensions.EntityUtil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.Internal.Model
{
    public class ColumnInfo
    {
        public TableInfo Table { get; set; }
        public string CsName { get; set; }
        public Type CsType { get; set; }
        public ColumnAttribute Attribute { get; set; }
        public string Comment { get; internal set; }
        public string DbTypeText { get; internal set; }
        public string DbDefaultValue { get; internal set; }
        public string DbInsertValue { get; internal set; }
        public string DbUpdateValue { get; internal set; }
        public int DbSize { get; internal set; }
        public byte DbPrecision { get; internal set; }
        public byte DbScale { get; internal set; }
        //public Func<object, object> ConversionCsToDb { get; internal set; }
        //public Func<object, object> ConversionDbToCs { get; internal set; }

        /// <summary>
        /// 获取 obj.CsName 属性值 MapType 之后的数据库值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object GetDbValue(object obj)
        {
            var dbval = Table.GetPropertyValue(obj, CsName);
            //if (ConversionCsToDb != null) dbval = ConversionCsToDb(dbval);
            if (Attribute.MapType != CsType) dbval = Utils.GetDataReaderValue(Attribute.MapType, dbval);
            return dbval;
        }
        /// <summary>
        /// 获取 obj.CsName 属性原始值（不经过 MapType）
        /// </summary>
        /// <param name="obj"></param>
        public object GetValue(object obj) => Table.GetPropertyValue(obj, CsName);
        /// <summary>
        /// 设置 obj.CsName 属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="val"></param>
        public void SetValue(object obj, object val) => Table.SetPropertyValue(obj, CsName, Utils.GetDataReaderValue(CsType, val));



        static ConcurrentDictionary<ColumnInfo, Func<object, object>> _dicGetMapValue = new ConcurrentDictionary<ColumnInfo, Func<object, object>>();
        [Obsolete("请使用 GetDbValue 或者 GetValue")]
        public object GetMapValue(object obj)
        {
            var func = _dicGetMapValue.GetOrAdd(this, col =>
            {
                var paramExp = Expression.Parameter(typeof(object));
                var returnTarget = Expression.Label(typeof(object));

                if (Attribute.MapType == CsType)
                    return Expression.Lambda<Func<object, object>>(
                        Expression.Block(
                            Expression.Return(returnTarget, Expression.Convert(
                                Expression.MakeMemberAccess(
                                    Expression.TypeAs(paramExp, col.Table.Type),
                                    Table.Properties[col.CsName]
                                ), typeof(object))),
                            Expression.Label(returnTarget, Expression.Default(typeof(object)))
                        ), new[] { paramExp }).Compile();

                var retExp = Expression.Variable(typeof(object), "ret");
                var blockExp = new List<Expression>();
                blockExp.AddRange(new Expression[] {
                    Expression.Assign(retExp, Utils.GetDataReaderValueBlockExpression(Attribute.MapType,
                        Expression.MakeMemberAccess(
                            Expression.TypeAs(paramExp, col.Table.Type),
                            Table.Properties[col.CsName]
                        )
                    )),
                    Expression.Return(returnTarget, retExp),
                    Expression.Label(returnTarget, Expression.Default(typeof(object)))
                });
                return Expression.Lambda<Func<object, object>>(Expression.Block(new[] { retExp }, blockExp), new[] { paramExp }).Compile();
            });
            return func(obj);
        }
        static ConcurrentDictionary<ColumnInfo, Action<object, object>> _dicSetMapValue = new ConcurrentDictionary<ColumnInfo, Action<object, object>>();
        [Obsolete("请使用 SetValue")]
        public void SetMapValue(object obj, object val)
        {
            var func = _dicSetMapValue.GetOrAdd(this, col =>
            {
                var objExp = Expression.Parameter(typeof(object), "obj");
                var valExp = Expression.Parameter(typeof(object), "val");

                //if (Attribute.MapType == CsType)
                //    return Expression.Lambda<Action<object, object>>(
                //        Expression.Assign(Expression.MakeMemberAccess(
                //            Expression.TypeAs(objExp, col.Table.Type),
                //            Table.Properties[col.CsName]
                //        ), Expression.Convert(
                //            valExp,
                //            Attribute.MapType)), objExp, valExp).Compile();

                return Expression.Lambda<Action<object, object>>(
                    Expression.Assign(Expression.MakeMemberAccess(
                        Expression.TypeAs(objExp, col.Table.Type),
                        Table.Properties[col.CsName]
                    ), Expression.Convert(
                        Utils.GetDataReaderValueBlockExpression(CsType, valExp),
                        CsType)), objExp, valExp).Compile();
            });
            func(obj, val);
        }
    }
}