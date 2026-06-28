using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Provider.TDengine.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.TDengine.Curd
{
    class TDengineInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public TDengineInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        internal bool InternalIsIgnoreInto = false;
        internal IFreeSql InternalOrm => _orm;
        internal TableInfo InternalTable => _table;
        internal DbParameter[] InternalParams => _params;
        internal DbConnection InternalConnection => _connection;
        internal DbTransaction InternalTransaction => _transaction;
        internal CommonUtils InternalCommonUtils => _commonUtils;
        internal CommonExpression InternalCommonExpression => _commonExpression;
        internal List<T1> InternalSource => _source;
        internal Dictionary<string, bool> InternalIgnore => _ignore;
        internal void InternalClearData() => ClearData();

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(
            _batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);

        public override long ExecuteIdentity() => base.SplitExecuteIdentity(
            _batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);

        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(
            _batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);


        public override string ToSql()
        {
            if (_customTableRule != null)
            {
                return ToSTableBatchInsertSql();
            }
            else
            {
                //处理Insert忽略Tag
                var ignoreColumnList = _ignoreInsertColumns.GetOrAdd(typeof(T1), s =>
                {
                    //如果是超表不处理
                    if (!s.IsDefined(typeof(TDengineSubTableAttribute))) return new List<string>(0);
                    var tableByEntity = _commonUtils.GetTableByEntity(s);
                    var keyValuePairs = tableByEntity.Properties.Where(pair =>
                        pair.Value.GetCustomAttribute<TDengineTagAttribute>() != null);
                    return keyValuePairs.Select(keyValuePair => keyValuePair.Value.Name).ToList();
                });
                if (InternalIsIgnoreInto == false) return base.ToSqlValuesOrSelectUnionAll(ignoreColumn: ignoreColumnList);
                var sql = base.ToSqlValuesOrSelectUnionAll(ignoreColumn: ignoreColumnList);
                return $"INSERT IGNORE INTO {sql.Substring(12)}";
            }
        }

        /// <summary>
        /// Key 为 T1 Type，Value 为 Tuple&lt;超级表名, Tag属性数组, Value属性数组&gt;
        /// </summary>
        private static ConcurrentDictionary<Type, Tuple<string, ColumnInfo[], ColumnInfo[]>> _superTableColumns =
            new ConcurrentDictionary<Type, Tuple<string, ColumnInfo[], ColumnInfo[]>>();

        private string ToSTableBatchInsertSql()
        {
            if (_source == null || _source.Count == 0) return null;

            var typePropertiesCached = _superTableColumns.GetOrAdd(typeof(T1), s =>
            {
                var tableInfo = _commonUtils.GetTableByEntity(s);
                var stableName = tableInfo.DbName;
                var tagColumnNames = tableInfo.Properties
                    .Where(kvp => kvp.Value.GetCustomAttribute<TDengineTagAttribute>() != null)
                    .Select(kvp => kvp.Value.Name).ToList();
                var tagColumns = tableInfo.Columns.Values
                    .Where(col => tagColumnNames.Contains(col.CsName))
                    .ToArray();

                var valueColumns = tableInfo.Columns.Values
                    .Except(tagColumns)
                    .Where(col => !_ignore.ContainsKey(col.CsName))
                    .ToArray();              
                return new Tuple<string, ColumnInfo[], ColumnInfo[]>(stableName, tagColumns, valueColumns);
            });

            var tagNames = string.Join(",", typePropertiesCached.Item2.Select(x => x.Attribute.Name));
            var valueColumnNames = string.Join(",", typePropertiesCached.Item3.Select(x => x.Attribute.Name));
            var groups = _source.GroupBy(_customTableRule);
            var sql = new StringBuilder();
            sql.Append("INSERT INTO ");
            foreach (var group in groups)
            {
                string childTableName = group.Key;
                var batchList = group.ToList();
                // 2. 取第一条数据（用于生成 TAGS）
                var first = batchList.First();
                var tagValues = typePropertiesCached.Item2
                    .Select(p => FormatValue(p.GetDbValue(first)))
                    .ToArray();
                var itemValueStringList = new List<string>();
                foreach (var item in batchList)
                {
                    var itemValues = typePropertiesCached.Item3.Select(p => FormatValue(p.GetDbValue(item))).ToArray();
                    var valueString = string.Join(", ", itemValues);
                    itemValueStringList.Add($"({valueString})");
                }

                var values = string.Join(" ", itemValueStringList);
                // TODO 待完善
                // 4. 拼 TDengine SQL
                sql.AppendLine().Append($"{childTableName} ") //子表名，不需要使用`包裹，可以忽略大小写
                          .Append($"USING {typePropertiesCached.Item1} ") //超级表名
                          .Append($"({tagNames}) ") //超级表名
                          .Append($"tags({string.Join(", ", tagValues)}) ") //子表tags
                          .AppendLine($"({valueColumnNames})") //除Tags和Ignored外的字段，不需要使用`包裹，可以忽略大小写
                          .Append($"values {values}");
            }
            return sql.ToString();
        }

        private static string FormatValue(object v)
        {
            if (v == null)
                return "NULL";

            if (v is string s)
            {
                return "'" + s.Replace("'", "''") + "'";
            }

            if (v is DateTime d)
            {
                return $"'{d:O}'";
            }

            return v.ToString();
        }

        private static ConcurrentDictionary<Type, List<string>> _ignoreInsertColumns =
            new ConcurrentDictionary<Type, List<string>>();

        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            sql = string.Concat(sql, "; SELECT LAST_INSERT_ID();");
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                ret = long.TryParse(
                    string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, sql,
                        _commandTimeout, _params)), out var trylng)
                    ? trylng
                    : 0;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }

            return ret;
        }

        protected override List<T1> RawExecuteInserted()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ")
                    .Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }

            sql = sb.ToString();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = _orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text,
                    sql, _commandTimeout, _params);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }

            return ret;
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) =>
            base.SplitExecuteAffrowsAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);

        public override Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default) =>
            base.SplitExecuteIdentityAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);

        public override Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default) =>
            base.SplitExecuteInsertedAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);

        protected override async Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            sql = string.Concat(sql, "; SELECT LAST_INSERT_ID();");
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                ret = long.TryParse(
                    string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, sql,
                        _commandTimeout, _params, cancellationToken)), out var trylng)
                    ? trylng
                    : 0;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }

            return ret;
        }

        protected override async Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ")
                    .Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }

            sql = sb.ToString();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = await _orm.Ado.QueryAsync<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction,
                    CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }

            return ret;
        }
#endif
    }
}