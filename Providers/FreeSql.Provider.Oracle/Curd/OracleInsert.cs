using FreeSql.Internal;
using FreeSql.Internal.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Oracle.Curd
{

    class OracleInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public OracleInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(500, 999);
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(500, 999);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(500, 999);
        public override Task<long> ExecuteIdentityAsync() => base.SplitExecuteIdentityAsync(500, 999);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(500, 999);
        public override Task<List<T1>> ExecuteInsertedAsync() => base.SplitExecuteInsertedAsync(500, 999);


        public override string ToSql()
        {
            if (_source == null || _source.Any() == false) return null;
            var sb = new StringBuilder();
            sb.Append("INSERT ");
            if (_source.Count > 1) sb.Append("ALL");

            _identCol = null;
            var sbtb = new StringBuilder();
            sbtb.Append("INTO ");
            sbtb.Append(_commonUtils.QuoteSqlName(_tableRule?.Invoke(_table.DbName) ?? _table.DbName)).Append("(");
            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (col.Attribute.IsIdentity == true)
                {
                    _identCol = col;
                    continue;
                }
                if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name) == false)
                {
                    if (colidx > 0) sbtb.Append(", ");
                    sbtb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name));
                    ++colidx;
                }
            }
            sbtb.Append(") ");

            _params = _noneParameter ? new DbParameter[0] : new DbParameter[colidx * _source.Count];
            var specialParams = new List<DbParameter>();
            var didx = 0;
            foreach (var d in _source)
            {
                if (_source.Count > 1) sb.Append("\r\n");
                sb.Append(sbtb);
                sb.Append("VALUES");
                sb.Append("(");
                var colidx2 = 0;
                foreach (var col in _table.Columns.Values)
                {
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
                }
                sb.Append(")");
                ++didx;
            }
            if (_source.Count > 1) sb.Append("\r\n SELECT 1 FROM DUAL");
            return sb.ToString();
        }

        ColumnInfo _identCol;
        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            long ret = 0;
            Exception exception = null;
            Aop.CurdBeforeEventArgs before = null;

            if (_identCol == null || _source.Count > 1)
            {
                before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, _params);
                _orm.Aop.CurdBefore?.Invoke(this, before);
                try
                {
                    ret = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _params);
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
                return 0;
            }
            var identColName = _commonUtils.QuoteSqlName(_identCol.Attribute.Name);
            var identParam = _commonUtils.AppendParamter(null, $"{_identCol.CsName}99", _identCol.Attribute.MapType, 0) as OracleParameter;
            identParam.Direction = ParameterDirection.Output;
            sql = $"{sql} RETURNING {identColName} INTO {identParam.ParameterName}";
            var dbParms = _params.Concat(new[] { identParam }).ToArray();
            before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, dbParms);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            try
            {
                _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, dbParms);
                long.TryParse(string.Concat(identParam.Value), out ret);
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
        async protected override Task<long> RawExecuteIdentityAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            long ret = 0;
            Exception exception = null;
            Aop.CurdBeforeEventArgs before = null;

            if (_identCol == null || _source.Count > 1)
            {
                before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, _params);
                _orm.Aop.CurdBefore?.Invoke(this, before);
                try
                {
                    ret = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _params);
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
                return 0;
            }
            var identColName = _commonUtils.QuoteSqlName(_identCol.Attribute.Name);
            var identParam = _commonUtils.AppendParamter(null, $"{_identCol.CsName}99", _identCol.Attribute.MapType, 0) as OracleParameter;
            identParam.Direction = ParameterDirection.Output;
            sql = $"{sql} RETURNING {identColName} INTO {identParam.ParameterName}";
            var dbParms = _params.Concat(new[] { identParam }).ToArray();
            before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, dbParms);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            try
            {
                await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, dbParms);
                long.TryParse(string.Concat(identParam.Value), out ret);
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

        protected override List<T1> RawExecuteInserted()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            this.RawExecuteAffrows();
            return _source;
        }
        async protected override Task<List<T1>> RawExecuteInsertedAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            await this.RawExecuteAffrowsAsync();
            return _source;
        }
    }
}
