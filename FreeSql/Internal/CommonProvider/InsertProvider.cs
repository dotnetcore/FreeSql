using FreeSql.Internal.Model;
using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    public abstract partial class InsertProvider<T1> : IInsert<T1> where T1 : class
    {
        protected IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        protected List<T1> _source = new List<T1>();
        protected Dictionary<string, bool> _ignore = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        protected TableInfo _table;
        protected Func<string, string> _tableRule;
        protected bool _noneParameter;
        protected DbParameter[] _params;
        protected DbTransaction _transaction;
        protected DbConnection _connection;

        public InsertProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
            _table = _commonUtils.GetTableByEntity(typeof(T1));
            _noneParameter = _orm.CodeFirst.IsNoneCommandParameter;
            if (_orm.CodeFirst.IsAutoSyncStructure && typeof(T1) != typeof(object)) _orm.CodeFirst.SyncStructure<T1>();
        }

        protected void ClearData()
        {
            _source.Clear();
            _ignore.Clear();
            _params = null;
        }

        public IInsert<T1> WithTransaction(DbTransaction transaction)
        {
            _transaction = transaction;
            _connection = _transaction?.Connection;
            return this;
        }
        public IInsert<T1> WithConnection(DbConnection connection)
        {
            if (_transaction?.Connection != connection) _transaction = null;
            _connection = connection;
            return this;
        }

        public IInsert<T1> NoneParameter()
        {
            _noneParameter = true;
            return this;
        }

        public IInsert<T1> AppendData(T1 source)
        {
            if (source != null) _source.Add(source);
            return this;
        }
        public IInsert<T1> AppendData(T1[] source)
        {
            if (source != null) _source.AddRange(source);
            return this;
        }
        public IInsert<T1> AppendData(IEnumerable<T1> source)
        {
            if (source != null) _source.AddRange(source.Where(a => a != null));
            return this;
        }

        #region 参数化数据限制，或values数量限制
        protected List<T1>[] SplitSource(int valuesLimit, int parameterLimit)
        {
            valuesLimit = valuesLimit - 1;
            parameterLimit = parameterLimit - 1;
            if (_source == null || _source.Any() == false) return new List<T1>[0];
            if (_source.Count == 1) return new[] { _source };
            if (_noneParameter)
            {
                if (_source.Count < valuesLimit) return new[] { _source };

                var execCount = (int)Math.Ceiling(1.0 * _source.Count / valuesLimit);
                var ret = new List<T1>[execCount];
                for (var a = 0; a < execCount; a++)
                {
                    var subSource = new List<T1>();
                    subSource = _source.GetRange(a * valuesLimit, Math.Min(valuesLimit, _source.Count - a * valuesLimit));
                    ret[a] = subSource;
                }
                return ret;
            }
            else
            {
                var colSum = _table.Columns.Count - _ignore.Count;
                var takeMax = parameterLimit / colSum;
                var pamTotal = colSum * _source.Count;
                if (pamTotal < parameterLimit) return new[] { _source };

                var execCount = (int)Math.Ceiling(1.0 * pamTotal / takeMax / colSum);
                var ret = new List<T1>[execCount];
                for (var a = 0; a < execCount; a++)
                {
                    var subSource = new List<T1>();
                    subSource = _source.GetRange(a * takeMax, Math.Min(takeMax, _source.Count - a * takeMax));
                    ret[a] = subSource;
                }
                return ret;
            }
        }
        protected int SplitExecuteAffrows(int valuesLimit, int parameterLimit)
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
                ret = this.RawExecuteAffrows();
                ClearData();
                return ret;
            }
            if (_transaction == null)
                this.WithTransaction(_orm.Ado.TransactionCurrentThread);

            if (_transaction != null)
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    ret += this.RawExecuteAffrows();
                }
            }
            else
            {
                using (var conn = _orm.Ado.MasterPool.Get())
                {
                    _transaction = conn.Value.BeginTransaction();
                    try
                    {
                        for (var a = 0; a < ss.Length; a++)
                        {
                            _source = ss[a];
                            ret += this.RawExecuteAffrows();
                        }
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                        throw;
                    }
                    _transaction = null;
                }
            }
            ClearData();
            return ret;
        }
        async protected Task<int> SplitExecuteAffrowsAsync(int valuesLimit, int parameterLimit)
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
                ret = await this.RawExecuteAffrowsAsync();
                ClearData();
                return ret;
            }
            if (_transaction == null)
                this.WithTransaction(_orm.Ado.TransactionCurrentThread);

            if (_transaction != null)
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    ret += await this.RawExecuteAffrowsAsync();
                }
            }
            else
            {
                using (var conn = await _orm.Ado.MasterPool.GetAsync())
                {
                    _transaction = conn.Value.BeginTransaction();
                    try
                    {
                        for (var a = 0; a < ss.Length; a++)
                        {
                            _source = ss[a];
                            ret += await this.RawExecuteAffrowsAsync();
                        }
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                        throw;
                    }
                    _transaction = null;
                }
            }
            ClearData();
            return ret;
        }
        protected long SplitExecuteIdentity(int valuesLimit, int parameterLimit)
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
                ret = this.RawExecuteIdentity();
                ClearData();
                return ret;
            }
            if (_transaction == null)
                this.WithTransaction(_orm.Ado.TransactionCurrentThread);

            if (_transaction != null)
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    if (a < ss.Length - 1) this.RawExecuteAffrows();
                    else ret = this.RawExecuteIdentity();
                }
            }
            else
            {
                using (var conn = _orm.Ado.MasterPool.Get())
                {
                    _transaction = conn.Value.BeginTransaction();
                    try
                    {
                        for (var a = 0; a < ss.Length; a++)
                        {
                            _source = ss[a];
                            if (a < ss.Length - 1) this.RawExecuteAffrows();
                            else ret = this.RawExecuteIdentity();
                        }
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                        throw;
                    }
                    _transaction = null;
                }
            }
            ClearData();
            return ret;
        }
        async protected Task<long> SplitExecuteIdentityAsync(int valuesLimit, int parameterLimit)
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
                ret = await this.RawExecuteIdentityAsync();
                ClearData();
                return ret;
            }
            if (_transaction == null)
                this.WithTransaction(_orm.Ado.TransactionCurrentThread);

            if (_transaction != null)
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    if (a < ss.Length - 1) await this.RawExecuteAffrowsAsync();
                    else ret = await this.RawExecuteIdentityAsync();
                }
            }
            else
            {
                using (var conn = await _orm.Ado.MasterPool.GetAsync())
                {
                    _transaction = conn.Value.BeginTransaction();
                    try
                    {
                        for (var a = 0; a < ss.Length; a++)
                        {
                            _source = ss[a];
                            if (a < ss.Length - 1) await this.RawExecuteAffrowsAsync();
                            else ret = await this.RawExecuteIdentityAsync();
                        }
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                        throw;
                    }
                    _transaction = null;
                }
            }
            ClearData();
            return ret;
        }
        protected List<T1> SplitExecuteInserted(int valuesLimit, int parameterLimit)
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
                ret = this.RawExecuteInserted();
                ClearData();
                return ret;
            }
            if (_transaction == null)
                this.WithTransaction(_orm.Ado.TransactionCurrentThread);

            if (_transaction != null)
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    ret.AddRange(this.RawExecuteInserted());
                }
            }
            else
            {
                using (var conn = _orm.Ado.MasterPool.Get())
                {
                    _transaction = conn.Value.BeginTransaction();
                    try
                    {
                        for (var a = 0; a < ss.Length; a++)
                        {
                            _source = ss[a];
                            ret.AddRange(this.RawExecuteInserted());
                        }
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                        throw;
                    }
                    _transaction = null;
                }
            }
            ClearData();
            return ret;
        }
        async protected Task<List<T1>> SplitExecuteInsertedAsync(int valuesLimit, int parameterLimit)
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
                ret = await this.RawExecuteInsertedAsync();
                ClearData();
                return ret;
            }
            if (_transaction == null)
                this.WithTransaction(_orm.Ado.TransactionCurrentThread);

            if (_transaction != null)
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    ret.AddRange(await this.RawExecuteInsertedAsync());
                }
            }
            else
            {
                using (var conn = await _orm.Ado.MasterPool.GetAsync())
                {
                    _transaction = conn.Value.BeginTransaction();
                    try
                    {
                        for (var a = 0; a < ss.Length; a++)
                        {
                            _source = ss[a];
                            ret.AddRange(await this.RawExecuteInsertedAsync());
                        }
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                        throw;
                    }
                    _transaction = null;
                }
            }
            ClearData();
            return ret;
        }
        #endregion

        protected int RawExecuteAffrows()
        {
            var sql = ToSql();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _params);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return affrows;
        }
        async protected Task<int> RawExecuteAffrowsAsync()
        {
            var sql = ToSql();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _params);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return affrows;
        }
        protected abstract long RawExecuteIdentity();
        protected abstract Task<long> RawExecuteIdentityAsync();
        protected abstract List<T1> RawExecuteInserted();
        protected abstract Task<List<T1>> RawExecuteInsertedAsync();

        public abstract int ExecuteAffrows();
        public abstract Task<int> ExecuteAffrowsAsync();
        public abstract long ExecuteIdentity();
        public abstract Task<long> ExecuteIdentityAsync();
        public abstract List<T1> ExecuteInserted();
        public abstract Task<List<T1>> ExecuteInsertedAsync();

        public IInsert<T1> IgnoreColumns(Expression<Func<T1, object>> columns)
        {
            var cols = _commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false, null).Distinct();
            _ignore.Clear();
            foreach (var col in cols) _ignore.Add(col, true);
            return this;
        }
        public IInsert<T1> InsertColumns(Expression<Func<T1, object>> columns)
        {
            var cols = _commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false, null).ToDictionary(a => a, a => true);
            _ignore.Clear();
            foreach (var col in _table.Columns.Values)
                if (cols.ContainsKey(col.Attribute.Name) == false)
                    _ignore.Add(col.Attribute.Name, true);
            return this;
        }

        public IInsert<T1> AsTable(Func<string, string> tableRule)
        {
            _tableRule = tableRule;
            return this;
        }
        public IInsert<T1> AsType(Type entityType)
        {
            if (entityType == typeof(object)) throw new Exception("IInsert.AsType 参数不支持指定为 object");
            if (entityType == _table.Type) return this;
            var newtb = _commonUtils.GetTableByEntity(entityType);
            _table = newtb ?? throw new Exception("IInsert.AsType 参数错误，请传入正确的实体类型");
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(entityType);
            return this;
        }

        public virtual string ToSql()
        {
            if (_source == null || _source.Any() == false) return null;
            var sb = new StringBuilder();
            sb.Append("INSERT INTO ").Append(_commonUtils.QuoteSqlName(_tableRule?.Invoke(_table.DbName) ?? _table.DbName)).Append("(");
            var colidx = 0;
            foreach (var col in _table.Columns.Values)
                if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name) == false)
                {
                    if (colidx > 0) sb.Append(", ");
                    sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name));
                    ++colidx;
                }
            sb.Append(") VALUES");
            _params = _noneParameter ? new DbParameter[0] : new DbParameter[colidx * _source.Count];
            var specialParams = new List<DbParameter>();
            var didx = 0;
            foreach (var d in _source)
            {
                if (didx > 0) sb.Append(", ");
                sb.Append("(");
                var colidx2 = 0;
                foreach (var col in _table.Columns.Values)
                    if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name) == false)
                    {
                        if (colidx2 > 0) sb.Append(", ");
                        object val = col.GetMapValue(d);
                        if (col.Attribute.IsPrimary && col.Attribute.MapType.NullableTypeOrThis() == typeof(Guid) && (val == null || (Guid)val == Guid.Empty))
                            col.SetMapValue(d, val = FreeUtil.NewMongodbId());
                        if (_noneParameter)
                            sb.Append(_commonUtils.GetNoneParamaterSqlValue(specialParams, col.Attribute.MapType, val));
                        else
                        {
                            sb.Append(_commonUtils.QuoteWriteParamter(col.Attribute.MapType, _commonUtils.QuoteParamterName($"{col.CsName}_{didx}")));
                            _params[didx * colidx + colidx2] = _commonUtils.AppendParamter(null, $"{col.CsName}_{didx}", col.Attribute.MapType, val);
                        }
                        ++colidx2;
                    }
                sb.Append(")");
                ++didx;
            }
            if (_noneParameter && specialParams.Any())
                _params = specialParams.ToArray();
            return sb.ToString();
        }
    }
}

