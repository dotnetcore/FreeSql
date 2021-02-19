using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace FreeSql
{
    public interface IAop
    {
        /// <summary>
        /// 可自定义解析表达式
        /// </summary>
        event EventHandler<Aop.ParseExpressionEventArgs> ParseExpression;
        EventHandler<Aop.ParseExpressionEventArgs> ParseExpressionHandler { get; }

        /// <summary>
        /// 自定义实体的配置，方便和多个 ORM 共同使用
        /// </summary>
        event EventHandler<Aop.ConfigEntityEventArgs> ConfigEntity;
        EventHandler<Aop.ConfigEntityEventArgs> ConfigEntityHandler { get; }
        /// <summary>
        /// 自定义实体的属性配置，方便和多个 ORM 共同使用
        /// </summary>
        event EventHandler<Aop.ConfigEntityPropertyEventArgs> ConfigEntityProperty;
        EventHandler<Aop.ConfigEntityPropertyEventArgs> ConfigEntityPropertyHandler { get; }

        /// <summary>
        /// 增删查改，执行命令之前触发
        /// </summary>
        event EventHandler<Aop.CurdBeforeEventArgs> CurdBefore;
        EventHandler<Aop.CurdBeforeEventArgs> CurdBeforeHandler { get; }
        /// <summary>
        /// 增删查改，执行命令完成后触发
        /// </summary>
        event EventHandler<Aop.CurdAfterEventArgs> CurdAfter;
        EventHandler<Aop.CurdAfterEventArgs> CurdAfterHandler { get; }

        /// <summary>
        /// CodeFirst迁移，执行之前触发
        /// </summary>
        event EventHandler<Aop.SyncStructureBeforeEventArgs> SyncStructureBefore;
        EventHandler<Aop.SyncStructureBeforeEventArgs> SyncStructureBeforeHandler { get; }
        /// <summary>
        /// CodeFirst迁移，执行完成触发
        /// </summary>
        event EventHandler<Aop.SyncStructureAfterEventArgs> SyncStructureAfter;
        EventHandler<Aop.SyncStructureAfterEventArgs> SyncStructureAfterHandler { get; }

        /// <summary>
        /// Insert/Update自动值处理
        /// </summary>
        event EventHandler<Aop.AuditValueEventArgs> AuditValue;
        EventHandler<Aop.AuditValueEventArgs> AuditValueHandler { get; }

        /// <summary>
        /// ADO.NET DataReader 拦截
        /// </summary>
        event EventHandler<Aop.AuditDataReaderEventArgs> AuditDataReader;
        EventHandler<Aop.AuditDataReaderEventArgs> AuditDataReaderHandler { get; }

        /// <summary>
        /// 监视数据库命令对象(执行前，调试)
        /// </summary>
        event EventHandler<Aop.CommandBeforeEventArgs> CommandBefore;
        EventHandler<Aop.CommandBeforeEventArgs> CommandBeforeHandler { get; }
        /// <summary>
        /// 监视数据库命令对象(执行后，用于监视执行性能)
        /// </summary>
        event EventHandler<Aop.CommandAfterEventArgs> CommandAfter;
        EventHandler<Aop.CommandAfterEventArgs> CommandAfterHandler { get; }

        /// <summary>
        /// 跟踪开始
        /// </summary>
        event EventHandler<Aop.TraceBeforeEventArgs> TraceBefore;
        EventHandler<Aop.TraceBeforeEventArgs> TraceBeforeHandler { get; }
        /// <summary>
        /// 跟踪结束
        /// </summary>
        event EventHandler<Aop.TraceAfterEventArgs> TraceAfter;
        EventHandler<Aop.TraceAfterEventArgs> TraceAfterHandler { get; }
    }
}

namespace FreeSql.Aop
{
    #region ParseExpression
    public class ParseExpressionEventArgs : EventArgs
    {
        public ParseExpressionEventArgs(Expression expression, Func<Expression, string> freeParse)
        {
            this.Expression = expression;
            this.FreeParse = freeParse;
        }

        /// <summary>
        /// 内置解析功能，可辅助您进行解析
        /// </summary>
        public Func<Expression, string> FreeParse { get; }

