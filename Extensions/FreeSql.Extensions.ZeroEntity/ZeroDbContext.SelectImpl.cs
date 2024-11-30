using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using T = System.Collections.Generic.Dictionary<string, object>;

namespace FreeSql.Extensions.ZeroEntity
{
	partial class ZeroDbContext
	{
		public class SelectImpl
		{
			ZeroDbContext _dbcontext;
			IFreeSql _orm => _dbcontext._orm;
			List<ZeroTableInfo> _tables => _dbcontext._tables;
			int _mainTableIndex = -1;
			List<TableAliasInfo> _tableAlias;
			ISelect<TestDynamicFilterInfo> _select;
			Select0Provider _selectProvider;
			string _field;
			Dictionary<string, string> _fieldAlias;
			List<Action<DbDataReaderContext>> _fieldReader;
			string _groupBy;
			List<DbParameter> _params = new List<DbParameter>();
			CommonUtils _common => _selectProvider._commonUtils;
			bool _useStates = true;
			bool _includeAll = false;

			SelectImpl() { }
			internal SelectImpl(ZeroDbContext dbcontext, string tableName)
			{
				_dbcontext = dbcontext;
				var tableIndex = _tables.FindIndex(a => a.CsName.ToLower() == tableName?.ToLower());
				if (tableIndex == -1) throw new Exception($"未定义表名 {tableName}");

				_mainTableIndex = tableIndex;
				_tableAlias = new List<TableAliasInfo>();
				_select = _orm.Select<TestDynamicFilterInfo>()
					.AsTable((t, old) => _tables[tableIndex].DbName);
				_selectProvider = _select as Select0Provider;
				_fieldAlias = new Dictionary<string, string>();
				_fieldReader = new List<Action<DbDataReaderContext>>();
				FlagFetchResult(_tables[_mainTableIndex], "a", "");
			}

			public SelectImpl NoTracking()
			{
				_useStates = false;
				return this;
			}
			public SelectImpl IncludeAll()
			{
				var ignores = new Dictionary<string, bool>();
				_includeAll = true;
				LocalAutoInclude(_tables[_mainTableIndex], "a");
				return this;

				void LocalAutoInclude(ZeroTableInfo table, string alias, string navPath = "")
				{
					if (ignores.ContainsKey(table.CsName)) return;
					ignores.Add(table.CsName, true);
					TableAliasInfo tableAlias = null;
					if (table != _tables[_mainTableIndex])
						tableAlias = FlagFetchResult(table, alias.ToString(), navPath);

					var leftJoins = table.Navigates.Where(a => a.Value.RefType == TableRefType.ManyToOne || a.Value.RefType == TableRefType.OneToOne).ToArray();
					foreach (var join in leftJoins)
					{
						var joinTable = join.Value.RefTable;
						if (ignores.ContainsKey(joinTable.CsName)) return;

						var joinAlias = GetMaxAlias();
						var joinOn = string.Join(" AND ", join.Value.RefColumns.Select((bname, idx) =>
							$"{joinAlias}.{_common.QuoteSqlName(join.Value.RefTable.ColumnsByCs[bname].Attribute.Name)} = {alias}.{_common.QuoteSqlName(join.Value.Table.ColumnsByCs[join.Value.Columns[idx]].Attribute.Name)}"));
						_select.LeftJoin($"{_common.QuoteSqlName(join.Value.RefTable.DbName)} {joinAlias} ON {joinOn}");

						LocalAutoInclude(joinTable, joinAlias, $"{navPath}.{join.Key}");
					}
					if (tableAlias == null) tableAlias = _tableAlias.Where(a => a.Alias == alias).First();
					var includeManys = table.Navigates.Where(a => a.Value.RefType == TableRefType.OneToMany || a.Value.RefType == TableRefType.ManyToMany).ToArray();
					foreach (var includeMany in includeManys)
						tableAlias.IncludeMany.Add(NativeTuple.Create(includeMany.Key, (Action<SelectImpl>)null));
				}
			}
			public SelectImpl Include(string navigate, Action<SelectImpl> then = null)
			{
				var alias = _tableAlias[0];
				var navPath = navigate.Split('.');
				var navPath2 = "";
				for (var navIdx = 0; navIdx < navPath.Length; navIdx++)
				{
					if (alias.Table.Navigates.TryGetValue(navPath[navIdx], out var nav) == false) throw new Exception($"{alias.Table.CsName} 未定义导航属性 {navPath[navIdx]}");
					if (nav.RefType == TableRefType.OneToMany || nav.RefType == TableRefType.ManyToMany)
					{
						if (navIdx < navPath.Length - 1) throw new Exception($"导航属性 OneToMany/ManyToMany {navPath[navIdx]} 不能处于中间位置");
						if (alias.IncludeMany.Where(a => a.Item1 == nav.NavigateKey).Any() == false)
							alias.IncludeMany.Add(NativeTuple.Create(nav.NavigateKey, then));
						break;
					}
					navPath2 = navIdx == 0 ? nav.NavigateKey : $"{navPath2}.{nav.NavigateKey}";
					var navAlias = _tableAlias.Where(a => string.Join(".", a.NavPath) == navPath2).FirstOrDefault();
					if (navAlias == null)
					{
						var joinAlias = GetMaxAlias();
						var joinOn = string.Join(" AND ", nav.RefColumns.Select((bname, idx) =>
							$"{joinAlias}.{_common.QuoteSqlName(nav.RefTable.ColumnsByCs[bname].Attribute.Name)} = {alias.Alias}.{_common.QuoteSqlName(nav.Table.ColumnsByCs[nav.Columns[idx]].Attribute.Name)}"));
						_select.LeftJoin($"{_common.QuoteSqlName(nav.RefTable.DbName)} {joinAlias} ON {joinOn}");
						navAlias = FlagFetchResult(nav.RefTable, joinAlias, navPath2);
					}
					alias = navAlias;
				}
				return this;
			}
			//public SelectImpl IncludeSubQuery(string resultKey, string tableName, Action<SelectImpl> then)
			//{
			//	var query = _dbcontext.SelectNoTracking(tableName);
			//	query._tableAlias.AddRange(_tableAlias);
			//	return this;
			//}

