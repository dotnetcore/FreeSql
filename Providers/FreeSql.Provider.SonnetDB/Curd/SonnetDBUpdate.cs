using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SonnetDB.Sql;

namespace FreeSql.SonnetDB.Curd
{
    /// <summary>
    /// 关系表 UPDATE，以及时序测量的单个 VECTOR FIELD 替换。
    /// </summary>
    class SonnetDBUpdate<T1> : UpdateProvider<T1>, IUpdate<T1>
    {
        ColumnInfo _versionForConcurrency;
        bool _suppressVersionAssignment;
        string _lastSql;
        DbParameter[] _lastDbParams;

        public SonnetDBUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
            // Update<object>() 是 FreeSql 支持的动态实体入口，必须等 AsType
            // 设置真实模型后再校验；其他实体仍在创建时立即拒绝测量模型。
            // Dictionary<string, object> 的 TableInfo 要等 SetSource 后才能构建，
            // 因此与 Update<object>() 一样延迟到生成 SQL 时校验。
            if (typeof(T1) != typeof(object) &&
                typeof(T1) != typeof(Dictionary<string, object>))
                EnsureUpdateModel(_table);
            // ROWVERSION 由 SonnetDB 维护。保留 FreeSql 生成的乐观锁条件，
            // 但绝不输出对该列的赋值。
            if (_table?.VersionColumn != null)
                _ignore[_table.VersionColumn.Attribute.Name] = true;
        }

        public new IUpdate<T1> AsType(Type entityType)
        {
            EnsureAsTypeTarget(entityType);
            var result = base.AsType(entityType);
            if (_table?.Type != typeof(object))
                EnsureUpdateModel(_table);
            return result;
        }

        // UpdateProvider<T1> 已实现同一接口；这里重新映射接口入口，确保
        // 通过 IUpdate<T1> 调用 AsType 时也先执行 SonnetDB 模型校验。
        IUpdate<T1> IUpdate<T1>.AsType(Type entityType) => AsType(entityType);

        // The shared implementation resolves selected names only through
        // ColumnsByCs. A renamed Column can be returned by its database name;
        // retain it when selecting a subset for RETURNING.
        public new List<TReturn> ExecuteUpdated<TReturn>(Expression<Func<T1, TReturn>> returnColumns) =>
            ExecuteUpdated<TReturn>(GetReturningColumns(returnColumns));

        public new Task<List<TReturn>> ExecuteUpdatedAsync<TReturn>(Expression<Func<T1, TReturn>> returnColumns,
            CancellationToken cancellationToken = default) => ExecuteUpdatedAsync<TReturn>(GetReturningColumns(returnColumns), cancellationToken);

