using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
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
			DataFilterUtil.SetRepositoryDataFilter(this, null);
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
			Add(entity);
			return entity;
		}
		public virtual List<TEntity> Insert(IEnumerable<TEntity> entitys) {
			AddRange(entitys);
			return entitys.ToList();
		}
		async public virtual Task<TEntity> InsertAsync(TEntity entity) {
			await AddAsync(entity);
			return entity;
		}
		async public virtual Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys) {
			await AddRangeAsync(entitys);
			return entitys.ToList();
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

		#region 参考 FreeSql.DbContext/dbset

		TableInfo _tablePriv;
		TableInfo _table => _tablePriv ?? (_tablePriv = _fsql.CodeFirst.GetTableByEntity(EntityType));
		ColumnInfo[] _tableIdentitysPriv;
		ColumnInfo[] _tableIdentitys => _tableIdentitysPriv ?? (_tableIdentitysPriv = _table.Primarys.Where(a => a.Attribute.IsIdentity).ToArray());

		bool CanAdd(TEntity[] data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			foreach (var s in data) if (CanAdd(s, isThrow) == false) return false;
			return true;
		}
		bool CanAdd(IEnumerable<TEntity> data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			foreach (var s in data) if (CanAdd(s, isThrow) == false) return false;
			return true;
		}
		bool CanAdd(TEntity data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			var key = _fsql.GetEntityKeyString(data);
			if (string.IsNullOrEmpty(key)) {
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						return true;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							return true;
						}
						if (isThrow) throw new Exception($"不可添加，未设置主键的值：{_fsql.GetEntityString(data)}");
						return false;
				}
			} else {
				var idval = _fsql.GetEntityIdentityValueWithPrimary(data);
				if (idval > 0) {
					if (isThrow) throw new Exception($"不可添加，自增属性有值：{_fsql.GetEntityString(data)}");
					return false;
				}
			}
			return true;
		}

		void AddPriv(TEntity data, bool isCheck) {
			if (isCheck && CanAdd(data, true) == false) return;
			if (_tableIdentitys.Length > 0) {
				//有自增，马上执行
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							var idtval = this.OrmInsert(data).ExecuteIdentity();
							_fsql.SetEntityIdentityValueWithPrimary(data, idtval);
						} else {
							var newval = this.OrmInsert(data).ExecuteInserted().First();
							_fsql.MapEntityValue(newval, data);
						}
						return;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							var idtval = this.OrmInsert(data).ExecuteIdentity();
							_fsql.SetEntityIdentityValueWithPrimary(data, idtval);
						}
						return;
				}
			} else {
				this.OrmInsert(data).ExecuteAffrows();
			}
		}
		public void Add(TEntity source) => AddPriv(source, true);
		public void AddRange(TEntity[] data) => AddRange(data.ToList());
		public void AddRange(IEnumerable<TEntity> data) {
			if (CanAdd(data, true) == false) return;
			if (data.ElementAtOrDefault(1) == default(TEntity)) {
				Add(data.First());
				return;
			}
			if (_tableIdentitys.Length > 0) {
				//有自增，马上执行
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						var rets = this.OrmInsert(data).ExecuteInserted();
						if (rets.Count != data.Count()) throw new Exception($"特别错误：批量添加失败，{_fsql.Ado.DataType} 的返回数据，与添加的数目不匹配");
						var idx = 0;
						foreach (var s in data)
							_fsql.MapEntityValue(rets[idx++], s);
						return;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						foreach (var s in data)
							AddPriv(s, false);
						return;
				}
			} else {
				this.OrmInsert(data).ExecuteAffrows();
			}
		}

		async Task AddPrivAsync(TEntity data, bool isCheck) {
			if (isCheck && CanAdd(data, true) == false) return;
			if (_tableIdentitys.Length > 0) {
				//有自增，马上执行
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							var idtval = await this.OrmInsert(data).ExecuteIdentityAsync();
							_fsql.SetEntityIdentityValueWithPrimary(data, idtval);
						} else {
							var newval = (await this.OrmInsert(data).ExecuteInsertedAsync()).First();
							_fsql.MapEntityValue(newval, data);
						}
						return;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							var idtval = await this.OrmInsert(data).ExecuteIdentityAsync();
							_fsql.SetEntityIdentityValueWithPrimary(data, idtval);
						}
						return;
				}
			} else {
				await this.OrmInsert(data).ExecuteAffrowsAsync();
			}
		}
		public Task AddAsync(TEntity source) => AddPrivAsync(source, true);
		public Task AddRangeAsync(TEntity[] data) => AddRangeAsync(data.ToList());
		async public Task AddRangeAsync(IEnumerable<TEntity> data) {
			if (CanAdd(data, true) == false) return;
			if (data.ElementAtOrDefault(1) == default(TEntity)) {
				Add(data.First());
				return;
			}
			if (_tableIdentitys.Length > 0) {
				//有自增，马上执行
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						var rets = await this.OrmInsert(data).ExecuteInsertedAsync();
						if (rets.Count != data.Count()) throw new Exception($"特别错误：批量添加失败，{_fsql.Ado.DataType} 的返回数据，与添加的数目不匹配");
						var idx = 0;
						foreach (var s in data)
							_fsql.MapEntityValue(rets[idx++], s);
						return;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						foreach (var s in data)
							await AddPrivAsync(s, false);
						return;
				}
			} else {
				await this.OrmInsert(data).ExecuteAffrowsAsync();
			}
		}
		#endregion
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