			/// <summary>
			/// 举例1：LeftJoin("table1", "id", "user.id") -> LEFT JOIN [table1] b ON b.[id] = a.[id]<para></para>
			/// 举例2：LeftJoin("table1", "id", "user.id", "xid", "user.xid") -> LEFT JOIN [table1] b ON b.[id] = [a].id] AND b.[xid] = a.[xid]
			/// </summary>
			public SelectImpl LeftJoin(string tableName, params string[] onFields) => Join("LEFT JOIN", tableName, onFields);
			/// <summary>
			/// 举例1：InnerJoin("table1", "id", "user.id") -> INNER JOIN [table1] b ON b.[id] = a.[id]<para></para>
			/// 举例2：InnerJoin("table1", "id", "user.id", "xid", "user.xid") -> INNER JOIN [table1] b ON b.[id] = [a].id] AND b.[xid] = a.[xid]
			/// </summary>
			public SelectImpl InnerJoin(string tableName, params string[] onFields) => Join("INNER JOIN", tableName, onFields);
			/// <summary>
			/// 举例1：RightJoin("table1", "id", "user.id") -> RIGTH JOIN [table1] b ON b.[id] = a.[id]<para></para>
			/// 举例2：RightJoin("table1", "id", "user.id", "xid", "user.xid") -> RIGTH JOIN [table1] b ON b.[id] = [a].id] AND b.[xid] = a.[xid]
			/// </summary>
			public SelectImpl RightJoin(string tableName, params string[] onFields) => Join("RIGTH JOIN", tableName, onFields);
			SelectImpl Join(string joinType, string tableName, params string[] onFields)
			{
				if (onFields == null || onFields.Length == 0) throw new Exception($"{joinType} 参数 {nameof(onFields)} 不能为空");
				if (onFields.Length % 2 != 0) throw new Exception($"{joinType} 参数 {nameof(onFields)} 数组长度必须为偶数，正确：.LeftJoin(\"table1\", \"id\", \"user.id\")");

				var table = _tables.Where(a => a.CsName.ToLower() == tableName?.ToLower()).FirstOrDefault();
				if (table == null) throw new Exception($"未定义表名 {tableName}");
				var alias = GetMaxAlias();
				var navKey = tableName;
				for (var a = 2; true; a++)
				{
					if (_tables[_mainTableIndex].ColumnsByCs.ContainsKey(navKey))
					{
						navKey = $"{tableName}{a}";
						continue;
					}
					if (_tableAlias.Any(b => b.NavPath.Length == 1 && b.NavPath.Last() == navKey))
					{
						navKey = $"{tableName}{a}";
						continue;
					}
					break;
				}
				FlagFetchResult(table, alias, navKey);
				var joinOn = new string[onFields.Length / 2];
				for (var a = 0; a < onFields.Length; a += 2)
				{
					var field1 = ParseField(table, alias, onFields[a]);
					if (field1 == null) throw new Exception($"未匹配字段名 {onFields[a]}");
					var field2 = ParseField(table, alias, onFields[a + 1]);
					if (field2 == null) throw new Exception($"未匹配字段名 {onFields[a + 1]}");
					joinOn[a / 2] = $"{field1.Item1} = {field2.Item1}";
				}
				_select.RawJoin($"{joinType} {_common.QuoteSqlName(table.DbName)} {alias} ON {string.Join(" AND ", joinOn)}");
				return this;
			}

