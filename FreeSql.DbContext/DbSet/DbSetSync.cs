using FreeSql.Extensions.EntityUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeSql {
	partial class DbSet<TEntity> {

		void DbContextExecCommand() {
			if (IsNoneDbContext == false) {
				_dicUpdateTimes.Clear();
				_ctx.ExecCommand();
			}
		}

		int DbContextBetchAdd(EntityState[] adds) {
			if (adds.Any() == false) return 0;
			var affrows = this.OrmInsert(adds.Select(a => a.Value)).ExecuteAffrows();
			return affrows;
		}

		#region Add
		void AddPriv(TEntity data, bool isCheck) {
			if (isCheck && CanAdd(data, true) == false) return;
			if (_tableIdentitys.Length > 0) {
				//有自增，马上执行
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							DbContextExecCommand();
							var idtval = this.OrmInsert(data).ExecuteIdentity();
							IncrAffrows(1);
							_fsql.SetEntityIdentityValueWithPrimary(data, idtval);
							var state = CreateEntityState(data);
							_states.Add(state.Key, state);
						} else {
							DbContextExecCommand();
							var newval = this.OrmInsert(data).ExecuteInserted().First();
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
							DbContextExecCommand();
							var idtval = this.OrmInsert(data).ExecuteIdentity();
							IncrAffrows(1);
							_fsql.SetEntityIdentityValueWithPrimary(data, idtval);
							var state = CreateEntityState(data);
							_states.Add(state.Key, state);
						}
						return;
				}
			} else {
				if (IsNoneDbContext)
					IncrAffrows(OrmInsert(data).ExecuteAffrows());
				else
					EnqueueToDbContext(DbContext.ExecCommandInfoType.Insert, CreateEntityState(data));
			}
		}
		public void Add(TEntity data) => AddPriv(data, true);
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
						DbContextExecCommand();
						var rets = this.OrmInsert(data).ExecuteInserted();
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
							AddPriv(s, false);
						return;
				}
			} else {
				if (IsNoneDbContext)
					IncrAffrows(OrmInsert(data).ExecuteAffrows());
				else
					//进入队列，等待 SaveChanges 时执行
					foreach (var s in data)
						EnqueueToDbContext(DbContext.ExecCommandInfoType.Insert, CreateEntityState(s));
			}
		}
		#endregion

		#region Update
		int DbContextBetchUpdate(EntityState[] ups) => DbContextBetchUpdatePriv(ups, false);
		int DbContextBetchUpdateNow(EntityState[] ups) => DbContextBetchUpdatePriv(ups, true);
		int DbContextBetchUpdatePriv(EntityState[] ups, bool isLiveUpdate) {
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

				var affrows = update.ExecuteAffrows();

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

		Dictionary<TEntity, byte> _dicUpdateTimes = new Dictionary<TEntity, byte>();
		internal int UpdateAffrows(TEntity data) => UpdateRangeAffrows(new[] { data });
		internal int UpdateRangeAffrows(IEnumerable<TEntity> data) {
			if (CanUpdate(data, true) == false) return 0;
			if (IsNoneDbContext) {
				var dataarray = data.ToArray();
				var ups = new List<EntityState>();
				var totalAffrows = 0;
				for (var a = 0; a < dataarray.Length + 1; a++) {
					var item = a < dataarray.Length ? dataarray[a] : null;
					if (item != null) {
						var state = CreateEntityState(item);
						state.Value = item;
						ups.Add(state);
					}

					var affrows = DbContextBetchUpdatePriv(ups.ToArray(), item == null);
					if (affrows == -999) { //最后一个元素已被删除
						ups.RemoveAt(ups.Count - 1);
						continue;
					}
					if (affrows == -998 || affrows == -997) { //没有执行更新
						var laststate = ups[ups.Count - 1];
						ups.Clear();
						if (affrows == -997) ups.Add(laststate); //保留最后一个
					}
					if (affrows > 0) {
						totalAffrows += affrows;
						var islastNotUpdated = ups.Count != affrows;
						var laststate = ups[ups.Count - 1];
						ups.Clear();
						if (islastNotUpdated) ups.Add(laststate); //保留最后一个
					}
				}
				IncrAffrows(totalAffrows);
				return totalAffrows;
			}
			foreach (var item in data) {
				if (_dicUpdateTimes.ContainsKey(item))
					DbContextExecCommand();
				_dicUpdateTimes.Add(item, 1);

				var state = CreateEntityState(item);
				state.OldValue = item;
				EnqueueToDbContext(DbContext.ExecCommandInfoType.Update, state);
			}
			return 0;
		}
		public void Update(TEntity data) => UpdateAffrows(data);
		public void UpdateRange(IEnumerable<TEntity> data) => UpdateRangeAffrows(data);
		#endregion

		#region Remove
		int DbContextBetchRemove(EntityState[] dels) {
			if (dels.Any() == false) return 0;
			var affrows = this.OrmDelete(dels.Select(a => a.Value)).ExecuteAffrows();
			return Math.Max(dels.Length, affrows);
		}

		internal int RemoveAffrows(TEntity data) => RemoveRangeAffrows(new[] { data });
		internal int RemoveRangeAffrows(IEnumerable<TEntity> data) {
			if (CanRemove(data, true) == false) return 0;
			var dels = new List<EntityState>();
			foreach (var item in data) {
				var state = CreateEntityState(item);
				if (_states.ContainsKey(state.Key)) _states.Remove(state.Key);
				_fsql.ClearEntityPrimaryValueWithIdentityAndGuid(item);

				if (IsNoneDbContext) dels.Add(state);
				EnqueueToDbContext(DbContext.ExecCommandInfoType.Delete, state);
			}
			if (IsNoneDbContext) {
				var affrows = DbContextBetchRemove(dels.ToArray());
				IncrAffrows(affrows);
				return affrows;
			}
			return 0;
		}
		public void Remove(TEntity data) => RemoveAffrows(data);
		public void RemoveRange(IEnumerable<TEntity> data) => RemoveRangeAffrows(data);
		#endregion
	}
}
