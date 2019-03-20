using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql {
	public abstract class BaseRepository<TEntity> : IRepository<TEntity>
		where TEntity : class {

		protected IFreeSql _fsql;
		internal UnitOfWork _unitOfWork;
		public IDataFilter<TEntity> DataFilter { get; } = new DataFilter<TEntity>();

		Func<string, string> _asTableVal;
		protected Func<string, string> AsTable {
			get => _asTableVal;
			set {
				_asTableVal = value;
				AsTableSelect = value == null ? null : new Func<Type, string, string>((a, b) => a == EntityType ? value(b) : null);
			}
		}
		protected Func<Type, string, string> AsTableSelect { get; private set; }
		internal Func<Type, string, string> AsTableSelectInternal => AsTableSelect;

		public Type EntityType { get; } = typeof(TEntity);

		protected BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable = null) : base() {
			_fsql = fsql ?? throw new NullReferenceException(nameof(fsql));
			Utils.SetRepositoryDataFilter(this, null);
			DataFilter.Apply("", filter);
			AsTable = asTable;
		}

		public ISelect<TEntity> Select => OrmSelect(null);
		public IUpdate<TEntity> UpdateDiy => OrmUpdate(null);

		public int Delete(Expression<Func<TEntity, bool>> predicate) => OrmDelete(null).Where(predicate).ExecuteAffrows();
		public int Delete(TEntity entity) => OrmDelete(entity).ExecuteAffrows();
		public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate) => OrmDelete(null).Where(predicate).ExecuteAffrowsAsync();
		public Task<int> DeleteAsync(TEntity entity) => OrmDelete(entity).ExecuteAffrowsAsync();

		public virtual TEntity Insert(TEntity entity) {
			switch (_fsql.Ado.DataType) {
				case DataType.SqlServer:
				case DataType.PostgreSQL:
					return OrmInsert(entity).ExecuteInserted().FirstOrDefault();
				case DataType.MySql:
				case DataType.Oracle:
				case DataType.Sqlite:
				default:
					throw new NotImplementedException($"{_fsql.Ado.DataType}不支持类似returning或output类型的特性，请参考FreeSql插入数据的方法重新实现。");
			}
		}
		public virtual List<TEntity> Insert(IEnumerable<TEntity> entitys) {
			switch (_fsql.Ado.DataType) {
				case DataType.SqlServer:
				case DataType.PostgreSQL:
					return OrmInsert(entitys).ExecuteInserted();
				case DataType.MySql:
				case DataType.Oracle:
				case DataType.Sqlite:
				default:
					throw new NotImplementedException($"{_fsql.Ado.DataType}不支持类似returning或output类型的特性，请参考FreeSql插入数据的方法重新实现。");
			}
		}
		async public virtual Task<TEntity> InsertAsync(TEntity entity) {
			switch (_fsql.Ado.DataType) {
				case DataType.SqlServer:
				case DataType.PostgreSQL:
					return (await OrmInsert(entity).ExecuteInsertedAsync()).FirstOrDefault();
				case DataType.MySql:
				case DataType.Oracle:
				case DataType.Sqlite:
				default:
					throw new NotImplementedException($"{_fsql.Ado.DataType}不支持类似returning或output类型的特性，请参考FreeSql插入数据的方法重新实现。");
			}
		}
		public virtual Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys) {
			switch (_fsql.Ado.DataType) {
				case DataType.SqlServer:
				case DataType.PostgreSQL:
					return OrmInsert(entitys).ExecuteInsertedAsync();
				case DataType.MySql:
				case DataType.Oracle:
				case DataType.Sqlite:
				default:
					throw new NotImplementedException($"{_fsql.Ado.DataType}不支持类似returning或output类型的特性，请参考FreeSql插入数据的方法重新实现。");
			}
		}

		public int Update(TEntity entity) => OrmUpdate(entity).ExecuteAffrows();
		public Task<int> UpdateAsync(TEntity entity) => OrmUpdate(entity).ExecuteAffrowsAsync();

		protected ISelect<TEntity> OrmSelect(object dywhere) {
			var select = _fsql.Select<TEntity>(dywhere).WithTransaction(_unitOfWork?.GetOrBeginTransaction(false));
			var filters = (DataFilter as DataFilter<TEntity>)._filters.Where(a => a.Value.IsEnabled == true);
			foreach (var filter in filters) select.Where(filter.Value.Expression);
			return select.AsTable(AsTableSelect);
		}
		protected IUpdate<TEntity> OrmUpdate(object dywhere) {
			var entityObj = dywhere as TEntity;
			var update = _fsql.Update<TEntity>(dywhere).WithTransaction(_unitOfWork?.GetOrBeginTransaction());
			var filters = (DataFilter as DataFilter<TEntity>)._filters.Where(a => a.Value.IsEnabled == true);
			foreach (var filter in filters) {
				if (entityObj != null && filter.Value.ExpressionDelegate?.Invoke(entityObj) == false)
					throw new Exception($"FreeSql.Repository Update 失败，因为设置了 {filter.Key}: {filter.Value.Expression}，更新的数据不符合");
				update.Where(filter.Value.Expression);
			}
			return update.AsTable(AsTable);
		}
		protected IDelete<TEntity> OrmDelete(object dywhere) {
			var delete = _fsql.Delete<TEntity>(dywhere).WithTransaction(_unitOfWork?.GetOrBeginTransaction());
			var filters = (DataFilter as DataFilter<TEntity>)._filters.Where(a => a.Value.IsEnabled == true);
			foreach (var filter in filters) delete.Where(filter.Value.Expression);
			return delete.AsTable(AsTable);
		}
		protected IInsert<TEntity> OrmInsert(TEntity entity) => OrmInsert(new[] { entity });
		protected IInsert<TEntity> OrmInsert(IEnumerable<TEntity> entitys) {
			var insert = _fsql.Insert<TEntity>(entitys).WithTransaction(_unitOfWork?.GetOrBeginTransaction());
			var filters = (DataFilter as DataFilter<TEntity>)._filters.Where(a => a.Value.IsEnabled == true);
			foreach (var filter in filters) {
				foreach (var entity in entitys)
					if (entity != null && filter.Value.ExpressionDelegate?.Invoke(entity) == false)
						throw new Exception($"FreeSql.Repository Insert 失败，因为设置了 {filter.Key}: {filter.Value.Expression}，插入的数据不符合");
			}
			return insert.AsTable(AsTable);
		}

		protected void ApplyDataFilter(string name, Expression<Func<TEntity, bool>> exp) => DataFilter.Apply(name, exp);
	}

	public abstract class BaseRepository<TEntity, TKey> : BaseRepository<TEntity>, IRepository<TEntity, TKey>
		where TEntity : class {

		public BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable = null) : base(fsql, filter, asTable) {
		}

		public int Delete(TKey id) => OrmDelete(id).ExecuteAffrows();
		public Task<int> DeleteAsync(TKey id) => OrmDelete(id).ExecuteAffrowsAsync();

		public TEntity Find(TKey id) => OrmSelect(id).ToOne();
		public Task<TEntity> FindAsync(TKey id) => OrmSelect(id).ToOneAsync();

		public TEntity Get(TKey id) => Find(id);
		public Task<TEntity> GetAsync(TKey id) => FindAsync(id);
	}
}