			class TableAliasInfo
			{
				public string Alias { get; set; }
				public ZeroTableInfo Table { get; set; }
				public string[] NavPath { get; set; }
				public List<NativeTuple<string, Action<SelectImpl>>> IncludeMany { get; set; } = new List<NativeTuple<string, Action<SelectImpl>>>();
			}
			class DbDataReaderContext
			{
				public DbDataReader Reader { get; set; }
				public int Index { get; set; }
				public T Result { get; set; }
			}
			string GetMaxAlias()
			{
				var max = (int)_tableAlias.Where(a => a.Alias.Length == 1).Max(a => a.Alias[0]);
				if (max < 'a') max = 'a';
				for (var a = 1; true; a++)
				{
					var alias = ((char)(max + a)).ToString();
					if (_tableAlias.Where(b => b.Alias == alias).Any()) continue;
					return alias;
				}
			}
			TableAliasInfo FlagFetchResult(ZeroTableInfo table, string alias, string navPath)
			{
				var tableAlias = _tableAlias.Where(a => a.Alias == alias).FirstOrDefault();
				if (tableAlias == null)
				{
					var navPathArray = navPath.Split('.').Where(a => string.IsNullOrWhiteSpace(a) == false).ToArray();
					_tableAlias.Add(tableAlias = new TableAliasInfo
					{
						Alias = alias,
						Table = table,
						NavPath = navPathArray
					});
				}
				var sbfield = new StringBuilder();
				if (string.IsNullOrEmpty(_field) == false) sbfield.Append(", ").Append(_field);
				foreach (var col in table.Columns.Values)
				{
					var colName = col.Attribute.Name;
					for (var a = 2; true; a++)
					{
						if (_fieldAlias.ContainsKey(colName)) colName = $"{col.Attribute.Name}{a}";
						else break;
					}
					_fieldAlias.Add(colName, col.Attribute.Name);
					sbfield.Append(", ").Append(alias).Append(".").Append(_common.QuoteSqlName(col.Attribute.Name));
					if (colName != col.Attribute.Name) sbfield.Append(_common.FieldAsAlias(colName));
				}
				_field = sbfield.Remove(0, 2).ToString();
				sbfield.Clear();

				_fieldReader.Add(dr =>
				{
					var pkIsNull = false;
					foreach (var col in table.Columns.Values)
					{
						if (pkIsNull == false && col.Attribute.IsPrimary)
						{
							pkIsNull = dr.Reader.IsDBNull(dr.Index);
							if (pkIsNull) dr.Result.Clear();
						}
						if (pkIsNull == false) dr.Result[col.CsName] = Utils.GetDataReaderValue(col.CsType, dr.Reader.GetValue(dr.Index));
						dr.Index++;
					}
				});
				return tableAlias;
			}
			T FetchResult(DbDataReader reader)
			{
				var fieldIndex = 0;
				var result = new T();
				for (var aliasIndex = 0; aliasIndex < _tableAlias.Count; aliasIndex++)
				{
					var navValue = result;
					var drctx = new DbDataReaderContext { Index = fieldIndex, Reader = reader };
					if (aliasIndex == 0)
					{
						drctx.Result = result;
					}
					else
					{
						var isNull = false;
						for (var navidx = 0; navidx < _tableAlias[aliasIndex].NavPath.Length - 1; navidx++)
						{
							var navKey = _tableAlias[aliasIndex].NavPath[navidx];
							if (navValue.ContainsKey(navKey) == false)
							{
								isNull = true;
								break;
							}
							navValue = navValue[navKey] as T;
							if (navValue == null)
							{
								isNull = true;
								break;
							}
						}
						if (isNull)
						{
							fieldIndex += _tableAlias[aliasIndex].Table.Columns.Count;
							continue;
						}
						drctx.Result = new T();
						navValue[_tableAlias[aliasIndex].NavPath.LastOrDefault()] = drctx.Result;
					}
					_fieldReader[aliasIndex](drctx);
					fieldIndex = drctx.Index;
					if (aliasIndex > 0 && drctx.Result.Any() == false) navValue.Remove(_tableAlias[aliasIndex].NavPath.LastOrDefault());
				}
				return result;
			}
			
