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

namespace FreeSql {
	partial class DbSet<TEntity> {

		async public Task AddAsync(TEntity source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			var key = GetEntityKeyString(source);
			TEntity newval = null;
			if (string.IsNullOrEmpty(key)) {
				var ids = _table.Primarys.Where(a => a.Attribute.IsIdentity).ToArray();

				switch (_ctx._orm.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						if (ids.Length == 1 && _table.Primarys.Length == 1) {
							await _ctx.ExecCommandAsync();
							var idtval = await this.OrmInsert(source).ExecuteIdentityAsync();
							_ctx._affrows++;
							SetEntityIdentityValue(source, idtval);
						} else {
							await _ctx.ExecCommandAsync();
							newval = (await this.OrmInsert(source).ExecuteInsertedAsync()).First();
							_ctx._affrows++;
							CopyNewValueToEntity(source, newval);
						}
						break;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						if (ids.Length == 1 && _table.Primarys.Length == 1) {
							await _ctx.ExecCommandAsync();
							var idtval = await this.OrmInsert(source).ExecuteIdentityAsync();
							_ctx._affrows++;
							SetEntityIdentityValue(source, idtval);
						} else {
							throw new Exception("DbSet.Add 失败，由于实体没有主键值，或者没有配置自增，或者自增列数不为1。");
						}
						break;
				}

				key = GetEntityKeyString(source);
			} else {
				if (_vals.ContainsKey(key))
					throw new Exception("DbSet.Add 失败，实体数据已存在，请勿重复添加。");
				_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Insert, _entityType, this, source);
			}
			if (newval == null) {
				newval = Activator.CreateInstance<TEntity>();
				CopyNewValueToEntity(newval, source);
			}
			_vals.Add(key, newval);
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

		async Task<int> DbContextBetchUpdateAsync(TEntity[] ups, bool isLiveUpdate) {
			if (ups.Any() == false) return 0;
			var uplst1 = ups[ups.Length - 1];
			var uplst2 = ups.Length > 1 ? ups[ups.Length - 2] : null;

			var lstkey1 = GetEntityKeyString(uplst1);
			if (_vals.TryGetValue(lstkey1, out var lstval1) == false) throw new Exception("DbSet.Update 失败，实体应该先查询再修改。");
			TEntity lstval2 = default(TEntity);
			if (uplst2 != null) {
				var lstkey2 = GetEntityKeyString(uplst2);
				if (_vals.TryGetValue(lstkey2, out lstval2) == false) throw new Exception("DbSet.Update 失败，实体应该先查询再修改。");
			}

			var cuig1 = CompareUpdateIngoreColumns(uplst1, lstval1);
			var cuig2 = uplst2 != null ? CompareUpdateIngoreColumns(uplst2, lstval2) : null;
			if (uplst2 != null && string.Compare(cuig1, cuig2, true) != 0) {
				//最后一个不保存
				var ignores = cuig2.Split(new[] { ", " }, StringSplitOptions.None);
				var source = ups.ToList();
				source.RemoveAt(ups.Length - 1);
				var affrows = await this.OrmUpdate(null).SetSource(source).IgnoreColumns(ignores).ExecuteAffrowsAsync();
				foreach (var newval in source) {
					var newkey = GetEntityKeyString(newval);
					if (_vals.TryGetValue(newkey, out var tryold))
						CopyNewValueToEntity(tryold, newval);
				}
				return affrows;
			} else if (isLiveUpdate) {
				//立即保存
				var ignores = cuig1.Split(new[] { ", " }, StringSplitOptions.None);
				var affrows = await this.OrmUpdate(null).SetSource(ups).IgnoreColumns(ignores).ExecuteAffrowsAsync();
				foreach (var newval in ups) {
					var newkey = GetEntityKeyString(newval);
					if (_vals.TryGetValue(newkey, out var tryold))
						CopyNewValueToEntity(tryold, newval);
				}
				return Math.Min(ups.Length, affrows);
			}
			//等待下次对比再保存
			return 0;
		}

		async Task<int> DbContextBetchRemoveAsync(TEntity[] dels) {
			if (dels.Any() == false) return 0;

			var affrows = await this.OrmDelete(dels).ExecuteAffrowsAsync();
			foreach (var del in dels) {
				var key = GetEntityKeyString(del);
				_vals.Remove(key);
			}
			return affrows;
		}
	}
}
