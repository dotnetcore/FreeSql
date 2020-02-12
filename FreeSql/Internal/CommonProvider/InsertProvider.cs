using FreeSql.Internal.Model;
using FreeSql.Extensions.EntityUtil;
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
        protected Dictionary<string, bool> _auditValueChangedDict = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        protected TableInfo _table;
        protected Func<string, string> _tableRule;
        protected bool _noneParameter, _insertIdentity;
        protected int _batchValuesLimit, _batchParameterLimit;
        protected bool _batchAutoTransaction = true;
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

        public IInsert<T1> InsertIdentity()
        {
            _insertIdentity = true;
            return this;
        }

        public IInsert<T1> NoneParameter()
        {
            _noneParameter = true;
            return this;
        }

        public virtual IInsert<T1> BatchOptions(int valuesLimit, int parameterLimit, bool autoTransaction = true)
        {
            _batchValuesLimit = valuesLimit;
            _batchParameterLimit = parameterLimit;
            _batchAutoTransaction = autoTransaction;
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
            foreach (var col in table.Columns.Values)
            {
                object val = col.GetMapValue(data);
                if (col.Attribute.IsPrimary)
                {
                    if (col.Attribute.MapType.NullableTypeOrThis() == typeof(Guid) && (val == null || (Guid)val == Guid.Empty))
                        col.SetMapValue(data, val = FreeUtil.NewMongodbId());
                    else if (col.CsType.NullableTypeOrThis() == typeof(Guid))
                    {
                        var guidVal = orm.GetEntityValueWithPropertyName(table.Type, data, col.CsName);
                        if (guidVal == null || (Guid)guidVal == Guid.Empty)
                        {
                            orm.SetEntityValueWithPropertyName(table.Type, data, col.CsName, FreeUtil.NewMongodbId());
                            val = col.GetMapValue(data);
                        }
                    }
                }
                if (orm.Aop.AuditValue != null)
                {
                    var auditArgs = new Aop.AuditValueEventArgs(Aop.AuditValueType.Insert, col, table.Properties[col.CsName], val);
                    orm.Aop.AuditValue(sender, auditArgs);
                    if (auditArgs.IsChanged)
                    {
                        col.SetMapValue(data, val = auditArgs.Value);
                        if (changedDict != null && changedDict.ContainsKey(col.Attribute.Name) == false)
                            changedDict.Add(col.Attribute.Name, true);
                    }
                }
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
                ret = this.RawExecuteAffrows();
                ClearData();
                return ret;
            }
            if (_transaction == null)
                this.WithTransaction(_orm.Ado.TransactionCurrentThread);

            if (_transaction != null || _batchAutoTransaction == false)
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

            if (_transaction != null || _batchAutoTransaction == false)
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

            if (_transaction != null || _batchAutoTransaction == false)
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
        #endregion

        protected virtual int RawExecuteAffrows()
        {
            var sql = ToSql();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
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

        protected abstract long RawExecuteIdentity();
        protected abstract List<T1> RawExecuteInserted();

        public abstract int ExecuteAffrows();
        public abstract long ExecuteIdentity();
        public abstract List<T1> ExecuteInserted();

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
                if (cols.ContainsKey(col.Attribute.Name) == false && _auditValueChangedDict.ContainsKey(col.Attribute.Name) == false)
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

        public virtual string ToSql() => ToSqlValuesOrSelectUnionAll(true);

        public string ToSqlValuesOrSelectUnionAll(bool isValues = true)
        {
            if (_source == null || _source.Any() == false) return null;
            var sb = new StringBuilder();
            sb.Append("INSERT INTO ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke())).Append("(");
            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (col.Attribute.IsIdentity && _insertIdentity == false) continue;
                if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name)) continue;

                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name));
                ++colidx;
            }
            sb.Append(") ");
            if (isValues) sb.Append(isValues ? "VALUES" : "SELECT ");
            _params = _noneParameter ? new DbParameter[0] : new DbParameter[colidx * _source.Count];
            var specialParams = new List<DbParameter>();
            var didx = 0;
            foreach (var d in _source)
            {
                if (didx > 0) sb.Append(isValues ? ", " : " \r\nUNION ALL\r\n ");
                sb.Append(isValues ? "(" : "SELECT ");
                var colidx2 = 0;
                foreach (var col in _table.Columns.Values)
                {
                    if (col.Attribute.IsIdentity && _insertIdentity == false) continue;
                    if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name)) continue;

                    if (colidx2 > 0) sb.Append(", ");
                    if (string.IsNullOrEmpty(col.DbInsertValue) == false)
                        sb.Append(col.DbInsertValue);
                    else
                    {
                        object val = col.GetMapValue(d);
                        if (_noneParameter)
                            sb.Append(_commonUtils.GetNoneParamaterSqlValue(specialParams, col.Attribute.MapType, val));
                        else
                        {
                            sb.Append(_commonUtils.QuoteWriteParamter(col.Attribute.MapType, _commonUtils.QuoteParamterName($"{col.CsName}_{didx}")));
                            _params[didx * colidx + colidx2] = _commonUtils.AppendParamter(null, $"{col.CsName}_{didx}", col, col.Attribute.MapType, val);
                        }
                    }
                    ++colidx2;
                }
                if (isValues) sb.Append(")");
                ++didx;
            }
            if (_noneParameter && specialParams.Any())
                _params = specialParams.ToArray();
            return sb.ToString();
        }

        public DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.TableName = TableRuleInvoke();
            foreach (var col in _table.ColumnsByPosition)
            {
                if (col.Attribute.IsIdentity && _insertIdentity == false) continue;
                if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name)) continue;
                dt.Columns.Add(col.Attribute.Name, col.Attribute.MapType);
            }
            if (dt.Columns.Count == 0) return dt;
            foreach (var d in _source)
            {
                var row = new object[dt.Columns.Count];
                var rowIndex = 0;
                foreach (var col in _table.ColumnsByPosition)
                {
                    if (col.Attribute.IsIdentity && _insertIdentity == false) continue;
                    if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name)) continue;
                    row[rowIndex++] = col.GetMapValue(d);
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}

