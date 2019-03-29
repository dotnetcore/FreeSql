using FreeSql.Extensions.EntityUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeSql {
	partial class DbSet<TEntity> {

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
							ExecuteCommand();
							var idtval = this.OrmInsert(data).ExecuteIdentity();
							IncrAffrows(1);
							_fsql.SetEntityIdentityValueWithPrimary(data, idtval);
							var state = CreateEntityState(data);
							_states.Add(state.Key, state);
						} else {
							ExecuteCommand();
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
							ExecuteCommand();
							var idtval = this.OrmInsert(data).ExecuteIdentity();
							IncrAffrows(1);
							_fsql.SetEntityIdentityValueWithPrimary(data, idtval);
							var state = CreateEntityState(data);
							_states.Add(state.Key, state);
						}
						return;
				}
			} else {
				//进入队列，等待 SaveChanges 时执行
				EnqueueAction(DbContext.ExecCommandInfoType.Insert, this, typeof(EntityState), CreateEntityState(data));
			}
		}
		public void Add(TEntity data) => AddPriv(data, true);
		#endregion

		#region AddRange
		public void AddRange(TEntity[] data) {
			if (CanAdd(data, true) == false) return;
			if (data.Length == 1) {
				Add(data.First());
				return;
			}
			if (_tableIdentitys.Length > 0) {
				//有自增，马上执行
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						ExecuteCommand();
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
				//进入队列，等待 SaveChanges 时执行
				foreach (var s in data)
					EnqueueAction(DbContext.ExecCommandInfoType.Insert, this, typeof(EntityState), CreateEntityState(s));
			}
		}
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
						ExecuteCommand();
						var rets = this.OrmInsert(data).ExecuteInserted();
						if (rets.Count != data.Count()) throw new Exception($"特别错误：批量添加失败，{_fsql.Ado.DataType} 的返回数据，与添加的数目不匹配");
						var idx = 0;
						foreach(var s in data)
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
				//进入队列，等待 SaveChanges 时执行
				foreach (var s in data) 
					EnqueueAction(DbContext.ExecCommandInfoType.Insert, this, typeof(EntityState), CreateEntityState(s));
			}
		}
		#endregion

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

		void UpdatePriv(TEntity data, bool isCheck) {
			if (isCheck && CanUpdate(data, true) == false) return;
			var state = CreateEntityState(data);
			state.OldValue = data;
			EnqueueAction(DbContext.ExecCommandInfoType.Update, this, typeof(EntityState), state);
		}
		public void Update(TEntity data) => UpdatePriv(data, true);
		public void UpdateRange(TEntity[] data) {
			if (CanUpdate(data, true) == false) return;
			foreach (var item in data)
				UpdatePriv(item, false);
		}
		public void UpdateRange(IEnumerable<TEntity> data) {
			if (CanUpdate(data, true) == false) return;
			foreach (var item in data)
				UpdatePriv(item, false);
		}

		int DbContextBetchRemove(EntityState[] dels) {
			if (dels.Any() == false) return 0;
			var affrows = this.OrmDelete(dels.Select(a => a.Value)).ExecuteAffrows();
			return Math.Max(dels.Length, affrows);
		}
		void RemovePriv(TEntity data, bool isCheck) {
			if (isCheck && CanRemove(data, true) == false) return;
			var state = CreateEntityState(data);
			EnqueueAction(DbContext.ExecCommandInfoType.Delete, this, typeof(EntityState), state);

			if (_states.ContainsKey(state.Key)) _states.Remove(state.Key);
			_fsql.ClearEntityPrimaryValueWithIdentityAndGuid(data);
		}
		public void Remove(TEntity data) => RemovePriv(data, true);
		public void RemoveRange(TEntity[] data) {
			if (CanRemove(data, true) == false) return;
			foreach (var item in data)
				RemovePriv(item, false);
		}
		public void RemoveRange(IEnumerable<TEntity> data) {
			if (CanRemove(data, true) == false) return;
			foreach (var item in data)
				RemovePriv(item, false);
		}
	}
}
