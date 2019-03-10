using Autofac;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

public static class FreeSqlRepositoryAutofacDependencyInjection {

	public static void RegisterFreeRepository(this ContainerBuilder builder) => RegisterFreeRepositoryPrivate<bool>(builder, null, null);
	public static void RegisterFreeRepositoryAddFilter<TEntity>(this ContainerBuilder builder, Func<Expression<Func<TEntity, bool>>> filterHandler) => RegisterFreeRepositoryPrivate<TEntity>(builder, filterHandler, null);

	static ConcurrentDictionary<Type, Delegate> _dicRegisterFreeRepositorySetFilterFunc = new ConcurrentDictionary<Type, Delegate>();
	static ConcurrentDictionary<Type, Delegate> _dicRegisterFreeRepositorySetAsTableFunc = new ConcurrentDictionary<Type, Delegate>();
	static void RegisterFreeRepositoryPrivate<TEntity>(ContainerBuilder builder, Func<Expression<Func<TEntity, bool>>> filterHandler, Func<string, string> asTableHandler) {

		Func<Type, Delegate> setFilterFunc = reposType => _dicRegisterFreeRepositorySetFilterFunc.GetOrAdd(reposType, type => { 
			var reposParameter = Expression.Parameter(type);
			var fitlerParameter = Expression.Parameter(typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(type.GenericTypeArguments[0], typeof(bool))));
			return Expression.Lambda(
				Expression.Block(
					Expression.Call(reposParameter, type.GetMethod("set_Filter", BindingFlags.Instance | BindingFlags.NonPublic), fitlerParameter)
				),
				new[] {
					reposParameter, fitlerParameter
				}
			).Compile();
		});
		Func<Type, LambdaExpression> convertFilter = type => {
			var filter = filterHandler?.Invoke();
			if (filter == null) return null;
			var entityType = type.GenericTypeArguments[0];
			var filterParameter1 = Expression.Parameter(entityType, filter.Parameters[0].Name);
			try {
				return Expression.Lambda(
					typeof(Func<,>).MakeGenericType(entityType, typeof(bool)),
					new ReplaceVisitor<TEntity>().Modify(filter.Body, filterParameter1),
					filterParameter1
				);
			} catch {
				return null;
			}
		};

		builder.RegisterGeneric(typeof(GuidRepository<>)).As(
			typeof(GuidRepository<>),
			typeof(BaseRepository<>),
			typeof(IBasicRepository<>),
			typeof(IReadOnlyRepository<>)
		).OnActivating(a => {
			if (filterHandler != null) {
				var type = a.Instance.GetType();
				setFilterFunc(type)?.DynamicInvoke(a.Instance, convertFilter(type));
			}
		}).InstancePerDependency();

		builder.RegisterGeneric(typeof(DefaultRepository<,>)).As(
			typeof(DefaultRepository<,>),
			typeof(BaseRepository<,>),
			typeof(IBasicRepository<,>),
			typeof(IReadOnlyRepository<,>)
		).OnActivating(a => {
			if (filterHandler != null) {
				var type = a.Instance.GetType();
				setFilterFunc(type)?.DynamicInvoke(a.Instance, convertFilter(type));
			}
		}).InstancePerDependency();
	}

	//static void RegisterRepository<TEntity>(this ContainerBuilder builder, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable, int regType) {

	//	Func<Type, LambdaExpression> reposFunc = type => {
	//		var entityType = type.GenericTypeArguments[0];
	//		var filterParameter1 = Expression.Parameter(entityType, filter.Parameters[0].Name);
	//		var convertFilter = Expression.Lambda(
	//			typeof(Func<,>).MakeGenericType(entityType, typeof(bool)),
	//			new ReplaceVisitor<TEntity>().Modify(filter.Body, filterParameter1),
	//			filterParameter1
	//		);
	//		var repos = Expression.Parameter(type);
	//		var blocks = new List<Expression>();
	//		if (filter != null) blocks.Add(Expression.Call(repos, type.GetMethod("set_Filter", BindingFlags.Instance | BindingFlags.NonPublic), Expression.Constant(convertFilter)));
	//		if (asTable != null) blocks.Add(Expression.Call(repos, type.GetMethod("set_AsTable", BindingFlags.Instance | BindingFlags.NonPublic), Expression.Constant(asTable)));
	//		return Expression.Lambda(
	//			//Expression.New(
	//			//	typeof(GuidRepository<>).MakeGenericType(type.GenericTypeArguments).GetConstructors()[1],
	//			//	Expression.Constant(a.Context.Resolve<IFreeSql>()),
	//			//	Expression.Constant(convertFilter),
	//			//	Expression.Constant(asTable)
	//			//)
	//			Expression.Block(blocks),
	//			repos
	//		);
	//	};

	//	if (regType == 1)
	//		builder.RegisterGeneric(typeof(GuidRepository<>)).As(
	//			typeof(GuidRepository<>),
	//			typeof(BaseRepository<>), typeof(BaseRepository<,>),
	//			typeof(IBasicRepository<>), typeof(IBasicRepository<,>),
	//			typeof(IReadOnlyRepository<>), typeof(IReadOnlyRepository<,>)
	//		).OnActivating(a => {
	//			if (filter != null)
	//				_dicAddGuidRepositoryFunc.GetOrAdd(a.Instance.GetType(), t => {
	//					try { reposFunc(t).Compile(); } catch { }
	//					return null;
	//				})?.DynamicInvoke(a.Instance);
	//		}).InstancePerDependency();


	//	if (regType == 2)
	//		builder.RegisterGeneric(typeof(DefaultRepository<,>)).As(
	//			typeof(DefaultRepository<,>),
	//			typeof(BaseRepository<,>),
	//			typeof(IBasicRepository<,>),
	//			typeof(IReadOnlyRepository<,>)
	//		).OnActivating(a => {
	//			if (filter != null)
	//				_dicAddGuidRepositoryFunc.GetOrAdd(a.Instance.GetType(), t => {
	//					try { reposFunc(t).Compile(); } catch { }
	//					return null;
	//				})?.DynamicInvoke(a.Instance);
	//		}).InstancePerDependency();
	//}

	//static ConcurrentDictionary<Type, Delegate> _dicAddGuidRepositoryFunc = new ConcurrentDictionary<Type, Delegate>();

	class ReplaceVisitor<TEntity1> : ExpressionVisitor {
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
