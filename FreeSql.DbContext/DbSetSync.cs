using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using FreeSql.Extensions;

namespace FreeSql {
	partial class DbSet<TEntity> {

		int DbContextBetchAdd(EntityState[] adds) {
			if (adds.Any() == false) return 0;
			var affrows = this.OrmInsert(adds.Select(a => a.Value)).ExecuteAffrows();
			return affrows;
		}

		#region Add
		void AddPriv(TEntity source, bool isCheck) {
			if (isCheck && CanAdd(source, true) == false) return;
			if (_tableIdentitys.Length > 0) {
				//有自增，马上执行
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							_ctx.ExecCommand();
							var idtval = this.OrmInsert(source).ExecuteIdentity();
							_ctx._affrows++;
							_fsql.SetEntityIdentityValueWithPrimary(source, idtval);
							var state = CreateEntityState(source);
							_states.Add(state.Key, state);
						} else {
							_ctx.ExecCommand();
							var newval = this.OrmInsert(source).ExecuteInserted().First();
							_ctx._affrows++;
							_fsql.MapEntityValue(newval, source);
							var state = CreateEntityState(newval);
							_states.Add(state.Key, state);
						}
						return;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							_ctx.ExecCommand();
							var idtval = this.OrmInsert(source).ExecuteIdentity();
							_ctx._affrows++;
							_fsql.SetEntityIdentityValueWithPrimary(source, idtval);
							var state = CreateEntityState(source);
							_states.Add(state.Key, state);
						}
						return;
				}
			} else {
				//进入队列，等待 SaveChanges 时执行
				_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Insert, this, typeof(EntityState), CreateEntityState(source));
			}
		}
		public void Add(TEntity source) => AddPriv(source, true);
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
						_ctx.ExecCommand();
						var rets = this.OrmInsert(data).ExecuteInserted();
						if (rets.Count != data.Count()) throw new Exception($"特别错误：批量添加失败，{_fsql.Ado.DataType} 的返回数据，与添加的数目不匹配");
						var idx = 0;
						foreach (var s in data)
							_fsql.MapEntityValue(rets[idx++], s);
						_ctx._affrows += rets.Count;
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
					_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Insert, this, typeof(EntityState), CreateEntityState(s));
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
						_ctx.ExecCommand();
						var rets = this.OrmInsert(data).ExecuteInserted();
						if (rets.Count != data.Count()) throw new Exception($"特别错误：批量添加失败，{_fsql.Ado.DataType} 的返回数据，与添加的数目不匹配");
						var idx = 0;
						foreach(var s in data)
							_fsql.MapEntityValue(rets[idx++], s);
						_ctx._affrows += rets.Count;
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
					_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Insert, this, typeof(EntityState), CreateEntityState(s));
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
			if (uplst2 != null && string.Compare(string.Join(",", cuig1), string.Join(",", cuig2)) != 0) {
				//最后一个不保存
				var data = ups.ToList();
				data.RemoveAt(ups.Length - 1);
				var affrows = this.OrmUpdate(null).SetSource(data.Select(a => a.Value)).IgnoreColumns(cuig2).ExecuteAffrows();
				foreach (var newval in data) {
					if (_states.TryGetValue(newval.Key, out var tryold))
						_fsql.MapEntityValue(newval.Value, tryold.Value);
				}
				return affrows;
			} else if (isLiveUpdate) {
				//立即保存
				var data = ups;
				var affrows = this.OrmUpdate(null).SetSource(data.Select(a => a.Value)).IgnoreColumns(cuig1).ExecuteAffrows();
				foreach (var newval in data) {
					if (_states.TryGetValue(newval.Key, out var tryold))
						_fsql.MapEntityValue(newval.Value, tryold.Value);
				}
				return Math.Min(ups.Length, affrows);
			}
			//等待下次对比再保存
			return 0;
		}

		void UpdatePriv(TEntity data, bool isCheck) {
			if (isCheck && CanUpdate(data, true) == false) return;
			_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Update, this, typeof(EntityState), CreateEntityState(data));
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
			return affrows;
		}
		void RemovePriv(TEntity data, bool isCheck) {
			if (isCheck && CanRemove(data, true) == false) return;
			var state = CreateEntityState(data);
			_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Delete, this, typeof(EntityState), state);

			_states.Remove(state.Key);
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