        /// <summary>
        /// 需要您解析的表达式
        /// </summary>
        public Expression Expression { get; }
        /// <summary>
        /// 解析后的内容
        /// </summary>
        public string Result { get; set; }
    }
    #endregion

    #region ConfigEntity/Property
    public class ConfigEntityEventArgs : EventArgs
    {
        public ConfigEntityEventArgs(Type entityType)
        {
            this.EntityType = entityType;
            this.ModifyResult = new TableAttribute();
            this.ModifyIndexResult = new List<IndexAttribute>();
        }

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type EntityType { get; }
        /// <summary>
        /// 实体配置
        /// </summary>
        public TableAttribute ModifyResult { get; }
        /// <summary>
        /// 索引配置
        /// </summary>
        public List<IndexAttribute> ModifyIndexResult { get; }
    }
    public class ConfigEntityPropertyEventArgs : EventArgs
    {
        public ConfigEntityPropertyEventArgs(Type entityType, PropertyInfo property)
        {
            this.EntityType = entityType;
            this.Property = property;
            this.ModifyResult = new ColumnAttribute();
        }

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type EntityType { get; }
        /// <summary>
        /// 实体的属性
        /// </summary>
        public PropertyInfo Property { get; }
        /// <summary>
        /// 实体的属性配置
        /// </summary>
        public ColumnAttribute ModifyResult { get; }
    }
    #endregion

    #region CurdBefore/After
    public class CurdBeforeEventArgs : EventArgs
    {
        public CurdBeforeEventArgs(Type entityType, TableInfo table, CurdType curdType, string sql, DbParameter[] dbParms) :
            this(Guid.NewGuid(), new Stopwatch(), entityType, table, curdType, sql, dbParms, new Dictionary<string, object>())
        {
            this.Stopwatch.Start();
        }
        protected CurdBeforeEventArgs(Guid identifier, Stopwatch stopwatch, Type entityType, TableInfo table, CurdType curdType, string sql, DbParameter[] dbParms, Dictionary<string, object> states)
        {
            this.Identifier = identifier;
            this.Stopwatch = stopwatch;
            this.EntityType = entityType;
            this.Table = table;
            this.CurdType = curdType;
            this.Sql = sql;
            this.DbParms = dbParms;
            this.States = states;
        }

        /// <summary>
        /// 标识符，可将 CurdBefore 与 CurdAfter 进行匹配
        /// </summary>
        public Guid Identifier { get; protected set; }
        protected Stopwatch Stopwatch { get; }
        internal Stopwatch StopwatchInternal => Stopwatch;
        /// <summary>
        /// 操作类型
        /// </summary>
        public CurdType CurdType { get; }
        /// <summary>
        /// 实体类型
        /// </summary>
        public Type EntityType { get; }
        /// <summary>
        /// 实体类型的元数据
        /// </summary>
        public TableInfo Table { get; }
        /// <summary>
        /// 执行的 SQL
        /// </summary>
        public string Sql { get; }
        /// <summary>
        /// 参数化命令
        /// </summary>
        public DbParameter[] DbParms { get; }
        /// <summary>
        /// 状态数据，可与 CurdAfter 共享
        /// </summary>
        public Dictionary<string, object> States { get; protected set; }
    }
    public enum CurdType { Select, Delete, Update, Insert, InsertOrUpdate }
    public class CurdAfterEventArgs : CurdBeforeEventArgs
    {
        public CurdAfterEventArgs(CurdBeforeEventArgs before, Exception exception, object executeResult) :
            base(before.Identifier, before.StopwatchInternal, before.EntityType, before.Table, before.CurdType, before.Sql, before.DbParms, before.States)
        {
            this.Exception = exception;
            this.ExecuteResult = executeResult;
            this.Stopwatch.Stop();
        }

        /// <summary>
        /// 发生的错误
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// 执行SQL命令，返回的结果
        /// </summary>
        public object ExecuteResult { get; }
        /// <summary>
        /// 耗时（单位：Ticks）
        /// </summary>
        public long ElapsedTicks => this.Stopwatch.ElapsedTicks;
        /// <summary>
        /// 耗时（单位：毫秒）
        /// </summary>
        public long ElapsedMilliseconds => this.Stopwatch.ElapsedMilliseconds;
    }
    #endregion

