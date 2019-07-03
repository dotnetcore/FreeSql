using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.MySql.Curd
{

    class MySqlUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1> where T1 : class
    {

        public MySqlUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(500, 3000);
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(500, 3000);
        public override List<T1> ExecuteUpdated() => base.SplitExecuteUpdated(500, 3000);
        public override Task<List<T1>> ExecuteUpdatedAsync() => base.SplitExecuteUpdatedAsync(500, 3000);


        protected override List<T1> RawExecuteUpdated()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteReadColumn(col.Attribute.MapType, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }
            sql = sb.ToString();
            var dbParms = _params.Concat(_paramsSource).ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Update, sql, dbParms);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = _orm.Ado.Query<T1>(_connection, _transaction, CommandType.Text, sql, dbParms);
                ValidateVersionAndThrow(ret.Count);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return ret;
        }
        async protected override Task<List<T1>> RawExecuteUpdatedAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteReadColumn(col.Attribute.MapType, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }
            sql = sb.ToString();
            var dbParms = _params.Concat(_paramsSource).ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Update, sql, dbParms);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = await _orm.Ado.QueryAsync<T1>(_connection, _transaction, CommandType.Text, sql, dbParms);
                ValidateVersionAndThrow(ret.Count);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return ret;
        }

        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys)
        {
            if (_table.Primarys.Length == 1)
            {
                caseWhen.Append(_commonUtils.QuoteReadColumn(_table.Primarys.First().Attribute.MapType, _commonUtils.QuoteSqlName(_table.Primarys.First().Attribute.Name)));
                return;
            }
            caseWhen.Append("CONCAT(");
            var pkidx = 0;
            foreach (var pk in _table.Primarys)
            {
                if (pkidx > 0) caseWhen.Append(", ");
                caseWhen.Append(_commonUtils.QuoteReadColumn(pk.Attribute.MapType, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                ++pkidx;
            }
            caseWhen.Append(")");
        }

        protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d)
        {
            if (_table.Primarys.Length == 1)
            {
                sb.Append(_commonUtils.FormatSql("{0}", _table.Primarys.First().GetMapValue(d)));
                return;
            }
            sb.Append("CONCAT(");
            var pkidx = 0;
            foreach (var pk in _table.Primarys)
            {
                if (pkidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.FormatSql("{0}", pk.GetMapValue(d)));
                ++pkidx;
            }
            sb.Append(")");
        }
    }
}