			void IncludeMany(TableAliasInfo alias, NativeTuple<string, Action<SelectImpl>> navMany, List<T> list, List<string> flagIndexs = null)
			{
				if (list?.Any() != true) return;
				if (flagIndexs == null) flagIndexs = new List<string>();
				flagIndexs.Add(alias.Table.CsName);

				var nav = alias.Table.Navigates[navMany.Item1];
				if (_includeAll && flagIndexs.Contains(nav.RefTable.CsName)) return;

				if (nav.RefType == TableRefType.OneToMany)
				{
					var subTable = nav.RefTable;
					var subSelect = new SelectImpl(_dbcontext, subTable.CsName);
					if (_includeAll) subSelect.IncludeAll();

					Func<Dictionary<string, bool>> getWhereDic = () =>
					{
						var sbDic = new Dictionary<string, bool>();
						for (var y = 0; y < list.Count; y++)
						{
							var sbWhereOne = new StringBuilder();
							sbWhereOne.Append("(");
							for (var z = 0; z < nav.Columns.Count; z++)
							{
								if (z > 0) sbWhereOne.Append(" AND ");
								var refcol = nav.RefTable.ColumnsByCs[nav.RefColumns[z]];
								var val = Utils.GetDataReaderValue(refcol.Attribute.MapType, list[y][nav.Columns[z]]);
								sbWhereOne.Append(_common.FormatSql($"a.{_common.QuoteSqlName(refcol.Attribute.Name)}={{0}}", val));
							}
							sbWhereOne.Append(")");
							var whereOne = sbWhereOne.ToString();
							sbWhereOne.Clear();
							if (sbDic.ContainsKey(whereOne) == false) sbDic.Add(whereOne, true);
						}
						return sbDic;
					};
					if (nav.Columns.Count == 1)
					{
						var refcol = nav.RefTable.ColumnsByCs[nav.RefColumns[0]];
						var args1 = $"a.{_common.QuoteSqlName(refcol.Attribute.Name)}";
						var left = _common.FormatSql("{0}", new object[] { list.Select(a => a[nav.Columns[0]]).Distinct().ToArray() });
						subSelect._select.Where($"({args1} in {left.Replace(",   \r\n    \r\n", $") \r\n OR {args1} in (")})");
					}
					else
					{
						var sbDic = getWhereDic();
						var sbWhere = new StringBuilder();
						foreach (var sbd in sbDic)
							sbWhere.Append(" OR ").Append(sbd.Key);
						subSelect._select.Where(sbWhere.Remove(0, 4).ToString());
						sbWhere.Clear();
						sbDic.Clear();
					}
					navMany.Item2?.Invoke(subSelect);
					var subList = subSelect.ToListPrivate(null, null, flagIndexs);
					foreach (var item in list)
					{
						item[nav.NavigateKey] = subList.Where(a =>
						{
							for (var z = 0; z < nav.Columns.Count; z++)
								if (CompareEntityPropertyValue(nav.Table.ColumnsByCs[nav.Columns[z]].Attribute.MapType, item[nav.Columns[z]], a[nav.RefColumns[z]]) == false)
									return false;
							return true;
						}).ToList();
					}
					subList.Clear();
				}
				else if (nav.RefType == TableRefType.ManyToMany)
				{
					var subTable = nav.RefTable;
					var subSelect = new SelectImpl(_dbcontext, subTable.CsName);
					if (_includeAll) subSelect.IncludeAll();

					var middleJoinOn = string.Join(" AND ", nav.RefColumns.Select((bname, idx) =>
							$"midtb.{_common.QuoteSqlName(nav.RefMiddleTable.ColumnsByCs[nav.MiddleColumns[nav.Columns.Count + idx]].Attribute.Name)} = a.{_common.QuoteSqlName(nav.RefTable.ColumnsByCs[bname].Attribute.Name)}"));
					subSelect._select.InnerJoin($"{_common.QuoteSqlName(nav.RefMiddleTable.DbName)} midtb ON {middleJoinOn}");

					Func<Dictionary<string, bool>> getWhereDic = () =>
					{
						var sbDic = new Dictionary<string, bool>();
						for (var y = 0; y < list.Count; y++)
						{
							var sbWhereOne = new StringBuilder();
							sbWhereOne.Append("(");
							for (var z = 0; z < nav.Columns.Count; z++)
							{
								if (z > 0) sbWhereOne.Append(" AND ");
								var midcol = nav.RefMiddleTable.ColumnsByCs[nav.MiddleColumns[z]];
								var val = Utils.GetDataReaderValue(midcol.Attribute.MapType, list[y][nav.Columns[z]]);
								sbWhereOne.Append(_common.FormatSql($"midtb.{_common.QuoteSqlName(midcol.Attribute.Name)}={{0}}", val));
							}
							sbWhereOne.Append(")");
							var whereOne = sbWhereOne.ToString();
							sbWhereOne.Clear();
							if (sbDic.ContainsKey(whereOne) == false) sbDic.Add(whereOne, true);
						}
						return sbDic;
					};
					if (nav.Columns.Count == 1)
					{
						var midcol = nav.RefMiddleTable.ColumnsByCs[nav.MiddleColumns[0]];
						var args1 = $"midtb.{_common.QuoteSqlName(midcol.Attribute.Name)}";
						var left = _common.FormatSql("{0}", new object[] { list.Select(a => a[nav.Columns[0]]).Distinct().ToArray() });
						subSelect._select.Where($"({args1} in {left.Replace(",   \r\n    \r\n", $") \r\n OR {args1} in (")})");
					}
					else
					{
						var sbDic = getWhereDic();
						var sbWhere = new StringBuilder();
						foreach (var sbd in sbDic)
							sbWhere.Append(" OR ").Append(sbd.Key);
						subSelect._select.Where(sbWhere.Remove(0, 4).ToString());
						sbWhere.Clear();
						sbDic.Clear();
					}
					navMany.Item2?.Invoke(subSelect);
					var subList = subSelect.ToListPrivate(
						string.Join(", ", nav.MiddleColumns.Select((a, idx) => $"midtb.{_common.QuoteSqlName(nav.RefMiddleTable.ColumnsByCs[a].Attribute.Name)}{_common.FieldAsAlias($"midtb_field__{idx}")}")),
						(dict, dr) =>
						{
							var fieldCount = dr.FieldCount - nav.MiddleColumns.Count;
							for (var z = 0; z < nav.MiddleColumns.Count; z++)
								dict[$"midtb_field__{z}"] = Utils.GetDataReaderValue(nav.RefMiddleTable.ColumnsByCs[nav.MiddleColumns[z]].CsType, dr.GetValue(fieldCount + z));
						}, flagIndexs);
					foreach (var item in list)
					{
						item[nav.NavigateKey] = subList.Where(a =>
						{
							for (var z = 0; z < nav.Columns.Count; z++)
								if (CompareEntityPropertyValue(nav.Table.ColumnsByCs[nav.Columns[z]].Attribute.MapType, item[nav.Columns[z]], a[$"midtb_field__{z}"]) == false)
									return false;
							return true;
						}).ToList();
					}
					foreach (var subItem in subList)
						for (var z = 0; z < nav.MiddleColumns.Count; z++)
							subItem.Remove($"midtb_field__{z}");
					subList.Clear();
				}
			}

