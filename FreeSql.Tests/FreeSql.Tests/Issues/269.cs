using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Npgsql.LegacyPostgis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Data.SqlClient;
using kwlib;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FreeSql.Tests.Issues
{
    public class _269
    {
        [Fact]
        public void ToSql()
        {
            var fsql = g.sqlite;

            var sql1 = fsql.Select<Edi>()
                .Where(a =>
                    DbFunc222.FuncExpression<EDIUseMode?, EDIUseMode, EDIUseMode>("ISNULL", a.UseMode, EDIUseMode.Both) == EDIUseMode.Both)
                .ToSql(); //这么写报错：ExpressionTree 转换类型错误，值(a.[EDI_USE_MODE])，类型(System.String)，目标类型(System.Nullable`1[EDIUseMode])，Requested value 'a.[EDI_USE_MODE]' was not found.
            var sql2 = fsql.Select<Edi>()
                .Where(a =>
                    DbFunc222.FuncExpression<int?, int, int>("ISNULL", (int)a.UseMode, (int)EDIUseMode.Both) == (int)EDIUseMode.Both)
                .ToSql(); //这么写就不报错

        }

        public enum EDIUseMode : byte
        {
            OutPut = 1,
            Import = 2,
            Both = 3,
        }
        [Table(Name = "EDI222")]
        public class Edi
        {
            [Column(Name = "EDI_ID")] public long Id { get; set; }
            [Column(Name = "EDI_USE_MODE")] public EDIUseMode? UseMode { get; set; }
        }
    }

    [ExpressionCall]
    public static class DbFunc222
    {
        static ThreadLocal<ExpressionCallContext> context = new ThreadLocal<ExpressionCallContext>();
        //自定义数据库方法
        public static TOut FuncExpression<T1, T2, TOut>(string func, T1 p1, T2 p2)
        {
            var up = context.Value;
            var values = up.ParsedContent.Values.ToArray();
            context.Value.Result = $"{func}({values[1]}, {values[2]})";
            return default;
        }
    }
}
