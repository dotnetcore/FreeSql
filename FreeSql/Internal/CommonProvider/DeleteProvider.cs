using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    public abstract partial class DeleteProvider<T1> : IDelete<T1> where T1 : class
    {
        protected IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        protected TableInfo _table;
        protected Func<string, string> _tableRule;
        protected StringBuilder _where = new StringBuilder();
        protected int _whereTimes = 0;
        protected List<DbParameter> _params = new List<DbParameter>();
        protected DbTransaction _transaction;
        protected DbConnection _connection;

        public DeleteProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
            _table = _commonUtils.GetTableByEntity(typeof(T1));
            this.Where(_commonUtils.WhereObject(_table, "", dywhere));
            if (_orm.CodeFirst.IsAutoSyncStructure && typeof(T1) != typeof(object)) _orm.CodeFirst.SyncStructure<T1>();
        }

        protected void ClearData()
        {
            _where.Clear();
            _whereTimes = 0;
            _params.Clear();
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

        public int ExecuteAffrows()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Delete, sql, dbParms);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, dbParms);
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
            this.ClearData();
            return affrows;
        }
        async public Task<int> ExecuteAffrowsAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Delete, sql, dbParms);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, dbParms);
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
            this.ClearData();
            return affrows;
        }
        public abstract List<T1> ExecuteDeleted();
        public abstract Task<List<T1>> ExecuteDeletedAsync();

        public IDelete<T1> Where(Expression<Func<T1, bool>> exp) => this.Where(_commonExpression.ExpressionWhereLambdaNoneForeignObject(null, _table, null, exp?.Body, null));
        public IDelete<T1> Where(string sql, object parms = null)
        {
            if (string.IsNullOrEmpty(sql)) return this;
            var args = new Aop.WhereEventArgs(sql, parms);
            _orm.Aop.Where?.Invoke(this, new Aop.WhereEventArgs(sql, parms));
            if (args.IsCancel == true) return this;

            if (++_whereTimes > 1) _where.Append(" AND ");
            _where.Append("(").Append(sql).Append(")");
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this;
        }
        public IDelete<T1> Where(T1 item) => this.Where(new[] { item });
        public IDelete<T1> Where(IEnumerable<T1> items) => this.Where(_commonUtils.WhereItems(_table, "", items));
        public IDelete<T1> WhereExists<TEntity2>(ISelect<TEntity2> select, bool notExists = false) where TEntity2 : class => this.Where($"{(notExists ? "NOT " : "")}EXISTS({select.ToSql("1")})");
        public IDelete<T1> WhereDynamic(object dywhere) => this.Where(_commonUtils.WhereObject(_table, "", dywhere));

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

        public string ToSql() => _whereTimes <= 0 ? null : new StringBuilder().Append("DELETE FROM ").Append(_commonUtils.QuoteSqlName(_tableRule?.Invoke(_table.DbName) ?? _table.DbName)).Append(" WHERE ").Append(_where).ToString();
    }
}
