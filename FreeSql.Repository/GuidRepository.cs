using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql {
	public class GuidRepository<TEntity> :
		BaseRepository<TEntity, Guid>
		where TEntity : class {

		public GuidRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter) : base(fsql, filter) {
		}

		public override List<TEntity> Insert(IEnumerable<TEntity> entity) {
			_fsql.Insert<TEntity>().AppendData(entity).ExecuteAffrows();
			return entity.ToList();
		}

		async public override Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entity) {
			await _fsql.Insert<TEntity>().AppendData(entity).ExecuteAffrowsAsync();
			return entity.ToList();
		}

		public override TEntity Insert(TEntity entity) {
			_fsql.Insert<TEntity>().AppendData(entity).ExecuteAffrows();
			return entity;
		}

		async public override Task<TEntity> InsertAsync(TEntity entity) {
			await _fsql.Insert<TEntity>().AppendData(entity).ExecuteAffrowsAsync();
			return entity;
		}

		public virtual string ToDataTable(TEntity entity) {
			return null;
		}
	}
}
