using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.ClickHouse.Curd
{

    class ClickHouseInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public ClickHouseInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        internal bool InternalIsIgnoreInto = false;
        internal IFreeSql InternalOrm => _orm;
        internal TableInfo InternalTable => _table;
        internal DbParameter[] InternalParams => _params;
        internal DbConnection InternalConnection => _connection;
        internal DbTransaction InternalTransaction => _transaction;
        internal CommonUtils InternalCommonUtils => _commonUtils;
        internal CommonExpression InternalCommonExpression => _commonExpression;
        internal List<T1> InternalSource => _source;
        internal Dictionary<string, bool> InternalIgnore => _ignore;
        internal void InternalClearData() => ClearData();

        public override int ExecuteAffrows() => SplitExecuteAffrows(_batchValuesLimit > 0 ? _batchValuesLimit : int.MaxValue, _batchParameterLimit > 0 ? _batchParameterLimit : int.MaxValue);
        public override long ExecuteIdentity() => SplitExecuteIdentity(_batchValuesLimit > 0 ? _batchValuesLimit : int.MaxValue, _batchParameterLimit > 0 ? _batchParameterLimit : int.MaxValue);
        public override List<T1> ExecuteInserted() => SplitExecuteInserted(_batchValuesLimit > 0 ? _batchValuesLimit : int.MaxValue, _batchParameterLimit > 0 ? _batchParameterLimit : int.MaxValue);


        public override string ToSql()
        {
            if (InternalIsIgnoreInto == false) return base.ToSqlValuesOrSelectUnionAll();
            var sql = base.ToSqlValuesOrSelectUnionAll();
            return $"INSERT IGNORE INTO {sql.Substring(12)}";
        }

        protected override int RawExecuteAffrows()
        {
            var affrows = 0;
            Exception exception = null;
            Aop.CurdBeforeEventArgs before = null;
            if (_source.Count > 1)
            {
                try
                {
                    before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, null, _params);
                    _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
                    var data = ToDataTable();
                    using (var conn = _orm.Ado.MasterPool.Get())
                    {
                        using var bulkCopyInterface = new ClickHouseBulkCopy(conn.Value as ClickHouseConnection)
                        {
                            DestinationTableName = data.TableName,
                            BatchSize = _source.Count
                        };
                        bulkCopyInterface.WriteToServerAsync(data, default).Wait();
                    }
                    return affrows;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw ex;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }
            }
            return base.RawExecuteAffrows();
        }

        private IDictionary<string, object> GetValue<T>(T u, System.Reflection.PropertyInfo[] columns)
        {
            try
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                foreach (var item in columns)
                {
                    object v = null;
                    if (u != null)
                    {
                        v = item.GetValue(u);
                    }
                    dic.TryAdd(item.Name, v);
                }
                return dic;
            }
            catch
            {
                throw;
            }
        }

        protected override long RawExecuteIdentity()
        {
            long ret = 0;
            var identCols = _table.Columns.Where(a => a.Value.Attribute.IsIdentity == true);
            if (identCols.Any()&&_source.Count==1)
                ret = (long)identCols.First().Value.GetValue(_source.First());
            return ret;
        }
        protected override List<T1> RawExecuteInserted()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }
            sql = sb.ToString();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = _orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, null, CommandType.Text, sql, _commandTimeout, _params);
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
            return ret;
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => SplitExecuteAffrowsAsync(_batchValuesLimit > 0 ? _batchValuesLimit : int.MaxValue, _batchValuesLimit > 0 ? _batchValuesLimit : int.MaxValue, cancellationToken);
        public override Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default) => SplitExecuteIdentityAsync(_batchValuesLimit > 0 ? _batchValuesLimit : int.MaxValue, _batchValuesLimit > 0 ? _batchValuesLimit : int.MaxValue, cancellationToken);
        public override Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default) => SplitExecuteInsertedAsync(_batchValuesLimit > 0 ? _batchValuesLimit : int.MaxValue, _batchValuesLimit > 0 ? _batchValuesLimit : int.MaxValue, cancellationToken);

        async protected override Task<int> RawExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            var affrows = 0;
            Exception exception = null;
            Aop.CurdBeforeEventArgs before = null;
            if (_source.Count > 1)
            {
                try
                {
                    before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, null, _params);
                    _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
                    var data = ToDataTable();
                    using (var conn = await _orm.Ado.MasterPool.GetAsync())
                    {
                        using var bulkCopyInterface = new ClickHouseBulkCopy(conn.Value as ClickHouseConnection)
                        {
                            DestinationTableName = data.TableName,
                            BatchSize = _source.Count
                        };
                        await bulkCopyInterface.WriteToServerAsync(data, default);
                    }
                    return affrows;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw ex;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }
            }
            return await base.RawExecuteAffrowsAsync(cancellationToken);
        }

        async protected override Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default)
        {
            //var sql = this.ToSql();
            //if (string.IsNullOrEmpty(sql)) return 0;

            //sql = string.Concat(sql, "; SELECT LAST_INSERT_ID();");
            //var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            //_orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            //Exception exception = null;
            //try
            //{
            //    ret = long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, null, CommandType.Text, sql, _commandTimeout, _params, cancellationToken)), out var trylng) ? trylng : 0;
            //}
            //catch (Exception ex)
            //{
            //    exception = ex;
            //    throw;
            //}
            //finally
            //{
            //    var after = new Aop.CurdAfterEventArgs(before, exception, ret);
            //    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            //}
            return await Task.FromResult(ret);
        }


        protected override int SplitExecuteAffrows(int valuesLimit, int parameterLimit)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = 0;
            if (ss.Any() == false)
            {
                ClearData();
                return ret;
            }
            if (ss.Length == 1)
            {
                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = this.RawExecuteAffrows();
                ClearData();
                return ret;
            }
            var before = new Aop.TraceBeforeEventArgs("SplitExecuteAffrows", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                    ret += this.RawExecuteAffrows();
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }
        async protected override Task<int> SplitExecuteAffrowsAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = 0;
            if (ss.Any() == false)
            {
                ClearData();
                return ret;
            }
            if (ss.Length == 1)
            {
                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                await this.RawExecuteAffrowsAsync(cancellationToken);
                ret = _source.Count;
                ClearData();
                return ret;
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteAffrowsAsync", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                    await this.RawExecuteAffrowsAsync(cancellationToken);
                    ret += _source.Count;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }

        async protected override Task<long> SplitExecuteIdentityAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            long ret = 0;
            if (ss.Any() == false)
            {
                ClearData();
                return ret;
            }
            if (ss.Length == 1)
            {
                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = await this.RawExecuteIdentityAsync(cancellationToken);
                ClearData();
                return ret;
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteIdentityAsync", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                    if (a < ss.Length - 1) await this.RawExecuteAffrowsAsync(cancellationToken);
                    else ret = await this.RawExecuteIdentityAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }

        async protected override Task<List<T1>> SplitExecuteInsertedAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = new List<T1>();
            if (ss.Any() == false)
            {
                ClearData();
                return ret;
            }
            if (ss.Length == 1)
            {
                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = await this.RawExecuteInsertedAsync(cancellationToken);
                ClearData();
                return ret;
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteInsertedAsync", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                    ret.AddRange(await this.RawExecuteInsertedAsync(cancellationToken));
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }

        async protected override Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }
            sql = sb.ToString();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = await _orm.Ado.QueryAsync<T1>(_table.TypeLazy ?? _table.Type, _connection, null, CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
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
            return ret;
        }
#endif
    }
}
