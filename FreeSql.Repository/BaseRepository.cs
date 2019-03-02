using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql {
	public abstract class BaseRepository<TEntity> : IRepository<TEntity>
		where TEntity : class {

		protected IFreeSql _fsql;
		protected Expression<Func<TEntity, bool>> _filter;
		protected Func<TEntity, bool> _filterCompile;

		public BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter) : base() {
			_fsql = fsql ?? throw new NullReferenceException("fsql 参数不可为空");
			_filter = filter;
			_filterCompile = filter?.Compile();
		}

		public ISelect<TEntity> Select => _fsql.Select<TEntity>().Where(_filter);

		public IUpdate<TEntity> UpdateDiy => _fsql.Update<TEntity>().Where(_filter);

		public int Delete(Expression<Func<TEntity, bool>> predicate) => _fsql.Delete<TEntity>().Where(_filter).Where(predicate).ExecuteAffrows();

		public int Delete(TEntity entity) {
			ValidatorEntityAndThrow(entity);
			return _fsql.Delete<TEntity>(entity).Where(_filter).ExecuteAffrows();
		}

		public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate) => _fsql.Delete<TEntity>().Where(_filter).Where(predicate).ExecuteAffrowsAsync();

		public Task<int> DeleteAsync(TEntity entity) {
			ValidatorEntityAndThrow(entity);
			return _fsql.Delete<TEntity>(entity).Where(_filter).ExecuteAffrowsAsync();
		}

		public virtual TEntity Insert(TEntity entity) {
			ValidatorEntityAndThrow(entity);
			switch (_fsql.Ado.DataType) {
				case DataType.SqlServer:
				case DataType.PostgreSQL:
					return _fsql.Insert<TEntity>().AppendData(entity).ExecuteInserted().FirstOrDefault();
				case DataType.MySql:
				case DataType.Oracle:
				case DataType.Sqlite:
				default:
					throw new NotImplementedException($"{_fsql.Ado.DataType}不支持类似returning或output类型的特性，请参考FreeSql插入数据的方法重新现实。");
			}
		}

		public virtual List<TEntity> Insert(IEnumerable<TEntity> entitys) {
			ValidatorEntityAndThrow(entitys);
			switch (_fsql.Ado.DataType) {
				case DataType.SqlServer:
				case DataType.PostgreSQL:
					return _fsql.Insert<TEntity>().AppendData(entitys).ExecuteInserted();
				case DataType.MySql:
				case DataType.Oracle:
				case DataType.Sqlite:
				default:
					throw new NotImplementedException($"{_fsql.Ado.DataType}不支持类似returning或output类型的特性，请参考FreeSql插入数据的方法重新现实。");
			}
		}

		async public virtual Task<TEntity> InsertAsync(TEntity entity) {
			ValidatorEntityAndThrow(entity);
			switch (_fsql.Ado.DataType) {
				case DataType.SqlServer:
				case DataType.PostgreSQL:
					return (await _fsql.Insert<TEntity>().AppendData(entity).ExecuteInsertedAsync()).FirstOrDefault();
				case DataType.MySql:
				case DataType.Oracle:
				case DataType.Sqlite:
				default:
					throw new NotImplementedException($"{_fsql.Ado.DataType}不支持类似returning或output类型的特性，请参考FreeSql插入数据的方法重新现实。");
			}
		}

		public virtual Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys) {
			ValidatorEntityAndThrow(entitys);
			switch (_fsql.Ado.DataType) {
				case DataType.SqlServer:
				case DataType.PostgreSQL:
					return _fsql.Insert<TEntity>().AppendData(entitys).ExecuteInsertedAsync();
				case DataType.MySql:
				case DataType.Oracle:
				case DataType.Sqlite:
				default:
					throw new NotImplementedException($"{_fsql.Ado.DataType}不支持类似returning或output类型的特性，请参考FreeSql插入数据的方法重新现实。");
			}
		}

		public int Update(TEntity entity) {
			ValidatorEntityAndThrow(entity);
			return _fsql.Update<TEntity>().SetSource(entity).Where(_filter).ExecuteAffrows();
		}

		public Task<int> UpdateAsync(TEntity entity) {
			ValidatorEntityAndThrow(entity);
			return _fsql.Update<TEntity>().SetSource(entity).Where(_filter).ExecuteAffrowsAsync();
		}

		protected void ValidatorEntityAndThrow(TEntity entity) => ValidatorEntityAndThrow(new[] { entity });
		protected virtual void ValidatorEntityAndThrow(IEnumerable<TEntity> entitys) {
			foreach (var entity in entitys) {
				if (_filterCompile?.Invoke(entity) == false) throw new Exception($"FreeSql.Repository Insert 失败，因为设置了 {_filter}，插入的数据不符合");
			}
		}
	}

	public abstract class BaseRepository<TEntity, TKey> : BaseRepository<TEntity>, IRepository<TEntity, TKey>
		where TEntity : class {

		public BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter) : base(fsql, filter) {
		}

		public int Delete(TKey id) => _fsql.Delete<TEntity>(id).Where(_filter).ExecuteAffrows();

		public Task<int> DeleteAsync(TKey id) => _fsql.Delete<TEntity>(id).Where(_filter).ExecuteAffrowsAsync();

		public TEntity Find(TKey id) => _fsql.Select<TEntity>(id).Where(_filter).ToOne();

		public Task<TEntity> FindAsync(TKey id) => _fsql.Select<TEntity>(id).Where(_filter).ToOneAsync();

		public TEntity Get(TKey id) => Find(id);

		public Task<TEntity> GetAsync(TKey id) => FindAsync(id);
	}
}
