using FreeSql.Extensions.EntityUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeSql {
	partial class DbSet<TEntity> {

		Task DbContextExecCommandAsync() {
			_dicUpdateTimes.Clear();
			return _ctx.ExecCommandAsync();
		}

		async Task<int> DbContextBetchAddAsync(EntityState[] adds) {
			if (adds.Any() == false) return 0;
			var affrows = await this.OrmInsert(adds.Select(a => a.Value)).ExecuteAffrowsAsync();
			return affrows;
		}

		#region Add
		async Task AddPrivAsync(TEntity data, bool isCheck) {
			if (isCheck && CanAdd(data, true) == false) return;
			if (_tableIdentitys.Length > 0) {
				//有自增，马上执行
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							await DbContextExecCommandAsync();
							var idtval = await this.OrmInsert(data).ExecuteIdentityAsync();
							IncrAffrows(1);
							_fsql.SetEntityIdentityValueWithPrimary(data, idtval);
							var state = CreateEntityState(data);
							_states.Add(state.Key, state);
						} else {
							await DbContextExecCommandAsync();
							var newval = (await this.OrmInsert(data).ExecuteInsertedAsync()).First();
							IncrAffrows(1);
							_fsql.MapEntityValue(newval, data);
							var state = CreateEntityState(newval);
							_states.Add(state.Key, state);
						}
						return;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							await DbContextExecCommandAsync();
							var idtval = await this.OrmInsert(data).ExecuteIdentityAsync();
							IncrAffrows(1);
							_fsql.SetEntityIdentityValueWithPrimary(data, idtval);
							var state = CreateEntityState(data);
							_states.Add(state.Key, state);
						}
						return;
				}
			} else {
				EnqueueToDbContext(DbContext.ExecCommandInfoType.Insert, CreateEntityState(data));
			}
		}
		public Task AddAsync(TEntity data) => AddPrivAsync(data, true);
		async public Task AddRangeAsync(IEnumerable<TEntity> data) {
			if (CanAdd(data, true) == false) return;
			if (data.ElementAtOrDefault(1) == default(TEntity)) {
				await AddAsync(data.First());
				return;
			}
			if (_tableIdentitys.Length > 0) {
				//有自增，马上执行
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						await DbContextExecCommandAsync();
						var rets = await this.OrmInsert(data).ExecuteInsertedAsync();
						if (rets.Count != data.Count()) throw new Exception($"特别错误：批量添加失败，{_fsql.Ado.DataType} 的返回数据，与添加的数目不匹配");
						var idx = 0;
						foreach (var s in data)
							_fsql.MapEntityValue(rets[idx++], s);
						IncrAffrows(rets.Count);
						TrackToList(rets);
						return;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						foreach (var s in data)
							await AddPrivAsync(s, false);
						return;
				}
			} else {
				//进入队列，等待 SaveChanges 时执行
				foreach (var item in data)
					EnqueueToDbContext(DbContext.ExecCommandInfoType.Insert, CreateEntityState(item));
			}
		}
		#endregion

		#region UpdateAsync
		Task<int> DbContextBetchUpdateAsync(EntityState[] ups) => DbContextBetchUpdatePrivAsync(ups, false);
		Task<int> DbContextBetchUpdateNowAsync(EntityState[] ups) => DbContextBetchUpdatePrivAsync(ups, true);
		async Task<int> DbContextBetchUpdatePrivAsync(EntityState[] ups, bool isLiveUpdate) {
			if (ups.Any() == false) return 0;
			var uplst1 = ups[ups.Length - 1];
			var uplst2 = ups.Length > 1 ? ups[ups.Length - 2] : null;

			if (_states.TryGetValue(uplst1.Key, out var lstval1) == false) return -999;
			var lstval2 = default(EntityState);
			if (uplst2 != null && _states.TryGetValue(uplst2.Key, out lstval2) == false) throw new Exception($"特别错误：更新失败，数据未被跟踪：{_fsql.GetEntityString(uplst2.Value)}");

			var cuig1 = _fsql.CompareEntityValueReturnColumns(uplst1.Value, lstval1.Value, true);
			var cuig2 = uplst2 != null ? _fsql.CompareEntityValueReturnColumns(uplst2.Value, lstval2.Value, true) : null;

			List<EntityState> data = null;
			string[] cuig = null;
			if (uplst2 != null && string.Compare(string.Join(",", cuig1), string.Join(",", cuig2)) != 0) {
				//最后一个不保存
				data = ups.ToList();
				data.RemoveAt(ups.Length - 1);
				cuig = cuig2;
			} else if (isLiveUpdate) {
				//立即保存
				data = ups.ToList();
				cuig = cuig1;
			}

			if (data?.Count > 0) {

				if (cuig.Length == _table.Columns.Count)
					return ups.Length == data.Count ? -998 : -997;

				var updateSource = data.Select(a => a.Value).ToArray();
				var update = this.OrmUpdate(null).SetSource(updateSource).IgnoreColumns(cuig);

				var affrows = await update.ExecuteAffrowsAsync();

				foreach (var newval in data) {
					if (_states.TryGetValue(newval.Key, out var tryold))
						_fsql.MapEntityValue(newval.Value, tryold.Value);
					if (newval.OldValue != null)
						_fsql.MapEntityValue(newval.Value, newval.OldValue);
				}
				return affrows;
			}

			//等待下次对比再保存
			return 0;
		}
		#endregion

		#region RemoveAsync
		async Task<int> DbContextBetchRemoveAsync(EntityState[] dels) {
			if (dels.Any() == false) return 0;
			var affrows = await this.OrmDelete(dels.Select(a => a.Value)).ExecuteAffrowsAsync();
			return Math.Max(dels.Length, affrows);
		}
		#endregion
	}
}
