﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using System.Linq.Expressions;
using System.Runtime;
using FreeSql.Internal.Model.Interface;
using System.Threading;
using FreeSql.Internal.Model;
using FreeSql.Interface;

namespace FreeSql
{
    public partial class FreeSqlBuilder
    {
        DataType _dataType;
        string _masterConnectionString;
        string[] _slaveConnectionString;
        int[] _slaveWeights;
        Func<DbConnection> _connectionFactory;
        bool _isAutoSyncStructure = false;
        bool _isConfigEntityFromDbFirst = false;
        bool _isNoneCommandParameter = false;
        bool _isGenerateCommandParameterWithLambda = false;
        bool _isLazyLoading = false;
        bool _isExitAutoDisposePool = true;
        bool _isQuoteSqlName = true;
        bool? _isAdoConnectionPool = false;
        MappingPriorityType[] _mappingPriorityTypes;
        NameConvertType _nameConvertType = NameConvertType.None;
        Action<DbCommand> _aopCommandExecuting = null;
        Action<DbCommand, string> _aopCommandExecuted = null;
        Type _providerType = null;

        /// <summary>
        /// 使用连接串（推荐）
        /// </summary>
        /// <param name="dataType">数据库类型</param>
        /// <param name="connectionString">数据库连接串</param>
        /// <param name="providerType">提供者的类型，一般不需要指定，如果一直提示“缺少 FreeSql 数据库实现包：FreeSql.Provider.MySql.dll，可前往 nuget 下载”的错误，说明反射获取不到类型，此时该参数可排上用场<para></para>例如：typeof(FreeSql.SqlServer.SqlServerProvider&lt;&gt;)</param>
        /// <returns></returns>
        public FreeSqlBuilder UseConnectionString(DataType dataType, string connectionString, Type providerType = null)
        {
            if (_connectionFactory != null) throw new Exception(CoreStrings.Has_Specified_Cannot_Specified_Second("UseConnectionFactory", "UseConnectionString"));
            _dataType = dataType;
            _masterConnectionString = connectionString;
            _providerType = providerType;
            return this;
        }

        /// <summary>
        /// 用于指定自定义实现TableEntiy 的缓存集合
        /// 解决多实例下相同类型映射到不同表的问题
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        [Obsolete("请使用 UseCacheFactory", true)]
        public FreeSqlBuilder UseCustomTableEntityCacheFactory(Func<ConcurrentDictionary<DataType, ConcurrentDictionary<Type, TableInfo>>> factory)
        {
            Utils._cacheGetTableByEntity = factory.Invoke();
            return this;
        }

