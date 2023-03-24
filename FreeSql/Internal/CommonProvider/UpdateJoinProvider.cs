using FreeSql;
using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{
    public class UpdateJoinProvider<T1, T2> : IUpdateJoin<T1, T2> where T1 : class where T2 : class
    {
        public IUpdate<T1> _update;
        public UpdateProvider<T1> _updateProvider;
        public ISelect<T2> _query;
        public Select0Provider _queryProvider;
        public ISelect<T1, T2> _query2;
        public Select2Provider<T1, T2> _query2Provider;

        public IFreeSql _orm;
        public CommonUtils _commonUtils;
        public CommonExpression _commonExpression;
        public string _joinOn;
        public string _tableName;

        public UpdateJoinProvider(IUpdate<T1> update, ISelect<T2> query, Expression<Func<T1, T2, bool>> on)
        {
            _update = update;
            _updateProvider = _update as UpdateProvider<T1>;
            _orm = _updateProvider._orm;
            _commonUtils = _updateProvider._commonUtils;
            _commonExpression = _updateProvider._commonExpression;

            ValidateDataType(null, null, null, null, null);
            _query = query;
            _queryProvider = _query as Select0Provider;
            _query2 = _orm.Select<T1>().DisableGlobalFilter().FromQuery(_query);
            _query2Provider = _query2 as Select2Provider<T1, T2>;

            _query2Provider._where.Clear();
            _query2.Where(on);
            _joinOn = _query2Provider._where.ToString();
            if (_joinOn.StartsWith(" AND ")) _joinOn = _joinOn.Substring(5);

            _updateProvider.Where("1=1");
        }

        public void ValidateDataType(Action InterceptSqlServer, Action InterceptMySql, Action InterceptPostgreSQL, Action InterceptMergeInto, Action InterceptGBase)
        {
            switch (_orm.Ado.DataType)
            {
                case DataType.SqlServer:
                case DataType.OdbcSqlServer:
                case DataType.CustomSqlServer:
                    InterceptSqlServer?.Invoke(); break;
                case DataType.MySql:
                case DataType.OdbcMySql:
                case DataType.CustomMySql:
                case DataType.MsAccess:
                    InterceptMySql?.Invoke(); break;
                case DataType.PostgreSQL:
                case DataType.OdbcPostgreSQL:
                case DataType.CustomPostgreSQL:
                case DataType.KingbaseES:
                case DataType.OdbcKingbaseES:
                case DataType.ShenTong:
                    InterceptPostgreSQL?.Invoke(); break;
                case DataType.Oracle:
                case DataType.OdbcOracle:
                case DataType.CustomOracle:
                case DataType.Dameng:
                case DataType.OdbcDameng:
                case DataType.Firebird:
                    InterceptMergeInto?.Invoke(); break;
                case DataType.GBase:
                    InterceptGBase?.Invoke(); break;

                default: throw new Exception($"{_orm.Ado.DataType} 暂时不支持 update join 操作。");
            }
        }

        #region proxy IUpdate
        public IUpdateJoin<T1, T2> AsTable(string tableName)
        {
            _update.AsTable(tableName);
            return this;
        }
        public IUpdateJoin<T1, T2> WithConnection(DbConnection connection)
        {
            _update.WithConnection(connection); 
            return this;
        }

        public IUpdateJoin<T1, T2> WithTransaction(DbTransaction transaction)
        {
            _update.WithTransaction(transaction);
            return this;
        }
        public IUpdateJoin<T1, T2> CommandTimeout(int timeout)
        {
            _update.CommandTimeout(timeout);
            return this;
        }
        public IUpdateJoin<T1, T2> DisableGlobalFilter(params string[] name)
        {
            _update.DisableGlobalFilter(name);
            return this;
        }
        public IUpdateJoin<T1, T2> Set<TMember>(Expression<Func<T1, TMember>> column, TMember value)
        {
            _update.Set(column, value);
            return this;
        }
        public IUpdateJoin<T1, T2> SetIf<TMember>(bool condition, Expression<Func<T1, TMember>> column, TMember value)
        {
            _update.SetIf(condition, column, value);
            return this;
        }
        public IUpdateJoin<T1, T2> SetRaw(string sql, object parms = null)
        {
            _update.SetRaw(sql, parms);
            return this;
        }
        public IUpdateJoin<T1, T2> Where(string sql, object parms = null)
        {
            _update.Where(sql, parms);
            return this;
        }
        #endregion

        public IUpdateJoin<T1, T2> Where(Expression<Func<T1, T2, bool>> exp) => WhereIf(true, exp);
        public IUpdateJoin<T1, T2> WhereIf(bool condition, Expression<Func<T1, T2, bool>> exp)
        {
            if (condition == false) return this;
            _query2Provider._where.Clear();
            _query2.Where(exp);
            _updateProvider._where.Append(_query2Provider._where);
            return this;
        }

        public IUpdateJoin<T1, T2> Set(Expression<Func<T1, T2, bool>> exp) => SetIf(true, exp);
        public IUpdateJoin<T1, T2> SetIf(bool condition, Expression<Func<T1, T2, bool>> exp)
        {
            var body = exp?.Body;
            var nodeType = body?.NodeType;
            if (nodeType == ExpressionType.Convert)
            {
                body = (body as UnaryExpression)?.Operand;
                nodeType = body?.NodeType;
            }
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    break;
                default:
                    throw new Exception("格式错了，请使用 .Set((a,b) => a.name == b.xname)");
            }

            var equalBinaryExp = body as BinaryExpression;
            var cols = new List<SelectColumnInfo>();
            _commonExpression.ExpressionSelectColumn_MemberAccess(null, null, cols, SelectTableInfoType.From, equalBinaryExp.Left, true, null);
            if (cols.Count != 1) return this;
            var col = cols[0].Column;
            var columnSql = $"{_commonUtils.QuoteSqlName(col.Attribute.Name)}";
            var valueSql = "";

            if (equalBinaryExp.Right.IsParameter())
            {
                _query2Provider._groupby = null;
                var valueExp = Expression.Lambda<Func<T1, T2, object>>(equalBinaryExp.Right, exp.Parameters);
                _query2.GroupBy(valueExp);
                valueSql = _query2Provider._groupby?.Remove(0, " \r\nGROUP BY ".Length);
            }
            else
            {
                valueSql = _commonExpression.ExpressionLambdaToSql(equalBinaryExp.Right, new CommonExpression.ExpTSC
                {
                    isQuoteName = true,
                    mapType = equalBinaryExp.Right is BinaryExpression ? null : col.Attribute.MapType
                });
            }
            if (string.IsNullOrEmpty(valueSql)) return this;

            switch (_orm.Ado.DataType)
            {
                case DataType.PostgreSQL:
                case DataType.OdbcPostgreSQL:
                case DataType.CustomPostgreSQL:
                case DataType.KingbaseES:
                case DataType.OdbcKingbaseES:
                case DataType.ShenTong:
                    break;
                default:
                    columnSql = $"{_query2Provider._tables[0].Alias}.{columnSql}";  //set a.name = b.name
                    break;
            }

            _update.SetRaw($"{columnSql} = {valueSql}");
            return this;
        }

        void InterceptSql(StringBuilder sb)
        {
            var sql = sb.ToString();
            if (!sql.StartsWith("UPDATE ")) return;
            var setStartIndex = sql.IndexOf(" SET ");
            if (setStartIndex == -1) return;
            var sqltab = sql.Substring(7, setStartIndex - 7);
            var sqlset = "";
            var sqlwhere = "";
            var sqltab2 = _query2Provider._tableRules.FirstOrDefault()?.Invoke(typeof(T2), null)?.Replace(" \r\n", " \r\n    ") ?? _commonUtils.QuoteSqlName(_query2Provider._tables[1].Table?.DbName);
            var whereStartIndex = sql.IndexOf(" \r\nWHERE ", setStartIndex);
            if (whereStartIndex == -1)
            {
                sqlset = sql.Substring(setStartIndex + 5);
            }
            else
            {
                sqlset = sql.Substring(setStartIndex + 5, whereStartIndex - setStartIndex - 5);
                sqlwhere = sql.Substring(whereStartIndex);
                if (sqlwhere == " \r\nWHERE (1=1)")
                    sqlwhere = "";
                else if (sqlwhere.StartsWith(" \r\nWHERE (1=1) AND "))
                    sqlwhere = $" \r\nWHERE {sqlwhere.Substring(" \r\nWHERE (1=1) AND ".Length)}";
            }
            string t0alias = _query2Provider._tables[0].Alias;
            string t1alias = _query2Provider._tables[1].Alias;

            ValidateDataType(InterceptSqlServer, InterceptMySql, InterceptPostgreSQL, InterceptMergeInto, InterceptGBase);
            void InterceptSqlServer()
            {
                sb.Clear().Append("UPDATE ").Append(t0alias).Append(" SET ").Append(sqlset)
                    .Append(" \r\nFROM ").Append(sqltab).Append(_commonUtils.FieldAsAlias(t0alias))
                    .Append(" \r\nINNER JOIN ").Append(sqltab2).Append(_commonUtils.FieldAsAlias(t1alias)).Append(" ON ").Append(_joinOn)
                    .Append(sqlwhere);
            }
            void InterceptMySql()
            {
                sb.Clear().Append("UPDATE ").Append(sqltab).Append(_commonUtils.FieldAsAlias(t0alias))
                    .Append(" \r\nINNER JOIN ").Append(sqltab2).Append(_commonUtils.FieldAsAlias(t1alias)).Append(" ON ").Append(_joinOn)
                    .Append(" \r\nSET ").Append(sqlset)
                    .Append(sqlwhere);
            }
            void InterceptPostgreSQL()
            {
                sb.Clear().Append("UPDATE ").Append(sqltab).Append(_commonUtils.FieldAsAlias(t0alias))
                    .Append(" \r\nSET ").Append(sqlset)
                    .Append(" \r\nFROM ").Append(sqltab2).Append(_commonUtils.FieldAsAlias(t1alias))
                    .Append(sqlwhere);
                if (string.IsNullOrEmpty(sqlwhere)) sb.Append(" \r\nWHERE ").Append(_joinOn);
                else sb.Append(" AND ").Append(_joinOn);
            }
            void InterceptMergeInto()
            {
                sb.Clear().Append("MERGE INTO ").Append(sqltab).Append(_commonUtils.FieldAsAlias(t0alias))
                    .Append(" \r\nUSING ").Append(sqltab2).Append(_commonUtils.FieldAsAlias(t1alias)).Append(" ON ").Append(_joinOn)
                    .Append(" \r\nWHEN MATCHED THEN")
                    .Append(" \r\nUPDATE SET ").Append(sqlset)
                    .Append(sqlwhere);
            }
            void InterceptGBase()
            {
                sb.Clear().Append("UPDATE ").Append(sqltab2).Append(_commonUtils.FieldAsAlias(t1alias)).Append(", ").Append(sqltab).Append(_commonUtils.FieldAsAlias(t0alias))
                    .Append(" \r\nSET ").Append(sqlset)
                    .Append(sqlwhere);
                if (string.IsNullOrEmpty(sqlwhere)) sb.Append(" \r\nWHERE ").Append(_joinOn);
                else sb.Append(" AND ").Append(_joinOn);
            }
        }

        public string ToSql()
        {
            _updateProvider._interceptSql = InterceptSql;
            try
            {
                return _update.ToSql();
            }
            finally
            {
                _updateProvider._interceptSql = null;
            }
        }
        public int ExecuteAffrows()
        {
            _updateProvider._interceptSql = InterceptSql;
            try
            {
                return _update.ExecuteAffrows();
            }
            finally
            {
                _updateProvider._interceptSql = null;
            }
        }
#if net40
#else
        async public Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            _updateProvider._interceptSql = InterceptSql;
            try
            {
                return await _update.ExecuteAffrowsAsync(cancellationToken);
            }
            finally
            {
                _updateProvider._interceptSql = null;
            }
        }
#endif
    }
}
