using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using FreeSql.Aop;
using FreeSql.Provider.TDengine.Attributes;
using FreeSql.TDengine.Curd;
using Newtonsoft.Json.Linq;
using TDengine.Data.Client;

namespace FreeSql.TDengine
{
    internal class TDengineProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public TDengineProvider(string masterConnectionString, string[] slaveConnectionString,
            Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new TDengineUtils(this);
            this.InternalCommonExpression = new TDengineExpression(this.InternalCommonUtils);
            this.Ado = new TDengineAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString,
                connectionFactory);
            this.Aop = new AopProvider();
            this.DbFirst = new TDengineDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new TDengineCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            //处理超级表查询问题
            //this.Aop.ConfigEntityProperty += (s, e) =>
            //{
            //    if (e.Property.ReflectedType == null)
            //        return;

            //    if (e.Property.ReflectedType.BaseType == null)
            //        return;

            //    var propertyInfo = e.Property.ReflectedType.BaseType.GetProperty(e.Property.Name);

            //    if (propertyInfo == null)
            //        return;

            //    if (propertyInfo.GetCustomAttribute(typeof(TDengineTagAttribute)) != null)
            //        e.ModifyResult.IsIgnore = true;
            //};

            //TDengine 特殊处理:
            this.Aop.AuditDataReader += (_, e) =>
            {
                var dataTypeName = e.DataReader.GetDataTypeName(e.Index);
                switch (dataTypeName)
                {
                    case "TIMESTAMP":
                        try
                        {
                            if (e.DataReader.IsDBNull(e.Index)) e.Value = null;
                            else e.Value = e.DataReader.GetDateTime(e.Index);
                        }
                        catch
                        {
                            e.Value = DateTime.MinValue;
                        }
                        return;
                }
            };

            //处理参数化
            this.Aop.CommandBefore += (_, e) =>
            {
                if (e.Command.Parameters.Count <= 0) return;
                var dengineParameters = new TDengineParameter[e.Command.Parameters.Count];
                e.Command.Parameters.CopyTo(dengineParameters, 0);
                var cmdText = e.Command.CommandText;
                var isChanged = false;
                foreach (var parameter in dengineParameters.OrderByDescending(a => a.ParameterName.Length))
                {
                    var idx = cmdText.IndexOf(parameter.ParameterName, StringComparison.Ordinal);
                    if (idx != -1)
                    {
                        isChanged = true;
                        cmdText =
                            $"{cmdText.Substring(0, idx)}?{cmdText.Substring(idx + parameter.ParameterName.Length)}";
                    }
                }

                if (isChanged) e.Command.CommandText = cmdText;
            };
        }

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) =>
            new TDengineSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsert<T1> CreateInsertProvider<T1>() =>
            new TDengineInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);


        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere)
        {
            throw new NotImplementedException(
                $"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
        }

        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere)
        {
            return new TDengineDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        }

        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>()
        {
            throw new NotImplementedException(
                $"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
        }

        ~TDengineProvider() => this.Dispose();
        int _disposeCounter;

        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}