        /// <summary>
        /// 解决多实例下相同类型映射到不同表的问题，可以覆盖为从自定义缓存中构建
        /// </summary>
        /// <param name="cacheFactory">自定义缓存策略，参考 <see cref="DefaultCacheFactory"/> </param>
        /// <returns></returns>
        public FreeSqlBuilder UseCacheFactory(IGlobalCacheFactory cacheFactory)
        {
            Utils.GlobalCacheFactory = cacheFactory;
            return this;
        }
        /// <summary>
        /// 使用原始连接池（ado.net、odbc、oledb）<para></para>
        /// 默认：false<para></para>
        /// UseConnectionString 默认使用 FreeSql 连接池，有以下特点：<para></para>
        /// - 状态不可用，断熔机制直到后台检测恢复<para></para>
        /// - 读写分离，从库不可用，会切换其他可用从库<para></para>
        /// - 监测连接池使用情况，fsql.Ado.Statistics<para></para>
        /// 有部分使用者不喜欢【断熔机制】，可使用此设置
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FreeSqlBuilder UseAdoConnectionPool(bool value)
        {
            _isAdoConnectionPool = value;
            return this;
        }
        /// <summary>
        /// 使用从数据库，支持多个
        /// </summary>
        /// <param name="slaveConnectionString">从数据库连接串</param>
        /// <returns></returns>
        public FreeSqlBuilder UseSlave(params string[] slaveConnectionString)
        {
            if (_connectionFactory != null) throw new Exception(CoreStrings.Has_Specified_Cannot_Specified_Second("UseConnectionFactory", "UseSlave"));
            _slaveConnectionString = slaveConnectionString;
            return this;
        }
        public FreeSqlBuilder UseSlaveWeight(params int[] slaveWeights)
        {
            if (_slaveConnectionString?.Length != slaveWeights.Length) throw new Exception(CoreStrings.Different_Number_SlaveConnectionString_SlaveWeights);
            _slaveWeights = slaveWeights;
            return this;
        }
        /// <summary>
        /// 使用自定义数据库连接对象（放弃内置对象连接池技术）
        /// </summary>
        /// <param name="dataType">数据库类型</param>
        /// <param name="connectionFactory">数据库连接对象创建器</param>
        /// <param name="providerType">提供者的类型，一般不需要指定，如果一直提示“缺少 FreeSql 数据库实现包：FreeSql.Provider.MySql.dll，可前往 nuget 下载”的错误，说明反射获取不到类型，此时该参数可排上用场<para></para>例如：typeof(FreeSql.SqlServer.SqlServerProvider&lt;&gt;)</param>
        /// <returns></returns>
        public FreeSqlBuilder UseConnectionFactory(DataType dataType, Func<DbConnection> connectionFactory, Type providerType = null)
        {
            if (string.IsNullOrEmpty(_masterConnectionString) == false) throw new Exception(CoreStrings.Has_Specified_Cannot_Specified_Second("UseConnectionString", "UseConnectionFactory"));
            if (_slaveConnectionString?.Any() == true) throw new Exception(CoreStrings.Has_Specified_Cannot_Specified_Second("UseSlave", "UseConnectionFactory"));
            _dataType = dataType;
            _connectionFactory = connectionFactory;
            _providerType = providerType;
            return this;
        }
        /// <summary>
        /// 【开发环境必备】自动同步实体结构到数据库，程序运行中检查实体表是否存在，然后创建或修改<para></para>
        /// 注意：生产环境中谨慎使用
        /// </summary>
        /// <param name="value">true:运行时检查自动同步结构, false:不同步结构(默认)</param>
        /// <returns></returns>
        public FreeSqlBuilder UseAutoSyncStructure(bool value)
        {
            _isAutoSyncStructure = value;
            return this;
        }
        /// <summary>
        /// 将数据库的主键、自增、索引设置导入，适用 DbFirst 模式，无须在实体类型上设置 [Column(IsPrimary)] 或者 ConfigEntity。此功能目前可用于 mysql/sqlserver/postgresql/oracle。<para></para>
        /// 本功能会影响 IFreeSql 首次访问的速度。<para></para>
        /// 若使用 CodeFirst 创建索引后，又直接在数据库上建了索引，若无本功能下一次 CodeFirst 迁移时数据库上创建的索引将被删除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FreeSqlBuilder UseConfigEntityFromDbFirst(bool value)
        {
            _isConfigEntityFromDbFirst = value;
            return this;
        }
        /// <summary>
        /// 不使用命令参数化执行，针对 Insert/Update，也可临时使用 IInsert/IUpdate.NoneParameter() 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FreeSqlBuilder UseNoneCommandParameter(bool value)
        {
            _isNoneCommandParameter = value;
            return this;
        }
        /// <summary>
        /// 是否生成命令参数化执行，针对 lambda 表达式解析<para></para>
        /// 注意：常量不会参数化，变量才会做参数化<para></para>
        /// var id = 100;
        /// fsql.Select&lt;T&gt;().Where(a => a.id == id) 会参数化<para></para>
        /// fsql.Select&lt;T&gt;().Where(a => a.id == 100) 不会参数化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FreeSqlBuilder UseGenerateCommandParameterWithLambda(bool value)
        {
            _isGenerateCommandParameterWithLambda = value;
            return this;
        }
        /// <summary>
        /// 延时加载导航属性对象，导航属性需要声明 virtual
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FreeSqlBuilder UseLazyLoading(bool value)
        {
            _isLazyLoading = value;
            return this;
        }
        /// <summary>
        /// 监视数据库命令对象
        /// </summary>
        /// <param name="executing">执行前</param>
        /// <param name="executed">执行后，可监视执行性能</param>
        /// <returns></returns>
        public FreeSqlBuilder UseMonitorCommand(Action<DbCommand> executing, Action<DbCommand, string> executed = null)
        {
            _aopCommandExecuting = executing;
            _aopCommandExecuted = executed;
            return this;
        }

