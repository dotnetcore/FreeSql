using Autofac;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

public static class FreeSqlRepositoryAutofacExtenssions {

	public static void RegisterFreeRepository(this ContainerBuilder builder, Action<GlobalDataFilter> globalDataFilter) => RegisterFreeRepositoryPrivate(builder, globalDataFilter);

	static ConcurrentDictionary<Type, Delegate> _dicRegisterFreeRepositoryPrivateSetFilterFunc = new ConcurrentDictionary<Type, Delegate>();
	static ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>> _dicRegisterFreeRepositoryPrivateConvertFilterNotExists = new ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>>();
	static void RegisterFreeRepositoryPrivate(ContainerBuilder builder, Action<GlobalDataFilter> globalDataFilter) {

		Action<object> funcSetDataFilter = instance => {
			if (globalDataFilter == null) return;
			var globalFilter = new GlobalDataFilter();
			globalDataFilter(globalFilter);

			var type = instance.GetType();
			var entityType = type.GenericTypeArguments[0];

			var notExists = _dicRegisterFreeRepositoryPrivateConvertFilterNotExists.GetOrAdd(type, t => new ConcurrentDictionary<string, bool>());
			var newFilter = new Dictionary<string, LambdaExpression>();
			foreach (var gf in globalFilter._filters) {
				if (notExists.ContainsKey(gf.name)) continue;

				LambdaExpression newExp = null;
				var filterParameter1 = Expression.Parameter(entityType, gf.exp.Parameters[0].Name);
				try {
					newExp = Expression.Lambda(
						typeof(Func<,>).MakeGenericType(entityType, typeof(bool)),
						new ReplaceVisitor().Modify(gf.exp.Body, filterParameter1),
						filterParameter1
					);
				} catch {
					notExists.TryAdd(gf.name, true); //防止第二次错误
					continue;
				}
				newFilter.Add(gf.name, gf.exp);
			}
			if (newFilter.Any() == false) return;

			var del = _dicRegisterFreeRepositoryPrivateSetFilterFunc.GetOrAdd(type, t => {
				var reposParameter = Expression.Parameter(type);
				var nameParameter = Expression.Parameter(typeof(string));
				var expressionParameter = Expression.Parameter(
					typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(entityType, typeof(bool)))
				);
				return Expression.Lambda(
					Expression.Block(
						Expression.Call(reposParameter, type.GetMethod("ApplyDataFilter", BindingFlags.Instance | BindingFlags.NonPublic), nameParameter, expressionParameter)
					),
					new[] {
						reposParameter, nameParameter, expressionParameter
					}
				).Compile();
			});
			foreach (var nf in newFilter) {
				del.DynamicInvoke(instance, nf.Key, nf.Value);
			}
		};

		builder.RegisterGeneric(typeof(GuidRepository<>)).As(
			typeof(GuidRepository<>),
			typeof(BaseRepository<>),
			typeof(IBasicRepository<>),
			typeof(IReadOnlyRepository<>)
		).OnActivating(a => {
			funcSetDataFilter(a.Instance);
		}).InstancePerDependency();

		builder.RegisterGeneric(typeof(DefaultRepository<,>)).As(
			typeof(DefaultRepository<,>),
			typeof(BaseRepository<,>),
			typeof(IBasicRepository<,>),
			typeof(IReadOnlyRepository<,>)
		).OnActivating(a => {
			funcSetDataFilter(a.Instance);
		}).InstancePerDependency();
	}

	class ReplaceVisitor : ExpressionVisitor {
		private ParameterExpression parameter;

		public Expression Modify(Expression expression, ParameterExpression parameter) {
			this.parameter = parameter;
			return Visit(expression);
		}

		protected override Expression VisitMember(MemberExpression node) {
			if (node.Expression?.NodeType == ExpressionType.Parameter)
				return Expression.Property(parameter, node.Member.Name);
			return base.VisitMember(node);
		}
	}
}
