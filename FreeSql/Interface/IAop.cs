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
        EventHandler<Aop.ParseExpressionEventArgs> ParseExpression { get; set; }

        /// <summary>
        /// 自定义实体的配置，方便和多个 ORM 共同使用
        /// </summary>
        EventHandler<Aop.ConfigEntityEventArgs> ConfigEntity { get; set; }
        /// <summary>
        /// 自定义实体的属性配置，方便和多个 ORM 共同使用
        /// </summary>
        EventHandler<Aop.ConfigEntityPropertyEventArgs> ConfigEntityProperty { get; set; }

        /// <summary>
        /// 增删查改，执行命令之前触发
        /// </summary>
        EventHandler<Aop.CurdBeforeEventArgs> CurdBefore { get; set; }
        /// <summary>
        /// 增删查改，执行命令完成后触发
        /// </summary>
        EventHandler<Aop.CurdAfterEventArgs> CurdAfter { get; set; }

        /// <summary>
        /// CodeFirst迁移，执行之前触发
        /// </summary>
        EventHandler<Aop.SyncStructureBeforeEventArgs> SyncStructureBefore { get; set; }
        /// <summary>
        /// CodeFirst迁移，执行完成触发
        /// </summary>
        EventHandler<Aop.SyncStructureAfterEventArgs> SyncStructureAfter { get; set; }

        /// <summary>
        /// Insert/Update自动值处理
        /// </summary>
        EventHandler<Aop.AuditValueEventArgs> AuditValue { get; set; }
    }
}

namespace FreeSql.Aop
{
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

    public class CurdBeforeEventArgs : EventArgs
    {
        public CurdBeforeEventArgs(Type entityType, TableInfo table, CurdType curdType, string sql, DbParameter[] dbParms) :
            this(Guid.NewGuid(), new Stopwatch(), entityType, table, curdType, sql, dbParms)
        {
            this.Stopwatch.Start();
        }
        protected CurdBeforeEventArgs(Guid identifier, Stopwatch stopwatch, Type entityType, TableInfo table, CurdType curdType, string sql, DbParameter[] dbParms)
        {
            this.Identifier = identifier;
            this.Stopwatch = stopwatch;
            this.EntityType = entityType;
            this.Table = table;
            this.CurdType = curdType;
            this.Sql = sql;
            this.DbParms = dbParms;
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
        public TableInfo Table { get; set; }
        /// <summary>
        /// 执行的 SQL
        /// </summary>
        public string Sql { get; }
        /// <summary>
        /// 参数化命令
        /// </summary>
        public DbParameter[] DbParms { get; }
    }
    public enum CurdType { Select, Delete, Update, Insert }
    public class CurdAfterEventArgs : CurdBeforeEventArgs
    {
        public CurdAfterEventArgs(CurdBeforeEventArgs before, Exception exception, object executeResult) :
            base(before.Identifier, before.StopwatchInternal, before.EntityType, before.Table, before.CurdType, before.Sql, before.DbParms)
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
        public object ExecuteResult { get; set; }
        /// <summary>
        /// 耗时（单位：Ticks）
        /// </summary>
        public long ElapsedTicks => this.Stopwatch.ElapsedTicks;
        /// <summary>
        /// 耗时（单位：毫秒）
        /// </summary>
        public long ElapsedMilliseconds => this.Stopwatch.ElapsedMilliseconds;
    }

    public class SyncStructureBeforeEventArgs : EventArgs
    {
        public SyncStructureBeforeEventArgs(Type[] entityTypes) :
            this(Guid.NewGuid(), new Stopwatch(), entityTypes)
        {
            this.Stopwatch.Start();
        }
        protected SyncStructureBeforeEventArgs(Guid identifier, Stopwatch stopwatch, Type[] entityTypes)
        {
            this.Identifier = identifier;
            this.Stopwatch = stopwatch;
            this.EntityTypes = entityTypes;
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
    }
    public class SyncStructureAfterEventArgs : SyncStructureBeforeEventArgs
    {
        public SyncStructureAfterEventArgs(SyncStructureBeforeEventArgs before, string sql, Exception exception) :
            base(before.Identifier, before.StopwatchInternal, before.EntityTypes)
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
                this.IsChanged = true;
            }
        }
        private object _value;
        public bool IsChanged { get; private set; }
    }
    public enum AuditValueType { Update, Insert }
}