using FreeSql.Aop;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Duckdb.Curd
{
    public class OnConflictDoUpdate<T1> where T1 : class
    {
        internal DuckdbInsert<T1> _duckdbInsert;
        internal DuckdbUpdate<T1> _duckdbUpdatePriv;
        internal DuckdbUpdate<T1> _duckdbUpdate => _duckdbUpdatePriv ??
            (_duckdbUpdatePriv = new DuckdbUpdate<T1>(_duckdbInsert.InternalOrm, _duckdbInsert.InternalCommonUtils, _duckdbInsert.InternalCommonExpression, null) { InternalTableAlias = "EXCLUDED" }
                .NoneParameter().SetSource(_duckdbInsert.InternalSource) as DuckdbUpdate<T1>);
        internal ColumnInfo[] _tempPrimarys;
        bool _doNothing;

        public OnConflictDoUpdate(IInsert<T1> insert, Expression<Func<T1, object>> columns = null)
        {
            _duckdbInsert = insert as DuckdbInsert<T1>;
            if (_duckdbInsert == null) throw new Exception(CoreErrorStrings.S_Features_Unique("OnConflictDoUpdate", "Duckdb"));
            if (_duckdbInsert._noneParameterFlag == "c") _duckdbInsert._noneParameterFlag = "cu";

            if (columns != null)
            {
                var colsList = new List<ColumnInfo>();
                var cols = _duckdbInsert.InternalCommonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, null, columns?.Body, false, null).ToDictionary(a => a, a => true);
                foreach (var col in _duckdbInsert.InternalTable.Columns.Values)
                    if (cols.ContainsKey(col.Attribute.Name))
                        colsList.Add(col);
                _tempPrimarys = colsList.ToArray();
            }
            if (_tempPrimarys == null || _tempPrimarys.Any() == false)
                _tempPrimarys = _duckdbInsert.InternalTable.Primarys;
            if (_tempPrimarys.Any() == false) throw new Exception(CoreErrorStrings.S_OnConflictDoUpdate_MustIsPrimary);
        }

        protected void ClearData()
        {
            _duckdbInsert.InternalClearData();
            _duckdbUpdatePriv = null;
        }

        public OnConflictDoUpdate<T1> IgnoreColumns(Expression<Func<T1, object>> columns)
        {
            _duckdbUpdate.IgnoreColumns(columns);
            return this;
        }
        public OnConflictDoUpdate<T1> UpdateColumns(Expression<Func<T1, object>> columns)
        {
            _duckdbUpdate.UpdateColumns(columns);
            return this;
        }
        public OnConflictDoUpdate<T1> IgnoreColumns(string[] columns)
        {
            _duckdbUpdate.IgnoreColumns(columns);
            return this;
        }
        public OnConflictDoUpdate<T1> UpdateColumns(string[] columns)
        {
            _duckdbUpdate.UpdateColumns(columns);
            return this;
        }

        public OnConflictDoUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> column, TMember value)
        {
            _duckdbUpdate.Set(column, value);
            return this;
        }
        //由于表达式解析问题，ON CONFLICT("id") DO UPDATE SET 需要指定表别名，如 Set(a => a.Clicks + 1) 解析会失败
        //暂时不开放这个功能，如有需要使用 SetRaw("click = t.click + 1") 替代该操作
        //public OnConflictDoUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> exp)
        //{
        //    _duckdbUpdate.Set(exp);
        //    return this;
        //}
        public OnConflictDoUpdate<T1> SetRaw(string sql)
        {
            _duckdbUpdate.SetRaw(sql);
            return this;
        }

        public OnConflictDoUpdate<T1> DoNothing()
        {
            _doNothing = true;
            return this;
        }

        public string ToSql()
        {
            var sb = new StringBuilder();
            sb.Append(_duckdbInsert.ToSql()).Append("\r\nON CONFLICT(");
            for (var a = 0; a < _tempPrimarys.Length; a++)
            {
                if (a > 0) sb.Append(", ");
                sb.Append(_duckdbInsert.InternalCommonUtils.QuoteSqlName(_tempPrimarys[a].Attribute.Name));
            }
            if (_doNothing)
            {
                sb.Append(") DO NOTHING");
            }
            else
            {
                sb.Append(") DO UPDATE SET\r\n");

                if (_duckdbUpdate._tempPrimarys.Any() == false) _duckdbUpdate._tempPrimarys = _tempPrimarys;
                var sbSetEmpty = _duckdbUpdate.InternalSbSet.Length == 0;
                var sbSetIncrEmpty = _duckdbUpdate.InternalSbSetIncr.Length == 0;
                if (sbSetEmpty == false || sbSetIncrEmpty == false)
                {
                    if (sbSetEmpty == false) sb.Append(_duckdbUpdate.InternalSbSet.ToString().Substring(2));
                    if (sbSetIncrEmpty == false) sb.Append(sbSetEmpty ? _duckdbUpdate.InternalSbSetIncr.ToString().Substring(2) : _duckdbUpdate.InternalSbSetIncr.ToString());
                }
                else
                {
                    var colidx = 0;
                    foreach (var col in _duckdbInsert.InternalTable.Columns.Values)
                    {
                        if (col.Attribute.IsPrimary || _duckdbUpdate.InternalIgnore.ContainsKey(col.Attribute.Name)) continue;

                        if (colidx > 0) sb.Append(", \r\n");

                        if (col.Attribute.IsVersion == true && col.Attribute.MapType != typeof(byte[]))
                        {
                            var field = _duckdbInsert.InternalCommonUtils.QuoteSqlName(col.Attribute.Name);
                            sb.Append(field).Append(" = ").Append(_duckdbInsert.InternalCommonUtils.QuoteSqlName(_duckdbInsert.InternalTable.DbName)).Append(".").Append(field).Append(" + 1");
                        }
                        else if (_duckdbInsert.InternalIgnore.ContainsKey(col.Attribute.Name))
                        {
                            if (string.IsNullOrEmpty(col.DbUpdateValue) == false)
                            {
                                sb.Append(_duckdbInsert.InternalCommonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ").Append(col.DbUpdateValue);
                            }
                            else
                            {
                                var caseWhen = _duckdbUpdate.InternalWhereCaseSource(col.CsName, sqlval => sqlval).Trim();
                                sb.Append(caseWhen);
                                if (caseWhen.EndsWith(" END")) _duckdbUpdate.InternalToSqlCaseWhenEnd(sb, col);
                            }
                        }
                        else
                        {
                            var field = _duckdbInsert.InternalCommonUtils.QuoteSqlName(col.Attribute.Name);
                            sb.Append(field).Append(" = EXCLUDED.").Append(field);
                        }
                        ++colidx;
                    }
                }
            }

            return sb.ToString();
        }

        public long ExecuteAffrows()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var before = new CurdBeforeEventArgs(_duckdbInsert.InternalTable.Type, _duckdbInsert.InternalTable, CurdType.Insert, sql, _duckdbInsert.InternalParams);
            _duckdbInsert.InternalOrm.Aop.CurdBeforeHandler?.Invoke(_duckdbInsert, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                ret = _duckdbInsert.InternalOrm.Ado.ExecuteNonQuery(_duckdbInsert.InternalConnection, _duckdbInsert.InternalTransaction, CommandType.Text, sql, _duckdbInsert._commandTimeout, _duckdbInsert.InternalParams);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new CurdAfterEventArgs(before, exception, ret);
                _duckdbInsert.InternalOrm.Aop.CurdAfterHandler?.Invoke(_duckdbInsert, after);
                ClearData();
            }
            return ret;
        }

#if net40
#else
        async public Task<long> ExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var before = new CurdBeforeEventArgs(_duckdbInsert.InternalTable.Type, _duckdbInsert.InternalTable, CurdType.Insert, sql, _duckdbInsert.InternalParams);
            _duckdbInsert.InternalOrm.Aop.CurdBeforeHandler?.Invoke(_duckdbInsert, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                ret = await _duckdbInsert.InternalOrm.Ado.ExecuteNonQueryAsync(_duckdbInsert.InternalConnection, _duckdbInsert.InternalTransaction, CommandType.Text, sql, _duckdbInsert._commandTimeout, _duckdbInsert.InternalParams, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new CurdAfterEventArgs(before, exception, ret);
                _duckdbInsert.InternalOrm.Aop.CurdAfterHandler?.Invoke(_duckdbInsert, after);
                ClearData();
            }
            return ret;
        }
#endif
    }
}
