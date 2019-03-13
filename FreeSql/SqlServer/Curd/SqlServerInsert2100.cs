//using FreeSql.Internal;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Common;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FreeSql.SqlServer.Curd {

//	class SqlServerInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class {
//		public SqlServerInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
//			: base(orm, commonUtils, commonExpression) {
//		}

//		public override int ExecuteAffrows() {
//			var sql = this.ToSql2100();
//			switch (sql.Count) {
//				case 0: return 0;
//				case 1: return _orm.Ado.ExecuteNonQuery(_transaction, CommandType.Text, sql[0].Item1, sql[0].Item2);
//				default:
//					var affrows = 0;
//					if (_transaction == null) {
//						using (var conn = _orm.Ado.MasterPool.Get()) {
//							var tran = conn.Value.BeginTransaction();
//							try {
//								foreach (var s in sql)
//									affrows += _orm.Ado.ExecuteNonQuery(tran, CommandType.Text, s.Item1, s.Item2);
//								tran.Commit();
//							} catch {
//								tran.Rollback();
//							}
//						}
//					} else {
//						foreach (var s in sql)
//							affrows += _orm.Ado.ExecuteNonQuery(_transaction, CommandType.Text, s.Item1, s.Item2);
//					}
//					return affrows;
//			}
//		}
//		async public override Task<int> ExecuteAffrowsAsync() {
//			var sql = this.ToSql2100();
//			switch (sql.Count) {
//				case 0: return 0;
//				case 1: return await _orm.Ado.ExecuteNonQueryAsync(_transaction, CommandType.Text, sql[0].Item1, sql[0].Item2);
//				default:
//					var affrows = 0;
//					if (_transaction == null) {
//						using (var conn = await _orm.Ado.MasterPool.GetAsync()) {
//							var tran = conn.Value.BeginTransaction();
//							try {
//								foreach (var s in sql)
//									affrows += await _orm.Ado.ExecuteNonQueryAsync(tran, CommandType.Text, s.Item1, s.Item2);
//								tran.Commit();
//							} catch {
//								tran.Rollback();
//							}
//						}
//					} else {
//						foreach (var s in sql)
//							affrows += await _orm.Ado.ExecuteNonQueryAsync(_transaction, CommandType.Text, s.Item1, s.Item2);
//					}
//					return affrows;
//			}
//		}

//		public override long ExecuteIdentity() {
//			var sql = this.ToSql2100();
//			switch (sql.Count) {
//				case 0: return 0;
//				case 1:
//					if (string.IsNullOrEmpty(sql[0].Item1)) return 0;
//					return long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_transaction, CommandType.Text, string.Concat(sql[0].Item1, "; SELECT SCOPE_IDENTITY();"), sql[0].Item2)), out var trylng) ? trylng : 0;
//				default:
//					long ret = 0;
//					if (_transaction == null) {
//						using (var conn = _orm.Ado.MasterPool.Get()) {
//							var tran = conn.Value.BeginTransaction();
//							try {
//								for (var a = 0; a < sql.Count; a++) {
//									var s = sql[a];
//									if (a < sql.Count - 1) _orm.Ado.ExecuteNonQuery(tran, CommandType.Text, s.Item1, s.Item2);
//									else ret = long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(tran, CommandType.Text, string.Concat(s.Item1, "; SELECT SCOPE_IDENTITY();"), s.Item2)), out trylng) ? trylng : 0;
//								}
//								tran.Commit();
//							} catch {
//								tran.Rollback();
//							}
//						}
//					} else {
//						for (var a = 0; a < sql.Count; a++) {
//							var s = sql[a];
//							if (a < sql.Count - 1) _orm.Ado.ExecuteNonQuery(_transaction, CommandType.Text, s.Item1, s.Item2);
//							else ret = long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_transaction, CommandType.Text, string.Concat(s.Item1, "; SELECT SCOPE_IDENTITY();"), s.Item2)), out trylng) ? trylng : 0;
//						}
//					}
//					return ret;
//			}
//		}
//		async public override Task<long> ExecuteIdentityAsync() {
//			var sql = this.ToSql2100();
//			switch (sql.Count) {
//				case 0: return 0;
//				case 1:
//					if (string.IsNullOrEmpty(sql[0].Item1)) return 0;
//					return long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_transaction, CommandType.Text, string.Concat(sql[0].Item1, "; SELECT SCOPE_IDENTITY();"), sql[0].Item2)), out var trylng) ? trylng : 0;
//				default:
//					long ret = 0;
//					if (_transaction == null) {
//						using (var conn = await _orm.Ado.MasterPool.GetAsync()) {
//							var tran = conn.Value.BeginTransaction();
//							try {
//								for (var a = 0; a < sql.Count; a++) {
//									var s = sql[a];
//									if (a < sql.Count - 1) await _orm.Ado.ExecuteNonQueryAsync(tran, CommandType.Text, s.Item1, s.Item2);
//									else ret = long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(tran, CommandType.Text, string.Concat(s.Item1, "; SELECT SCOPE_IDENTITY();"), s.Item2)), out trylng) ? trylng : 0;
//								}
//								tran.Commit();
//							} catch {
//								tran.Rollback();
//							}
//						}
//					} else {
//						for (var a = 0; a < sql.Count; a++) {
//							var s = sql[a];
//							if (a < sql.Count - 1) await _orm.Ado.ExecuteNonQueryAsync(_transaction, CommandType.Text, s.Item1, s.Item2);
//							else ret = long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_transaction, CommandType.Text, string.Concat(s.Item1, "; SELECT SCOPE_IDENTITY();"), s.Item2)), out trylng) ? trylng : 0;
//						}
//					}
//					return ret;
//			}
//		}