        ColumnInfo[] GetReturningColumns<TReturn>(Expression<Func<T1, TReturn>> returnColumns) =>
            _commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, null, returnColumns?.Body, false, null)
                .Select(name => _table.ColumnsByCs.TryGetValue(name, out var column) ? column :
                    _table.Columns.TryGetValue(name, out column) ? column : null).Where(column => column != null).ToArray();

        void EnsureAsTypeTarget(Type entityType)
        {
            if (entityType == null || entityType == typeof(object)) return;
            var target = _commonUtils.GetTableByEntity(entityType);
            if (target != null) EnsureUpdateModel(target);
        }

        public override int ExecuteAffrows() =>
            ExecuteAffrowsCore();

        int ExecuteAffrowsCore()
        {
            EnsureUpdateOperation();
            var expectedVersionRows = _table?.VersionColumn != null && _source?.Count > 0
                ? _source.Count
                : 0;
            _lastSql = null;
            _lastDbParams = null;
            try
            {
                var affrows = base.SplitExecuteAffrows(_batchRowsLimit > 0 ? _batchRowsLimit : 500,
                    _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
                ValidateVersionRows(expectedVersionRows, affrows);
                return affrows;
            }
            catch (Exception ex)
            {
                var converted = ConvertConcurrencyConflict(ex);
                if (!ReferenceEquals(converted, ex)) throw converted;
                throw;
            }
        }

        public override string ToSql()
        {
            EnsureUpdateOperation();
            var sql = base.ToSql();
            if (_table?.Primarys == null || _table.Primarys.Length == 0) return sql;
            var projection = _table.Primarys.Length == 1
                ? "ftb_upd." + _commonUtils.QuoteSqlName(_table.Primarys[0].Attribute.Name)
                : "ftb_upd.as1";
            return SonnetDBMutationSql.RewritePrimaryKeySubquery(sql, projection);
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            EnsureUpdateOperation();
            return ExecuteAffrowsAsyncCore(cancellationToken);
        }

        async Task<int> ExecuteAffrowsAsyncCore(CancellationToken cancellationToken)
        {
            var expectedVersionRows = _table?.VersionColumn != null && _source?.Count > 0
                ? _source.Count
                : 0;
            _lastSql = null;
            _lastDbParams = null;
            try
            {
                var affrows = await base.SplitExecuteAffrowsAsync(_batchRowsLimit > 0 ? _batchRowsLimit : 500,
                    _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
                ValidateVersionRows(expectedVersionRows, affrows);
                return affrows;
            }
            catch (Exception ex)
            {
                var converted = ConvertConcurrencyConflict(ex);
                if (!ReferenceEquals(converted, ex)) throw converted;
                throw;
            }
        }

        protected override List<TReturn> ExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns)
        {
            SonnetDBModel.EnsureTable(_table, "UPDATE ... RETURNING");
            _lastSql = null;
            _lastDbParams = null;
            try
            {
                return base.SplitExecuteUpdated<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : 500,
                    _batchParameterLimit > 0 ? _batchParameterLimit : 3000, columns);
            }
            catch (Exception ex)
            {
                var converted = ConvertConcurrencyConflict(ex);
                if (!ReferenceEquals(converted, ex)) throw converted;
                throw;
            }
        }

        protected override async Task<List<TReturn>> ExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns,
            CancellationToken cancellationToken = default)
        {
            SonnetDBModel.EnsureTable(_table, "UPDATE ... RETURNING");
            _lastSql = null;
            _lastDbParams = null;
            try
            {
                return await base.SplitExecuteUpdatedAsync<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : 500,
                    _batchParameterLimit > 0 ? _batchParameterLimit : 3000, columns, cancellationToken);
            }
            catch (Exception ex)
            {
                var converted = ConvertConcurrencyConflict(ex);
                if (!ReferenceEquals(converted, ex)) throw converted;
                throw;
            }
        }
