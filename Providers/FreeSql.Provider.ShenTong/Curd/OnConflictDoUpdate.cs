using FreeSql.Aop;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.ShenTong.Curd
{
    public class OnConflictDoUpdate<T1> where T1 : class
    {
        internal ShenTongInsert<T1> _oscarInsert;
        internal ShenTongUpdate<T1> _oscarUpdatePriv;
        internal ShenTongUpdate<T1> _oscarUpdate => _oscarUpdatePriv ?? 
            (_oscarUpdatePriv = new ShenTongUpdate<T1>(_oscarInsert.InternalOrm, _oscarInsert.InternalCommonUtils, _oscarInsert.InternalCommonExpression, null) { InternalTableAlias = "EXCLUDED" }
                .NoneParameter().SetSource(_oscarInsert.InternalSource) as ShenTongUpdate<T1>);
        ColumnInfo[] _columns;
        bool _doNothing;

        public OnConflictDoUpdate(IInsert<T1> insert, Expression<Func<T1, object>> columns = null)
        {
            _oscarInsert = insert as ShenTongInsert<T1>;
            if (_oscarInsert == null) throw new Exception("OnConflictDoUpdate 是 FreeSql.Provider.ShenTong 特有的功能");

            if (columns != null)
            {
                var colsList = new List<ColumnInfo>();
                var cols = _oscarInsert.InternalCommonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false, null).ToDictionary(a => a, a => true);
                foreach (var col in _oscarInsert.InternalTable.Columns.Values)
                    if (cols.ContainsKey(col.Attribute.Name))
                        colsList.Add(col);
                _columns = colsList.ToArray();
            }
            if (_columns == null || _columns.Any() == false)
                _columns = _oscarInsert.InternalTable.Primarys;
            if (_columns.Any() == false) throw new Exception("OnConflictDoUpdate 功能要求实体类必须设置 IsPrimary 属性");
        }

        protected void ClearData()
        {
            _oscarInsert.InternalClearData();
            _oscarUpdatePriv = null;
        }

        public OnConflictDoUpdate<T1> IgnoreColumns(Expression<Func<T1, object>> columns)
        {
            _oscarUpdate.IgnoreColumns(columns);
            return this;
        }
        public OnConflictDoUpdate<T1> UpdateColumns(Expression<Func<T1, object>> columns)
        {
            _oscarUpdate.UpdateColumns(columns);
            return this;
        }
        public OnConflictDoUpdate<T1> IgnoreColumns(string[] columns)
        {
            _oscarUpdate.IgnoreColumns(columns);
            return this;
        }
        public OnConflictDoUpdate<T1> UpdateColumns(string[] columns)
        {
            _oscarUpdate.UpdateColumns(columns);
            return this;
        }

        public OnConflictDoUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> column, TMember value)
        {
            _oscarUpdate.Set(column, value);
            return this;
        }
        //由于表达式解析问题，ON CONFLICT("id") DO UPDATE SET 需要指定表别名，如 Set(a => a.Clicks + 1) 解析会失败
        //暂时不开放这个功能，如有需要使用 SetRaw("click = t.click + 1") 替代该操作
        //public OnConflictDoUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> exp)
        //{
        //    _oscarUpdate.Set(exp);
        //    return this;
        //}
        public OnConflictDoUpdate<T1> SetRaw(string sql)
        {
            _oscarUpdate.SetRaw(sql);
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
            sb.Append(_oscarInsert.ToSql()).Append("\r\nON CONFLICT(");
            for (var a = 0; a < _columns.Length; a++)
            {
                if (a > 0) sb.Append(", ");
                sb.Append(_oscarInsert.InternalCommonUtils.QuoteSqlName(_columns[a].Attribute.Name));
            }
            if (_doNothing)
            {
                sb.Append(") DO NOTHING");
            }
            else
            {
                sb.Append(") DO UPDATE SET\r\n");

                var sbSetEmpty = _oscarUpdate.InternalSbSet.Length == 0;
                var sbSetIncrEmpty = _oscarUpdate.InternalSbSetIncr.Length == 0;
                if (sbSetEmpty == false || sbSetIncrEmpty == false)
                {
                    if (sbSetEmpty == false) sb.Append(_oscarUpdate.InternalSbSet.ToString().Substring(2));
                    if (sbSetIncrEmpty == false) sb.Append(sbSetEmpty ? _oscarUpdate.InternalSbSetIncr.ToString().Substring(2) : _oscarUpdate.InternalSbSetIncr.ToString());
                }
                else
                {
                    var colidx = 0;
                    foreach (var col in _oscarInsert.InternalTable.Columns.Values)
                    {
                        if (col.Attribute.IsPrimary || _oscarUpdate.InternalIgnore.ContainsKey(col.Attribute.Name)) continue;

                        if (colidx > 0) sb.Append(", \r\n");

                        if (col.Attribute.IsVersion == true)
                        {
                            var field = _oscarInsert.InternalCommonUtils.QuoteSqlName(col.Attribute.Name);
                            sb.Append(field).Append(" = ").Append(_oscarInsert.InternalCommonUtils.QuoteSqlName(_oscarInsert.InternalTable.DbName)).Append(".").Append(field).Append(" + 1");
                        }
                        else if (_oscarInsert.InternalIgnore.ContainsKey(col.Attribute.Name))
                        {
                            var caseWhen = _oscarUpdate.InternalWhereCaseSource(col.CsName, sqlval => sqlval).Trim();
                            sb.Append(caseWhen);
                            if (caseWhen.EndsWith(" END")) _oscarUpdate.InternalToSqlCaseWhenEnd(sb, col);
                        }
                        else
                        {
                            var field = _oscarInsert.InternalCommonUtils.QuoteSqlName(col.Attribute.Name);
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

            var before = new CurdBeforeEventArgs(_oscarInsert.InternalTable.Type, _oscarInsert.InternalTable, CurdType.Insert, sql, _oscarInsert.InternalParams);
            _oscarInsert.InternalOrm.Aop.CurdBeforeHandler?.Invoke(_oscarInsert, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                ret = _oscarInsert.InternalOrm.Ado.ExecuteNonQuery(_oscarInsert.InternalConnection, _oscarInsert.InternalTransaction, CommandType.Text, sql, _oscarInsert.InternalParams);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new CurdAfterEventArgs(before, exception, ret);
                _oscarInsert.InternalOrm.Aop.CurdAfterHandler?.Invoke(_oscarInsert, after);
                ClearData();
            }
            return ret;
        }

#if net40
#else
        async public Task<long> ExecuteAffrowsAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var before = new CurdBeforeEventArgs(_oscarInsert.InternalTable.Type, _oscarInsert.InternalTable, CurdType.Insert, sql, _oscarInsert.InternalParams);
            _oscarInsert.InternalOrm.Aop.CurdBeforeHandler?.Invoke(_oscarInsert, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                ret = await _oscarInsert.InternalOrm.Ado.ExecuteNonQueryAsync(_oscarInsert.InternalConnection, _oscarInsert.InternalTransaction, CommandType.Text, sql, _oscarInsert.InternalParams);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new CurdAfterEventArgs(before, exception, ret);
                _oscarInsert.InternalOrm.Aop.CurdAfterHandler?.Invoke(_oscarInsert, after);
                ClearData();
            }
            return ret;
        }
#endif
    }
}