    #region SyncStructureBefore/After
    public class SyncStructureBeforeEventArgs : EventArgs
    {
        public SyncStructureBeforeEventArgs(Type[] entityTypes) :
            this(Guid.NewGuid(), new Stopwatch(), entityTypes, new Dictionary<string, object>())
        {
            this.Stopwatch.Start();
        }
        protected SyncStructureBeforeEventArgs(Guid identifier, Stopwatch stopwatch, Type[] entityTypes, Dictionary<string, object> states)
        {
            this.Identifier = identifier;
            this.Stopwatch = stopwatch;
            this.EntityTypes = entityTypes;
            this.States = states;
        }

        /// <summary>
        /// 标识符，可将 SyncStructureBeforeEventArgs 与 SyncStructureAfterEventArgs 进行匹配
        /// </summary>
        public Guid Identifier { get; protected set; }
        protected Stopwatch Stopwatch { get; }
        internal Stopwatch StopwatchInternal => Stopwatch;
        /// <summary>
        /// 实体类型
        /// </summary>
        public Type[] EntityTypes { get; }
        /// <summary>
        /// 状态数据，可与 SyncStructureAfter 共享
        /// </summary>
        public Dictionary<string, object> States { get; protected set; }
    }
    public class SyncStructureAfterEventArgs : SyncStructureBeforeEventArgs
    {
        public SyncStructureAfterEventArgs(SyncStructureBeforeEventArgs before, string sql, Exception exception) :
            base(before.Identifier, before.StopwatchInternal, before.EntityTypes, before.States)
        {
            this.Sql = sql;
            this.Exception = exception;
            this.Stopwatch.Stop();
        }

        /// <summary>
        /// 执行的 SQL
        /// </summary>
        public string Sql { get; }
        /// <summary>
        /// 发生的错误
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// 耗时（单位：Ticks）
        /// </summary>
        public long ElapsedTicks => this.Stopwatch.ElapsedTicks;
        /// <summary>
        /// 耗时（单位：毫秒）
        /// </summary>
        public long ElapsedMilliseconds => this.Stopwatch.ElapsedMilliseconds;
    }
    #endregion

    #region AuditValue
    public class AuditValueEventArgs : EventArgs
    {
        public AuditValueEventArgs(AuditValueType autoValueType, ColumnInfo column, PropertyInfo property, object value)
        {
            this.AuditValueType = autoValueType;
            this.Column = column;
            this.Property = property;
            this._value = value;
        }

