using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.DataAnnotations
{
    /// <summary>
    /// 自定义表达式函数解析<para></para>
    /// 注意：请使用静态方法、或者在类上标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExpressionCallAttribute : Attribute
    {
    }
    /// <summary>
    /// 自定义表达式函数解析的时候，指定参数不解析 SQL，而是直接传进来
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RawValueAttribute : Attribute
    {
    }

    public class ExpressionCallContext
    {
        internal ExpressionCallContext()
        {
            Utility = new DefaultUtility { _context = this };
        }

        public IUtility Utility { get; }

        internal CommonExpression _commonExp;
        internal CommonExpression.ExpTSC _tsc;
        /// <summary>
        /// 数据库类型，可用于适配多种数据库环境
        /// </summary>
        public DataType DataType { get; internal set; }

        /// <summary>
        /// 已解析的表达式中参数内容
        /// </summary>
        public Dictionary<string, string> ParsedContent { get; } = new Dictionary<string, string>();
        /// <summary>
        /// 表达式原始值
        /// </summary>
        public Dictionary<string, Expression> RawExpression { get; } = new Dictionary<string, Expression>();

        /// <summary>
        /// 主对象的参数化对象，可重塑其属性
        /// </summary>
        public DbParameter DbParameter { get; internal set; }

        /// <summary>
        /// 可附加参数化对象<para></para>
        /// 注意：本属性只有 Where 的表达式解析才可用
        /// </summary>
        public List<DbParameter> UserParameters { get; internal set; }

        /// <summary>
        /// 将 c# 对象转换为 SQL
        /// </summary>
        public Func<object, string> FormatSql { get; internal set; }

        /// <summary>
        /// 返回表达式函数表示的 SQL 字符串
        /// </summary>
        public string Result { get; set; }

        public interface IUtility
        {
            /// <summary>
            /// 获取实体元数据
            /// </summary>
            /// <param name="entityType"></param>
            /// <returns></returns>
            TableInfo GetTableByEntity(Type entityType);

            /// <summary>
            /// 解析表达式
            /// </summary>
            /// <param name="exp"></param>
            /// <returns></returns>
            string ParseExpression(Expression exp);
        }

        class DefaultUtility : IUtility
        {
            internal ExpressionCallContext _context;

            public TableInfo GetTableByEntity(Type entityType) => _context?._commonExp._common.GetTableByEntity(entityType);

            public string ParseExpression(Expression exp) => _context?._commonExp.ExpressionLambdaToSql(exp, _context._tsc.CloneDisableDiyParse());
        }
    }
}
