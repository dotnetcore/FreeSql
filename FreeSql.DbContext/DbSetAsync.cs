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
	abstract partial class DbSet<TEntity> {

		async Task<int> DbContextBetcAddAsync(EntityState[] dels) {
			if (dels.Any() == false) return 0;
			var affrows = await this.OrmInsert(dels.Select(a => a.Value)).ExecuteAffrowsAsync();
			return affrows;
		}
		async public Task AddAsync(TEntity source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			var key = _fsql.GetEntityKeyString(source);
			EntityState state = new EntityState();
			if (string.IsNullOrEmpty(key)) {
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							_ctx.ExecCommand();
							var idtval = await this.OrmInsert(source).ExecuteIdentityAsync();
							_ctx._affrows++;
							_fsql.SetEntityIdentityValue(source, idtval);
						} else {
							_ctx.ExecCommand();
							state.Value = (await this.OrmInsert(source).ExecuteInsertedAsync()).First();
							_ctx._affrows++;
							_fsql.CopyEntityValue(source, state.Value);
						}
						break;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							_ctx.ExecCommand();
							var idtval = await this.OrmInsert(source).ExecuteIdentityAsync();
							_ctx._affrows++;
							_fsql.SetEntityIdentityValue(source, idtval);
						} else {
							throw new Exception($"DbSet.Add 失败，未设置主键的值，或者没有配置自增，或者自增列数不为1：{_fsql.GetEntityString(source)}");
						}
						break;
				}

				state.Key = key = _fsql.GetEntityKeyString(source);
				state.Time = DateTime.Now;
			} else {
				if (_vals.ContainsKey(key))
					throw new Exception($"DbSet.Add 失败，实体数据已存在，请勿重复添加：{_fsql.GetEntityString(source)}");
				_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Insert, this, typeof(EntityState), state);
			}
			if (state.Value == null) {
				state.Value = Activator.CreateInstance<TEntity>();
				_fsql.CopyEntityValue(state.Value, source); //copy, 记录旧值版本
			}
			_vals.Add(key, state);
		}
		async public Task AddRangeAsync(TEntity[] source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			for (var a = 0; a < source.Length; a++)
				await AddAsync(source[a]);
		}
		async public Task AddRangeAsync(IEnumerable<TEntity> source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			foreach (var item in source)
				await AddAsync(item);
		}

		Task<int> DbContextBetchUpdateAsync(EntityState[] ups) => DbContextBetchUpdatePrivAsync(ups, false);
		Task<int> DbContextBetchUpdateNowAsync(EntityState[] ups) => DbContextBetchUpdatePrivAsync(ups, true);
		async Task<int> DbContextBetchUpdatePrivAsync(EntityState[] ups, bool isLiveUpdate) {
			if (ups.Any() == false) return 0;
			var uplst1 = ups[ups.Length - 1];
			var uplst2 = ups.Length > 1 ? ups[ups.Length - 2] : null;

			if (_vals.TryGetValue(uplst1.Key, out var lstval1) == false) return -999;
			var lstval2 = default(EntityState);
			if (uplst2 != null && _vals.TryGetValue(uplst2.Key, out lstval2) == false) throw new Exception($"DbSet.Update 失败，实体应该先查询再修改：{_fsql.GetEntityString(uplst2.Value)}");

			var cuig1 = _fsql.CompareEntityValueReturnColumns(uplst1.Value, lstval1.Value, true);
			var cuig2 = uplst2 != null ? _fsql.CompareEntityValueReturnColumns(uplst2.Value, lstval2.Value, true) : null;
			if (uplst2 != null && string.Compare(string.Join(",", cuig1), string.Join(",", cuig2)) != 0) {
				//最后一个不保存
				var source = ups.ToList();
				source.RemoveAt(ups.Length - 1);
				var affrows = await this.OrmUpdate(null).SetSource(source.Select(a => a.Value)).IgnoreColumns(cuig2).ExecuteAffrowsAsync();
				foreach (var newval in source) {
					if (_vals.TryGetValue(newval.Key, out var tryold))
						_fsql.CopyEntityValue(tryold.Value, newval.Value);
				}
				return affrows;
			} else if (isLiveUpdate) {
				//立即保存
				var source = ups;
				var affrows = await this.OrmUpdate(null).SetSource(source.Select(a => a.Value)).IgnoreColumns(cuig1).ExecuteAffrowsAsync();
				foreach (var newval in source) {
					if (_vals.TryGetValue(newval.Key, out var tryold))
						_fsql.CopyEntityValue(tryold.Value, newval.Value);
				}
				return Math.Min(ups.Length, affrows);
			}
			//等待下次对比再保存
			return 0;
		}
		async Task<int> DbContextBetchRemoveAsync(EntityState[] dels) {
			if (dels.Any() == false) return 0;
			var affrows = await this.OrmDelete(dels.Select(a => a.Value)).ExecuteAffrowsAsync();
			//foreach (var del in dels)
			//	_vals.Remove(del.Key);
			return affrows;
		}
	}
}