#endif

        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys)
        {
            // 单主键和复合主键均使用搜索式 CASE 的逐列条件，避免拼接主键值
            // 造成冲突，或把原有键转换成另一种类型。
        }

        protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d)
        {
            for (var index = 0; index < primarys.Length; index++)
            {
                if (index > 0) sb.Append(" AND ");
                var pk = primarys[index];
                var value = _commonUtils.FormatSql("{0}", pk.GetDbValue(d));
                sb.Append(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name)))
                    .Append(" = ").Append(value);
            }
        }

        public override void ToSqlExtension110(StringBuilder sb, bool isAsTableSplited)
        {
            // AsType 可以在构造后替换 _table，因此每次生成 SQL 都要依据当前
            // 映射重新校验，不能只依赖构造函数中的检查。
            EnsureUpdateOperation();

            // FreeSql 允许显式调用 Set(x => x.Version, ...)，但 SonnetDB 的
            // ROWVERSION 只能由数据库生成。先检查并拒绝该赋值，避免通用
            // 提供程序输出无效 SQL；SetSource 更新仍保留正常的乐观锁条件
            // 和更新后的版本号递增处理。
            if (_table.VersionColumn != null)
            {
                var versionName = _table.VersionColumn.Attribute.Name;
                if (AssignsColumn(_set.ToString(), versionName) || AssignsColumn(_setIncr.ToString(), versionName))
                {
                    throw new NotSupportedException(
                        "SonnetDB ROWVERSION 由数据库自动生成，FreeSql UPDATE 不能为其赋值。");
                }
            }

            // FreeSql 通用实现会把 IsVersion 列追加为“旧值 + 1”。
            // SonnetDB 在数据库端生成 ROWVERSION，因此只暂时屏蔽这段赋值，
            // 再由 ToSqlWhere 保留乐观锁条件；不修改共享 ColumnInfo 元数据。
            var previousVersion = _versionColumn;
            var previousConcurrencyVersion = _versionForConcurrency;
            var previousSuppress = _suppressVersionAssignment;
            _versionForConcurrency = previousVersion;
            _versionColumn = null;
            _suppressVersionAssignment = true;
            try
            {
                base.ToSqlExtension110(sb, isAsTableSplited);
                ValidateMeasurementAssignments(sb.ToString());
                if (_table?.Primarys == null || _table.Primarys.Length == 0)
                {
                    _lastSql = sb.ToString();
                    _lastDbParams = _params?.Concat(_paramsSource ?? new List<DbParameter>()).ToArray();
                    return;
                }
                var projection = _table.Primarys.Length == 1
                    ? "ftb_upd." + _commonUtils.QuoteSqlName(_table.Primarys[0].Attribute.Name)
                    : "ftb_upd.as1";
                var rewritten = SonnetDBMutationSql.RewritePrimaryKeySubquery(sb.ToString(), projection);
                sb.Clear().Append(rewritten);
                _lastSql = sb.ToString();
                _lastDbParams = _params.Concat(_paramsSource).ToArray();
            }
            finally
            {
                _versionColumn = previousVersion;
                _versionForConcurrency = previousConcurrencyVersion;
                _suppressVersionAssignment = previousSuppress;
            }
        }

        static bool AssignsColumn(string assignments, string columnName)
        {
            if (string.IsNullOrWhiteSpace(assignments)) return false;
            var tokens = SqlLexer.Tokenize(assignments);
            var depth = 0;
            var assignmentStart = true;
            for (var index = 0; index < tokens.Count - 1; index++)
            {
                var token = tokens[index];
                if (token.Kind == TokenKind.LeftParen || token.Kind == TokenKind.LeftBracket) depth++;
                else if (token.Kind == TokenKind.RightParen || token.Kind == TokenKind.RightBracket) depth--;
                else if (depth == 0 && token.Kind == TokenKind.Comma)
                {
                    assignmentStart = true;
                    continue;
                }
                if (depth == 0 && assignmentStart && token.Kind == TokenKind.IdentifierLiteral &&
                    tokens[index + 1].Kind == TokenKind.Equal &&
                    string.Equals(token.Text, columnName, StringComparison.OrdinalIgnoreCase)) return true;
                assignmentStart = false;
            }
            return false;
        }

        public override void ToSqlWhere(StringBuilder sb)
        {
            base.ToSqlWhere(sb);
            if (!_suppressVersionAssignment || _versionForConcurrency == null) return;

            var versionCondition = WhereCaseSource(_versionForConcurrency.CsName, sqlValue => sqlValue);
            if (!string.IsNullOrEmpty(versionCondition))
                sb.Append(" AND ").Append(versionCondition);
        }

        protected override List<TReturn> RawExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns)
        {
            SonnetDBModel.EnsureTable(_table, "UPDATE ... RETURNING");
            var result = new List<TReturn>();
            var returning = BuildReturningClause(columns);
            ToSqlFetch(sb =>
            {
                var sql = sb.Append(returning).ToString();
                var parameters = _params.Concat(_paramsSource).ToArray();
                _lastSql = sql;
                _lastDbParams = parameters;
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, parameters);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
                Exception exception = null;
                try
                {
                    var queryType = typeof(TReturn) == typeof(T1) ? (_table.TypeLazy ?? _table.Type) : null;
                    var rows = SonnetDBReturning.Query<TReturn>(_orm, _commonUtils, _table, queryType, _connection, _transaction,
                        sql, _commandTimeout, parameters);
                    ValidateVersionAndThrow(rows.Count, sql, parameters);
                    result.AddRange(rows);
                }
                catch (Exception ex) { exception = ex; throw; }
                finally
                {
                    _orm.Aop.CurdAfterHandler?.Invoke(this, new Aop.CurdAfterEventArgs(before, exception, result));
                }
            });
            return result;
        }

        string BuildReturningClause(IEnumerable<ColumnInfo> columns) =>
            " RETURNING " + string.Join(", ", columns.Select(column => _commonUtils.QuoteSqlName(column.Attribute.Name)));

        // UpdateProvider 的异步基类实现会调用上面的提供程序重写。
        // 在抽象成员被编译到异步部分类的目标框架中，显式保留此实现。
#if net40
#else
        protected override async Task<List<TReturn>> RawExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns,
            CancellationToken cancellationToken = default)
        {
            SonnetDBModel.EnsureTable(_table, "UPDATE ... RETURNING");
            var result = new List<TReturn>();
            var returning = BuildReturningClause(columns);
            await ToSqlFetchAsync(async sb =>
            {
                var sql = sb.Append(returning).ToString();
                var parameters = _params.Concat(_paramsSource).ToArray();
                _lastSql = sql;
                _lastDbParams = parameters;
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, parameters);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
                Exception exception = null;
                try
                {
                    var queryType = typeof(TReturn) == typeof(T1) ? (_table.TypeLazy ?? _table.Type) : null;
                    var rows = await SonnetDBReturning.QueryAsync<TReturn>(_orm, _commonUtils, _table, queryType, _connection, _transaction,
                        sql, _commandTimeout, parameters, cancellationToken);
                    ValidateVersionAndThrow(rows.Count, sql, parameters);
                    result.AddRange(rows);
                }
                catch (Exception ex) { exception = ex; throw; }
                finally
                {
                    _orm.Aop.CurdAfterHandler?.Invoke(this, new Aop.CurdAfterEventArgs(before, exception, result));
                }
            });
            return result;
        }
