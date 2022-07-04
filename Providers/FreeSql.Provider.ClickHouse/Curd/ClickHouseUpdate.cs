using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.ClickHouse.Curd
{

    class ClickHouseUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1>
    {

        public ClickHouseUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        internal StringBuilder InternalSbSet => _set;
        internal StringBuilder InternalSbSetIncr => _setIncr;
        internal Dictionary<string, bool> InternalIgnore => _ignore;
        internal void InternalResetSource(List<T1> source) => _source = source;
        internal string InternalWhereCaseSource(string CsName, Func<string, string> thenValue) => WhereCaseSource(CsName, thenValue);
        internal void InternalToSqlCaseWhenEnd(StringBuilder sb, ColumnInfo col) => ToSqlCaseWhenEnd(sb, col);

        public override int ExecuteAffrows() => SplitExecuteAffrows(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        public override List<T1> ExecuteUpdated() => SplitExecuteUpdated(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        protected override List<T1> RawExecuteUpdated() => throw new NotImplementedException($"FreeSql.Provider.ClickHouse {CoreStrings.S_Not_Implemented_Feature}");
        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys)
        {
            if (primarys.Length == 1)
            {
                var pk = primarys.First();
                caseWhen.Append(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                return;
            }
            caseWhen.Append("CONCAT(");
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) caseWhen.Append(", '+', ");
                caseWhen.Append(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                ++pkidx;
            }
            caseWhen.Append(")");
        }
        protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d)
        {
            if (primarys.Length == 1)
            {
                sb.Append(_commonUtils.FormatSql("{0}", primarys[0].GetDbValue(d)));
                return;
            }
            sb.Append("concat(");
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) sb.Append(", '+', ");
                sb.Append(_commonUtils.FormatSql("{0}", pk.GetDbValue(d)));
                ++pkidx;
            }
            sb.Append(")");
        }

        public override void ToSqlExtension110(StringBuilder sb, bool isAsTableSplited)
        {
            if (_where.Length == 0 && _source.Any() == false) return;
            if (_source.Any() == false && _set.Length == 0 && _setIncr.Length == 0) return;

            if (_table.AsTableImpl != null && isAsTableSplited == false && _source == _sourceOld && _source.Any())
            {
                var atarr = _source.Select(a => new
                {
                    item = a,
                    splitKey = _table.AsTableImpl.GetTableNameByColumnValue(_table.AsTableColumn.GetValue(a))
                }).GroupBy(a => a.splitKey, a => a.item).ToArray();
                if (atarr.Length > 1)
                {
                    var oldSource = _source;
                    var arrret = new List<List<T1>>();
                    foreach (var item in atarr)
                    {
                        _source = item.ToList();
                        ToSqlExtension110(sb, true);
                        sb.Append("\r\n\r\n;\r\n\r\n");
                    }
                    _source = oldSource;
                    if (sb.Length > 0) sb.Remove(sb.Length - 9, 9);
                    return;
                }
            }

            sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke())).Append(" UPDATE ");

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
                    if (col.Attribute.IsPrimary) continue;
                    if (_tempPrimarys.Any(a => a.CsName == col.CsName)) continue;
                    if (col.Attribute.IsIdentity == false && col.Attribute.IsVersion == false && _ignore.ContainsKey(col.Attribute.Name) == false)
                    {
                        if (colidx > 0) sb.Append(", ");
                        sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ");

                        if (col.Attribute.CanUpdate && string.IsNullOrEmpty(col.DbUpdateValue) == false)
                            sb.Append(col.DbUpdateValue);
                        else
                        {
                            var val = col.GetDbValue(_source.First());

                            var colsql = _noneParameter ? _commonUtils.GetNoneParamaterSqlValue(_paramsSource, "u", col, col.Attribute.MapType, val) :
                                _commonUtils.QuoteWriteParamterAdapter(col.Attribute.MapType, _commonUtils.QuoteParamterName($"p_{_paramsSource.Count}"));
                            sb.Append(_commonUtils.RewriteColumn(col, colsql));
                            if (_noneParameter == false)
                                _commonUtils.AppendParamter(_paramsSource, null, col, col.Attribute.MapType, val);
                        }
                        ++colidx;
                    }
                }
                if (colidx == 0) return;

            }
            else if (_source.Count > 1)
            { //批量保存 Source
                if (_tempPrimarys.Any() == false) return;

                var caseWhen = new StringBuilder();
                ToSqlCase(caseWhen, _tempPrimarys);
                var cw = $"{caseWhen.ToString()}=";
                _paramsSource.Clear();
                var colidx = 0;
                foreach (var col in _table.Columns.Values)
                {
                    if (col.Attribute.IsPrimary) continue;
                    if (_tempPrimarys.Any(a => a.CsName == col.CsName)) continue;
                    if (col.Attribute.IsIdentity == false && col.Attribute.IsVersion == false && _ignore.ContainsKey(col.Attribute.Name) == false)
                    {
                        if (colidx > 0) sb.Append(", ");
                        var columnName = _commonUtils.QuoteSqlName(col.Attribute.Name);
                        sb.Append(columnName).Append(" = ");

                        if (col.Attribute.CanUpdate && string.IsNullOrEmpty(col.DbUpdateValue) == false)
                            sb.Append(col.DbUpdateValue);
                        else
                        {
                            var nulls = 0;
                            var cwsb = new StringBuilder().Append(" multiIf( ");
                            foreach (var d in _source)
                            {
                                cwsb.Append(cw);
                                ToSqlWhen(cwsb, _tempPrimarys, d);
                                cwsb.Append(",");
                                var val = col.GetDbValue(d);

                                var colsql = _noneParameter ? _commonUtils.GetNoneParamaterSqlValue(_paramsSource, "u", col, col.Attribute.MapType, val) :
                                    _commonUtils.QuoteWriteParamterAdapter(col.Attribute.MapType, _commonUtils.QuoteParamterName($"p_{_paramsSource.Count}"));
                                cwsb.Append(_commonUtils.RewriteColumn(col, colsql));
                                if (_noneParameter == false)
                                    _commonUtils.AppendParamter(_paramsSource, null, col, col.Attribute.MapType, val);
                                if (val == null || val == DBNull.Value) nulls++;
                                cwsb.Append(", ");
                            }
                            if (nulls == _source.Count) sb.Append("NULL");
                            else
                            {
                                cwsb.Append(columnName).Append(" )");
                                ToSqlCaseWhenEnd(cwsb, col);
                                sb.Append(cwsb);
                            }
                            cwsb.Clear();
                        }
                        ++colidx;
                    }
                }
                if (colidx == 0) return;
            }
            else if (_setIncr.Length == 0)
                return;

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
                if (_table.VersionColumn.Attribute.MapType == typeof(byte[]))
                {
                    _updateVersionValue = Utils.GuidToBytes(Guid.NewGuid());
                    sb.Append(", ").Append(vcname).Append(" = ").Append(_commonUtils.GetNoneParamaterSqlValue(_paramsSource, "uv", _table.VersionColumn, _table.VersionColumn.Attribute.MapType, _updateVersionValue));
                }
                else if (_versionColumn.Attribute.MapType == typeof(string))
                {
                    _updateVersionValue = Guid.NewGuid().ToString();
                    sb.Append(", ").Append(vcname).Append(" = ").Append(_noneParameter ? _commonUtils.GetNoneParamaterSqlValue(_paramsSource, "uv", _versionColumn, _versionColumn.Attribute.MapType, _updateVersionValue) :
                        _commonUtils.QuoteWriteParamterAdapter(_versionColumn.Attribute.MapType, _commonUtils.QuoteParamterName($"p_{_paramsSource.Count}")));
                    if (_noneParameter == false)
                        _commonUtils.AppendParamter(_paramsSource, null, _versionColumn, _versionColumn.Attribute.MapType, _updateVersionValue);
                }
                else
                    sb.Append(", ").Append(vcname).Append(" = ").Append(_commonUtils.IsNull(vcname, 0)).Append(" + 1");
            }
            ToSqlWhere(sb);
            _interceptSql?.Invoke(sb);
            return;
        }

        protected override int SplitExecuteAffrows(int valuesLimit, int parameterLimit)
        {
            var ss = base.SplitSource(valuesLimit, parameterLimit);
            var ret = 0;
            if (ss.Length <= 1)
            {
                if (_source?.Any() == true) _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = this.RawExecuteAffrows();
                ClearData();
                return ret;
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteAffrows", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                    ret += this.RawExecuteAffrows();
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }
        protected override List<T1> SplitExecuteUpdated(int valuesLimit, int parameterLimit)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = new List<T1>();
            if (ss.Length <= 1)
            {
                if (_source?.Any() == true) _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = this.RawExecuteUpdated();
                ClearData();
                return ret;
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteUpdated", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                    ret.AddRange(this.RawExecuteUpdated());
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }


#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => SplitExecuteAffrowsAsync(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        public override Task<List<T1>> ExecuteUpdatedAsync(CancellationToken cancellationToken = default) => SplitExecuteUpdatedAsync(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        protected override Task<List<T1>> RawExecuteUpdatedAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException($"FreeSql.Provider.ClickHouse {CoreStrings.S_Not_Implemented_Feature}");

        async protected override Task<int> SplitExecuteAffrowsAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = 0;
            if (ss.Length <= 1)
            {
                if (_source?.Any() == true) _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                await this.RawExecuteAffrowsAsync(cancellationToken);
                ret = _source.Count;
                ClearData();
                return ret;
            }


            var before = new Aop.TraceBeforeEventArgs("SplitExecuteAffrowsAsync", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                    await this.RawExecuteAffrowsAsync(cancellationToken);
                    ret += _source.Count;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }
        async protected override Task<List<T1>> SplitExecuteUpdatedAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = new List<T1>();
            if (ss.Length <= 1)
            {
                if (_source?.Any() == true) _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = await this.RawExecuteUpdatedAsync(cancellationToken);
                ClearData();
                return ret;
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteUpdatedAsync", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                    ret.AddRange(await this.RawExecuteUpdatedAsync(cancellationToken));
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }

#endif
    }
}
