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

		public GuidRepository(IFreeSql fsql) : this(fsql, null, null) {

		}
		public GuidRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable) : base(fsql, filter, asTable) {
		}

		public override List<TEntity> Insert(IEnumerable<TEntity> entity) {
			base.ValidatorEntityAndThrow(entity);
			_fsql.Insert<TEntity>().AppendData(entity).AsTable(AsTable).ExecuteAffrows();
			return entity.ToList();
		}

		async public override Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entity) {
			base.ValidatorEntityAndThrow(entity);
			await _fsql.Insert<TEntity>().AppendData(entity).AsTable(AsTable).ExecuteAffrowsAsync();
			return entity.ToList();
		}

		public override TEntity Insert(TEntity entity) {
			base.ValidatorEntityAndThrow(entity);
			_fsql.Insert<TEntity>().AppendData(entity).AsTable(AsTable).ExecuteAffrows();
			return entity;
		}

		async public override Task<TEntity> InsertAsync(TEntity entity) {
			base.ValidatorEntityAndThrow(entity);
			await _fsql.Insert<TEntity>().AppendData(entity).AsTable(AsTable).ExecuteAffrowsAsync();
			return entity;
		}
	}
}