        /// <summary>
        /// 实体类名 -> 数据库表名，命名转换（类名、属性名都生效）<para></para>
        /// 优先级小于 [Column(Name = "xxx")]
        /// </summary>
        /// <param name="convertType"></param>
        /// <returns></returns>
        public FreeSqlBuilder UseNameConvert(NameConvertType convertType)
        {
            _nameConvertType = convertType;
            return this;
        }
        /// <summary>
        /// SQL名称是否使用 [] `` ""<para></para>
        /// true: SELECT .. FROM [table]<para></para>
        /// false: SELECT .. FROM table
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FreeSqlBuilder UseQuoteSqlName(bool value)
        {
            _isQuoteSqlName = value;
            return this;
        }

        /// <summary>
        /// 指定映射优先级（从小到大）<para></para>
        /// 例如表名：实体类名 &lt; Aop &lt; FluentApi &lt; Attribute &lt; AsTable<para></para>
        /// 事件 Aop -------> fsql.Aop.ConfigEntity/fsql.Aop.ConfigEntityProperty<para></para>
        /// 方法 FluentApi -> fsql.CodeFirst.ConfigEntity/fsql.CodeFirst.Entity<para></para>
        /// 特性 Attribute -> [Table(Name = xxx, ...)]<para></para>
        /// -----------------------------------------------------------------------------<para></para>
        /// 默认规则：关于映射优先级，Attribute 可以更直观排查问题，即使任何地方使用 FluentApi/Aop 设置 TableName 都不生效。<para></para>
        /// 调整规则：UseMappingPriority(Attribute, FluentApi, Aop) <para></para>
        /// 实体类名 &lt; Attribute &lt; FluentApi &lt; Aop &lt; AsTable
        /// </summary>
        /// <param name="mappingType1"></param>
        /// <param name="mappingType2"></param>
        /// <param name="mappingType3"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public FreeSqlBuilder UseMappingPriority(MappingPriorityType mappingType1, MappingPriorityType mappingType2, MappingPriorityType mappingType3)
        {
            if (mappingType1 == mappingType2 || mappingType1 == mappingType3 || mappingType2 == mappingType3) throw new ArgumentException($"{nameof(mappingType1)}、{nameof(mappingType2)}、{nameof(mappingType3)} 不可以相等");
            _mappingPriorityTypes = new[] { mappingType1, mappingType2, mappingType3 };
            return this;
        }

        /// <summary>
        /// 监听 AppDomain.CurrentDomain.ProcessExit/Console.CancelKeyPress 事件自动释放连接池<para></para>
        /// 默认值: true
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FreeSqlBuilder UseExitAutoDisposePool(bool value)
        {
            _isExitAutoDisposePool = value;
            return this;
        }

