using Autofac;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

public static class FreeSqlRepositoryAutofacExtenssions {

	/// <summary>
	/// 注册 FreeSql.Repository 包括 泛型、继承实现的仓储
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="globalDataFilter">全局过滤设置</param>
	/// <param name="assemblies">继承实现的仓储，所在的程序集</param>
	public static void RegisterFreeRepository(this ContainerBuilder builder, Action<FluentDataFilter> globalDataFilter = null, params Assembly[] assemblies) => 
		RegisterFreeRepositoryPrivate(builder, globalDataFilter, assemblies);

	static void RegisterFreeRepositoryPrivate(ContainerBuilder builder, Action<FluentDataFilter> globalDataFilter, params Assembly[] assemblies) {

		Utils._globalDataFilter = globalDataFilter;

		builder.RegisterGeneric(typeof(GuidRepository<>)).As(
			typeof(GuidRepository<>),
			typeof(BaseRepository<>),
			typeof(IBasicRepository<>),
			typeof(IReadOnlyRepository<>)
		).OnActivating(a => {
			//Utils.SetRepositoryDataFilter(a.Instance);
		}).InstancePerDependency();
		
		builder.RegisterGeneric(typeof(DefaultRepository<,>)).As(
			typeof(DefaultRepository<,>),
			typeof(BaseRepository<,>),
			typeof(IBasicRepository<,>),
			typeof(IReadOnlyRepository<,>)
		).OnActivating(a => {
			//Utils.SetRepositoryDataFilter(a.Instance);
		}).InstancePerDependency();

		builder.RegisterAssemblyTypes(assemblies).Where(a => {
			return typeof(IRepository).IsAssignableFrom(a);
		}).InstancePerDependency();

	}
}
