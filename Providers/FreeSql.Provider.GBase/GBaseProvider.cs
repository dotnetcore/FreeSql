using FreeSql.GBase.Curd;
using FreeSql.Internal.CommonProvider;
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace FreeSql.GBase
{

    public class GBaseProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new GBaseSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new GBaseInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new GBaseUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new GBaseDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new GBaseInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public GBaseProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new GBaseUtils(this);
            this.InternalCommonExpression = new GBaseExpression(this.InternalCommonUtils);

            this.Ado = new GBaseAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new GBaseDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new GBaseCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            this.Aop.CommandBefore += (_, e) =>
            {
                if (e.Command.CommandType == CommandType.StoredProcedure)
                {
                    if (e.Command.CommandText.Trim().StartsWith("{call ", StringComparison.OrdinalIgnoreCase) == false)
                    {
                        var args = string.Join(", ", Enumerable.Range(0, e.Command.Parameters.Count)
                            .Select(a => "?"));
                        var cmdText = $"{{call {e.Command.CommandText}({args})}}";

                        foreach (DbParameter parameter in e.Command.Parameters)
                        {
                            if (parameter.DbType == DbType.String && parameter.Size <= 0)
                                parameter.Size = 255;
                        }
                        e.Command.CommandText = cmdText;
                    }
                }
            };

            this.Aop.AuditDataReader += (_, e) =>
            {
                var dbtype = e.DataReader.GetDataTypeName(e.Index);
                switch (dbtype)
                {
                    case "CHAR":
                    case "VARCHAR":
                    case "BOOLEAN":
                    case "SMALLINT":
                    case "INTEGER":
                    case "DECIMAL":
                    case "FLOAT":
                    case "SMALLFLOAT":
                        return;
                    case "BIGINT":
                        //Unkonw SQL type -- 114.
                        try
                        {
                            e.Value = e.DataReader.GetInt64(e.Index);
                            return;
                        }
                        catch
                        {
                            e.Value = e.DataReader.GetValue(e.Index);
                            if (e.Value == DBNull.Value) e.Value = null;
                            return;
                        }
                    case "BLOB":
                        //Unkonw SQL type -- 102.
                        Stream stm = null;
                        try
                        {
                            stm = e.DataReader.GetStream(e.Index);
                        }
                        catch
                        {
                            e.Value = e.DataReader.GetValue(e.Index);
                            if (e.Value == DBNull.Value) e.Value = null;
                            return;
                        }

                        using (var ms = new MemoryStream())
                        {
                            var stmbuf = new byte[1];
                            while (true)
                            {
                                if (stm.Read(stmbuf, 0, 1) <= 0) break;
                                ms.Write(stmbuf, 0, 1);
                            }
                            e.Value = ms.ToArray();
                            ms.Close();
                        }
                        return;
                }
                if (dbtype.StartsWith("INTERVAL DAY"))
                {
                    //INTERVAL DAY(3) TO FRACTION(3)
                    //异常：Unknown SQL type - 110.
                    var tsv = "";
                    try
                    {
                        tsv = e.DataReader.GetString(e.Index)?.Trim().Replace(' ', ':');
                    }
                    catch
                    {
                        e.Value = e.DataReader.GetValue(e.Index);
                        if (e.Value == DBNull.Value) e.Value = null;
                        return;
                    }
                    e.Value = TimeSpan.Parse(tsv);
                    return;
                }
            };
        }

        ~GBaseProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}


//--== GBase 8s Information for this install ==--
//$GBASEDBTSERVER : gbase01
//$GBASEDBTDIR    : /opt/gbase
//USER HOME       : /home/gbase
//DBSPACE DIR     : /data/gbase
//IP ADDRESS      : 192.168.164.134 127.0.0.1 
//PORT NUMBER     : 9088
//$DB_LOCALE      : zh_CN.utf8
//$CLIENT_LOCALE  : zh_CN.utf8
//JDBC URL        : jdbc:gbasedbt-sqli://IPADDR:9088/testdb:GBASEDBTSERVER=gbase01;DB_LOCALE=zh_CN.utf8;CLIENT_LOCALE=zh_CN.utf8;IFX_LOCK_MODE_WAIT=10
//JDBC USERNAME   : gbasedbt
//JDBC PASSWORD   : GBase123