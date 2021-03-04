using FreeSql.Internal.Model;
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

    public abstract partial class DeleteProvider<T1> : IDelete<T1>
    {
        public IFreeSql _orm;
        public CommonUtils _commonUtils;
        public CommonExpression _commonExpression;
        public TableInfo _table;
        public Func<string, string> _tableRule;
        public StringBuilder _where = new StringBuilder();
        public int _whereTimes = 0;
        public List<GlobalFilter.Item> _whereGlobalFilter;
        public List<DbParameter> _params = new List<DbParameter>();
        public DbTransaction _transaction;
        public DbConnection _connection;
        public int _commandTimeout = 0;
        public Action<StringBuilder> _interceptSql;

        public DeleteProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
            _table = _commonUtils.GetTableByEntity(typeof(T1));
            this.Where(_commonUtils.WhereObject(_table, "", dywhere));
            if (_orm.CodeFirst.IsAutoSyncStructure && typeof(T1) != typeof(object)) _orm.CodeFirst.SyncStructure<T1>();
            _whereGlobalFilter = _orm.GlobalFilter.GetFilters();
        }

        protected void ClearData()
        {
            _where.Clear();
            _whereTimes = 0;
            _params.Clear();
            _whereGlobalFilter = _orm.GlobalFilter.GetFilters();
        }

        public IDelete<T1> WithTransaction(DbTransaction transaction)
        {
            _transaction = transaction;
            _connection = _transaction?.Connection;
            return this;
        }
        public IDelete<T1> WithConnection(DbConnection connection)
        {
            if (_transaction?.Connection != connection) _transaction = null;
            _connection = connection;
            return this;
        }
        public IDelete<T1> CommandTimeout(int timeout)
        {
            _commandTimeout = timeout;
            return this;
        }

        public int ExecuteAffrows()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
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
            this.ClearData();
            return affrows;
        }
        public abstract List<T1> ExecuteDeleted();

        public IDelete<T1> Where(Expression<Func<T1, bool>> exp) => WhereIf(true, exp);
        public IDelete<T1> WhereIf(bool condition, Expression<Func<T1, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            return this.Where(_commonExpression.ExpressionWhereLambdaNoneForeignObject(null, _table, null, exp?.Body, null, _params));
        }
        public IDelete<T1> Where(string sql, object parms = null) => WhereIf(true, sql, parms);
        public IDelete<T1> WhereIf(bool condition, string sql, object parms = null)
        {
            if (condition == false || string.IsNullOrEmpty(sql)) return this;
            if (++_whereTimes > 1) _where.Append(" AND ");
            _where.Append('(').Append(sql).Append(')');
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this;
        }
        public IDelete<T1> Where(T1 item) => this.Where(new[] { item });
        public IDelete<T1> Where(IEnumerable<T1> items) => this.Where(_commonUtils.WhereItems(_table.Primarys, "", items));
        public IDelete<T1> WhereDynamic(object dywhere, bool not = false) => not == false ?
            this.Where(_commonUtils.WhereObject(_table, "", dywhere)) :
            this.Where($"not({_commonUtils.WhereObject(_table, "", dywhere)})");

        public IDelete<T1> DisableGlobalFilter(params string[] name)
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
        public IDelete<T1> AsTable(Func<string, string> tableRule)
        {
            _tableRule = tableRule;
            return this;
        }
        public IDelete<T1> AsType(Type entityType)
        {
            if (entityType == typeof(object)) throw new Exception("IDelete.AsType 参数不支持指定为 object");
            if (entityType == _table.Type) return this;
            var newtb = _commonUtils.GetTableByEntity(entityType);
            _table = newtb ?? throw new Exception("IDelete.AsType 参数错误，请传入正确的实体类型");
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(entityType);
            return this;
        }

        public string ToSql()
        {
            if (_whereTimes <= 0) return null;
            var sb = new StringBuilder().Append("DELETE FROM ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke())).Append(" WHERE ").Append(_where);

            if (_whereGlobalFilter.Any())
            {
                var globalFilterCondi = _commonExpression.GetWhereCascadeSql(new SelectTableInfo { Table = _table }, _whereGlobalFilter, false);
                if (string.IsNullOrEmpty(globalFilterCondi) == false)
                    sb.Append(" AND ").Append(globalFilterCondi);
            }
            _interceptSql?.Invoke(sb);
            return sb.ToString();
        }
    }
}