        public IFreeSql Build() => Build<IFreeSql>();
        public IFreeSql<TMark> Build<TMark>()
        {
            if (string.IsNullOrEmpty(_masterConnectionString) && _connectionFactory == null) throw new Exception(CoreStrings.Check_UseConnectionString);
            IFreeSql<TMark> ret = null;
            var type = _providerType;
            if (type != null)
            {
                if (type.IsGenericTypeDefinition)
                    type = type.MakeGenericType(typeof(TMark));
            }
            else
            {
                Action<string, string> throwNotFind = (dll, providerType) => throw new Exception(CoreStrings.Missing_FreeSqlProvider_Package_Reason(dll, providerType));
                switch (_dataType)
                {
                    case DataType.MySql:
                        if (_isAdoConnectionPool == null) _isAdoConnectionPool = true;
                        type = Type.GetType("FreeSql.MySql.MySqlProvider`1,FreeSql.Provider.MySql")?.MakeGenericType(typeof(TMark)); //MySql.Data.dll
                        if (type == null) type = Type.GetType("FreeSql.MySql.MySqlProvider`1,FreeSql.Provider.MySqlConnector")?.MakeGenericType(typeof(TMark)); //MySqlConnector.dll
                        if (type == null) throwNotFind("FreeSql.Provider.MySql.dll", "FreeSql.MySql.MySqlProvider<>");
                        break;
                    case DataType.SqlServer:
                        if (_isAdoConnectionPool == null) _isAdoConnectionPool = true;
                        type = Type.GetType("FreeSql.SqlServer.SqlServerProvider`1,FreeSql.Provider.SqlServer")?.MakeGenericType(typeof(TMark)); //Microsoft.Data.SqlClient.dll
                        if (type == null) type = Type.GetType("FreeSql.SqlServer.SqlServerProvider`1,FreeSql.Provider.SqlServerForSystem")?.MakeGenericType(typeof(TMark)); //System.Data.SqlClient.dll
                        if (type == null) throwNotFind("FreeSql.Provider.SqlServer.dll", "FreeSql.SqlServer.SqlServerProvider<>");
                        break;
                    case DataType.PostgreSQL:
                        if (_isAdoConnectionPool == null) _isAdoConnectionPool = true;
                        type = Type.GetType("FreeSql.PostgreSQL.PostgreSQLProvider`1,FreeSql.Provider.PostgreSQL")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.PostgreSQL.dll", "FreeSql.PostgreSQL.PostgreSQLProvider<>");
                        break;
                    case DataType.Oracle:
                        type = Type.GetType("FreeSql.Oracle.OracleProvider`1,FreeSql.Provider.Oracle")?.MakeGenericType(typeof(TMark));
                        if (type == null) type = Type.GetType("FreeSql.Oracle.OracleProvider`1,FreeSql.Provider.OracleOledb")?.MakeGenericType(typeof(TMark)); //基于 oledb 实现，解决 US7ASCII 中文乱码问题
                        if (type == null) throwNotFind("FreeSql.Provider.Oracle.dll", "FreeSql.Oracle.OracleProvider<>");
                        break;
                    case DataType.Sqlite:
                        type = Type.GetType("FreeSql.Sqlite.SqliteProvider`1,FreeSql.Provider.Sqlite")?.MakeGenericType(typeof(TMark));
                        if (type == null) type = Type.GetType("FreeSql.Sqlite.SqliteProvider`1,FreeSql.Provider.SqliteCore")?.MakeGenericType(typeof(TMark)); //Microsoft.Data.Sqlite.Core.dll
                        if (type == null) throwNotFind("FreeSql.Provider.Sqlite.dll", "FreeSql.Sqlite.SqliteProvider<>");
                        break;

                    case DataType.OdbcOracle:
                        type = Type.GetType("FreeSql.Odbc.Oracle.OdbcOracleProvider`1,FreeSql.Provider.Odbc")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Odbc.dll", "FreeSql.Odbc.Oracle.OdbcOracleProvider<>");
                        break;
                    case DataType.OdbcSqlServer:
                        type = Type.GetType("FreeSql.Odbc.SqlServer.OdbcSqlServerProvider`1,FreeSql.Provider.Odbc")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Odbc.dll", "FreeSql.Odbc.SqlServer.OdbcSqlServerProvider<>");
                        break;
                    case DataType.OdbcMySql:
                        type = Type.GetType("FreeSql.Odbc.MySql.OdbcMySqlProvider`1,FreeSql.Provider.Odbc")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Odbc.dll", "FreeSql.Odbc.MySql.OdbcMySqlProvider<>");
                        break;
                    case DataType.OdbcPostgreSQL:
                        type = Type.GetType("FreeSql.Odbc.PostgreSQL.OdbcPostgreSQLProvider`1,FreeSql.Provider.Odbc")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Odbc.dll", "FreeSql.Odbc.PostgreSQL.OdbcPostgreSQLProvider<>");
                        break;
                    case DataType.Odbc:
                        type = Type.GetType("FreeSql.Odbc.Default.OdbcProvider`1,FreeSql.Provider.Odbc")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Odbc.dll", "FreeSql.Odbc.Default.OdbcProvider<>");
                        break;

                    case DataType.OdbcDameng:
                        type = Type.GetType("FreeSql.Odbc.Dameng.OdbcDamengProvider`1,FreeSql.Provider.Odbc")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Odbc.dll", "FreeSql.Odbc.Dameng.OdbcDamengProvider<>");
                        break;

                    case DataType.MsAccess:
                        type = Type.GetType("FreeSql.MsAccess.MsAccessProvider`1,FreeSql.Provider.MsAccess")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.MsAccess.dll", "FreeSql.MsAccess.MsAccessProvider<>");
                        break;

                    case DataType.Dameng:
                        type = Type.GetType("FreeSql.Dameng.DamengProvider`1,FreeSql.Provider.Dameng")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Dameng.dll", "FreeSql.Dameng.DamengProvider<>");
                        break;

                    case DataType.OdbcKingbaseES:
                        type = Type.GetType("FreeSql.Odbc.KingbaseES.OdbcKingbaseESProvider`1,FreeSql.Provider.Odbc")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Odbc.dll", "FreeSql.Odbc.KingbaseES.OdbcKingbaseESProvider<>");
                        break;

                    case DataType.ShenTong:
                        type = Type.GetType("FreeSql.ShenTong.ShenTongProvider`1,FreeSql.Provider.ShenTong")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.ShenTong.dll", "FreeSql.ShenTong.ShenTongProvider<>");
                        break;

                    case DataType.KingbaseES:
                        type = Type.GetType("FreeSql.KingbaseES.KingbaseESProvider`1,FreeSql.Provider.KingbaseES")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.KingbaseES.dll", "FreeSql.KingbaseES.KingbaseESProvider<>");
                        break;

                    case DataType.Firebird:
                        type = Type.GetType("FreeSql.Firebird.FirebirdProvider`1,FreeSql.Provider.Firebird")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Firebird.dll", "FreeSql.Firebird.FirebirdProvider<>");
                        break;

                    case DataType.Custom:
                        type = Type.GetType("FreeSql.Custom.CustomProvider`1,FreeSql.Provider.Custom")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Custom.dll", "FreeSql.Custom.CustomProvider<>");
                        break;

                    case DataType.ClickHouse:
                        type = Type.GetType("FreeSql.ClickHouse.ClickHouseProvider`1,FreeSql.Provider.ClickHouse")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.ClickHouse.dll", "FreeSql.ClickHouse.ClickHouseProvider<>");
                        break;

                    case DataType.GBase:
                        type = Type.GetType("FreeSql.GBase.GBaseProvider`1,FreeSql.Provider.GBase")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.GBase.dll", "FreeSql.GBase.GBaseProvider<>");
                        break;

                    case DataType.QuestDb:
                        type = Type.GetType("FreeSql.QuestDb.QuestDbProvider`1,FreeSql.Provider.QuestDb")?.MakeGenericType(typeof(TMark));
                        if (type == null)
                            throwNotFind("FreeSql.Provider.QuestDb.dll", "FreeSql.QuestDb.QuestDbProvider<>");
                        break;

                    case DataType.Xugu:
                        type = Type.GetType("FreeSql.Xugu.XuguProvider`1,FreeSql.Provider.Xugu")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Xugu.dll", "FreeSql.Xugu.XuguProvider<>");
                        break;

                    case DataType.CustomOracle:
                        type = Type.GetType("FreeSql.Custom.Oracle.CustomOracleProvider`1,FreeSql.Provider.Custom")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Custom.dll", "FreeSql.Custom.Oracle.CustomOracleProvider<>");
                        break;

                    case DataType.CustomSqlServer:
                        type = Type.GetType("FreeSql.Custom.SqlServer.CustomSqlServerProvider`1,FreeSql.Provider.Custom")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Custom.dll", "FreeSql.Custom.SqlServer.CustomSqlServerProvider<>");
                        break;

                    case DataType.CustomMySql:
                        type = Type.GetType("FreeSql.Custom.MySql.CustomMySqlProvider`1,FreeSql.Provider.Custom")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Custom.dll", "FreeSql.Custom.MySql.CustomMySqlProvider<>");
                        break;

                    case DataType.CustomPostgreSQL:
                        type = Type.GetType("FreeSql.Custom.PostgreSQL.CustomPostgreSQLProvider`1,FreeSql.Provider.Custom")?.MakeGenericType(typeof(TMark));
                        if (type == null) throwNotFind("FreeSql.Provider.Custom.dll", "FreeSql.Custom.PostgreSQL.CustomPostgreSQLProvider<>");
                        break;

                    default: throw new Exception(CoreStrings.NotSpecified_UseConnectionString_UseConnectionFactory);
                }
            }
            ret = Activator.CreateInstance(type, new object[]
            {
                _isAdoConnectionPool == true ? $"AdoConnectionPool,{_masterConnectionString}" : _masterConnectionString,
                _slaveConnectionString,
                _connectionFactory
            }) as IFreeSql<TMark>;
            if (ret != null)
            {
                ret.CodeFirst.IsAutoSyncStructure = _isAutoSyncStructure;

                ret.CodeFirst.IsConfigEntityFromDbFirst = _isConfigEntityFromDbFirst;
                ret.CodeFirst.IsNoneCommandParameter = _isNoneCommandParameter;
                ret.CodeFirst.IsGenerateCommandParameterWithLambda = _isGenerateCommandParameterWithLambda;
                ret.CodeFirst.IsLazyLoading = _isLazyLoading;

                if (_mappingPriorityTypes != null)
                    (ret.Select<object>() as Select0Provider)._commonUtils._mappingPriorityTypes = _mappingPriorityTypes;

                if (_aopCommandExecuting != null)
                    ret.Aop.CommandBefore += new EventHandler<Aop.CommandBeforeEventArgs>((s, e) => _aopCommandExecuting?.Invoke(e.Command));
                if (_aopCommandExecuted != null)
                    ret.Aop.CommandAfter += new EventHandler<Aop.CommandAfterEventArgs>((s, e) => _aopCommandExecuted?.Invoke(e.Command, e.Log));

                //添加实体属性名全局AOP转换处理
                if (_nameConvertType != NameConvertType.None)
                {
                    string PascalCaseToUnderScore(string str) => string.IsNullOrWhiteSpace(str) ? str : string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));
                    //string UnderScorePascalCase(string str) => string.IsNullOrWhiteSpace(str) ? str : string.Join("", str.Split('_').Select(a => a.Length > 0 ? string.Concat(char.ToUpper(a[0]), a.Substring(1)) : ""));

                    switch (_nameConvertType)
                    {
                        case NameConvertType.ToLower:
                            ret.Aop.ConfigEntity += (_, e) => { if (string.IsNullOrWhiteSpace(e.ModifyResult.AsTable)) e.ModifyResult.Name = e.ModifyResult.Name?.ToLower(); };
                            ret.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = e.ModifyResult.Name?.ToLower();
                            ret.CodeFirst.IsSyncStructureToLower = true;
                            break;
                        case NameConvertType.ToUpper:
                            ret.Aop.ConfigEntity += (_, e) => { if (string.IsNullOrWhiteSpace(e.ModifyResult.AsTable)) e.ModifyResult.Name = e.ModifyResult.Name?.ToUpper(); };
                            ret.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = e.ModifyResult.Name?.ToUpper();
                            ret.CodeFirst.IsSyncStructureToUpper = true;
                            break;
                        case NameConvertType.PascalCaseToUnderscore:
                            ret.Aop.ConfigEntity += (_, e) => { if (string.IsNullOrWhiteSpace(e.ModifyResult.AsTable)) e.ModifyResult.Name = PascalCaseToUnderScore(e.ModifyResult.Name); };
                            ret.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = PascalCaseToUnderScore(e.ModifyResult.Name);
                            break;
                        case NameConvertType.PascalCaseToUnderscoreWithLower:
                            ret.Aop.ConfigEntity += (_, e) => { if (string.IsNullOrWhiteSpace(e.ModifyResult.AsTable)) e.ModifyResult.Name = PascalCaseToUnderScore(e.ModifyResult.Name)?.ToLower(); };
                            ret.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = PascalCaseToUnderScore(e.ModifyResult.Name)?.ToLower();
                            break;
                        case NameConvertType.PascalCaseToUnderscoreWithUpper:
                            ret.Aop.ConfigEntity += (_, e) => { if (string.IsNullOrWhiteSpace(e.ModifyResult.AsTable)) e.ModifyResult.Name = PascalCaseToUnderScore(e.ModifyResult.Name)?.ToUpper(); };
                            ret.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = PascalCaseToUnderScore(e.ModifyResult.Name)?.ToUpper();
                            break;
                        //case NameConvertType.UnderscoreToPascalCase:
                        //    ret.Aop.ConfigEntity += (_, e) => { if (string.IsNullOrWhiteSpace(e.ModifyResult.AsTable)) e.ModifyResult.Name = UnderScorePascalCase(e.ModifyResult.Name); };
                        //    ret.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = UnderScorePascalCase(e.ModifyResult.Name);
                        //    break;
                        default:
                            break;
                    }
                }
                //处理 MaxLength、EFCore 特性
                ret.Aop.ConfigEntityProperty += new EventHandler<Aop.ConfigEntityPropertyEventArgs>((s, e) =>
                {
                    object[] attrs = null;
                    try
                    {
                        attrs = e.Property.GetCustomAttributes(false).ToArray(); //.net core 反射存在版本冲突问题，导致该方法异常
                    }
                    catch { }

                    var dyattr = attrs?.Where(a =>
                    {
                        return ((a as Attribute)?.TypeId as Type)?.Name == "MaxLengthAttribute";
                    }).FirstOrDefault();
                    if (dyattr != null)
                    {
                        var lenProp = dyattr.GetType().GetProperties().Where(a => a.PropertyType.IsNumberType()).FirstOrDefault();
                        if (lenProp != null && int.TryParse(string.Concat(lenProp.GetValue(dyattr, null)), out var tryval) && tryval != 0)
                        {
                            e.ModifyResult.StringLength = tryval;
                        }
                    }

                    dyattr = attrs?.Where(a =>
                    {
                        return ((a as Attribute)?.TypeId as Type)?.FullName == "System.ComponentModel.DataAnnotations.RequiredAttribute";
                    }).FirstOrDefault();
                    if (dyattr != null)
                    {
                        e.ModifyResult.IsNullable = false;
                    }

                    dyattr = attrs?.Where(a =>
                    {
                        return ((a as Attribute)?.TypeId as Type)?.FullName == "System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute";
                    }).FirstOrDefault();
                    if (dyattr != null)
                    {
                        e.ModifyResult.IsIgnore = true;
                    }

                    dyattr = attrs?.Where(a =>
                    {
                        return ((a as Attribute)?.TypeId as Type)?.FullName == "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute";
                    }).FirstOrDefault();
                    if (dyattr != null)
                    {
                        var name = dyattr.GetType().GetProperties().Where(a => a.PropertyType == typeof(string) && a.Name == "Name").FirstOrDefault()?.GetValue(dyattr, null)?.ToString();
                        short.TryParse(string.Concat(dyattr.GetType().GetProperties().Where(a => a.PropertyType == typeof(int) && a.Name == "Order").FirstOrDefault()?.GetValue(dyattr, null)), out var order);
                        var typeName = dyattr.GetType().GetProperties().Where(a => a.PropertyType == typeof(string) && a.Name == "TypeName").FirstOrDefault()?.GetValue(dyattr, null)?.ToString();

                        if (string.IsNullOrEmpty(name) == false)
                            e.ModifyResult.Name = name;
                        if (order != 0)
                            e.ModifyResult.Position = order;
                        if (string.IsNullOrEmpty(typeName) == false)
                            e.ModifyResult.DbType = typeName;
                    }

                    dyattr = attrs?.Where(a =>
                    {
                        return ((a as Attribute)?.TypeId as Type)?.FullName == "System.ComponentModel.DataAnnotations.KeyAttribute";
                    }).FirstOrDefault();
                    if (dyattr != null)
                    {
                        e.ModifyResult.IsPrimary = true;
                    }

                    dyattr = attrs?.Where(a =>
                    {
                        return ((a as Attribute)?.TypeId as Type)?.FullName == "System.ComponentModel.DataAnnotations.StringLengthAttribute";
                    }).FirstOrDefault();
                    if (dyattr != null)
                    {
                        var lenProps = dyattr.GetType().GetProperties().Where(a => a.PropertyType.IsNumberType()).ToArray();
                        var lenProp = lenProps.Length == 1 ? lenProps.FirstOrDefault() : lenProps.Where(a => a.Name == "MaximumLength").FirstOrDefault();
                        if (lenProp != null && int.TryParse(string.Concat(lenProp.GetValue(dyattr, null)), out var tryval) && tryval != 0)
                        {
                            e.ModifyResult.StringLength = tryval;
                        }
                    }

                    //https://github.com/dotnetcore/FreeSql/issues/378
                    dyattr = attrs?.Where(a =>
                    {
                        return ((a as Attribute)?.TypeId as Type)?.FullName == "System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute";
                    }).FirstOrDefault();
                    if (dyattr != null)
                    {
                        switch (string.Concat(dyattr.GetType().GetProperty("DatabaseGeneratedOption")?.GetValue(dyattr, null)))
                        {
                            case "Identity":
                            case "1":
                                e.ModifyResult.IsIdentity = true;
                                break;
                            default:
                                e.ModifyResult.CanInsert = false;
                                e.ModifyResult.CanUpdate = false;
                                break;
                        }
                    }
                });
                //EFCore 特性
                ret.Aop.ConfigEntity += new EventHandler<Aop.ConfigEntityEventArgs>((s, e) =>
                {
                    object[] attrs = null;
                    try
                    {
                        attrs = e.EntityType.GetCustomAttributes(false).ToArray(); //.net core 反射存在版本冲突问题，导致该方法异常
                    }
                    catch { }

                    var dyattr = attrs?.Where(a =>
                    {
                        return ((a as Attribute)?.TypeId as Type)?.FullName == "System.ComponentModel.DataAnnotations.Schema.TableAttribute";
                    }).FirstOrDefault();
                    if (dyattr != null)
                    {
                        var name = dyattr.GetType().GetProperties().Where(a => a.PropertyType == typeof(string) && a.Name == "Name").FirstOrDefault()?.GetValue(dyattr, null)?.ToString();
                        var schema = dyattr.GetType().GetProperties().Where(a => a.PropertyType == typeof(string) && a.Name == "Schema").FirstOrDefault()?.GetValue(dyattr, null)?.ToString();
                        if (string.IsNullOrEmpty(name) == false && string.IsNullOrEmpty(schema) == false)
                            e.ModifyResult.Name = $"{schema}.{name}";
                        else if (string.IsNullOrEmpty(name) == false)
                            e.ModifyResult.Name = name;
                        else if (string.IsNullOrEmpty(schema) == false)
                            e.ModifyResult.Name = $"{schema}.{e.ModifyResult.Name}";
                    }
                });