			public string ToSql(string field = null)
			{
				if (string.IsNullOrWhiteSpace(field)) return _select.ToSql(_field);
				return _select.ToSql(field);
			}
			List<T> ToListPrivate(string otherField, Action<T, DbDataReader> otherReader, List<string> flagIndexs = null)
			{
				var sql = string.IsNullOrWhiteSpace(otherField) ? this.ToSql() : this.ToSql($"{_field},{otherField}");
				var ret = new List<T>();
				var dbParms = _params.ToArray();
				var before = new Aop.CurdBeforeEventArgs(_tables[_mainTableIndex].Type, _tables[_mainTableIndex], Aop.CurdType.Select, sql, dbParms);
				_orm.Aop.CurdBeforeHandler?.Invoke(this, before);
				Exception exception = null;
				try
				{
					_orm.Ado.ExecuteReader(_dbcontext._transaction?.Connection, _dbcontext._transaction, fetch =>
					{
						var item = FetchResult(fetch.Object);
						otherReader?.Invoke(item, fetch.Object);
						ret.Add(item);
					}, CommandType.Text, sql, _dbcontext._commandTimeout, dbParms);
				}
				catch (Exception ex)
				{
					exception = ex;
					throw;
				}
				finally
				{
					var after = new Aop.CurdAfterEventArgs(before, exception, ret);
					_orm.Aop.CurdAfterHandler?.Invoke(this, after);
				}
				foreach (var join in _tableAlias)
				{
					if (join.IncludeMany.Any() == false) continue;
					var list = new List<T>();
					if (join.Alias == "a") list = ret;
					else
					{
						foreach (var obj in ret)
						{
							T item = obj;
							foreach (var navKey in join.NavPath)
							{
								if (string.IsNullOrWhiteSpace(navKey)) continue;
								item.TryGetValue(navKey, out var obj2);
								item = obj2 as T;
								if (item == null) break;
							}
							if (item != null) list.Add(item);
						}
					}
					foreach(var navMany in join.IncludeMany)
						IncludeMany(join, navMany, list, flagIndexs);
				}
				if (_useStates && flagIndexs == null)
					foreach (var item in ret)
						_dbcontext.AttachCascade(_tables[_mainTableIndex], item, true);
				return ret;
			}
			public List<T> ToList() => ToListPrivate(null, null);
			public T ToOne() => ToListPrivate(null, null).FirstOrDefault();
			public T First() => ToOne();
			public bool Any() => _select.Any();
			public long Count() => _select.Count();
			public SelectImpl Count(out long count)
			{
				_select.Count(out count);
				return this;
			}
			public SelectImpl WithTransaction(DbTransaction transaction)
			{
				_select.WithTransaction(transaction);
				return this;
			}
			public SelectImpl WithConnection(DbConnection connection)
			{
				_select.WithConnection(connection);
				return this;
			}
			public SelectImpl CommandTimeout(int timeout)
			{
				_select.CommandTimeout(timeout);
				return this;
			}
			public SelectImpl Distinct()
			{
				_select.Distinct();
				return this;
			}
			public SelectImpl Master()
			{
				_select.Master();
				return this;
			}
			public SelectImpl ForUpdate(bool nowait = false)
			{
				_select.ForUpdate(nowait);
				return this;
			}

