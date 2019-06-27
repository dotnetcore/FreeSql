using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
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
        /// 监控 ToList 返回的的数据，用于拦截重新装饰
        /// </summary>
        EventHandler<Aop.ToListEventArgs> ToList { get; set; }

        /// <summary>
        /// 监视 Where，包括 select/update/delete，可控制使上层不被执行。
        /// </summary>
        EventHandler<Aop.WhereEventArgs> Where { get; set; }

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
    }
}

namespace FreeSql.Aop
{
    public class ToListEventArgs : EventArgs
    {
        public ToListEventArgs(object list)
        {
            this.List = list;
        }
        /// <summary>
        /// 可重新装饰的引用数据
        /// </summary>
        public object List { get; }
    }
    public class WhereEventArgs : EventArgs
    {
        public WhereEventArgs(params object[] parameters)
        {
            this.Parameters = parameters;
        }
        public object[] Parameters { get; }
        /// <summary>
        /// 可使上层不被执行这个条件
        /// </summary>
        public bool IsCancel { get; set; }
    }
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
        }

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type EntityType { get; }
        /// <summary>
        /// 实体配置
        /// </summary>
        public TableAttribute ModifyResult { get; }
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
        public CurdBeforeEventArgs(Type entityType, CurdType curdType, string sql, DbParameter[] dbParms) :
            this(Guid.NewGuid(), new Stopwatch(), entityType, curdType, sql, dbParms)
        {
            this.Stopwatch.Start();
        }
        protected CurdBeforeEventArgs(Guid identifier, Stopwatch stopwatch, Type entityType, CurdType curdType, string sql, DbParameter[] dbParms)
        {
            this.Identifier = identifier;
            this.Stopwatch = stopwatch;
            this.EntityType = entityType;
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
            base(before.Identifier, before.StopwatchInternal, before.EntityType, before.CurdType, before.Sql, before.DbParms)
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
}