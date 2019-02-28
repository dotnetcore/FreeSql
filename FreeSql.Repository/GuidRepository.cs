using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql {
	public class GuidRepository<TEntity> :
		BaseRepository<TEntity, Guid>
		where TEntity : class {

		public GuidRepository(IFreeSql fsql) : base(fsql) {
		}

		public override List<TEntity> Insert(List<TEntity> entity) {
			_fsql.Insert<TEntity>().AppendData(entity).ExecuteAffrows();
			return entity;
		}

		async public override Task<List<TEntity>> InsertAsync(List<TEntity> entity) {
			await _fsql.Insert<TEntity>().AppendData(entity).ExecuteAffrowsAsync();
			return entity;
		}

		public override TEntity Insert(TEntity entity) {
			_fsql.Insert<TEntity>().AppendData(entity).ExecuteAffrows();
			return entity;
		}

		async public override Task<TEntity> InsertAsync(TEntity entity) {
			await _fsql.Insert<TEntity>().AppendData(entity).ExecuteAffrowsAsync();
			return entity;
		}
	}
}
