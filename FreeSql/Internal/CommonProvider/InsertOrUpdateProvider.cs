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

    public abstract partial class InsertOrUpdateProvider<T1> : IInsertOrUpdate<T1> where T1 : class
    {
        protected IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        protected List<T1> _source = new List<T1>();
        protected Dictionary<string, bool> _auditValueChangedDict = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        protected TableInfo _table;
        protected Func<string, string> _tableRule;
        protected DbParameter[] _params;
        protected DbTransaction _transaction;
        protected DbConnection _connection;

        public InsertOrUpdateProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
            _table = _commonUtils.GetTableByEntity(typeof(T1));
            if (_orm.CodeFirst.IsAutoSyncStructure && typeof(T1) != typeof(object)) _orm.CodeFirst.SyncStructure<T1>();
        }

        protected void ClearData()
        {
            _source.Clear();
            _auditValueChangedDict.Clear();
        }

        public IInsertOrUpdate<T1> WithTransaction(DbTransaction transaction)
        {
            _transaction = transaction;
            _connection = _transaction?.Connection;
            return this;
        }
        public IInsertOrUpdate<T1> WithConnection(DbConnection connection)
        {
            if (_transaction?.Connection != connection) _transaction = null;
            _connection = connection;
            return this;
        }

        public static void AuditDataValue(object sender, IEnumerable<T1> data, IFreeSql orm, TableInfo table, Dictionary<string, bool> changedDict)
        {
            if (data?.Any() != true) return;
            if (orm.Aop.AuditValueHandler == null) return;
            foreach (var d in data)
            {
                if (d == null) continue;
                foreach (var col in table.Columns.Values)
                {
                    object val = col.GetMapValue(d);
                    var auditArgs = new Aop.AuditValueEventArgs(Aop.AuditValueType.InsertOrUpdate, col, table.Properties[col.CsName], val);
                    orm.Aop.AuditValueHandler(sender, auditArgs);
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
            if (orm.Aop.AuditValueHandler == null) return;
            if (data == null) return;
            if (typeof(T1) == typeof(object) && new[] { table.Type, table.TypeLazy }.Contains(data.GetType()) == false)
                throw new Exception($"操作的数据类型({data.GetType().DisplayCsharp()}) 与 AsType({table.Type.DisplayCsharp()}) 不一致，请检查。");
            foreach (var col in table.Columns.Values)
            {
                object val = col.GetMapValue(data);
                var auditArgs = new Aop.AuditValueEventArgs(Aop.AuditValueType.InsertOrUpdate, col, table.Properties[col.CsName], val);
                orm.Aop.AuditValueHandler(sender, auditArgs);
                if (auditArgs.IsChanged)
                {
                    col.SetMapValue(data, val = auditArgs.Value);
                    if (changedDict != null && changedDict.ContainsKey(col.Attribute.Name) == false)
                        changedDict.Add(col.Attribute.Name, true);
                }
            }
        }

        public IInsertOrUpdate<T1> SetSource(T1 source) => this.SetSource(new[] { source });
        public IInsertOrUpdate<T1> SetSource(IEnumerable<T1> source)
        {
            if (source == null || source.Any() == false) return this;
            AuditDataValue(this, source, _orm, _table, _auditValueChangedDict);
            _source.AddRange(source.Where(a => a != null));
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
        public IInsertOrUpdate<T1> AsTable(Func<string, string> tableRule)
        {
            _tableRule = tableRule;
            return this;
        }
        public IInsertOrUpdate<T1> AsType(Type entityType)
        {
            if (entityType == typeof(object)) throw new Exception("IInsertOrUpdate.AsType 参数不支持指定为 object");
            if (entityType == _table.Type) return this;
            var newtb = _commonUtils.GetTableByEntity(entityType);
            _table = newtb ?? throw new Exception("IInsertOrUpdate.AsType 参数错误，请传入正确的实体类型");
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(entityType);
            return this;
        }

        public void WriteSourceSelectUnionAll(StringBuilder sb)
        {
            var specialParams = new List<DbParameter>();
            var didx = 0;
            foreach (var d in _source)
            {
                if (didx > 0) sb.Append(" \r\nUNION ALL\r\n ");
                sb.Append("SELECT ");
                var colidx2 = 0;
                foreach (var col in _table.Columns.Values)
                {
                    if (colidx2 > 0) sb.Append(", ");
                    if (string.IsNullOrEmpty(col.DbInsertValue) == false)
                        sb.Append(col.DbInsertValue);
                    else
                    {
                        object val = col.GetMapValue(d);
                        sb.Append(_commonUtils.GetNoneParamaterSqlValue(specialParams, col.Attribute.MapType, val));
                    }
                    if (didx == 0) sb.Append(" as ").Append(col.Attribute.Name);
                    ++colidx2;
                }
                switch (_orm.Ado.DataType)
                {
                    case DataType.OdbcOracle:
                    case DataType.Oracle:
                    case DataType.OdbcDameng:
                    case DataType.Dameng:
                        sb.Append(" FROM dual");
                        break;
                }
                ++didx;
            }
            if (specialParams.Any()) _params = specialParams.ToArray();
        }

        public abstract string ToSql();
        public int ExecuteAffrows()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.InsertOrUpdate, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
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
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return affrows;
        }
#if net40
#else
        async public Task<int> ExecuteAffrowsAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.InsertOrUpdate, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
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
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return affrows;
        }
#endif
    }
}
