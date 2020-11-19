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

namespace FreeSql.KingbaseES
{
    public class KingbaseESOnConflictDoUpdate<T1> where T1 : class
    {
        internal KingbaseESInsert<T1> _insert;
        internal KingbaseESUpdate<T1> _updatePriv;
        internal KingbaseESUpdate<T1> _update => _updatePriv ?? 
            (_updatePriv = new KingbaseESUpdate<T1>(_insert.InternalOrm, _insert.InternalCommonUtils, _insert.InternalCommonExpression, null) { InternalTableAlias = "EXCLUDED" }
                .NoneParameter().SetSource(_insert.InternalSource) as KingbaseESUpdate<T1>);
        ColumnInfo[] _columns;
        bool _doNothing;

        public KingbaseESOnConflictDoUpdate(IInsert<T1> insert, Expression<Func<T1, object>> columns = null)
        {
            _insert = insert as KingbaseESInsert<T1>;
            if (_insert == null) throw new Exception("OnConflictDoUpdate 是 FreeSql.Provider.KingbaseES 特有的功能");
            if (_insert._noneParameterFlag == "c") _insert._noneParameterFlag = "cu";

            if (columns != null)
            {
                var colsList = new List<ColumnInfo>();
                var cols = _insert.InternalCommonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false, null).ToDictionary(a => a, a => true);
                foreach (var col in _insert.InternalTable.Columns.Values)
                    if (cols.ContainsKey(col.Attribute.Name))
                        colsList.Add(col);
                _columns = colsList.ToArray();
            }
            if (_columns == null || _columns.Any() == false)
                _columns = _insert.InternalTable.Primarys;
            if (_columns.Any() == false) throw new Exception("OnConflictDoUpdate 功能要求实体类必须设置 IsPrimary 属性");
        }

        protected void ClearData()
        {
            _insert.InternalClearData();
            _updatePriv = null;
        }

        public KingbaseESOnConflictDoUpdate<T1> IgnoreColumns(Expression<Func<T1, object>> columns)
        {
            _update.IgnoreColumns(columns);
            return this;
        }
        public KingbaseESOnConflictDoUpdate<T1> UpdateColumns(Expression<Func<T1, object>> columns)
        {
            _update.UpdateColumns(columns);
            return this;
        }
        public KingbaseESOnConflictDoUpdate<T1> IgnoreColumns(string[] columns)
        {
            _update.IgnoreColumns(columns);
            return this;
        }
        public KingbaseESOnConflictDoUpdate<T1> UpdateColumns(string[] columns)
        {
            _update.UpdateColumns(columns);
            return this;
        }

        public KingbaseESOnConflictDoUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> column, TMember value)
        {
            _update.Set(column, value);
            return this;
        }
        //由于表达式解析问题，ON CONFLICT("id") DO UPDATE SET 需要指定表别名，如 Set(a => a.Clicks + 1) 解析会失败
        //暂时不开放这个功能，如有需要使用 SetRaw("click = t.click + 1") 替代该操作
        //public OnConflictDoUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> exp)
        //{
        //    _update.Set(exp);
        //    return this;
        //}
        public KingbaseESOnConflictDoUpdate<T1> SetRaw(string sql)
        {
            _update.SetRaw(sql);
            return this;
        }

        public KingbaseESOnConflictDoUpdate<T1> DoNothing()
        {
            _doNothing = true;
            return this;
        }

        public string ToSql()
        {
            var sb = new StringBuilder();
            sb.Append(_insert.ToSql()).Append("\r\nON CONFLICT(");
            for (var a = 0; a < _columns.Length; a++)
            {
                if (a > 0) sb.Append(", ");
                sb.Append(_insert.InternalCommonUtils.QuoteSqlName(_columns[a].Attribute.Name));
            }
            if (_doNothing)
            {
                sb.Append(") DO NOTHING");
            }
            else
            {
                sb.Append(") DO UPDATE SET\r\n");

                var sbSetEmpty = _update.InternalSbSet.Length == 0;
                var sbSetIncrEmpty = _update.InternalSbSetIncr.Length == 0;
                if (sbSetEmpty == false || sbSetIncrEmpty == false)
                {
                    if (sbSetEmpty == false) sb.Append(_update.InternalSbSet.ToString().Substring(2));
                    if (sbSetIncrEmpty == false) sb.Append(sbSetEmpty ? _update.InternalSbSetIncr.ToString().Substring(2) : _update.InternalSbSetIncr.ToString());
                }
                else
                {
                    var colidx = 0;
                    foreach (var col in _insert.InternalTable.Columns.Values)
                    {
                        if (col.Attribute.IsPrimary || _update.InternalIgnore.ContainsKey(col.Attribute.Name)) continue;

                        if (colidx > 0) sb.Append(", \r\n");

                        if (col.Attribute.IsVersion == true && col.Attribute.MapType != typeof(byte[]))
                        {
                            var field = _insert.InternalCommonUtils.QuoteSqlName(col.Attribute.Name);
                            sb.Append(field).Append(" = ").Append(_insert.InternalCommonUtils.QuoteSqlName(_insert.InternalTable.DbName)).Append(".").Append(field).Append(" + 1");
                        }
                        else if (_insert.InternalIgnore.ContainsKey(col.Attribute.Name))
                        {
                            var caseWhen = _update.InternalWhereCaseSource(col.CsName, sqlval => sqlval).Trim();
                            sb.Append(caseWhen);
                            if (caseWhen.EndsWith(" END")) _update.InternalToSqlCaseWhenEnd(sb, col);
                        }
                        else
                        {
                            var field = _insert.InternalCommonUtils.QuoteSqlName(col.Attribute.Name);
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

            var before = new CurdBeforeEventArgs(_insert.InternalTable.Type, _insert.InternalTable, CurdType.Insert, sql, _insert.InternalParams);
            _insert.InternalOrm.Aop.CurdBeforeHandler?.Invoke(_insert, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                ret = _insert.InternalOrm.Ado.ExecuteNonQuery(_insert.InternalConnection, _insert.InternalTransaction, CommandType.Text, sql, _insert._commandTimeout, _insert.InternalParams);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new CurdAfterEventArgs(before, exception, ret);
                _insert.InternalOrm.Aop.CurdAfterHandler?.Invoke(_insert, after);
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

            var before = new CurdBeforeEventArgs(_insert.InternalTable.Type, _insert.InternalTable, CurdType.Insert, sql, _insert.InternalParams);
            _insert.InternalOrm.Aop.CurdBeforeHandler?.Invoke(_insert, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                ret = await _insert.InternalOrm.Ado.ExecuteNonQueryAsync(_insert.InternalConnection, _insert.InternalTransaction, CommandType.Text, sql, _insert._commandTimeout, _insert.InternalParams, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new CurdAfterEventArgs(before, exception, ret);
                _insert.InternalOrm.Aop.CurdAfterHandler?.Invoke(_insert, after);
                ClearData();
            }
            return ret;
        }
#endif
    }
}