//		public override List<T1> ExecuteInserted() {
//			string output = null;
//			Func<string, string> getOutputSql = oldsql => {
//				if (string.IsNullOrEmpty(output)) {
//					var sb = new StringBuilder();
//					sb.Append(" OUTPUT ");
//					var colidx = 0;
//					foreach (var col in _table.Columns.Values) {
//						if (colidx > 0) sb.Append(", ");
//						sb.Append(_commonUtils.QuoteReadColumn(col.CsType, $"INSERTED.{_commonUtils.QuoteSqlName(col.Attribute.Name)}")).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
//						++colidx;
//					}
//					output = sb.ToString();
//				}
//				var validx = oldsql.IndexOf(") VALUES");
//				if (validx == -1) throw new ArgumentException("找不到 VALUES");
//				var newsql = new StringBuilder().Append(output);
//				newsql.Insert(0, oldsql.Substring(0, validx + 1));
//				newsql.Append(oldsql.Substring(validx + 1));
//				return newsql.ToString();
//			};

//			var sql = this.ToSql2100();
//			switch (sql.Count) {
//				case 0: return new List<T1>();
//				case 1:
//					if (string.IsNullOrEmpty(sql[0].Item1)) return new List<T1>();
//					return _orm.Ado.Query<T1>(_transaction, CommandType.Text, getOutputSql(sql[0].Item1), sql[0].Item2);
//				default:
//					var ret = new List<T1>();
//					if (_transaction == null) {
//						using (var conn = _orm.Ado.MasterPool.Get()) {
//							var tran = conn.Value.BeginTransaction();
//							try {
//								foreach (var s in sql)
//									ret.AddRange(_orm.Ado.Query<T1>(tran, CommandType.Text, getOutputSql(s.Item1), s.Item2));
//								tran.Commit();
//							} catch {
//								tran.Rollback();
//							}
//						}
//					} else {
//						foreach (var s in sql)
//							ret.AddRange(_orm.Ado.Query<T1>(_transaction, CommandType.Text, getOutputSql(s.Item1), s.Item2));
//					}
//					return ret;
//			}
//		}
//		async public override Task<List<T1>> ExecuteInsertedAsync() {
//			string output = null;
//			Func<string, string> getOutputSql = oldsql => {
//				if (string.IsNullOrEmpty(output)) {
//					var sb = new StringBuilder();
//					sb.Append(" OUTPUT ");
//					var colidx = 0;
//					foreach (var col in _table.Columns.Values) {
//						if (colidx > 0) sb.Append(", ");
//						sb.Append(_commonUtils.QuoteReadColumn(col.CsType, $"INSERTED.{_commonUtils.QuoteSqlName(col.Attribute.Name)}")).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
//						++colidx;
//					}
//					output = sb.ToString();
//				}
//				var validx = oldsql.IndexOf(") VALUES");
//				if (validx == -1) throw new ArgumentException("找不到 VALUES");
//				var newsql = new StringBuilder().Append(output);
//				newsql.Insert(0, oldsql.Substring(0, validx + 1));
//				newsql.Append(oldsql.Substring(validx + 1));
//				return oldsql;
//			};

