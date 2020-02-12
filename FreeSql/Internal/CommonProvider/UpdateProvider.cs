using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    public abstract partial class UpdateProvider<T1> : IUpdate<T1> where T1 : class
    {
        protected IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        protected List<T1> _source = new List<T1>();
        protected Dictionary<string, bool> _ignore = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        protected Dictionary<string, bool> _auditValueChangedDict = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        protected TableInfo _table;
        protected Func<string, string> _tableRule;
        protected StringBuilder _where = new StringBuilder();
        protected List<GlobalFilter.Item> _whereGlobalFilter;
        protected StringBuilder _set = new StringBuilder();
        protected StringBuilder _setIncr = new StringBuilder();
        protected List<DbParameter> _params = new List<DbParameter>();
        protected List<DbParameter> _paramsSource = new List<DbParameter>();
        protected bool _noneParameter;
        protected int _batchRowsLimit, _batchParameterLimit;
        protected bool _batchAutoTransaction = true;
        protected DbTransaction _transaction;
        protected DbConnection _connection;

        public UpdateProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
            _table = _commonUtils.GetTableByEntity(typeof(T1));
            _noneParameter = _orm.CodeFirst.IsNoneCommandParameter;
            this.Where(_commonUtils.WhereObject(_table, "", dywhere));
            if (_orm.CodeFirst.IsAutoSyncStructure && typeof(T1) != typeof(object)) _orm.CodeFirst.SyncStructure<T1>();
            IgnoreCanUpdate();
            _whereGlobalFilter = _orm.GlobalFilter.GetFilters();
        }

        /// <summary>
        /// AsType, Ctor, ClearData 三处地方需要重新加载
        /// </summary>
        protected void IgnoreCanUpdate()
        {
            if (_table == null || _table.Type == typeof(object)) return;
            foreach (var col in _table.Columns.Values)
                if (col.Attribute.CanUpdate == false)
                    _ignore.Add(col.Attribute.Name, true);
        }
        protected void ClearData()
        {
            _batchRowsLimit = _batchParameterLimit = 0;
            _batchAutoTransaction = true;
            _source.Clear();
            _ignore.Clear();
            _auditValueChangedDict.Clear();
            _where.Clear();
            _set.Clear();
            _setIncr.Clear();
            _params.Clear();
            _paramsSource.Clear();
            IgnoreCanUpdate();
            _whereGlobalFilter = _orm.GlobalFilter.GetFilters();
        }

        public IUpdate<T1> WithTransaction(DbTransaction transaction)
        {
            _transaction = transaction;
            _connection = _transaction?.Connection;
            return this;
        }
        public IUpdate<T1> WithConnection(DbConnection connection)
        {
            if (_transaction?.Connection != connection) _transaction = null;
            _connection = connection;
            return this;
        }

        public IUpdate<T1> NoneParameter()
        {
            _noneParameter = true;
            return this;
        }

        public virtual IUpdate<T1> BatchOptions(int rowsLimit, int parameterLimit, bool autoTransaction = true)
        {
            _batchRowsLimit = rowsLimit;
            _batchParameterLimit = parameterLimit;
            _batchAutoTransaction = autoTransaction;
            return this;
        }

        protected void ValidateVersionAndThrow(int affrows)
        {
            if (_table.VersionColumn != null && _source.Count > 0)
            {
                if (affrows != _source.Count)
                    throw new Exception($"记录可能不存在，或者【行级乐观锁】版本过旧，更新数量{_source.Count}，影响的行数{affrows}。");
                foreach (var d in _source)
                    _orm.SetEntityIncrByWithPropertyName(_table.Type, d, _table.VersionColumn.CsName, 1);
            }
        }

        #region 参数化数据限制，或values数量限制
        internal List<T1>[] SplitSource(int valuesLimit, int parameterLimit)
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
            if (ss.Length <= 1)
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

        protected List<T1> SplitExecuteUpdated(int valuesLimit, int parameterLimit)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = new List<T1>();
            if (ss.Length <= 1)
            {
                ret = this.RawExecuteUpdated();
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
                    ret.AddRange(this.RawExecuteUpdated());
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
                            ret.AddRange(this.RawExecuteUpdated());
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
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;
            var dbParms = _params.Concat(_paramsSource).ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, dbParms);
                ValidateVersionAndThrow(affrows);
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

        protected abstract List<T1> RawExecuteUpdated();

        public abstract int ExecuteAffrows();
        public abstract List<T1> ExecuteUpdated();

        public IUpdate<T1> IgnoreColumns(Expression<Func<T1, object>> columns) => IgnoreColumns(_commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false, null));
        public IUpdate<T1> UpdateColumns(Expression<Func<T1, object>> columns) => UpdateColumns(_commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false, null));

        public IUpdate<T1> IgnoreColumns(string[] columns)
        {
            var cols = columns.ToDictionary(a => a);
            _ignore.Clear();
            foreach (var col in _table.Columns.Values)
                if (cols.ContainsKey(col.Attribute.Name) == true || cols.ContainsKey(col.CsName) == true)
                    _ignore.Add(col.Attribute.Name, true);
            return this;
        }
        public IUpdate<T1> UpdateColumns(string[] columns)
        {
            var cols = columns.ToDictionary(a => a);
            _ignore.Clear();
            foreach (var col in _table.Columns.Values)
                if (cols.ContainsKey(col.Attribute.Name) == false && cols.ContainsKey(col.CsName) == false && _auditValueChangedDict.ContainsKey(col.Attribute.Name) == false)
                    _ignore.Add(col.Attribute.Name, true);
            return this;
        }

        public static void AuditDataValue(object sender, IEnumerable<T1> data, IFreeSql orm, TableInfo table, Dictionary<string, bool> changedDict)
        {
            if (data?.Any() != true) return;
            if (orm.Aop.AuditValue == null) return;
            foreach (var d in data)
            {
                if (d == null) continue;
                foreach (var col in table.Columns.Values)
                {
                    object val = col.GetMapValue(d);
                    var auditArgs = new Aop.AuditValueEventArgs(Aop.AuditValueType.Update, col, table.Properties[col.CsName], val);
                    orm.Aop.AuditValue(sender, auditArgs);
                    if (auditArgs.IsChanged)
                    {
                        col.SetMapValue(d, val = auditArgs.Value);
                        if (changedDict != null && changedDict.ContainsKey(col.Attribute.Name) == false)
                            changedDict.Add(col.Attribute.Name, true);
                    }
                }
            }
        }
        public static void AuditDataValue(object sender, T1 data, IFreeSql orm, TableInfo table, Dictionary<string, bool> changedDict)
        {
            if (orm.Aop.AuditValue == null) return;
            if (data == null) return;
            foreach (var col in table.Columns.Values)
            {
                object val = col.GetMapValue(data);
                var auditArgs = new Aop.AuditValueEventArgs(Aop.AuditValueType.Update, col, table.Properties[col.CsName], val);
                orm.Aop.AuditValue(sender, auditArgs);
                if (auditArgs.IsChanged)
                {
                    col.SetMapValue(data, val = auditArgs.Value);
                    if (changedDict != null && changedDict.ContainsKey(col.Attribute.Name) == false)
                        changedDict.Add(col.Attribute.Name, true);
                }
            }
        }

        public IUpdate<T1> SetSource(T1 source) => this.SetSource(new[] { source });
        public IUpdate<T1> SetSource(IEnumerable<T1> source)
        {
            if (source == null || source.Any() == false) return this;
            AuditDataValue(this, source, _orm, _table, _auditValueChangedDict);
            _source.AddRange(source.Where(a => a != null));
            return this;
        }

        public IUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> column, TMember value)
        {
            var cols = new List<SelectColumnInfo>();
            _commonExpression.ExpressionSelectColumn_MemberAccess(null, cols, SelectTableInfoType.From, column?.Body, true, null);
            if (cols.Count != 1) return this;
            var col = cols.First();
            object paramVal = null;
            if (col.Column.Attribute.MapType == typeof(TMember)) paramVal = value;
            else paramVal = Utils.GetDataReaderValue(col.Column.Attribute.MapType, value);
            _set.Append(", ").Append(_commonUtils.QuoteSqlName(col.Column.Attribute.Name)).Append(" = ");
            if (_noneParameter)
            {
                _set.Append(_commonUtils.GetNoneParamaterSqlValue(_params, col.Column.Attribute.MapType, paramVal));
            }
            else
            {
                _set.Append(_commonUtils.QuoteWriteParamter(col.Column.Attribute.MapType, $"{_commonUtils.QuoteParamterName("p_")}{_params.Count}"));
                _commonUtils.AppendParamter(_params, null, col.Column, col.Column.Attribute.MapType, paramVal);
            }
            //foreach (var t in _source) Utils.FillPropertyValue(t, tryf.CsName, value);
            return this;
        }
        public IUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> exp)
        {
            var body = exp?.Body;
            var nodeType = body?.NodeType;
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    var equalBinaryExp = body as BinaryExpression;
                    _set.Append(", ").Append(_commonExpression.ExpressionWhereLambdaNoneForeignObject(null, _table, null, equalBinaryExp.Left, null, null))
                        .Append(" = ").Append(_commonExpression.ExpressionWhereLambdaNoneForeignObject(null, _table, null, equalBinaryExp.Right, null, null));
                    return this;
                case ExpressionType.MemberInit:
                    var initExp = body as MemberInitExpression;
                    if (initExp.Bindings?.Count > 0)
                    {
                        for (var a = 0; a < initExp.Bindings.Count; a++)
                        {
                            var initAssignExp = (initExp.Bindings[a] as MemberAssignment);
                            if (initAssignExp == null) continue;
                            var memberName = initExp.Bindings[a].Member.Name;
                            if (_table.ColumnsByCsIgnore.ContainsKey(memberName)) continue;
                            if (_table.ColumnsByCs.TryGetValue(memberName, out var col) == false) throw new Exception($"找不到属性：{memberName}");
                            var memberValue = _commonExpression.ExpressionLambdaToSql(initAssignExp.Expression, new CommonExpression.ExpTSC { isQuoteName = true });
                            _setIncr.Append(", ").Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ").Append(memberValue);
                        }
                    }
                    return this;
                case ExpressionType.New:
                    var newExp = body as NewExpression;
                    if (newExp.Members?.Count > 0)
                    {
                        for (var a = 0; a < newExp.Members.Count; a++)
                        {
                            var memberName = newExp.Members[a].Name;
                            if (_table.ColumnsByCsIgnore.ContainsKey(memberName)) continue;
                            if (_table.ColumnsByCs.TryGetValue(memberName, out var col) == false) throw new Exception($"找不到属性：{memberName}");
                            var memberValue = _commonExpression.ExpressionLambdaToSql(newExp.Arguments[a], new CommonExpression.ExpTSC { isQuoteName = true });
                            _setIncr.Append(", ").Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ").Append(memberValue);
                        }
                    }
                    return this;
            }
            if (body is BinaryExpression == false &&
                nodeType != ExpressionType.Call) return this;
            var cols = new List<SelectColumnInfo>();
            var expt = _commonExpression.ExpressionWhereLambdaNoneForeignObject(null, _table, cols, exp, null, null);
            if (cols.Any() == false) return this;
            foreach (var col in cols)
            {
                if (col.Column.Attribute.IsNullable == true && col.Column.Attribute.MapType.IsNullableType())
                {
                    var replval = _orm.CodeFirst.GetDbInfo(col.Column.Attribute.MapType.GetGenericArguments().FirstOrDefault())?.defaultValue;
                    if (replval == null) continue;
                    var replname = _commonUtils.QuoteSqlName(col.Column.Attribute.Name);
                    expt = expt.Replace(replname, _commonUtils.IsNull(replname, _commonUtils.FormatSql("{0}", replval)));
                }
            }
            _setIncr.Append(", ").Append(_commonUtils.QuoteSqlName(cols.First().Column.Attribute.Name)).Append(" = ").Append(expt);
            return this;
        }
        public IUpdate<T1> SetRaw(string sql, object parms = null)
        {
            if (string.IsNullOrEmpty(sql)) return this;
            _set.Append(", ").Append(sql);
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this;
        }

        public IUpdate<T1> Where(Expression<Func<T1, bool>> expression) => this.Where(_commonExpression.ExpressionWhereLambdaNoneForeignObject(null, _table, null, expression?.Body, null, _params));
        public IUpdate<T1> Where(string sql, object parms = null)
        {
            if (string.IsNullOrEmpty(sql)) return this;
            _where.Append(" AND (").Append(sql).Append(")");
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this;
        }
        public IUpdate<T1> Where(T1 item) => this.Where(new[] { item });
        public IUpdate<T1> Where(IEnumerable<T1> items) => this.Where(_commonUtils.WhereItems(_table, "", items));
        public IUpdate<T1> WhereDynamic(object dywhere, bool not = false) => not == false ?
            this.Where(_commonUtils.WhereObject(_table, "", dywhere)) :
            this.Where($"not({_commonUtils.WhereObject(_table, "", dywhere)})");

        public IUpdate<T1> DisableGlobalFilter(params string[] name)
        {
            if (_whereGlobalFilter.Any() == false) return this;
            if (name?.Any() != true)
            {
                _whereGlobalFilter.Clear();
                return this;
            }
            foreach (var n in name)
            {
                if (n == null) continue;
                var idx = _whereGlobalFilter.FindIndex(a => string.Compare(a.Name, n, true) == 0);
                if (idx == -1) continue;
                _whereGlobalFilter.RemoveAt(idx);
            }
            return this;
        }

        protected string WhereCaseSource(string CsName, Func<string, string> thenValue)
        {
            if (_source.Any() == false) return null;
            if (_table.ColumnsByCs.ContainsKey(CsName) == false) throw new Exception($"找不到 {CsName} 对应的列");
            if (thenValue == null) throw new ArgumentNullException("thenValue 参数不可为 null");

            if (_source.Count == 0) return null;
            if (_source.Count == 1)
            {

                var col = _table.ColumnsByCs[CsName];
                var sb = new StringBuilder();

                sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ");
                sb.Append(thenValue(_commonUtils.GetNoneParamaterSqlValue(_paramsSource, col.Attribute.MapType, col.GetMapValue(_source.First()))));

                return sb.ToString();

            }
            else
            {
                var caseWhen = new StringBuilder();
                caseWhen.Append("CASE ");
                ToSqlCase(caseWhen, _table.Primarys);
                var cw = caseWhen.ToString();

                var col = _table.ColumnsByCs[CsName];
                var sb = new StringBuilder();
                sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ");

                var nulls = 0;
                var cwsb = new StringBuilder().Append(cw);
                foreach (var d in _source)
                {
                    cwsb.Append(" \r\nWHEN ");
                    ToSqlWhen(cwsb, _table.Primarys, d);
                    cwsb.Append(" THEN ");
                    var val = col.GetMapValue(d);
                    cwsb.Append(thenValue(_commonUtils.GetNoneParamaterSqlValue(_paramsSource, col.Attribute.MapType, val)));
                    if (val == null || val == DBNull.Value) nulls++;
                }
                cwsb.Append(" END");
                if (nulls == _source.Count) sb.Append("NULL");
                else sb.Append(cwsb.ToString());
                cwsb.Clear();

                return sb.ToString();
            }
        }

        protected abstract void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys);
        protected abstract void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d);
        protected virtual void ToSqlCaseWhenEnd(StringBuilder sb, ColumnInfo col) { }

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
        public IUpdate<T1> AsTable(Func<string, string> tableRule)
        {
            _tableRule = tableRule;
            return this;
        }
        public IUpdate<T1> AsType(Type entityType)
        {
            if (entityType == typeof(object)) throw new Exception("IUpdate.AsType 参数不支持指定为 object");
            if (entityType == _table.Type) return this;
            var newtb = _commonUtils.GetTableByEntity(entityType);
            _table = newtb ?? throw new Exception("IUpdate.AsType 参数错误，请传入正确的实体类型");
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(entityType);
            IgnoreCanUpdate();
            return this;
        }

        public string ToSql()
        {
            if (_where.Length == 0 && _source.Any() == false) return null;

            var sb = new StringBuilder();
            sb.Append("UPDATE ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke())).Append(" SET ");

            if (_set.Length > 0)
            { //指定 set 更新
                sb.Append(_set.ToString().Substring(2));

            }
            else if (_source.Count == 1)
            { //保存 Source
                _paramsSource.Clear();
                var colidx = 0;
                foreach (var col in _table.Columns.Values)
                {
                    if (col.Attribute.IsIdentity == false && col.Attribute.IsVersion == false && _ignore.ContainsKey(col.Attribute.Name) == false)
                    {
                        if (colidx > 0) sb.Append(", ");
                        sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ");

                        if (col.Attribute.CanUpdate && string.IsNullOrEmpty(col.DbUpdateValue) == false)
                            sb.Append(col.DbUpdateValue);
                        else
                        {
                            var val = col.GetMapValue(_source.First());
                            if (_noneParameter)
                                sb.Append(_commonUtils.GetNoneParamaterSqlValue(_paramsSource, col.Attribute.MapType, val));
                            else
                            {
                                sb.Append(_commonUtils.QuoteWriteParamter(col.Attribute.MapType, _commonUtils.QuoteParamterName($"p_{_paramsSource.Count}")));
                                _commonUtils.AppendParamter(_paramsSource, null, col, col.Attribute.MapType, val);
                            }
                        }
                        ++colidx;
                    }
                }
                if (colidx == 0) return null;

            }
            else if (_source.Count > 1)
            { //批量保存 Source
                if (_table.Primarys.Any() == false) return null;
                
                var caseWhen = new StringBuilder();
                caseWhen.Append("CASE ");
                ToSqlCase(caseWhen, _table.Primarys);
                var cw = caseWhen.ToString();

                _paramsSource.Clear();
                var colidx = 0;
                foreach (var col in _table.Columns.Values)
                {
                    if (col.Attribute.IsIdentity == false && col.Attribute.IsVersion == false && _ignore.ContainsKey(col.Attribute.Name) == false)
                    {
                        if (colidx > 0) sb.Append(", ");
                        sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ");

                        if (col.Attribute.CanUpdate && string.IsNullOrEmpty(col.DbUpdateValue) == false)
                            sb.Append(col.DbUpdateValue);
                        else
                        {
                            var nulls = 0;
                            var cwsb = new StringBuilder().Append(cw);
                            foreach (var d in _source)
                            {
                                cwsb.Append(" \r\nWHEN ");
                                ToSqlWhen(cwsb, _table.Primarys, d);
                                cwsb.Append(" THEN ");
                                var val = col.GetMapValue(d);
                                if (_noneParameter)
                                    cwsb.Append(_commonUtils.GetNoneParamaterSqlValue(_paramsSource, col.Attribute.MapType, val));
                                else
                                {
                                    cwsb.Append(_commonUtils.QuoteWriteParamter(col.Attribute.MapType, _commonUtils.QuoteParamterName($"p_{_paramsSource.Count}")));
                                    _commonUtils.AppendParamter(_paramsSource, null, col, col.Attribute.MapType, val);
                                }
                                if (val == null || val == DBNull.Value) nulls++;
                            }
                            cwsb.Append(" END");
                            if (nulls == _source.Count) sb.Append("NULL");
                            else
                            {
                                ToSqlCaseWhenEnd(cwsb, col);
                                sb.Append(cwsb.ToString());
                            }
                            cwsb.Clear();
                        }
                        ++colidx;
                    }
                }
                if (colidx == 0) return null;
            }
            else if (_setIncr.Length == 0)
                return null;

            if (_setIncr.Length > 0)
                sb.Append(_set.Length > 0 ? _setIncr.ToString() : _setIncr.ToString().Substring(2));

            if (_source.Any() == false)
            {
                foreach (var col in _table.Columns.Values)
                    if (col.Attribute.CanUpdate && string.IsNullOrEmpty(col.DbUpdateValue) == false)
                        sb.Append(", ").Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ").Append(col.DbUpdateValue);
            }

            if (_table.VersionColumn != null)
            {
                var vcname = _commonUtils.QuoteSqlName(_table.VersionColumn.Attribute.Name);
                sb.Append(", ").Append(vcname).Append(" = ").Append(_commonUtils.IsNull(vcname, 0)).Append(" + 1");
            }

            sb.Append(" \r\nWHERE ");
            if (_source.Any())
                sb.Append("(").Append(_commonUtils.WhereItems(_table, "", _source)).Append(")");

            if (_where.Length > 0)
                sb.Append(_source.Any() ? _where.ToString() : _where.ToString().Substring(5));

            if (_whereGlobalFilter.Any())
            {
                var globalFilterCondi = _commonExpression.GetWhereCascadeSql(new SelectTableInfo { Table = _table }, _whereGlobalFilter.Select(a => a.Where).ToList());
                if (string.IsNullOrEmpty(globalFilterCondi) == false)
                    sb.Append(" AND ").Append(globalFilterCondi);
            }

            if (_table.VersionColumn != null)
            {
                var versionCondi = WhereCaseSource(_table.VersionColumn.CsName, sqlval => sqlval);
                if (string.IsNullOrEmpty(versionCondi) == false)
                    sb.Append(" AND ").Append(versionCondi);
            }

            return sb.ToString();
        }
    }
}