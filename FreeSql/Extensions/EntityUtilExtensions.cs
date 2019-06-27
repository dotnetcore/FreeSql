using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Extensions.EntityUtil
{
    public static class EntityUtilExtensions
    {

        static readonly MethodInfo MethodStringBuilderAppend = typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(object) });
        static readonly MethodInfo MethodStringBuilderToString = typeof(StringBuilder).GetMethod("ToString", new Type[0]);
        static readonly PropertyInfo MethodStringBuilderLength = typeof(StringBuilder).GetProperty("Length");
        static readonly MethodInfo MethodStringConcat = typeof(string).GetMethod("Concat", new Type[] { typeof(object) });
        static readonly MethodInfo MethodFreeUtilNewMongodbId = typeof(FreeUtil).GetMethod("NewMongodbId");

        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, bool, string>>> _dicGetEntityKeyString = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, bool, string>>>();
        /// <summary>
        /// 获取实体的主键值，以 "*|_,[,_|*" 分割，当任意一个主键属性无值时，返回 null
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity"></param>
        /// <param name="genGuid">当Guid无值时，会生成有序的新值</param>
        /// <param name="splitString"></param>
        /// <returns></returns>
        //public static string GetEntityKeyString<TEntity>(this IFreeSql orm, TEntity entity, string splitString = "*|_,[,_|*") => GetEntityKeyString(orm, typeof(TEntity), entity, splitString);
        public static string GetEntityKeyString(this IFreeSql orm, Type entityType, object entity, bool genGuid, string splitString = "*|_,[,_|*")
        {
            if (entity == null) return null;
            if (entityType == null) entityType = entity.GetType();
            var func = _dicGetEntityKeyString.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Func<object, bool, string>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var pks = _table.Primarys;
                var returnTarget = Expression.Label(typeof(string));
                var parm1 = Expression.Parameter(typeof(object));
                var parm2 = Expression.Parameter(typeof(bool));
                var var1Parm = Expression.Variable(t);
                var var2Sb = Expression.Variable(typeof(StringBuilder));
                var var3IsNull = Expression.Variable(typeof(bool));
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
                    Expression.Assign(var2Sb, Expression.New(typeof(StringBuilder))),
                    Expression.Assign(var3IsNull, Expression.Constant(false))
                });
                for (var a = 0; a < pks.Length; a++)
                {
                    var isguid = pks[a].Attribute.MapType.NullableTypeOrThis() == typeof(Guid) || pks[a].CsType.NullableTypeOrThis() == typeof(Guid);
                    Expression expthen = null;
                    if (isguid)
                    {
                        if (pks[a].Attribute.MapType == pks[a].CsType)
                        {
                            expthen = Expression.Block(
                                new Expression[]{
                                    Expression.Assign(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), Expression.Call(MethodFreeUtilNewMongodbId)),
                                    a > 0 ? Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant(splitString)) : null,
                                    Expression.Call(var2Sb, MethodStringBuilderAppend,
                                        Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), typeof(object))
                                    )
                                }.Where(c => c != null).ToArray()
                            );
                        }
                        else
                        {
                            expthen = Expression.Block(
                                new Expression[]{
                                    Expression.Assign(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(pks[a].CsType, Expression.Call(MethodFreeUtilNewMongodbId))),
                                    a > 0 ? Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant(splitString)) : null,
                                    Expression.Call(var2Sb, MethodStringBuilderAppend,
                                        Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), typeof(object))
                                    )
                                }.Where(c => c != null).ToArray()
                            );
                        }
                    }
                    else if (pks.Length > 1 && pks[a].Attribute.IsIdentity)
                    {
                        expthen = Expression.Block(
                                new Expression[]{
                                    a > 0 ? Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant(splitString)) : null,
                                    Expression.Call(var2Sb, MethodStringBuilderAppend,
                                        Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), typeof(object))
                                    )
                                }.Where(c => c != null).ToArray()
                            );
                    }
                    else
                    {
                        expthen = Expression.Assign(var3IsNull, Expression.Constant(true));
                    }
                    if (pks[a].Attribute.IsIdentity || isguid || pks[a].CsType == typeof(string) || pks[a].CsType.IsNullableType())
                    {
                        exps.Add(
                            Expression.IfThen(
                                Expression.IsFalse(var3IsNull),
                                Expression.IfThenElse(
                                    Expression.Equal(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), Expression.Default(pks[a].CsType)),
                                    Expression.IfThen(
                                        Expression.IsTrue(parm2),
                                        expthen
                                    ),
                                    Expression.Block(
                                        new Expression[]{
                                            a > 0 ? Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant(splitString)) : null,
                                            Expression.Call(var2Sb, MethodStringBuilderAppend,
                                                Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), typeof(object))
                                            )
                                        }.Where(c => c != null).ToArray()
                                    )
                                )
                            )
                        );
                    }
                    else
                    {
                        exps.Add(
                            Expression.IfThen(
                                Expression.IsFalse(var3IsNull),
                                Expression.Block(
                                    new Expression[]{
                                        a > 0 ? Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant(splitString)) : null,
                                        Expression.Call(var2Sb, MethodStringBuilderAppend,
                                            Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), typeof(object))
                                        )
                                    }.Where(c => c != null).ToArray()
                                )
                            )
                        );
                    }
                }
                exps.Add(
                    Expression.IfThen(
                        Expression.IsFalse(var3IsNull),
                        Expression.Return(returnTarget, Expression.Call(var2Sb, MethodStringBuilderToString))
                    )
                );
                exps.Add(Expression.Label(returnTarget, Expression.Default(typeof(string))));
                return Expression.Lambda<Func<object, bool, string>>(Expression.Block(new[] { var1Parm, var2Sb, var3IsNull }, exps), new[] { parm1, parm2 }).Compile();
            });
            return func(entity, genGuid);
        }
        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, object[]>>> _dicGetEntityKeyValues = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, object[]>>>();
        /// <summary>
        /// 获取实体的主键值，多个主键返回数组
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        //public static object[] GetEntityKeyValues<TEntity>(this IFreeSql orm, TEntity entity) => GetEntityKeyValues(orm, typeof(TEntity), entity);
        public static object[] GetEntityKeyValues(this IFreeSql orm, Type entityType, object entity)
        {
            if (entity == null) return null;
            if (entityType == null) entityType = entity.GetType();
            var func = _dicGetEntityKeyValues.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Func<object, object[]>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var pks = _table.Primarys;
                var returnTarget = Expression.Label(typeof(object[]));
                var parm1 = Expression.Parameter(typeof(object));
                var var1Parm = Expression.Variable(t);
                var var2Ret = Expression.Variable(typeof(object[]));
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
                    Expression.Assign(var2Ret, Expression.NewArrayBounds(typeof(object), Expression.Constant(pks.Length))),
                });
                for (var a = 0; a < pks.Length; a++)
                {
                    exps.Add(
                        Expression.Assign(
                            Expression.ArrayAccess(var2Ret, Expression.Constant(a)),
                            Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), typeof(object))
                        )
                    );
                }
                exps.AddRange(new Expression[] {
                    Expression.Return(returnTarget, var2Ret),
                    Expression.Label(returnTarget, Expression.Default(typeof(object[])))
                });
                return Expression.Lambda<Func<object, object[]>>(Expression.Block(new[] { var1Parm, var2Ret }, exps), new[] { parm1 }).Compile();
            });
            return func(entity);
        }
        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object, object>>>> _dicGetEntityValueWithPropertyName = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object, object>>>>();
        /// <summary>
        /// 获取实体的属性值
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        //public static object GetEntityValueWithPropertyName<TEntity>(this IFreeSql orm, TEntity entity, string propertyName) => GetEntityKeyValues(orm, typeof(TEntity), entity, propertyName);
        public static object GetEntityValueWithPropertyName(this IFreeSql orm, Type entityType, object entity, string propertyName)
        {
            if (entity == null) return null;
            if (entityType == null) entityType = entity.GetType();
            var func = _dicGetEntityValueWithPropertyName
                .GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object, object>>>())
                .GetOrAdd(entityType, et => new ConcurrentDictionary<string, Func<object, object>>())
                .GetOrAdd(propertyName, pn =>
                {
                    var _table = orm.CodeFirst.GetTableByEntity(entityType);
                    var pks = _table.Primarys;
                    var returnTarget = Expression.Label(typeof(object));
                    var parm1 = Expression.Parameter(typeof(object));
                    var var1Parm = Expression.Variable(entityType);
                    var var2Ret = Expression.Variable(typeof(object));
                    var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, entityType)),
                    Expression.Assign(
                        var2Ret,
                        Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[pn]), typeof(object))
                    )
                });
                    exps.AddRange(new Expression[] {
                    Expression.Return(returnTarget, var2Ret),
                    Expression.Label(returnTarget, Expression.Default(typeof(object)))
                });
                    return Expression.Lambda<Func<object, object>>(Expression.Block(new[] { var1Parm, var2Ret }, exps), new[] { parm1 }).Compile();
                });
            return func(entity);
        }
        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, string>>> _dicGetEntityString = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, string>>>();
        /// <summary>
        /// 获取实体的所有数据，以 (1, 2, xxx) 的形式
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        //public static string GetEntityString<TEntity>(this IFreeSql orm, TEntity entity) => GetEntityString(orm, typeof(TEntity), entity);
        public static string GetEntityString(this IFreeSql orm, Type entityType, object entity)
        {
            if (entity == null) return null;
            if (entityType == null) entityType = entity.GetType();
            var func = _dicGetEntityString.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Func<object, string>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var cols = _table.Columns;
                var returnTarget = Expression.Label(typeof(string));
                var parm1 = Expression.Parameter(typeof(object));
                var var1Parm = Expression.Variable(t);
                var var2Sb = Expression.Variable(typeof(StringBuilder));
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
                    Expression.Assign(var2Sb, Expression.New(typeof(StringBuilder))),
                    Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant("(" ))
                });
                var a = 0;
                foreach (var col in cols.Values)
                {
                    exps.Add(
                        Expression.Block(
                            new Expression[]{
                                a > 0 ? Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant(", " )) : null,
                                Expression.Call(var2Sb, MethodStringBuilderAppend,
                                    Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[col.CsName]), typeof(object))
                                )
                            }.Where(c => c != null).ToArray()
                        )
                    );
                    a++;
                }
                exps.AddRange(new Expression[] {
                    Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant(")" )),
                    Expression.Return(returnTarget, Expression.Call(var2Sb, MethodStringBuilderToString)),
                    Expression.Label(returnTarget, Expression.Default(typeof(string)))
                });
                return Expression.Lambda<Func<object, string>>(Expression.Block(new[] { var1Parm, var2Sb }, exps), new[] { parm1 }).Compile();
            });
            return func(entity);
        }

        /// <summary>
        /// 使用新实体的值，复盖旧实体的值
        /// </summary>
        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, object>>> _dicMapEntityValue = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, object>>>();
        //public static void MapEntityValue<TEntity>(this IFreeSql orm, TEntity entityFrom, TEntity entityTo) => MapEntityValue(orm, typeof(TEntity), entityFrom, entityTo);
        public static void MapEntityValue(this IFreeSql orm, Type entityType, object entityFrom, object entityTo)
        {
            if (entityType == null) entityType = entityFrom?.GetType() ?? entityTo?.GetType();
            var func = _dicMapEntityValue.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object, object>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var parm1 = Expression.Parameter(typeof(object));
                var parm2 = Expression.Parameter(typeof(object));
                var var1Parm = Expression.Variable(t);
                var var2Parm = Expression.Variable(t);
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
                    Expression.Assign(var2Parm, Expression.TypeAs(parm2, t))
                });
                foreach (var prop in _table.Properties.Values)
                {
                    if (_table.ColumnsByCsIgnore.ContainsKey(prop.Name)) continue;
                    if (_table.ColumnsByCs.ContainsKey(prop.Name))
                    {
                        exps.Add(
                            Expression.Assign(
                                Expression.MakeMemberAccess(var2Parm, prop),
                                Expression.MakeMemberAccess(var1Parm, prop)
                            )
                        );
                    }

                    //else if (prop.GetSetMethod() != null) {
                    //	exps.Add(
                    //		Expression.Assign(
                    //			Expression.MakeMemberAccess(var2Parm, prop),
                    //			Expression.Default(prop.PropertyType)
                    //		)
                    //	);
                    //}
                }
                return Expression.Lambda<Action<object, object>>(Expression.Block(new[] { var1Parm, var2Parm }, exps), new[] { parm1, parm2 }).Compile();
            });
            func(entityFrom, entityTo);
        }

        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, object>>> _dicMapEntityKeyValue = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, object>>>();
        /// <summary>
        /// 使用新实体的主键值，复盖旧实体的主键值
        /// </summary>
        //public static void MapEntityKeyValue<TEntity>(this IFreeSql orm, TEntity entityFrom, TEntity entityTo) => MapEntityKeyValue(orm, typeof(TEntity), entityFrom, entityTo);
        public static void MapEntityKeyValue(this IFreeSql orm, Type entityType, object entityFrom, object entityTo)
        {
            if (entityType == null) entityType = entityFrom?.GetType() ?? entityTo?.GetType();
            var func = _dicMapEntityKeyValue.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object, object>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var pks = _table.Primarys;
                var parm1 = Expression.Parameter(typeof(object));
                var parm2 = Expression.Parameter(typeof(object));
                var var1Parm = Expression.Variable(t);
                var var2Parm = Expression.Variable(t);
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
                    Expression.Assign(var2Parm, Expression.TypeAs(parm2, t))
                });
                foreach (var pk in pks)
                {
                    exps.Add(
                        Expression.Assign(
                            Expression.MakeMemberAccess(var2Parm, _table.Properties[pk.CsName]),
                            Expression.MakeMemberAccess(var1Parm, _table.Properties[pk.CsName])
                        )
                    );
                }
                return Expression.Lambda<Action<object, object>>(Expression.Block(new[] { var1Parm, var2Parm }, exps), new[] { parm1, parm2 }).Compile();
            });
            func(entityFrom, entityTo);
        }

        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, long>>> _dicSetEntityIdentityValueWithPrimary = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, long>>>();
        /// <summary>
        /// 设置实体中主键内的自增字段值（若存在）
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity"></param>
        /// <param name="idtval"></param>
        //public static void SetEntityIdentityValueWithPrimary<TEntity>(this IFreeSql orm, TEntity entity, long idtval) => SetEntityIdentityValueWithPrimary(orm, typeof(TEntity), entity, idtval);
        public static void SetEntityIdentityValueWithPrimary(this IFreeSql orm, Type entityType, object entity, long idtval)
        {
            if (entity == null) return;
            if (entityType == null) entityType = entity.GetType();
            var func = _dicSetEntityIdentityValueWithPrimary.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object, long>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var identitys = _table.Primarys.Where(a => a.Attribute.IsIdentity);
                var parm1 = Expression.Parameter(typeof(object));
                var parm2 = Expression.Parameter(typeof(long));
                var var1Parm = Expression.Variable(t);
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t))
                });
                if (identitys.Any())
                {
                    var idts0 = identitys.First();
                    exps.Add(
                        Expression.Assign(
                            Expression.MakeMemberAccess(var1Parm, _table.Properties[idts0.CsName]),
                            Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(idts0.CsType, Expression.Convert(parm2, typeof(object))), idts0.CsType)
                        )
                    );
                }
                return Expression.Lambda<Action<object, long>>(Expression.Block(new[] { var1Parm }, exps), new[] { parm1, parm2 }).Compile();
            });
            func(entity, idtval);
        }
        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, long>>> _dicGetEntityIdentityValueWithPrimary = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, long>>>();
        /// <summary>
        /// 获取实体中主键内的自增字段值（若存在）
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity"></param>
        //public static long GetEntityIdentityValueWithPrimary<TEntity>(this IFreeSql orm, TEntity entity) => GetEntityIdentityValueWithPrimary(orm, typeof(TEntity), entity);
        public static long GetEntityIdentityValueWithPrimary(this IFreeSql orm, Type entityType, object entity)
        {
            if (entity == null) return 0;
            if (entityType == null) entityType = entity.GetType();
            var func = _dicGetEntityIdentityValueWithPrimary.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Func<object, long>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var identitys = _table.Primarys.Where(a => a.Attribute.IsIdentity);
                var returnTarget = Expression.Label(typeof(long));
                var parm1 = Expression.Parameter(typeof(object));
                var var1Parm = Expression.Variable(t);
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t))
                });
                if (identitys.Any())
                {
                    var idts0 = identitys.First();
                    exps.Add(
                        Expression.IfThen(
                            Expression.NotEqual(
                                Expression.MakeMemberAccess(var1Parm, _table.Properties[idts0.CsName]),
                                Expression.Default(idts0.CsType)
                            ),
                            Expression.Return(
                                returnTarget,
                                Expression.Convert(
                                    FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(
                                        typeof(long),
                                        Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[idts0.CsName]), typeof(object))
                                    ),
                                    typeof(long)
                                )
                            )
                        )
                    );
                }
                exps.Add(Expression.Label(returnTarget, Expression.Default(typeof(long))));
                return Expression.Lambda<Func<object, long>>(Expression.Block(new[] { var1Parm }, exps), new[] { parm1 }).Compile();
            });
            return func(entity);
        }

        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object>>> _dicClearEntityPrimaryValueWithIdentityAndGuid = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object>>>();
        /// <summary>
        /// 清除实体的主键值，将自增、Guid类型的主键值清除
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity"></param>
        //public static void ClearEntityPrimaryValueWithIdentityAndGuid<TEntity>(this IFreeSql orm, TEntity entity) => ClearEntityPrimaryValueWithIdentityAndGuid(orm, typeof(TEntity), entity);
        public static void ClearEntityPrimaryValueWithIdentityAndGuid(this IFreeSql orm, Type entityType, object entity)
        {
            if (entity == null) return;
            if (entityType == null) entityType = entity.GetType();
            var func = _dicClearEntityPrimaryValueWithIdentityAndGuid.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var identitys = _table.Primarys.Where(a => a.Attribute.IsIdentity);
                var parm1 = Expression.Parameter(typeof(object));
                var var1Parm = Expression.Variable(t);
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t))
                });
                foreach (var pk in _table.Primarys)
                {
                    if (pk.Attribute.IsIdentity || pk.Attribute.MapType == pk.CsType && pk.Attribute.MapType.NullableTypeOrThis() == typeof(Guid))
                    {
                        exps.Add(
                            Expression.Assign(
                                Expression.MakeMemberAccess(var1Parm, _table.Properties[pk.CsName]),
                                Expression.Default(pk.CsType)
                            )
                        );
                        continue;
                    }
                    if (pk.Attribute.MapType != pk.CsType && (pk.Attribute.MapType.NullableTypeOrThis() == typeof(Guid) || pk.CsType.NullableTypeOrThis() == typeof(Guid)))
                    {
                        exps.Add(
                            Expression.Assign(
                                Expression.MakeMemberAccess(var1Parm, _table.Properties[pk.CsName]),
                                FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(pk.CsType, Expression.Default(pk.Attribute.MapType))
                            )
                        );
                        continue;
                    }
                }
                return Expression.Lambda<Action<object>>(Expression.Block(new[] { var1Parm }, exps), new[] { parm1 }).Compile();
            });
            func(entity);
        }

        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object>>> _dicClearEntityPrimaryValueWithIdentity = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object>>>();
        /// <summary>
        /// 清除实体的主键值，将自增、Guid类型的主键值清除
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity"></param>
        //public static void ClearEntityPrimaryValueWithIdentity<TEntity>(this IFreeSql orm, TEntity entity) => ClearEntityPrimaryValueWithIdentity(orm, typeof(TEntity), entity);
        public static void ClearEntityPrimaryValueWithIdentity(this IFreeSql orm, Type entityType, object entity)
        {
            if (entity == null) return;
            if (entityType == null) entityType = entity.GetType();
            var func = _dicClearEntityPrimaryValueWithIdentity.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var identitys = _table.Primarys.Where(a => a.Attribute.IsIdentity);
                var parm1 = Expression.Parameter(typeof(object));
                var var1Parm = Expression.Variable(t);
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t))
                });
                foreach (var pk in _table.Primarys)
                {
                    if (pk.Attribute.IsIdentity)
                    {
                        exps.Add(
                            Expression.Assign(
                                Expression.MakeMemberAccess(var1Parm, _table.Properties[pk.CsName]),
                                Expression.Default(pk.CsType)
                            )
                        );
                    }
                }
                return Expression.Lambda<Action<object>>(Expression.Block(new[] { var1Parm }, exps), new[] { parm1 }).Compile();
            });
            func(entity);
        }

        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, object, bool, string[]>>> _dicCompareEntityValueReturnColumns = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, object, bool, string[]>>>();
        /// <summary>
        /// 对比两个实体值，返回相同/或不相同的列名
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity1"></param>
        /// <param name="entity2"></param>
        /// <param name="isEqual"></param>
        /// <returns></returns>
        //public static string[] CompareEntityValueReturnColumns<TEntity>(this IFreeSql orm, TEntity entity1, TEntity entity2, bool isEqual) => CompareEntityValueReturnColumns(orm, typeof(TEntity), entity1, entity2, isEqual);
        public static string[] CompareEntityValueReturnColumns(this IFreeSql orm, Type entityType, object entity1, object entity2, bool isEqual)
        {
            if (entityType == null) entityType = entity1?.GetType() ?? entity2?.GetType();
            var func = _dicCompareEntityValueReturnColumns.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Func<object, object, bool, string[]>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var returnTarget = Expression.Label(typeof(string[]));
                var parm1 = Expression.Parameter(typeof(object));
                var parm2 = Expression.Parameter(typeof(object));
                var parm3 = Expression.Parameter(typeof(bool));
                var var1Ret = Expression.Variable(typeof(List<string>));
                var var1Parm = Expression.Variable(t);
                var var2Parm = Expression.Variable(t);
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
                    Expression.Assign(var2Parm, Expression.TypeAs(parm2, t)),
                    Expression.Assign(var1Ret, Expression.New(typeof(List<string>)))
                });
                var a = 0;
                foreach (var prop in _table.Properties.Values)
                {
                    if (_table.ColumnsByCs.TryGetValue(prop.Name, out var trycol) == false) continue;
                    exps.Add(
                        Expression.IfThenElse(
                            Expression.Equal(
                                Expression.MakeMemberAccess(var1Parm, prop),
                                Expression.MakeMemberAccess(var2Parm, prop)
                            ),
                            Expression.IfThen(
                                Expression.IsTrue(parm3),
                                Expression.Call(var1Ret, typeof(List<string>).GetMethod("Add", new Type[] { typeof(string) }), Expression.Constant(trycol.Attribute.Name))
                            ),
                            Expression.IfThen(
                                Expression.IsFalse(parm3),
                                Expression.Call(var1Ret, typeof(List<string>).GetMethod("Add", new Type[] { typeof(string) }), Expression.Constant(trycol.Attribute.Name))
                            )
                        )
                    );
                    a++;
                }
                exps.Add(Expression.Return(returnTarget, Expression.Call(var1Ret, typeof(List<string>).GetMethod("ToArray", new Type[0]))));
                exps.Add(Expression.Label(returnTarget, Expression.Constant(new string[0])));
                return Expression.Lambda<Func<object, object, bool, string[]>>(Expression.Block(new[] { var1Ret, var1Parm, var2Parm }, exps), new[] { parm1, parm2, parm3 }).Compile();
            });
            return func(entity1, entity2, isEqual);
        }

        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, string, int>>> _dicSetEntityIncrByWithPropertyName = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, string, int>>>();
        /// <summary>
        /// 设置实体中某属性的数值增加指定的值
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="incrBy"></param>
        //public static void SetEntityIncrByWithPropertyName<TEntity>(this IFreeSql orm, TEntity entity, string propertyName, int incrBy) => SetEntityIncrByWithPropertyName(orm, typeof(TEntity), entity, propertyName, incrBy);
        public static void SetEntityIncrByWithPropertyName(this IFreeSql orm, Type entityType, object entity, string propertyName, int incrBy)
        {
            if (entity == null) return;
            if (entityType == null) entityType = entity.GetType();
            var func = _dicSetEntityIncrByWithPropertyName.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object, string, int>>()).GetOrAdd(entityType, t =>
            {
                var _table = orm.CodeFirst.GetTableByEntity(t);
                var parm1 = Expression.Parameter(typeof(object));
                var parm2 = Expression.Parameter(typeof(string));
                var parm3 = Expression.Parameter(typeof(int));
                var var1Parm = Expression.Variable(t);
                var exps = new List<Expression>(new Expression[] {
                    Expression.Assign(var1Parm, Expression.TypeAs(parm1, t))
                });
                if (_table.Properties.ContainsKey(propertyName))
                {
                    var prop = _table.Properties[propertyName];
                    exps.Add(
                        Expression.Assign(
                            Expression.MakeMemberAccess(var1Parm, prop),
                            Expression.Add(
                                Expression.MakeMemberAccess(var1Parm, prop),
                                Expression.Convert(
                                    FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(prop.PropertyType, Expression.Convert(parm3, typeof(object))),
                                    prop.PropertyType
                                )
                            )
                        )
                    );
                }
                return Expression.Lambda<Action<object, string, int>>(Expression.Block(new[] { var1Parm }, exps), new[] { parm1, parm2, parm3 }).Compile();
            });
            func(entity, propertyName, incrBy);
        }

        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, ConcurrentDictionary<string, Action<object, string, object>>>> _dicSetEntityValueWithPropertyName = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, ConcurrentDictionary<string, Action<object, string, object>>>>();
        /// <summary>
        /// 设置实体中某属性的值
        /// </summary>
        /// <param name="orm"></param>
        /// <param name="entityType"></param>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        //public static void SetEntityValueWithPropertyName<TEntity>(this IFreeSql orm, TEntity entity, string propertyName, object value) => SetEntityValueWithPropertyName(orm, typeof(TEntity), entity, propertyName, value);
        public static void SetEntityValueWithPropertyName(this IFreeSql orm, Type entityType, object entity, string propertyName, object value)
        {
            if (entity == null) return;
            if (entityType == null) entityType = entity.GetType();
            var func = _dicSetEntityValueWithPropertyName.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, ConcurrentDictionary<string, Action<object, string, object>>>())
                .GetOrAdd(entityType, et => new ConcurrentDictionary<string, Action<object, string, object>>())
                .GetOrAdd(propertyName, pn =>
                {
                    var t = entityType;
                    var _table = orm.CodeFirst.GetTableByEntity(t);
                    var parm1 = Expression.Parameter(typeof(object));
                    var parm2 = Expression.Parameter(typeof(string));
                    var parm3 = Expression.Parameter(typeof(object));
                    var var1Parm = Expression.Variable(t);
                    var exps = new List<Expression>(new Expression[] {
                        Expression.Assign(var1Parm, Expression.TypeAs(parm1, t))
                    });
                    if (_table.Properties.ContainsKey(pn))
                    {
                        var prop = _table.Properties[pn];

                        if (_table.ColumnsByCs.ContainsKey(pn))
                        {
                            exps.Add(
                                Expression.Assign(
                                    Expression.MakeMemberAccess(var1Parm, prop),
                                    Expression.Convert(
                                        FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(prop.PropertyType, parm3),
                                        prop.PropertyType
                                    )
                                )
                            );
                        }
                        else
                        {
                            exps.Add(
                                Expression.Assign(
                                    Expression.MakeMemberAccess(var1Parm, prop),
                                    Expression.Convert(
                                        parm3,
                                        prop.PropertyType
                                    )
                                )
                            );
                        }
                    }
                    return Expression.Lambda<Action<object, string, object>>(Expression.Block(new[] { var1Parm }, exps), new[] { parm1, parm2, parm3 }).Compile();
                });
            func(entity, propertyName, value);
        }

        static ConcurrentDictionary<Type, MethodInfo[]> _dicAppendEntityUpdateSetWithColumnMethods = new ConcurrentDictionary<Type, MethodInfo[]>();
        static ConcurrentDictionary<Type, ConcurrentDictionary<Type, MethodInfo>> _dicAppendEntityUpdateSetWithColumnMethod = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, MethodInfo>>();
        /// <summary>
        /// 缓存执行 IUpdate.Set
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="update"></param>
        /// <param name="columnType"></param>
        /// <param name="setExp"></param>
        public static void AppendEntityUpdateSetWithColumn<TEntity>(this IUpdate<TEntity> update, Type columnType, LambdaExpression setExp) where TEntity : class
        {

            var setMethod = _dicAppendEntityUpdateSetWithColumnMethod.GetOrAdd(typeof(IUpdate<TEntity>), uptp => new ConcurrentDictionary<Type, MethodInfo>()).GetOrAdd(columnType, coltp =>
            {
                var allMethods = _dicAppendEntityUpdateSetWithColumnMethods.GetOrAdd(typeof(IUpdate<TEntity>), uptp => uptp.GetMethods());
                return allMethods.Where(a => a.Name == "Set" && a.IsGenericMethod && a.GetParameters().Length == 1 && a.GetGenericArguments().First().Name == "TMember").FirstOrDefault()
                    .MakeGenericMethod(columnType);
            });

            setMethod.Invoke(update, new object[] { setExp });
        }
    }
}
