using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    public abstract partial class InsertOrUpdateProvider
    {
        public IFreeSql _orm;
        public CommonUtils _commonUtils;
        public CommonExpression _commonExpression;
        public bool _doNothing = false;
        public Dictionary<string, bool> _updateIgnore = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, bool> _auditValueChangedDict = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        public TableInfo _table;
        public ColumnInfo[] _tempPrimarys;
        public Func<string, string> _tableRule;
        public DbParameter[] _params;
        public DbTransaction _transaction;
        public DbConnection _connection;
        public int _commandTimeout = 0;
        public ColumnInfo IdentityColumn { get; protected set; }
    }

    public abstract partial class InsertOrUpdateProvider<T1> : InsertOrUpdateProvider, IInsertOrUpdate<T1> where T1 : class
    {
        public List<T1> _source = new List<T1>();
        public string _sourceSql = null;

        public InsertOrUpdateProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
            _table = _commonUtils.GetTableByEntity(typeof(T1));
            _tempPrimarys = _table?.Primarys ?? new ColumnInfo[0];
            if (_table == null && typeof(T1) != typeof(Dictionary<string, object>))
                throw new Exception(CoreStrings.InsertOrUpdate_NotSuport_Generic_UseEntity(typeof(T1)));
            if (_orm.CodeFirst.IsAutoSyncStructure && typeof(T1) != typeof(object)) _orm.CodeFirst.SyncStructure<T1>();
            IdentityColumn = _table?.Primarys.Where(a => a.Attribute.IsIdentity).FirstOrDefault();
        }

        protected void ClearData()
        {
            _source.Clear();
            _sourceSql = null;
            _auditValueChangedDict.Clear();
        }

        public IInsertOrUpdate<T1> WithTransaction(DbTransaction transaction)
        {
            _transaction = transaction;
            if (transaction != null) _connection = transaction.Connection;
            return this;
        }
        public IInsertOrUpdate<T1> WithConnection(DbConnection connection)
        {
            if (_transaction?.Connection != connection) _transaction = null;
            _connection = connection;
            return this;
        }
        public IInsertOrUpdate<T1> CommandTimeout(int timeout)
        {
            _commandTimeout = timeout;
            return this;
        }

        public IInsertOrUpdate<T1> UpdateColumns(Expression<Func<T1, object>> columns) => UpdateColumns(_commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, null, columns?.Body, false, null));
        public IInsertOrUpdate<T1> UpdateColumns(string[] columns)
        {
            var cols = columns.Distinct().ToDictionary(a => a);
            _updateIgnore.Clear();
            foreach (var col in _table.Columns.Values)
                if (cols.ContainsKey(col.Attribute.Name) == false && cols.ContainsKey(col.CsName) == false)
                    _updateIgnore.Add(col.Attribute.Name, true);
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
            if (data == null || table == null) return;
            if (typeof(T1) == typeof(object) && new[] { table.Type, table.TypeLazy }.Contains(data.GetType()) == false)
                throw new Exception(CoreStrings.DataType_AsType_Inconsistent(data.GetType().DisplayCsharp(), table.Type.DisplayCsharp()));
            if (orm.Aop.AuditValueHandler == null) return;
            foreach (var col in table.Columns.Values)
            {
                object val = col.GetValue(data);
                var auditArgs = new Aop.AuditValueEventArgs(Aop.AuditValueType.InsertOrUpdate, col, table.Properties.TryGetValue(col.CsName, out var tryprop) ? tryprop : null, val, data);
                orm.Aop.AuditValueHandler(sender, auditArgs);
                if (auditArgs.ValueIsChanged)
                {
                    col.SetValue(data, val = auditArgs.Value);
                    if (changedDict != null && changedDict.ContainsKey(col.Attribute.Name) == false)
                        changedDict.Add(col.Attribute.Name, true);
                }
                if (auditArgs.ObjectAuditBreak) break;

                if (val == null && col.Attribute.MapType == typeof(string) && col.Attribute.IsNullable == false)
                    col.SetValue(data, val = "");
            }
        }

        public IInsertOrUpdate<T1> SetSource(T1 source) => this.SetSource(new[] { source });
        public IInsertOrUpdate<T1> SetSource(T1 source, Expression<Func<T1, object>> tempPrimarys) => this.SetSource(new[] { source }, tempPrimarys);
        public IInsertOrUpdate<T1> SetSource(IEnumerable<T1> source, Expression<Func<T1, object>> tempPrimarys = null)
        {
            if (source == null || source.Any() == false) return this;
            _sourceSql = null;
            UpdateProvider<T1>.GetDictionaryTableInfo(source, _orm, ref _table);
            AuditDataValue(this, source, _orm, _table, _auditValueChangedDict);
            _source.AddRange(source.Where(a => a != null));

            if (tempPrimarys != null)
            {
                var cols = _commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, null, tempPrimarys?.Body, false, null).Distinct().ToDictionary(a => a);
                _tempPrimarys = cols.Keys.Select(a => _table.Columns.TryGetValue(a, out var col) ? col : null).ToArray().Where(a => a != null).ToArray();
            }
            return this;
        }
        public virtual IInsertOrUpdate<T1> SetSource(string sql, Expression<Func<T1, object>> tempPrimarys = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) return this;
            _source.Clear();
            _sourceSql = sql;

            if (tempPrimarys != null)
            {
                var cols = _commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, null, tempPrimarys?.Body, false, null).Distinct().ToDictionary(a => a);
                _tempPrimarys = cols.Keys.Select(a => _table.Columns.TryGetValue(a, out var col) ? col : null).ToArray().Where(a => a != null).ToArray();
            }
            return this;
        }

        public IInsertOrUpdate<T1> IfExistsDoNothing()
        {
            _doNothing = true;
            return this;
        }

        protected string TableRuleInvoke()
        {
            var tbname = _table?.DbName ?? "";
            if (_tableRule == null && _table.AsTableImpl == null) return tbname;
            string newname = null;
            if (_table.AsTableImpl != null)
            {
                if (_source.Any())
                    newname = _table.AsTableImpl.GetTableNameByColumnValue(_table.AsTableColumn.GetValue(_source.FirstOrDefault()));
                else if (_tableRule == null)
                    newname = _table.AsTableImpl.GetTableNameByColumnValue(DateTime.Now);
                else
                    newname = _tableRule(_table.DbName);
            }
            else
                newname = _tableRule(_table.DbName);
            if (newname == tbname) return tbname;
            if (string.IsNullOrEmpty(newname)) return tbname;
            if (_orm.CodeFirst.IsSyncStructureToLower) newname = newname.ToLower();
            if (_orm.CodeFirst.IsSyncStructureToUpper) newname = newname.ToUpper();
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(_table?.Type, newname);
            return newname;
        }
        public IInsertOrUpdate<T1> AsTable(Func<string, string> tableRule)
        {
            _tableRule = tableRule;
            return this;
        }
        public IInsertOrUpdate<T1> AsTable(string tableName)
        {
            _tableRule = (oldname) => tableName;
            return this;
        }
        public IInsertOrUpdate<T1> AsType(Type entityType)
        {
            if (entityType == typeof(object)) throw new Exception(CoreStrings.TypeAsType_NotSupport_Object("IInsertOrUpdate"));
            if (entityType == _table.Type) return this;
            var newtb = _commonUtils.GetTableByEntity(entityType);
            _table = newtb ?? throw new Exception(CoreStrings.Type_AsType_Parameter_Error("IInsertOrUpdate"));
            _tempPrimarys = _table.Primarys;
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(entityType);
            IdentityColumn = _table.Primarys.Where(a => a.Attribute.IsIdentity).FirstOrDefault();
            return this;
        }

        public void WriteSourceSelectUnionAll(List<T1> source, StringBuilder sb, List<DbParameter> dbParams)
        {
            if (_sourceSql != null)
            {
                sb.Append(_sourceSql).Append("\r\n");
                return;
            }

            var didx = 0;
            foreach (var d in source)
            {
                if (didx > 0) sb.Append(" \r\nUNION ALL\r\n ");
                sb.Append("SELECT ");
                switch (_orm.Ado.DataType)
                {
                    case DataType.Firebird:
                        sb.Append("FIRST 1 ");
                        break;
                }
                var colidx2 = 0;
                foreach (var col in _table.Columns.Values)
                {
                    if (colidx2 > 0) sb.Append(", ");
                    if (string.IsNullOrEmpty(col.DbInsertValue) == false)
                        sb.Append(col.DbInsertValue);
                    else
                    {
                        object val = col.GetDbValue(d);
                        sb.Append(_commonUtils.RewriteColumn(col, _commonUtils.GetNoneParamaterSqlValue(dbParams, "cu", col, col.Attribute.MapType, val)));
                    }
                    if (didx == 0) sb.Append(" as ").Append(_commonUtils.QuoteSqlName(col.Attribute.Name));
                    ++colidx2;
                }
                switch (_orm.Ado.DataType)
                {
                    case DataType.OdbcOracle:
                    case DataType.Oracle:
                    case DataType.OdbcDameng:
                    case DataType.Dameng:
                    case DataType.GBase:
                        sb.Append(" FROM dual");
                        break;
                    case DataType.Firebird:
                        sb.Append(" FROM rdb$database");
                        break;
                }
                ++didx;
            }
        }

        byte _SplitSourceByIdentityValueIsNullFlag = 0 ;//防止重复计算 SplitSource
        /// <summary>
        /// 如果实体类有自增属性，分成两个 List，有值的Item1 merge，无值的Item2 insert
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public NativeTuple<List<T1>[], List<T1>[]> SplitSourceByIdentityValueIsNull(List<T1> source)
        {
            if (source.Any() == false) return NativeTuple.Create(new List<T1>[0], new List<T1>[0]);
            if (_SplitSourceByIdentityValueIsNullFlag == 1) return NativeTuple.Create(new[] { source }, new List<T1>[0]);
            if (_SplitSourceByIdentityValueIsNullFlag == 2) return NativeTuple.Create(new List<T1>[0], new[] { source });
            if (IdentityColumn == null || _tempPrimarys != _table.Primarys) return NativeTuple.Create(LocalSplitSourceByAsTable(source), new List<T1>[0]);
            var item1 = new List<T1>();
            var item2 = new List<T1>();
            foreach (var item in source)
            {
                if (object.Equals(_orm.GetEntityValueWithPropertyName(_table.Type, item, IdentityColumn.CsName), IdentityColumn.CsType.CreateInstanceGetDefaultValue()))
                    item2.Add(item); //自增无值的，记录为直接插入
                else
                    item1.Add(item);
            }
            return NativeTuple.Create(LocalSplitSourceByAsTable(item1), LocalSplitSourceByAsTable(item2));

            List<T1>[] LocalSplitSourceByAsTable(List<T1> loc1)
            {
                if (loc1.Any() == false) return new List<T1>[0];
                if (_table.AsTableImpl != null)
                {
                    var atarr = loc1.Select(a => new
                    {
                        item = a,
                        splitKey = _table.AsTableImpl.GetTableNameByColumnValue(_table.AsTableColumn.GetValue(a), true)
                    }).GroupBy(a => a.splitKey, a => a.item).Select(a => a.ToList()).ToArray();
                    return atarr;
                }
                return new[] { loc1 };
            }
        }

        public abstract string ToSql();
        public int ExecuteAffrows()
        {
            if (_sourceSql != null) return this.RawExecuteAffrows();
            var affrows = 0;
            var ss = SplitSourceByIdentityValueIsNull(_source);
            try
            {
                if (_transaction == null)
                {
                    var threadTransaction = _orm.Ado.TransactionCurrentThread;
                    if (threadTransaction != null) this.WithTransaction(threadTransaction);
                }

                if (_transaction != null || _orm.Ado.MasterPool == null)
                {
                    _SplitSourceByIdentityValueIsNullFlag = 1;
                    foreach (var tmpsource in ss.Item1)
                    {
                        _source = tmpsource;
                        affrows += this.RawExecuteAffrows();
                    }
                    _SplitSourceByIdentityValueIsNullFlag = 2;
                    foreach (var tmpsource in ss.Item2)
                    {
                        _source = tmpsource;
                        affrows += this.RawExecuteAffrows();
                    }
                }
                else
                {
                    using (var conn = _orm.Ado.MasterPool.Get())
                    {
                        _transaction = conn.Value.BeginTransaction();
                        var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                        _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                        try
                        {
                            _SplitSourceByIdentityValueIsNullFlag = 1;
                            foreach (var tmpsource in ss.Item1)
                            {
                                _source = tmpsource;
                                affrows += this.RawExecuteAffrows();
                            }
                            _SplitSourceByIdentityValueIsNullFlag = 2;
                            foreach (var tmpsource in ss.Item2)
                            {
                                _source = tmpsource;
                                affrows += this.RawExecuteAffrows();
                            }
                            _transaction.Commit();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreStrings.Commit, null));
                        }
                        catch (Exception ex)
                        {
                            _transaction.Rollback();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreStrings.RollBack, ex));
                            throw;
                        }
                        _transaction = null;
                    }
                }
            }
            finally
            {
                _SplitSourceByIdentityValueIsNullFlag = 0;
                ClearData();
            }
            return affrows;
        }
        public int RawExecuteAffrows()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.InsertOrUpdate, sql, _params);
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
#if net40
#else
        async public Task<int> RawExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.InsertOrUpdate, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
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
        async public Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            if (_sourceSql != null) return this.RawExecuteAffrows();
            var affrows = 0;
            var ss = SplitSourceByIdentityValueIsNull(_source);
            try
            {
                if (_transaction == null)
                {
                    var threadTransaction = _orm.Ado.TransactionCurrentThread;
                    if (threadTransaction != null) this.WithTransaction(threadTransaction);
                }

                if (_transaction != null || _orm.Ado.MasterPool == null)
                {
                    _SplitSourceByIdentityValueIsNullFlag = 1;
                    foreach (var tmpsource in ss.Item1)
                    {
                        _source = tmpsource;
                        affrows += await this.RawExecuteAffrowsAsync(cancellationToken);
                    }
                    _SplitSourceByIdentityValueIsNullFlag = 2;
                    foreach (var tmpsource in ss.Item2)
                    {
                        _source = tmpsource;
                        affrows += await this.RawExecuteAffrowsAsync(cancellationToken);
                    }
                }
                else
                {
                    using (var conn = await _orm.Ado.MasterPool.GetAsync())
                    {
                        _transaction = conn.Value.BeginTransaction();
                        var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                        _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                        try
                        {
                            _SplitSourceByIdentityValueIsNullFlag = 1;
                            foreach (var tmpsource in ss.Item1)
                            {
                                _source = tmpsource;
                                affrows += await this.RawExecuteAffrowsAsync(cancellationToken);
                            }
                            _SplitSourceByIdentityValueIsNullFlag = 2;
                            foreach (var tmpsource in ss.Item2)
                            {
                                _source = tmpsource;
                                affrows += await this.RawExecuteAffrowsAsync(cancellationToken);
                            }
                            _transaction.Commit();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreStrings.Commit, null));
                        }
                        catch (Exception ex)
                        {
                            _transaction.Rollback();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreStrings.RollBack, ex));
                            throw;
                        }
                        _transaction = null;
                    }
                }
            }
            finally
            {
                _SplitSourceByIdentityValueIsNullFlag = 0;
                ClearData();
            }
            return affrows;
        }
#endif
    }
}