//			var sql = this.ToSql2100();
//			switch (sql.Count) {
//				case 0: return new List<T1>();
//				case 1:
//					if (string.IsNullOrEmpty(sql[0].Item1)) return new List<T1>();
//					return await _orm.Ado.QueryAsync<T1>(_transaction, CommandType.Text, getOutputSql(sql[0].Item1), sql[0].Item2);
//				default:
//					var ret = new List<T1>();
//					if (_transaction == null) {
//						using (var conn = await _orm.Ado.MasterPool.GetAsync()) {
//							var tran = conn.Value.BeginTransaction();
//							try {
//								foreach (var s in sql)
//									ret.AddRange(await _orm.Ado.QueryAsync<T1>(tran, CommandType.Text, getOutputSql(s.Item1), s.Item2));
//								tran.Commit();
//							} catch {
//								tran.Rollback();
//							}
//						}
//					} else {
//						foreach (var s in sql)
//							ret.AddRange(await _orm.Ado.QueryAsync<T1>(_transaction, CommandType.Text, getOutputSql(s.Item1), s.Item2));
//					}
//					return ret;
//			}
//		}

//		public List<(string, DbParameter[])> ToSql2100() { //传入的请求具有过多的参数。该服务器支持最多 2100 个参数。请减少参数的数目，然后重新发送该请求。
//			if (_source == null || _source.Any() == false) return new List<(string, DbParameter[])>();
//			var ret = new List<(string, DbParameter[])>();
//			var sbhead = new StringBuilder();
//			sbhead.Append("INSERT INTO ").Append(_commonUtils.QuoteSqlName(_tableRule?.Invoke(_table.DbName) ?? _table.DbName)).Append("(");
//			var colidx = 0;
//			foreach (var col in _table.Columns.Values)
//				if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name) == false) {
//					if (colidx > 0) sbhead.Append(", ");
//					sbhead.Append(_commonUtils.QuoteSqlName(col.Attribute.Name));
//					++colidx;
//				}
//			sbhead.Append(") VALUES");
//			var sbh = sbhead.ToString();

//			var sbsql = new StringBuilder().Append(sbh);
//			var sbsqlParams = new List<DbParameter>();
//			var didx = 0;
//			foreach (var d in _source) {
//				if ((didx + 1) * colidx >= 2100) {
//					ret.Add((sbsql.ToString(), sbsqlParams.ToArray()));
//					sbsql.Clear().Append(sbh);
//					sbsqlParams.Clear();
//					didx = 0;
//				}

//				if (sbsqlParams.Count > 0) sbsql.Append(", ");
//				sbsql.Append("(");
//				var colidx2 = 0;
//				foreach (var col in _table.Columns.Values)
//					if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name) == false) {
//						if (colidx2 > 0) sbsql.Append(", ");
//						sbsql.Append(_commonUtils.QuoteWriteParamter(col.CsType, $"{_commonUtils.QuoteParamterName(col.CsName)}{didx}"));
//						object val = null;
//						if (_table.Properties.TryGetValue(col.CsName, out var tryp)) {
//							val = tryp.GetValue(d);
//							if (col.Attribute.IsPrimary && (col.CsType == typeof(Guid) || col.CsType == typeof(Guid?))
//								&& (val == null || (Guid)val == Guid.Empty)) tryp.SetValue(d, val = FreeUtil.NewMongodbId());
//						}
//						sbsqlParams.Add(_commonUtils.AppendParamter(null, $"{col.CsName}{didx}", col.CsType, val));
//						++colidx2;
//					}
//				sbsql.Append(")");
//				++didx;
//			}
//			ret.Add((sbsql.ToString(), sbsqlParams.ToArray()));
//			return ret;
//		}
//	}
//}
