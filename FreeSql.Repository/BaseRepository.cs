using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql {
	public abstract class BaseRepository<TEntity> : IRepository<TEntity>
		where TEntity : class {

		protected IFreeSql _fsql;

		public BaseRepository(IFreeSql fsql) : base() {
			_fsql = fsql;
			if (_fsql == null) throw new NullReferenceException("fsql 参数不可为空");
		}

		public ISelect<TEntity> Select => _fsql.Select<TEntity>();

		public IUpdate<TEntity> UpdateDiy => _fsql.Update<TEntity>();

		public int Delete(Expression<Func<TEntity, bool>> predicate) => _fsql.Delete<TEntity>().Where(predicate).ExecuteAffrows();

		public int Delete(TEntity entity) => _fsql.Delete<TEntity>(entity).ExecuteAffrows();

		public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate) => _fsql.Delete<TEntity>().Where(predicate).ExecuteAffrowsAsync();

		public Task<int> DeleteAsync(TEntity entity) => _fsql.Delete<TEntity>(entity).ExecuteAffrowsAsync();

		public virtual TEntity Insert(TEntity entity) {
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

		public virtual List<TEntity> Insert(IEnumerable<TEntity> entity) {
			switch (_fsql.Ado.DataType) {
				case DataType.SqlServer:
				case DataType.PostgreSQL:
					return _fsql.Insert<TEntity>().AppendData(entity).ExecuteInserted();
				case DataType.MySql:
				case DataType.Oracle:
				case DataType.Sqlite:
				default:
					throw new NotImplementedException($"{_fsql.Ado.DataType}不支持类似returning或output类型的特性，请参考FreeSql插入数据的方法重新现实。");
			}
		}

		async public virtual Task<TEntity> InsertAsync(TEntity entity) {
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

		public virtual Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entity) {
			switch (_fsql.Ado.DataType) {
				case DataType.SqlServer:
				case DataType.PostgreSQL:
					return _fsql.Insert<TEntity>().AppendData(entity).ExecuteInsertedAsync();
				case DataType.MySql:
				case DataType.Oracle:
				case DataType.Sqlite:
				default:
					throw new NotImplementedException($"{_fsql.Ado.DataType}不支持类似returning或output类型的特性，请参考FreeSql插入数据的方法重新现实。");
			}
		}

		public int Update(TEntity entity) => _fsql.Update<TEntity>().SetSource(entity).ExecuteAffrows();

		public Task<int> UpdateAsync(TEntity entity) => _fsql.Update<TEntity>().SetSource(entity).ExecuteAffrowsAsync();
	}

	public abstract class BaseRepository<TEntity, TKey> : BaseRepository<TEntity>, IRepository<TEntity, TKey>
		where TEntity : class {

		public BaseRepository(IFreeSql fsql) : base(fsql) {
		}

		public int Delete(TKey id) => _fsql.Delete<TEntity>(id).ExecuteAffrows();

		public Task<int> DeleteAsync(TKey id) => _fsql.Delete<TEntity>(id).ExecuteAffrowsAsync();

		public TEntity Find(TKey id) => _fsql.Select<TEntity>(id).ToOne();

		public Task<TEntity> FindAsync(TKey id) => _fsql.Select<TEntity>(id).ToOneAsync();

		public TEntity Get(TKey id) => Find(id);

		public Task<TEntity> GetAsync(TKey id) => FindAsync(id);
	}
}
