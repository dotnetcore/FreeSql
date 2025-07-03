using FreeSql.Internal.CommonProvider;
using FreeSql.Oracle.Curd;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace FreeSql.Oracle
{

	public class OracleProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new OracleSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new OracleInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new OracleUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new OracleDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new OracleInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public OracleProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new OracleUtils(this);
            this.InternalCommonExpression = new OracleExpression(this.InternalCommonUtils);

            this.Ado = new OracleAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new OracleDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new OracleCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

#if oledb
            Select0Provider._dicMethodDataReaderGetValueOverride[DataType.Oracle] = new Dictionary<Type, MethodInfo>();
            Select0Provider._dicMethodDataReaderGetValueOverride[DataType.Oracle][typeof(Guid)] = typeof(DbDataReader).GetMethod("GetGuid", new Type[] { typeof(int) });
            Select0Provider._dicMethodDataReaderGetValueOverride[DataType.Oracle][typeof(bool)] = typeof(DbDataReader).GetMethod("GetBoolean", new Type[] { typeof(int) });
            Select0Provider._dicMethodDataReaderGetValueOverride[DataType.Oracle][typeof(int)] = typeof(DbDataReader).GetMethod("GetDecimal", new Type[] { typeof(int) });
            Select0Provider._dicMethodDataReaderGetValueOverride[DataType.Oracle][typeof(long)] = typeof(DbDataReader).GetMethod("GetDecimal", new Type[] { typeof(int) });
            Select0Provider._dicMethodDataReaderGetValueOverride[DataType.Oracle][typeof(decimal)] = typeof(DbDataReader).GetMethod("GetDecimal", new Type[] { typeof(int) });
            Select0Provider._dicMethodDataReaderGetValueOverride[DataType.Oracle][typeof(DateTime)] = typeof(DbDataReader).GetMethod("GetDateTime", new Type[] { typeof(int) });
            Select0Provider._dicMethodDataReaderGetValueOverride[DataType.Oracle][typeof(string)] = typeof(DbDataReader).GetMethod("GetString", new Type[] { typeof(int) });

            this.Aop.CommandBefore += (_, e) =>
            {
                if (e.Command.Parameters.Count > 0)
                {
                    var dbparms = new DbParameter[e.Command.Parameters.Count];
                    e.Command.Parameters.CopyTo(dbparms, 0);
                    var cmdText = e.Command.CommandText;
                    var ischanged = false;
                    foreach(var dbparm in dbparms.OrderByDescending(a => a.ParameterName.Length))
                    {
                        if (dbparm.ParameterName[0] != ':') continue;
                        var idx = cmdText.IndexOf(dbparm.ParameterName);
                        if (idx != -1)
                        {
                            ischanged = true;
							cmdText = $"{cmdText.Substring(0, idx)}?{cmdText.Substring(idx + dbparm.ParameterName.Length)}";
                        }
                    }
                    if (ischanged) e.Command.CommandText = cmdText;
                }
            };

            this.Aop.AuditDataReader += (_, e) =>
            {
                
            };
        }
        readonly static Regex _regCommandText = new Regex(@"\:[_\w]+", RegexOptions.Compiled);
#else
        }
#endif

        ~OracleProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
