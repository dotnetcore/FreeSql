using FreeSql.Internal.Model;
using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.ObjectPool;
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
        public IFreeSql _orm;
        public CommonUtils _commonUtils;
        public CommonExpression _commonExpression;
        public List<T1> _source = new List<T1>();
        public Dictionary<string, bool> _ignore = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, bool> _auditValueChangedDict = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        public TableInfo _table;
        public Func<string, string> _tableRule;
        public string _noneParameterFlag = "c";
        public bool _noneParameter, _insertIdentity;
        public int _batchValuesLimit, _batchParameterLimit;
        public bool _batchAutoTransaction = true;
        public Action<BatchProgressStatus<T1>> _batchProgress;
        public DbParameter[] _params;
        public DbTransaction _transaction;
        public DbConnection _connection;
        public int _commandTimeout = 0;

        public InsertProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
            _table = _commonUtils.GetTableByEntity(typeof(T1));
            _noneParameter = _orm.CodeFirst.IsNoneCommandParameter;
            if (_orm.CodeFirst.IsAutoSyncStructure && typeof(T1) != typeof(object)) _orm.CodeFirst.SyncStructure<T1>();
            IgnoreCanInsert();
        }

        /// <summary>
        /// AsType, Ctor, ClearData 三处地方需要重新加载
        /// </summary>
        protected void IgnoreCanInsert()
        {
            if (_table == null || _table.Type == typeof(object)) return;
            foreach (var col in _table.Columns.Values)
                if (col.Attribute.CanInsert == false)
                    _ignore.Add(col.Attribute.Name, true);
        }
        protected void ClearData()
        {
            _batchValuesLimit = _batchParameterLimit = 0;
            _batchAutoTransaction = true;
            _batchProgress = null;
            _insertIdentity = false;
            _source.Clear();
            _ignore.Clear();
            _auditValueChangedDict.Clear();
            _params = null;
            IgnoreCanInsert();
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
        public IInsert<T1> CommandTimeout(int timeout)
        {
            _commandTimeout = timeout;
            return this;
        }

        public IInsert<T1> InsertIdentity()
        {
            _insertIdentity = true;
            return this;
        }

        public IInsert<T1> NoneParameter(bool isNotCommandParameter = true)
        {
            _noneParameter = isNotCommandParameter;
            return this;
        }

        public virtual IInsert<T1> BatchOptions(int valuesLimit, int parameterLimit, bool autoTransaction = true)
        {
            _batchValuesLimit = valuesLimit;
            _batchParameterLimit = parameterLimit;
            _batchAutoTransaction = autoTransaction;
            return this;
        }

        public IInsert<T1> BatchProgress(Action<BatchProgressStatus<T1>> callback)
        {
            _batchProgress = callback;
            return this;
        }

        public IInsert<T1> AppendData(T1 source)
        {
            if (source != null)
            {
                AuditDataValue(this, source, _orm, _table, _auditValueChangedDict);
                _source.Add(source);
            }
            return this;
        }
        public IInsert<T1> AppendData(T1[] source)
        {
            if (source != null)
            {
                AuditDataValue(this, source, _orm, _table, _auditValueChangedDict);
                _source.AddRange(source);
            }
            return this;
        }
        public IInsert<T1> AppendData(IEnumerable<T1> source)
        {
            if (source != null)
            {
                source = source.Where(a => a != null).ToList();
                AuditDataValue(this, source, _orm, _table, _auditValueChangedDict);
                _source.AddRange(source);

            }
            return this;
        }
        public static void AuditDataValue(object sender, IEnumerable<T1> data, IFreeSql orm, TableInfo table, Dictionary<string, bool> changedDict)
        {
            if (data?.Any() != true) return;
            foreach (var d in data)
                AuditDataValue(sender, d, orm, table, changedDict);
        }
        public static void AuditDataValue(object sender, T1 data, IFreeSql orm, TableInfo table, Dictionary<string, bool> changedDict)
        {
            if (data == null) return;
            if (typeof(T1) == typeof(object) && new[] { table.Type, table.TypeLazy }.Contains(data.GetType()) == false)
                throw new Exception($"操作的数据类型({data.GetType().DisplayCsharp()}) 与 AsType({table.Type.DisplayCsharp()}) 不一致，请检查。");
            foreach (var col in table.Columns.Values)
            {
                object val = col.GetValue(data);
                if (orm.Aop.AuditValueHandler != null)
                {
                    var auditArgs = new Aop.AuditValueEventArgs(Aop.AuditValueType.Insert, col, table.Properties[col.CsName], val);
                    orm.Aop.AuditValueHandler(sender, auditArgs);
                    if (auditArgs.ValueIsChanged)
                    {
                        col.SetValue(data, val = auditArgs.Value);
                        if (changedDict != null && changedDict.ContainsKey(col.Attribute.Name) == false)
                            changedDict.Add(col.Attribute.Name, true);
                    }
                }
                if (col.Attribute.IsPrimary)
                {
                    val = col.GetDbValue(data);
                    if (col.Attribute.MapType.NullableTypeOrThis() == typeof(Guid) && (val == null || (Guid)val == Guid.Empty))
                        col.SetValue(data, val = FreeUtil.NewMongodbId());
                    else if (col.CsType.NullableTypeOrThis() == typeof(Guid))
                    {
                        val = col.GetValue(data);
                        if (val == null || (Guid)val == Guid.Empty)
                            col.SetValue(data, val = FreeUtil.NewMongodbId());
                    }
                }
                if (val == null && col.Attribute.MapType == typeof(string) && col.Attribute.IsNullable == false)
                    col.SetValue(data, val = "");
                if (col.Attribute.MapType == typeof(byte[]) && (val == null || (val is byte[] bytes && bytes.Length == 0)) && col.Attribute.IsVersion)
                    col.SetValue(data, val = Utils.GuidToBytes(Guid.NewGuid()));
            }
        }

        #region 参数化数据限制，或values数量限制
        protected List<T1>[] SplitSource(int valuesLimit, int parameterLimit)
        {
            valuesLimit = valuesLimit - 1;
            parameterLimit = parameterLimit - 1;
            if (valuesLimit <= 0) valuesLimit = 1;
            if (parameterLimit <= 0) parameterLimit = 999;
            if (_source == null || _source.Any() == false) return new List<T1>[0];
            if (_source.Count == 1) return new[] { _source };

            var takeMax = valuesLimit;
            if (_noneParameter == false)
            {
                var colSum = _table.Columns.Count - _ignore.Count;
                if (colSum <= 0) colSum = 1;
                takeMax = parameterLimit / colSum;
                if (takeMax > valuesLimit) takeMax = valuesLimit;
            }
            if (_source.Count <= takeMax) return new[] { _source };

            var execCount = (int)Math.Ceiling(1.0 * _source.Count / takeMax);
            var ret = new List<T1>[execCount];
            for (var a = 0; a < execCount; a++)
                ret[a] = _source.GetRange(a * takeMax, Math.Min(takeMax, _source.Count - a * takeMax));
            return ret;
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
                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = this.RawExecuteAffrows();
                ClearData();
                return ret;
            }
            if (_transaction == null)
            {
                var threadTransaction = _orm.Ado.TransactionCurrentThread;
                if (threadTransaction != null) this.WithTransaction(threadTransaction);
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteAffrows", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                if (_transaction != null || _batchAutoTransaction == false)
                {
                    for (var a = 0; a < ss.Length; a++)
                    {
                        _source = ss[a];
                        _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                        ret += this.RawExecuteAffrows();
                    }
                }
                else
                {
                    if (_orm.Ado.MasterPool == null) throw new Exception("Ado.MasterPool 值为 null，该操作无法自启用事务，请显式传递【事务对象】解决");
                    using (var conn = _orm.Ado.MasterPool.Get())
                    {
                        _transaction = conn.Value.BeginTransaction();
                        var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                        _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                        try
                        {
                            for (var a = 0; a < ss.Length; a++)
                            {
                                _source = ss[a];
                                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                                ret += this.RawExecuteAffrows();
                            }
                            _transaction.Commit();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, "提交", null));
                        }
                        catch (Exception ex)
                        {
                            _transaction.Rollback();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, "回滚", ex));
                            throw;
                        }
                        _transaction = null;
                    }
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
                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = this.RawExecuteIdentity();
                ClearData();
                return ret;
            }
            if (_transaction == null)
            {
                var threadTransaction = _orm.Ado.TransactionCurrentThread;
                if (threadTransaction != null) this.WithTransaction(threadTransaction);
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteIdentity", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                if (_transaction != null || _batchAutoTransaction == false)
                {
                    for (var a = 0; a < ss.Length; a++)
                    {
                        _source = ss[a];
                        _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                        if (a < ss.Length - 1) this.RawExecuteAffrows();
                        else ret = this.RawExecuteIdentity();
                    }
                }
                else
                {
                    if (_orm.Ado.MasterPool == null) throw new Exception("Ado.MasterPool 值为 null，该操作无法自启用事务，请显式传递【事务对象】解决");
                    using (var conn = _orm.Ado.MasterPool.Get())
                    {
                        _transaction = conn.Value.BeginTransaction();
                        var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                        _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                        try
                        {
                            for (var a = 0; a < ss.Length; a++)
                            {
                                _source = ss[a];
                                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                                if (a < ss.Length - 1) this.RawExecuteAffrows();
                                else ret = this.RawExecuteIdentity();
                            }
                            _transaction.Commit();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, "提交", null));
                        }
                        catch (Exception ex)
                        {
                            _transaction.Rollback();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, "回滚", ex));
                            throw;
                        }
                        _transaction = null;
                    }
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
                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = this.RawExecuteInserted();
                ClearData();
                return ret;
            }
            if (_transaction == null)
            {
                var threadTransaction = _orm.Ado.TransactionCurrentThread;
                if (threadTransaction != null) this.WithTransaction(threadTransaction);
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteInserted", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                if (_transaction != null || _batchAutoTransaction == false)
                {
                    for (var a = 0; a < ss.Length; a++)
                    {
                        _source = ss[a];
                        _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                        ret.AddRange(this.RawExecuteInserted());
                    }
                }
                else
                {
                    if (_orm.Ado.MasterPool == null) throw new Exception("Ado.MasterPool 值为 null，该操作无法自启用事务，请显式传递【事务对象】解决");
                    using (var conn = _orm.Ado.MasterPool.Get())
                    {
                        _transaction = conn.Value.BeginTransaction();
                        var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                        _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                        try
                        {
                            for (var a = 0; a < ss.Length; a++)
                            {
                                _source = ss[a];
                                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                                ret.AddRange(this.RawExecuteInserted());
                            }
                            _transaction.Commit();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, "提交", null));
                        }
                        catch (Exception ex)
                        {
                            _transaction.Rollback();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, "回滚", ex));
                            throw;
                        }
                        _transaction = null;
                    }
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
        #endregion

        protected virtual int RawExecuteAffrows()
        {
            var sql = ToSql();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return affrows;
        }

        protected abstract long RawExecuteIdentity();
        protected abstract List<T1> RawExecuteInserted();

        public abstract int ExecuteAffrows();
        public abstract long ExecuteIdentity();
        public abstract List<T1> ExecuteInserted();

        public IInsert<T1> IgnoreColumns(Expression<Func<T1, object>> columns) => IgnoreColumns(_commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false, null));
        public IInsert<T1> InsertColumns(Expression<Func<T1, object>> columns) => InsertColumns(_commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false, null));

        public IInsert<T1> IgnoreColumns(string[] columns)
        {
            var cols = columns.Distinct().ToDictionary(a => a);
            _ignore.Clear();
            foreach (var col in _table.Columns.Values)
                if (cols.ContainsKey(col.Attribute.Name) == true || cols.ContainsKey(col.CsName) == true)
                    _ignore.Add(col.Attribute.Name, true);
            return this;
        }
        public IInsert<T1> InsertColumns(string[] columns)
        {
            var cols = columns.Distinct().ToDictionary(a => a);
            _ignore.Clear();
            foreach (var col in _table.Columns.Values)
                if (cols.ContainsKey(col.Attribute.Name) == false && cols.ContainsKey(col.CsName) == false && _auditValueChangedDict.ContainsKey(col.Attribute.Name) == false)
                    _ignore.Add(col.Attribute.Name, true);
            return this;
        }

        protected string TableRuleInvoke()
        {
            if (_tableRule == null) return _table.DbName;
            var newname = _tableRule(_table.DbName);
            if (newname == _table.DbName) return _table.DbName;
            if (string.IsNullOrEmpty(newname)) return _table.DbName;
            if (_orm.CodeFirst.IsSyncStructureToLower) newname = newname.ToLower();
            if (_orm.CodeFirst.IsSyncStructureToUpper) newname = newname.ToUpper();
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(_table.Type, newname);
            return newname;
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
            IgnoreCanInsert();
            return this;
        }

        public virtual string ToSql() => ToSqlValuesOrSelectUnionAllExtension102(true, null, null);

        public string ToSqlValuesOrSelectUnionAll(bool isValues = true) => ToSqlValuesOrSelectUnionAllExtension102(isValues, null, null);
        public string ToSqlValuesOrSelectUnionAllExtension101(bool isValues, Action<object, int, StringBuilder> onrow) => ToSqlValuesOrSelectUnionAllExtension102(isValues, null, onrow);
        public string ToSqlValuesOrSelectUnionAllExtension102(bool isValues, Action<object, int, StringBuilder> onrowPre, Action<object, int, StringBuilder> onrow)
        {
            if (_source == null || _source.Any() == false) return null;
            var sb = new StringBuilder();
            sb.Append("INSERT INTO ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke())).Append('(');
            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (col.Attribute.IsIdentity && _insertIdentity == false && string.IsNullOrEmpty(col.DbInsertValue)) continue;
                if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name)) continue;

                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name));
                ++colidx;
            }
            sb.Append(") ");
            if (isValues) sb.Append("VALUES");
            _params = _noneParameter ? new DbParameter[0] : new DbParameter[colidx * _source.Count];
            var specialParams = new List<DbParameter>();
            var didx = 0;
            foreach (var d in _source)
            {
                if (didx > 0) sb.Append(isValues ? ", " : " \r\nUNION ALL\r\n ");
                sb.Append(isValues ? "(" : "SELECT ");
                onrowPre?.Invoke(d, didx, sb);
                var colidx2 = 0;
                foreach (var col in _table.Columns.Values)
                {
                    if (col.Attribute.IsIdentity && _insertIdentity == false && string.IsNullOrEmpty(col.DbInsertValue)) continue;
                    if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name)) continue;

                    if (colidx2 > 0) sb.Append(", ");
                    if (string.IsNullOrEmpty(col.DbInsertValue) == false)
                        sb.Append(col.DbInsertValue);
                    else
                    {
                        object val = col.GetDbValue(d);
                        if (val == null && col.Attribute.IsNullable == false) val = col.CsType == typeof(string) ? "" : Utils.GetDataReaderValue(col.CsType.NullableTypeOrThis(), null);//#384

                        var colsql = _noneParameter ? _commonUtils.GetNoneParamaterSqlValue(specialParams, _noneParameterFlag, col, col.Attribute.MapType, val) :
                            _commonUtils.QuoteWriteParamterAdapter(col.Attribute.MapType, _commonUtils.QuoteParamterName($"{col.CsName}_{didx}"));
                        sb.Append(_commonUtils.RewriteColumn(col, colsql));
                        if (_noneParameter == false)
                            _params[didx * colidx + colidx2] = _commonUtils.AppendParamter(null, $"{col.CsName}_{didx}", col, col.Attribute.MapType, val);
                    }
                    ++colidx2;
                }
                if (isValues) sb.Append(')');
                onrow?.Invoke(d, didx, sb);
                ++didx;
            }
            if (_noneParameter && specialParams.Any()) _params = specialParams.ToArray();
            return sb.ToString();
        }

        public DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.TableName = TableRuleInvoke();
            var dtCols = new List<NativeTuple<ColumnInfo, Type, bool>>();
            foreach (var col in _table.ColumnsByPosition)
            {
                if (col.Attribute.IsIdentity && _insertIdentity == false) continue;
                if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name)) continue;
                dt.Columns.Add(col.Attribute.Name, col.Attribute.MapType.NullableTypeOrThis());
                dtCols.Add(NativeTuple.Create(col, col.Attribute.MapType.NullableTypeOrThis(), col.Attribute.MapType.IsNullableType()));
            }
            if (dt.Columns.Count == 0) return dt;
            var didx = 0;
            foreach (var d in _source)
            {
                var row = new object[dt.Columns.Count];
                var rowIndex = 0;
                foreach (var col in dtCols)
                {
                    var val = col.Item1.GetDbValue(d);
                    if (col.Item3 == true)
                    {
                        //if (val == null) throw new Exception($"[{didx}].{col.Item1.CsName} 值不可为 null；DataTable 限制不可使用 int?/long? 可空类型，IInsert.ToDataTable 将映射成 int/long，因此不可接受 null 值");
                        if (val == null)
                            val = DBNull.Value;
                        else
                            val = Utils.GetDataReaderValue(col.Item2, val);
                    }
                    row[rowIndex++] = val;
                }
                dt.Rows.Add(row);
                didx++;
            }
            return dt;
        }
    }
}