        /// <summary>
        /// 类型
        /// </summary>
        public AuditValueType AuditValueType { get; }
        /// <summary>
        /// 属性列的元数据
        /// </summary>
        public ColumnInfo Column { get; }
        /// <summary>
        /// 反射的属性信息
        /// </summary>
        public PropertyInfo Property { get; }
        /// <summary>
        /// 获取实体的属性值，也可以设置实体的属性新值
        /// </summary>
        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                this.ValueIsChanged = true;
            }
        }
        private object _value;
        public bool ValueIsChanged { get; private set; }
    }
    public enum AuditValueType { Update, Insert, InsertOrUpdate }
    #endregion

    #region AuditDataReader
    public class AuditDataReaderEventArgs : EventArgs
    {
        public AuditDataReaderEventArgs(DbDataReader dataReader, int index)
        {
            this.DataReader = dataReader;
            this.Index = index;
        }

        /// <summary>
        /// ADO.NET 数据流读取对象
        /// </summary>
        public DbDataReader DataReader { get; }
        /// <summary>
        /// DataReader 对应的 Index 位置
        /// </summary>
        public int Index { get; }
        /// <summary>
        /// 获取 Index 对应的值，也可以设置拦截的新值
        /// </summary>
        public object Value
        {
            get
            {
                if (_valueIsGeted == false)
                {
                    _value = DataReader.GetValue(Index);
                    _valueIsGeted = true;
                }
                return _value;
            }
            set
            {
                _value = value;
                ValueIsChanged = true;
                _valueIsGeted = true;
            }
        }
        private object _value;
        internal bool _valueIsGeted;
        public bool ValueIsChanged { get; private set; }
    }
    #endregion

    #region CommandBefore/After
    public class CommandBeforeEventArgs : EventArgs
    {
        public CommandBeforeEventArgs(DbCommand command) :
            this(Guid.NewGuid(), new Stopwatch(), command, new Dictionary<string, object>())
        {
            this.Stopwatch.Start();
        }
        protected CommandBeforeEventArgs(Guid identifier, Stopwatch stopwatch, DbCommand command, Dictionary<string, object> states)
        {
            this.Identifier = identifier;
            this.Stopwatch = stopwatch;
            this.Command = command;
            this.States = states;
        }

        /// <summary>
        /// 标识符，可将 CommandBefore 与 CommandAfter 进行匹配
        /// </summary>
        public Guid Identifier { get; protected set; }
        protected Stopwatch Stopwatch { get; }
        internal Stopwatch StopwatchInternal => Stopwatch;
        public DbCommand Command { get; }
        /// <summary>
        /// 状态数据，可与 CommandAfter 共享
        /// </summary>
        public Dictionary<string, object> States { get; protected set; }
    }
    public class CommandAfterEventArgs : CommandBeforeEventArgs
    {
        public CommandAfterEventArgs(CommandBeforeEventArgs before, Exception exception, string log) :
            base(before.Identifier, before.StopwatchInternal, before.Command, before.States)
        {
            this.Exception = exception;
            this.Log = log;
            this.Stopwatch.Stop();
        }

        /// <summary>
        /// 发生的错误
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// 执行SQL命令，返回的结果
        /// </summary>
        public string Log { get; }
        /// <summary>
        /// 耗时（单位：Ticks）
        /// </summary>
        public long ElapsedTicks => this.Stopwatch.ElapsedTicks;
        /// <summary>
        /// 耗时（单位：毫秒）
        /// </summary>
        public long ElapsedMilliseconds => this.Stopwatch.ElapsedMilliseconds;
    }
    #endregion

    #region TraceBefore/After
    public class TraceBeforeEventArgs : EventArgs
    {
        public TraceBeforeEventArgs(string operation, object value) :
            this(Guid.NewGuid(), new Stopwatch(), operation, value, new Dictionary<string, object>())
        {
            this.Stopwatch.Start();
        }
        protected TraceBeforeEventArgs(Guid identifier, Stopwatch stopwatch, string operation, object value, Dictionary<string, object> states)
        {
            this.Identifier = identifier;
            this.Stopwatch = stopwatch;
            this.Operation = operation;
            this.Value = value;
            this.States = states;
        }

        /// <summary>
        /// 标识符，可将 TraceBeforeEventArgs 与 TraceAfterEventArgs 进行匹配
        /// </summary>
        public Guid Identifier { get; protected set; }
        protected Stopwatch Stopwatch { get; }
        internal Stopwatch StopwatchInternal => Stopwatch;
        public string Operation { get; }
        public object Value { get; }
        /// <summary>
        /// 状态数据，可与 TraceAfter 共享
        /// </summary>
        public Dictionary<string, object> States { get; protected set; }
    }
    public class TraceAfterEventArgs : TraceBeforeEventArgs
    {
        public TraceAfterEventArgs(TraceBeforeEventArgs before, string remark, Exception exception) :
            base(before.Identifier, before.StopwatchInternal, before.Operation, before.Value, before.States)
        {
            this.Remark = remark;
            this.Exception = exception;
            this.Stopwatch.Stop();
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; }
        /// <summary>
        /// 发生的错误
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// 耗时（单位：Ticks）
        /// </summary>
        public long ElapsedTicks => this.Stopwatch.ElapsedTicks;
        /// <summary>
        /// 耗时（单位：毫秒）
        /// </summary>
        public long ElapsedMilliseconds => this.Stopwatch.ElapsedMilliseconds;
    }
    #endregion
}