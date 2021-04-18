using FreeSql.Internal.CommonProvider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.Model
{
    public class AdoCommandFluent
    {
        internal protected AdoProvider Ado { get; protected set; }
        internal protected DbConnection Connection { get; protected set; }
        internal protected DbTransaction Transaction { get; protected set; }
        internal protected CommandType CmdType { get; protected set; } = System.Data.CommandType.Text;
        internal protected string CmdText { get; protected set; }
        internal protected int CmdTimeout { get; protected set; }
        internal protected List<DbParameter> CmdParameters { get; } = new List<DbParameter>();

        public AdoCommandFluent(AdoProvider ado, string commandText, object parms)
        {
            this.Ado = ado;
            this.CmdText = commandText;
            this.CmdParameters.AddRange(this.Ado.GetDbParamtersByObject(parms));
        }

        /// <summary>
        /// 使用指定 DbConnection 连接执行
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public AdoCommandFluent WithConnection(DbConnection conn)
        {
            this.Transaction = null;
            this.Connection = conn;
            return this;
        }
        /// <summary>
        /// 使用指定 DbTransaction 事务执行
        /// </summary>
        /// <param name="tran"></param>
        /// <returns></returns>
        public AdoCommandFluent WithTransaction(DbTransaction tran)
        {
            this.Transaction = tran;
            if (tran != null) this.Connection = tran.Connection;
            return this;
        }

        /// <summary>
        /// 增加参数化对象
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="value">参数值</param>
        /// <param name="modify">修改本次创建好的参数化对象，比如将 parameterName 参数修改为 Output 类型</param>
        /// <returns></returns>
        public AdoCommandFluent WithParameter(string parameterName, object value, Action<DbParameter> modify = null)
        {
            var param = this.Ado.GetDbParamtersByObject(new Dictionary<string, object> { [parameterName] = value }).FirstOrDefault();
            if (CmdType == System.Data.CommandType.StoredProcedure) param.ParameterName = parameterName; //#739
            modify?.Invoke(param);
            this.CmdParameters.Add(param);
            return this;
        }

        /// <summary>
        /// 设置执行的命令类型，SQL文本、或存储过程
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public AdoCommandFluent CommandType(CommandType commandType)
        {
            this.CmdType = commandType;
            return this;
        }
        /// <summary>
        /// 设置命令执行超时（秒）
        /// </summary>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public AdoCommandFluent CommandTimeout(int commandTimeout)
        {
            this.CmdTimeout = commandTimeout;
            return this;
        }

        public int ExecuteNonQuery() => this.Ado.ExecuteNonQuery(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray());
        public object ExecuteScalar() => this.Ado.ExecuteScalar(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray());
        public DataTable ExecuteDataTable() => this.Ado.ExecuteDataTable(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray());
        public DataSet ExecuteDataSet() => this.Ado.ExecuteDataSet(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray());
        public object[][] ExecuteArray() => this.Ado.ExecuteArray(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray());
        public List<T> Query<T>() => this.Ado.Query<T>(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray());
        public T QuerySingle<T>() => Query<T>().FirstOrDefault();
        public NativeTuple<List<T1>, List<T2>> Query<T1, T2>() => this.Ado.Query<T1, T2>(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray());
        public NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>() => this.Ado.Query<T1, T2, T3>(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray());
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>() => this.Ado.Query<T1, T2, T3, T4>(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray());
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>() => this.Ado.Query<T1, T2, T3, T4, T5>(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray());

#if net40
#else
        public Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default) => this.Ado.ExecuteNonQueryAsync(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray(), cancellationToken);
        public Task<object> ExecuteScalarAsync(CancellationToken cancellationToken = default) => this.Ado.ExecuteScalarAsync(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray(), cancellationToken);
        public Task<DataTable> ExecuteDataTableAsync(CancellationToken cancellationToken = default) => this.Ado.ExecuteDataTableAsync(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray(), cancellationToken);
        public Task<DataSet> ExecuteDataSetAsync(CancellationToken cancellationToken = default) => this.Ado.ExecuteDataSetAsync(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray(), cancellationToken);
        public Task<object[][]> ExecuteArrayAsync(CancellationToken cancellationToken = default) => this.Ado.ExecuteArrayAsync(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray(), cancellationToken);
        public Task<List<T>> QueryAsync<T>(CancellationToken cancellationToken = default) => this.Ado.QueryAsync<T>(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray(), cancellationToken);
        async public Task<T> QuerySingleAsync<T>(CancellationToken cancellationToken = default) => (await QueryAsync<T>(cancellationToken)).FirstOrDefault();
        public Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(CancellationToken cancellationToken = default) => this.Ado.QueryAsync<T1, T2>(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray(), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(CancellationToken cancellationToken = default) => this.Ado.QueryAsync<T1, T2, T3>(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray(), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(CancellationToken cancellationToken = default) => this.Ado.QueryAsync<T1, T2, T3, T4>(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray(), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(CancellationToken cancellationToken = default) => this.Ado.QueryAsync<T1, T2, T3, T4, T5>(this.Connection, this.Transaction, this.CmdType, this.CmdText, this.CmdTimeout, this.CmdParameters.ToArray(), cancellationToken);
#endif
    }
}