#endif

        void EnsureUpdateOperation()
        {
            EnsureUpdateModel(_table);
            if (!SonnetDBModel.IsMeasurement(_table)) return;
            if (_transaction != null || _orm.Ado.TransactionCurrentThread != null)
                throw new NotSupportedException("SonnetDB 时序测量 VECTOR UPDATE 不能在事务中执行。");
            _batchAutoTransaction = false;
        }

        static void EnsureUpdateModel(TableInfo table)
        {
            if (SonnetDBModel.IsTable(table)) return;
            if (table?.Columns.Values.Any(IsSupportedMeasurementField) == true) return;
            throw new NotSupportedException("SonnetDB 时序测量 UPDATE 仅支持已声明的 INT/FLOAT 或 VECTOR FIELD；TAG、time、STRING FIELD 和其它类型不支持更新。");
        }

        static bool IsVectorColumn(ColumnInfo column) =>
            column.Attribute.MapType.NullableTypeOrThis() == typeof(float[]) ||
            (column.Attribute.DbType ?? string.Empty).IndexOf("VECTOR", StringComparison.OrdinalIgnoreCase) >= 0;

        static bool IsSupportedMeasurementField(ColumnInfo column)
        {
            if (column == null || IsVectorColumn(column)) return IsVectorColumn(column);
            var dbType = column.Attribute?.DbType ?? column.DbTypeText ?? string.Empty;
            if (dbType.IndexOf("TAG", StringComparison.OrdinalIgnoreCase) >= 0 ||
                dbType.IndexOf("TIME", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            var type = column.Attribute?.MapType?.NullableTypeOrThis() ?? column.CsType?.NullableTypeOrThis();
            return type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) ||
                type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) ||
                type == typeof(float) || type == typeof(double);
        }

        void ValidateMeasurementAssignments(string sql)
        {
            if (!SonnetDBModel.IsMeasurement(_table) || string.IsNullOrWhiteSpace(sql)) return;
            var update = global::SonnetDB.Sql.SqlParser.Parse(sql) as global::SonnetDB.Sql.Ast.UpdateStatement;
            if (update == null || update.Assignments.Count != 1 ||
                !_table.Columns.TryGetValue(update.Assignments[0].ColumnName, out var column) || !IsSupportedMeasurementField(column))
                throw new NotSupportedException("SonnetDB 时序测量 UPDATE 每次只能修改一个已声明的 INT/FLOAT 或 VECTOR FIELD；TAG、time、STRING FIELD 和其它类型不支持更新。");
        }

        Exception ConvertConcurrencyConflict(Exception exception)
        {
            if (!IsConcurrencyConflict(exception)) return exception;

            // SonnetDB.Data 3.1 的 ADO 层会把执行异常包装成普通 Exception，
            // 但保留带 ErrorCode 的 TableConstraintException 作为 InnerException。
            // 还原 FreeSql 的标准乐观锁异常，调用方即可沿用既有重试/冲突处理。
            var source = _source == null
                ? Enumerable.Empty<object>()
                : _source.Cast<object>().ToArray();
            return new DbUpdateVersionException(
                "SonnetDB 关系表更新发生乐观锁冲突：提交的 ROWVERSION 已过期，请重新读取后重试。",
                _table,
                _lastSql,
                _lastDbParams,
                0,
                source);
        }

        void ValidateVersionRows(int expectedRows, int affrows)
        {
            if (expectedRows <= 0 || affrows == expectedRows) return;
            throw new DbUpdateVersionException(
                $"SonnetDB 关系表更新发生乐观锁冲突：期望更新 {expectedRows} 行，实际更新 {affrows} 行，请重新读取后重试。",
                _table,
                _lastSql,
                _lastDbParams,
                affrows,
                _source?.Cast<object>() ?? Enumerable.Empty<object>());
        }

        static bool IsConcurrencyConflict(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                var type = current.GetType();
                if (!string.Equals(type.FullName, "SonnetDB.Tables.TableConstraintException", StringComparison.Ordinal))
                    continue;

                var errorCode = type.GetProperty("ErrorCode")?.GetValue(current, null) as string;
                if (string.Equals(errorCode, "table_concurrency_conflict", StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }
}