                ret.Ado.MasterPool.Policy.IsAutoDisposeWithSystem = _isExitAutoDisposePool;
                ret.Ado.SlavePools.ForEach(a => a.Policy.IsAutoDisposeWithSystem = _isExitAutoDisposePool);
                if (_slaveWeights != null)
                    for (var x = 0; x < _slaveWeights.Length; x++)
                        ret.Ado.SlavePools[x].Policy.Weight = _slaveWeights[x];

                (ret.Select<object>() as Select0Provider)._commonUtils.IsQuoteSqlName = _isQuoteSqlName;
            }

            if (Interlocked.CompareExchange(ref _isTypeHandlered, 1, 0) == 0)
            {
                FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, Type type2) =>
                {
                    if (FreeSql.Internal.Utils.TypeHandlers.TryGetValue(type2, out var typeHandler))
                    {
                        var valueExpRet = Expression.Call(
                            Expression.Constant(typeHandler, typeof(ITypeHandler)),
                            typeof(ITypeHandler).GetMethod(nameof(typeHandler.Deserialize)),
                            Expression.Convert(valueExp, typeof(object)));
                        return Expression.IfThenElse(
                            Expression.TypeIs(valueExp, type2),
                            Expression.Return(returnTarget, valueExp),
                            Expression.Return(returnTarget, Expression.Convert(valueExpRet, typeof(object))) //此时不能设置 type2
                        );
                    }
                    return null;
                });
            }

            ret.Aop.ConfigEntityProperty += (s, e) =>
            {
                foreach (var typeHandler in FreeSql.Internal.Utils.TypeHandlers.Values)
                {
                    if (e.Property.PropertyType == typeHandler.Type)
                    {
                        if (_dicTypeHandlerTypes.ContainsKey(e.Property.PropertyType)) return;
                        if (e.Property.PropertyType.NullableTypeOrThis() != typeof(DateTime) &&
                            FreeSql.Internal.Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(e.Property.PropertyType))
                            return; //基础类型无效，DateTime 除外

                        if (_dicTypeHandlerTypes.TryAdd(e.Property.PropertyType, true))
                        {
                            lock (_concurrentObj)
                            {
                                FreeSql.Internal.Utils.dicExecuteArrayRowReadClassOrTuple[e.Property.PropertyType] = true;
                                FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionObjectToStringIfThenElse.Add((LabelTarget returnTarget, Expression valueExp, Expression elseExp, Type type2) =>
                                {
                                    return Expression.IfThenElse(
                                        Expression.TypeIs(valueExp, e.Property.PropertyType),
                                        Expression.Return(returnTarget, Expression.Call(
                                            Expression.Constant(typeHandler, typeof(ITypeHandler)),
                                            typeof(ITypeHandler).GetMethod(nameof(typeHandler.Serialize)),
                                            Expression.Convert(valueExp, typeof(object))
                                            ), typeof(object)),
                                        elseExp);
                                });
                            }
                        }
                        break;
                    }
                }
            };

            return ret;
        }
        static int _isTypeHandlered = 0;
        ConcurrentDictionary<Type, bool> _dicTypeHandlerTypes = Utils.GlobalCacheFactory.CreateCacheItem(new ConcurrentDictionary<Type, bool>());
        object _concurrentObj = new object();
    }
}