			NativeTuple<string, ColumnInfo> ParseField(ZeroTableInfo firstTable, string firstTableAlias, string property)
			{
				if (string.IsNullOrEmpty(property)) return null;
				var field = property.Split('.').Select(a => a.Trim()).ToArray();

				if (field.Length == 1)
				{
					if (firstTable != null && firstTable.ColumnsByCs.TryGetValue(field[0], out var col2) == true)
						return NativeTuple.Create($"{firstTableAlias}.{_common.QuoteSqlName(col2.Attribute.Name)}", col2);

					foreach (var ta2 in _tableAlias)
					{
						if (ta2.Table.ColumnsByCs.TryGetValue(field[0], out col2))
							return NativeTuple.Create($"{ta2.Alias}.{_common.QuoteSqlName(col2.Attribute.Name)}", col2);
					}
				}
				else if (field.Length == 2)
				{
					if (firstTable != null && firstTable.CsName.ToLower() == field[0].ToLower() && firstTable.ColumnsByCs.TryGetValue(field[1], out var col2) == true)
						return NativeTuple.Create($"{firstTableAlias}.{_common.QuoteSqlName(col2.Attribute.Name)}", col2);

					var ta2s = _tableAlias.Where(a => a.Table.CsName.ToLower() == field[0].ToLower()).ToArray();
					if (ta2s.Length == 1 && ta2s[0].Table.ColumnsByCs.TryGetValue(field[1], out col2) == true)
						return NativeTuple.Create($"{ta2s[0].Alias}.{_common.QuoteSqlName(col2.Attribute.Name)}", col2);
					if (ta2s.Length > 1)
					{
						ta2s = _tableAlias.Where(a => a.Table.CsName == field[0]).ToArray();
						if (ta2s.Length == 1 && ta2s[0].Table.ColumnsByCs.TryGetValue(field[1], out col2) == true)
							return NativeTuple.Create($"{ta2s[0].Alias}.{_common.QuoteSqlName(col2.Attribute.Name)}", col2);
					}
					if (_tableAlias.Where(a => a.Alias == field[0]).FirstOrDefault()?.Table.ColumnsByCs.TryGetValue(field[1], out col2) == true)
						return NativeTuple.Create($"{field[0]}.{_common.QuoteSqlName(col2.Attribute.Name)}", col2);
				}

				var navPath = string.Join(".", field.Skip(1).Take(field.Length - 1));
				var ta = _tableAlias.Where(a => string.Join(".", a.NavPath) == navPath).FirstOrDefault();
				if (ta?.Table.ColumnsByCs.TryGetValue(field.Last(), out var col) == true)
					return NativeTuple.Create($"{ta.Alias}.{_common.QuoteSqlName(col.Attribute.Name)}", col);
				throw new Exception(CoreErrorStrings.Cannot_Match_Property(property));
			}

			/// <summary>
			/// WHERE [Id] IN (...)
			/// </summary>
			public SelectImpl Where(IEnumerable<T> items)
			{
				var alias = _tableAlias.Where(a => a.Table == _tables[_mainTableIndex]).FirstOrDefault()?.Alias;
				if (!string.IsNullOrWhiteSpace(alias)) alias = $"{alias}.";
				var where = _common.WhereItems(_tables[_mainTableIndex].Primarys, alias, items, _selectProvider._params);
				_select.Where(where);
				return this;
			}
			/// <summary>
			/// Where(new { Year = 2017, CategoryId = 198, IsPublished = true })<para></para>
			/// WHERE [Year] = 2017 AND [CategoryId] = 198 AND [IsPublished] = 1
			/// </summary>
			public SelectImpl Where(object multipleFields)
			{
				if (multipleFields == null) return this;
				foreach (var prop in multipleFields.GetType().GetProperties())
					WhereDynamicFilter(new DynamicFilterInfo { Field = prop.Name, Operator = DynamicFilterOperator.Eq, Value = prop.GetValue(multipleFields, null) });
				return this;
			}
			/// <summary>
			/// WHERE [field] = ..
			/// </summary>
			public SelectImpl Where(string field, object value) => WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.Eq, Value = value });
			public SelectImpl Where(string field, string @operator, object value)
			{
				switch (@operator?.ToLower().Trim())
				{
					case "=":
					case "==":
					case "eq":
						return WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.Eq, Value = value });
					case "!=":
					case "<>":
						return WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.NotEqual, Value = value });
					case ">":
						return WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.GreaterThan, Value = value });
					case ">=":
						return WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.GreaterThanOrEqual, Value = value });
					case "<":
						return WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.LessThan, Value = value });
					case "<=":
						return WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.LessThanOrEqual, Value = value });
					case "like":
					case "contains":
						return WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.Contains, Value = value });
					case "!like":
					case "notlike":
					case "not like":
						return WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.NotContains, Value = value });
					case "in":
						return WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.Any, Value = value });
					case "!in":
					case "notin":
					case "not in":
						return WhereDynamicFilter(new DynamicFilterInfo { Field = field, Operator = DynamicFilterOperator.Any, Value = value });
				}
				throw new Exception($"未实现 {@operator}");
			}
			public SelectImpl WhereColumns(string field1, string @operator, string field2)
			{
				var field1Result = ParseField(null, null, field1);
				if (field1Result == null) throw new Exception($"未匹配字段名 {field1}");
				var field2Result = ParseField(null, null, field2);
				if (field2Result == null) throw new Exception($"未匹配字段名 {field2}");
				switch (@operator?.ToLower().Trim())
				{
					case "=":
					case "==":
					case "eq":
						_select.Where($"{field1Result.Item1} = {field2Result.Item1}");
						return this;
					case "!=":
					case "<>":
						_select.Where($"{field1Result.Item1} <> {field2Result.Item1}");
						return this;
					case ">":
						_select.Where($"{field1Result.Item1} > {field2Result.Item1}");
						return this;
					case ">=":
						_select.Where($"{field1Result.Item1} >= {field2Result.Item1}");
						return this;
					case "<":
						_select.Where($"{field1Result.Item1} < {field2Result.Item1}");
						return this;
					case "<=":
						_select.Where($"{field1Result.Item1} <= {field2Result.Item1}");
						return this;
				}
				throw new Exception($"未实现 {@operator}");
			}
			public SelectImpl WhereDynamicFilter(DynamicFilterInfo filter)
			{
				var sql = ParseDynamicFilter(filter);
				_selectProvider._where.Append(sql);
				return this;
			}
			string ParseDynamicFilter(DynamicFilterInfo filter)
			{
				var replacedFilter = new DynamicFilterInfo();
				var replacedMap = new List<NativeTuple<string, string>>();
				LocalCloneFilter(filter, replacedFilter);
				var oldWhere = _selectProvider._where.ToString();
				var newWhere = "";
				try
				{
					_selectProvider._where.Clear();
					_select.WhereDynamicFilter(replacedFilter);
					newWhere = _selectProvider._where.ToString();
				}
				finally
				{
					_selectProvider._where.Clear().Append(oldWhere);
				}
				foreach (var rm in replacedMap)
				{
					var find = $"a.{_common.QuoteSqlName(rm.Item1)}";
					var idx = newWhere.IndexOf(find);
					if (idx != -1) newWhere = $"{newWhere.Substring(0, idx)}{rm.Item2}{newWhere.Substring(idx + find.Length)}";
				}
				return newWhere;

				void LocalCloneFilter(DynamicFilterInfo source, DynamicFilterInfo target)
				{
					target.Field = source.Field;
					target.Operator = source.Operator;
					target.Value = source.Value;
					target.Logic = source.Logic;
					if (string.IsNullOrWhiteSpace(source.Field) == false)
					{
						var parseResult = ParseField(null, null, source.Field);
						if (parseResult != null)
						{
							if (TestDynamicFilterInfo._dictTypeToPropertyname.TryGetValue(parseResult.Item2.Attribute.MapType, out var pname))
								target.Field = pname;
							else
								target.Field = TestDynamicFilterInfo._dictTypeToPropertyname[typeof(string)];
							replacedMap.Add(NativeTuple.Create(target.Field, parseResult.Item1));
						}
					}
					if (source.Filters?.Any() == true)
					{
						target.Filters = new List<DynamicFilterInfo>();
						foreach (var sourceChild in source.Filters)
						{
							var targetChild = new DynamicFilterInfo();
							target.Filters.Add(targetChild);
							LocalCloneFilter(sourceChild, targetChild);
						}
					}
				}
			}
			public class SubQuery
			{
				internal SelectImpl _parentQuery;
				internal SubQuery() { }
				public SelectImpl From(string tableName)
				{
					var query = _parentQuery._dbcontext.SelectNoTracking(tableName);
					query._selectProvider._tables[0].Alias =
						query._tableAlias[0].Alias = $"sub_{_parentQuery._tableAlias[0].Alias}";
					query._tableAlias.AddRange(_parentQuery._tableAlias);
					return query;
				}
			}
			public SelectImpl WhereExists(Func<SubQuery, SelectImpl> q)
			{
				var query = q?.Invoke(new SubQuery { _parentQuery = this });
				switch (_orm.Ado.DataType)
				{
					case DataType.Oracle:
					case DataType.OdbcOracle:
					case DataType.CustomOracle:
					case DataType.Dameng:
					case DataType.GBase:
						query.Limit(-1);
						break;
					default:
						query.Limit(1); //#462 ORACLE rownum <= 2 会影响索引变慢
						break;
				}
				_selectProvider._where.Append($" AND EXISTS({query.ToSql("1").Replace(" \r\n", " \r\n    ")})");
				return this;
			}
			public SelectImpl GroupByRaw(string sql)
			{
				if (string.IsNullOrWhiteSpace(sql)) return this;
				_useStates = false;
				_groupBy = $"{_groupBy}, {sql}";
				_useStates = false;
				_select.GroupBy(_groupBy.Substring(2));
				return this;
			}
			public SelectImpl GroupBy(string[] fields)
			{
				var count = 0;
				for (var a = 0; a < fields.Length; a++)
				{
					if (string.IsNullOrWhiteSpace(fields[a])) continue;
					var field1 = ParseField(null, null, fields[a]);
					if (field1 == null) throw new Exception($"未匹配字段名 {fields[a]}");
					_groupBy = $"{_groupBy}, {field1.Item1}";
					count++;
				}
				if (count > 0)
				{
					_useStates = false;
					_select.GroupBy(_groupBy.Substring(2));
				}
				return this;
			}
			public SelectImpl HavingRaw(string sql)
			{
				_select.Having(sql);
				return this;
			}
			public SelectImpl OrderByRaw(string sql)
			{
				_select.OrderBy(sql);
				return this;
			}
			SelectImpl OrderBy(bool isdesc, string[] fields)
			{
				for (var a = 0; a < fields.Length; a ++)
				{
					if (string.IsNullOrWhiteSpace(fields[a])) continue;
					var field1 = ParseField(null, null, fields[a]);
					if (field1 == null) throw new Exception($"未匹配字段名 {fields[a]}");
					if (isdesc) _select.OrderBy($"{field1.Item1} DESC");
					else _select.OrderBy(field1.Item1);
				}
				return this;
			}
			public SelectImpl OrderBy(params string[] fields) => OrderBy(false, fields);
			public SelectImpl OrderByDescending(params string[] fields) => OrderBy(true, fields);
			public SelectImpl Offset(int offset)
			{
				_select.Offset(offset);
				return this;
			}
			public SelectImpl Limit(int limit)
			{
				_select.Limit(limit);
				return this;
			}
			public SelectImpl Skip(int offset) => Offset(offset);
			public SelectImpl Take(int limit) => Limit(limit);
			public SelectImpl Page(int pageNumber, int pageSize)
			{
				_select.Page(pageNumber, pageSize);
				return this;
			}

			TResult InternalQuerySingle<TResult>(string field) => _orm.Ado.CommandFluent(this.ToSql(field))
				.WithConnection(_selectProvider._connection)
				.WithTransaction(_selectProvider._transaction).QuerySingle<TResult>();
			public decimal Sum(string field)
			{
				var field1 = ParseField(null, null, field);
				if (field1 == null) throw new Exception($"未匹配字段名 {field}");
				return InternalQuerySingle<decimal>($"sum({field1.Item1})");
			}
			public TMember Min<TMember>(string field)
			{
				var field1 = ParseField(null, null, field);
				if (field1 == null) throw new Exception($"未匹配字段名 {field}");
				return InternalQuerySingle<TMember>($"min({field1.Item1})");
			}
			public TMember Max<TMember>(string field)
			{
				var field1 = ParseField(null, null, field);
				if (field1 == null) throw new Exception($"未匹配字段名 {field}");
				return InternalQuerySingle<TMember>($"max({field1.Item1})");
			}
			public double Avg(string field)
			{
				var field1 = ParseField(null, null, field);
				if (field1 == null) throw new Exception($"未匹配字段名 {field}");
				return InternalQuerySingle<double>($"avg({field1.Item1})");
			}


			[Table(DisableSyncStructure = true)]
			class TestDynamicFilterInfo
			{
				public Guid DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf00 { get; set; }
				public bool DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf01 { get; set; }
				public string DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf02 { get; set; }
				public char DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf03 { get; set; }
				public DateTime DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf04 { get; set; }
				public DateTimeOffset DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf05 { get; set; }
				public TimeSpan DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf06 { get; set; }

				public int DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf07 { get; set; }
				public long DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf08 { get; set; }
				public short DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf09 { get; set; }
				public sbyte DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf10 { get; set; }

				public uint DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf11 { get; set; }
				public ulong DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf12 { get; set; }
				public ushort DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf13 { get; set; }
				public byte DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf14 { get; set; }
				public byte[] DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf15 { get; set; }

				public double DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf16 { get; set; }
				public float DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf17 { get; set; }
				public decimal DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf18 { get; set; }

				internal static Dictionary<Type, string> _dictTypeToPropertyname = new Dictionary<Type, string>
				{
					[typeof(Guid)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf00),
					[typeof(bool)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf01),
					[typeof(string)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf02),
					[typeof(char)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf03),
					[typeof(DateTime)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf04),
					[typeof(DateTimeOffset)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf05),
					[typeof(TimeSpan)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf06),

					[typeof(int)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf07),
					[typeof(long)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf08),
					[typeof(short)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf09),
					[typeof(sbyte)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf10),

					[typeof(uint)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf11),
					[typeof(ulong)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf12),
					[typeof(ushort)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf13),
					[typeof(byte)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf14),
					[typeof(byte[])] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf15),

					[typeof(double)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf16),
					[typeof(float)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf17),
					[typeof(decimal)] = nameof(DynamicField_4bf98fbe2b4d4d14bbb3fc66fa08bf18),
				};
			}
		}
	}
}
