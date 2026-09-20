// SonnetDB 专有函数定义。
using FreeSql.DataAnnotations;
using SonnetDB.Model;
using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// SonnetDB 专有 SQL 函数集合。
    /// <para>所有方法均通过 FreeSql <see cref="ExpressionCallAttribute"/> 机制翻译为对应的 SonnetDB SQL 片段；
    /// 不应在非 FreeSql Lambda 上下文中直接调用这些方法（调用时仅返回类型默认值）。</para>
    /// </summary>
    [ExpressionCall]
    public static class SonnetDBFunctions
    {
        // ExpressionCall 的表达式翻译上下文。
        static readonly ThreadLocal<ExpressionCallContext> context =
            new ThreadLocal<ExpressionCallContext>();

        /// <summary>
        /// <b>PID 聚合函数</b>（SonnetDB 独有）。
        /// <para>在 GROUP BY time(...) 时间窗口内，基于 <paramref name="field"/> 与
        /// <paramref name="setpoint"/> 的误差历史，计算增量式 PID 控制律输出 u(t)。</para>
        /// <para>SQL：<c>pid(field, setpoint, kp, ki, kd)</c></para>
        /// <para>典型用途：实时闭环控制仿真、控制性能分析。</para>
        /// </summary>
        /// <param name="field">过程变量，如传感器测量值。</param>
        /// <param name="setpoint">目标设定值。</param>
        /// <param name="kp">比例增益 Kp。</param>
        /// <param name="ki">积分增益 Ki。</param>
        /// <param name="kd">微分增益 Kd。</param>
        /// <returns>PID 控制律输出值（FreeSql 表达式解析结果，运行时返回 <c>default</c>）。</returns>
        public static double Pid(double field, double setpoint, double kp, double ki, double kd)
        {
            var ctx = context.Value;
            ctx.Result = $"pid({ctx.ParsedContent["field"]}, {ctx.ParsedContent["setpoint"]}, " +
                         $"{ctx.ParsedContent["kp"]}, {ctx.ParsedContent["ki"]}, {ctx.ParsedContent["kd"]})";
            return default;
        }

        /// <summary>
        /// <b>PID 窗口流式函数</b>（SonnetDB 独有）。
        /// <para>逐行输出 PID 控制量，无需 GROUP BY，适合对完整时序轨迹进行控制仿真。</para>
        /// <para>SQL：<c>pid_series(field, setpoint, kp, ki, kd)</c></para>
        /// </summary>
        /// <param name="field">过程变量列。</param>
        /// <param name="setpoint">目标设定值。</param>
        /// <param name="kp">比例增益。</param>
        /// <param name="ki">积分增益。</param>
        /// <param name="kd">微分增益。</param>
        public static double PidSeries(double field, double setpoint, double kp, double ki, double kd)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "pid_series");
            ctx.Result = $"pid_series({ctx.ParsedContent["field"]}, {ctx.ParsedContent["setpoint"]}, " +
                         $"{ctx.ParsedContent["kp"]}, {ctx.ParsedContent["ki"]}, {ctx.ParsedContent["kd"]})";
            return default;
        }

        /// <summary>
        /// <b>PID 参数自动整定函数</b>（SonnetDB 独有）。
        /// <para>对 <paramref name="field"/> 列的阶跃响应数据进行 FOPDT 模型辨识，
        /// 然后按指定方法自动计算最优 Kp/Ki/Kd，返回 JSON 字符串
        /// <c>{"kp":...,"ki":...,"kd":...}</c>。</para>
        /// <para>SQL：<c>pid_estimate(field, method, step_magnitude, initial_fraction,
        /// final_fraction, imc_lambda)</c></para>，参数名称保持与 SonnetDB 函数一致。
        /// </summary>
        /// <param name="field">过程变量列（阶跃响应历史数据）。</param>
        /// <param name="method">整定方法：<c>'zn'</c>（齐格勒-尼科尔斯法）、
        /// <c>'cc'</c>（科恩-库恩法）、<c>'imc'</c>（内部模型控制），也可传入
        /// <c>null</c> 使用默认的 ZN 方法。该参数必须是字符串字面量或 NULL。</param>
        /// <param name="stepMagnitude">输入阶跃幅值；传入 <c>null</c> 时按 1.0 处理。
        /// 必须是数值字面量或 NULL。</param>
        /// <param name="initialFraction">初始响应分位点，通常为 0.1；传入 <c>null</c> 时使用默认值。
        /// 必须是数值字面量或 NULL，取值范围为 (0, 0.5)。</param>
        /// <param name="finalFraction">最终响应分位点，通常为 0.1；传入 <c>null</c> 时使用默认值。
        /// 必须是数值字面量或 NULL，取值范围为 (0, 0.5)。</param>
        /// <param name="imcLambda">IMC 滤波时间常数；传入 <c>null</c> 时按过程滞后时间处理。
        /// 必须是数值字面量或 NULL。</param>
        /// <returns>JSON 字符串（含 kp/ki/kd 参数），FreeSql 翻译结果。</returns>
        public static string PidEstimate(
            double field,
            string method,
            double? stepMagnitude,
            double? initialFraction,
            double? finalFraction,
            double? imcLambda)
        {
            var ctx = context.Value;
            ctx.Result = $"pid_estimate({ctx.ParsedContent["field"]}, {ctx.ParsedContent["method"]}, " +
                         $"{ctx.ParsedContent["stepMagnitude"]}, {ctx.ParsedContent["initialFraction"]}, " +
                         $"{ctx.ParsedContent["finalFraction"]}, {ctx.ParsedContent["imcLambda"]})";
            return default;
        }

        /// <summary>
        /// 旧版两参数入口。SonnetDB 3.1 要求完整的六参数形式，缺省参数统一传入
        /// <c>NULL</c>，由数据库使用默认整定选项。
        /// </summary>
        [Obsolete("SonnetDB 3.1 建议使用包含六个参数的 PidEstimate 重载。")]
        public static string PidEstimate(double field, string method)
        {
            var ctx = context.Value;
            ctx.Result = $"pid_estimate({ctx.ParsedContent["field"]}, {ctx.ParsedContent["method"]}, NULL, NULL, NULL, NULL)";
            return default;
        }


        /// <summary>
        /// <b>差分</b>：返回当前值与上一个值的差 <c>value[t] - value[t-1]</c>。
        /// <para>SQL：<c>difference(field)</c></para>
        /// <para>注意：第一行无前驱值，结果为 NULL。</para>
        /// </summary>
        public static double Difference(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "difference");
            ctx.Result = $"difference({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>增量</b>：返回当前值与上一值的有符号差值。
        /// <para>SQL：<c>delta(field)</c></para>
        /// </summary>
        public static double Delta(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "delta");
            ctx.Result = $"delta({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>计数器增长量</b>：只累计正向变化，忽略计数器重置产生的负差值。
        /// <para>SQL：<c>increase(field)</c></para>
        /// </summary>
        public static double Increase(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "increase");
            ctx.Result = $"increase({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>非负差分</b>为旧版兼容入口。SonnetDB 3.1 没有
        /// <c>non_negative_difference</c> 函数；<see cref="Increase"/> 的计数器增长语义
        /// 也不能替代原有 API，因此翻译时会明确拒绝。
        /// </summary>
        public static double NonNegativeDifference(double field)
        {
            throw new NotSupportedException(
                "SonnetDB 3.1 不支持 non_negative_difference；请根据业务语义改用 Increase，或在应用层处理负差值。");
        }

        /// <summary>
        /// <b>变化率</b>（每秒）：<c>difference(field) / elapsed_seconds</c>。
        /// <para>SQL：<c>derivative(field)</c></para>
        /// </summary>
        public static double Derivative(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "derivative");
            ctx.Result = $"derivative({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>非负变化率</b>：与 <see cref="Derivative"/> 相同，负值置 NULL。
        /// <para>适用于单调计数器回绕场景。</para>
        /// <para>SQL：<c>non_negative_derivative(field)</c></para>
        /// </summary>
        public static double NonNegativeDerivative(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "non_negative_derivative");
            ctx.Result = $"non_negative_derivative({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>区间变化率</b>：当前时间窗口内的增量 / 时间跨度（秒）。
        /// <para>SQL：<c>rate(field)</c></para>
        /// </summary>
        public static double Rate(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "rate");
            ctx.Result = $"rate({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>瞬时变化率</b>：仅基于最近两个样本计算，对突发变化更敏感。
        /// <para>SQL：<c>irate(field)</c></para>
        /// </summary>
        public static double Irate(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "irate");
            ctx.Result = $"irate({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>累积和</b>：从第一行到当前行的滚动前缀和。
        /// <para>SQL：<c>cumulative_sum(field)</c></para>
        /// </summary>
        public static double CumulativeSum(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "cumulative_sum");
            ctx.Result = $"cumulative_sum({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>滚动求和</b>：按窗口顺序返回截至当前行的累计和。
        /// <para>SQL：<c>running_sum(field)</c></para>
        /// </summary>
        public static double RunningSum(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "running_sum");
            ctx.Result = $"running_sum({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>滚动最小值</b>：返回截至当前行的最小值。
        /// <para>SQL：<c>running_min(field)</c></para>
        /// </summary>
        public static double RunningMin(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "running_min");
            ctx.Result = $"running_min({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>滚动最大值</b>：返回截至当前行的最大值。
        /// <para>SQL：<c>running_max(field)</c></para>
        /// </summary>
        public static double RunningMax(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "running_max");
            ctx.Result = $"running_max({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>梯形积分</b>：计算时间轴下方的面积（∫field dt），单位为 field 单位·秒。
        /// <para>SQL：<c>integral(field)</c></para>
        /// </summary>
        public static double Integral(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "integral");
            ctx.Result = $"integral({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>简单移动平均（SMA）</b>：对最近 <paramref name="n"/> 个样本求均值。
        /// <para>SQL：<c>moving_average(field, n)</c></para>
        /// </summary>
        /// <param name="field">待平滑的 FIELD 列。</param>
        /// <param name="n">滑动窗口大小（样本数）。</param>
        public static double MovingAverage(double field, int n)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "moving_average");
            ctx.Result = $"moving_average({ctx.ParsedContent["field"]}, {ctx.ParsedContent["n"]})";
            return default;
        }

        /// <summary>
        /// <b>指数加权移动平均（EWMA）</b>：
        /// <c>ewma[t] = alpha * value[t] + (1 - alpha) * ewma[t-1]</c>。
        /// <para>SQL：<c>ewma(field, alpha)</c></para>
        /// </summary>
        /// <param name="field">待平滑的 FIELD 列。</param>
        /// <param name="alpha">平滑系数，范围 (0, 1]，越小平滑效果越强。</param>
        public static double Ewma(double field, double alpha)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "ewma");
            ctx.Result = $"ewma({ctx.ParsedContent["field"]}, {ctx.ParsedContent["alpha"]})";
            return default;
        }

        /// <summary>
        /// <b>Holt-Winters 双指数平滑</b>：同时追踪水平和趋势分量，适合带趋势的时序。
        /// <para>SQL：<c>holt_winters(field, alpha, beta)</c></para>
        /// </summary>
        /// <param name="field">FIELD 列。</param>
        /// <param name="alpha">水平平滑系数 α，范围 (0, 1)。</param>
        /// <param name="beta">趋势平滑系数 β，范围 (0, 1)。</param>
        public static double HoltWinters(double field, double alpha, double beta)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "holt_winters");
            ctx.Result = $"holt_winters({ctx.ParsedContent["field"]}, {ctx.ParsedContent["alpha"]}, {ctx.ParsedContent["beta"]})";
            return default;
        }

        /// <summary>
        /// <b>常数填充</b>：用指定常数替换 NULL 值。
        /// <para>SQL：<c>fill(field, value)</c></para>
        /// </summary>
        public static double Fill(double field, double value)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "fill");
            ctx.Result = $"fill({ctx.ParsedContent["field"]}, {ctx.ParsedContent["value"]})";
            return default;
        }

        /// <summary>
        /// <b>最近值前向填充（LOCF）</b>：
        /// 用最近一次非 NULL 值填充当前 NULL 行。
        /// <para>SQL：<c>locf(field)</c></para>
        /// </summary>
        public static double Locf(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "locf");
            ctx.Result = $"locf({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>线性插值</b>：在相邻两个非 NULL 样本之间按时间比例线性插值。
        /// <para>SQL：<c>interpolate(field)</c></para>
        /// </summary>
        public static double Interpolate(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "interpolate");
            ctx.Result = $"interpolate({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>状态变更标记</b>：当前行的值与上一行不同时输出 1，相同时输出 0。
        /// <para>SQL：<c>state_changes(field)</c></para>
        /// </summary>
        public static long StateChanges(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "state_changes");
            ctx.Result = $"state_changes({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>统计字符串状态列的变更次数，生成 <c>state_changes(field)</c>。</summary>
        public static long StateChanges(string field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "state_changes");
            ctx.Result = $"state_changes({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>统计布尔状态列的变更次数，生成 <c>state_changes(field)</c>。</summary>
        public static long StateChanges(bool field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "state_changes");
            ctx.Result = $"state_changes({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>状态持续时长</b>：当前状态自上次变更以来已持续的毫秒数。
        /// <para>SQL：<c>state_duration(field)</c></para>
        /// </summary>
        public static long StateDuration(double field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "state_duration");
            ctx.Result = $"state_duration({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>计算字符串状态列的持续时长，生成 <c>state_duration(field)</c>。</summary>
        public static long StateDuration(string field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "state_duration");
            ctx.Result = $"state_duration({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>计算布尔状态列的持续时长，生成 <c>state_duration(field)</c>。</summary>
        public static long StateDuration(bool field)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "state_duration");
            ctx.Result = $"state_duration({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>极差</b>：窗口内最大值 - 最小值，反映数据波动范围。
        /// <para>SQL：<c>spread(field)</c></para>
        /// </summary>
        public static double Spread(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"spread({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>标准差</b>：样本标准差 √( Σ(xi-x̄)² / (n-1) )。
        /// <para>SQL：<c>stddev(field)</c></para>
        /// </summary>
        public static double Stddev(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"stddev({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>方差</b>：样本方差。
        /// <para>SQL：<c>variance(field)</c></para>
        /// </summary>
        public static double Variance(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"variance({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>众数</b>：出现频次最多的值；若多个值并列则返回最小者。
        /// <para>SQL：<c>mode(field)</c></para>
        /// </summary>
        public static double Mode(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"mode({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>返回字符串 FIELD 列的众数，生成 <c>mode(field)</c>。</summary>
        public static string Mode(string field)
        {
            var ctx = context.Value;
            ctx.Result = $"mode({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>返回布尔 FIELD 列的众数，生成 <c>mode(field)</c>。</summary>
        public static bool Mode(bool field)
        {
            var ctx = context.Value;
            ctx.Result = $"mode({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>中位数</b>：相当于 percentile(field, 50)。
        /// <para>SQL：<c>median(field)</c></para>
        /// </summary>
        public static double Median(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"median({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>任意分位数</b>：返回第 <paramref name="p"/> 百分位的值。
        /// <para>SQL：<c>percentile(field, p)</c></para>
        /// </summary>
        /// <param name="field">FIELD 列。</param>
        /// <param name="p">百分位，范围 [0, 100]。</param>
        public static double Percentile(double field, double p)
        {
            var ctx = context.Value;
            ctx.Result = $"percentile({ctx.ParsedContent["field"]}, {ctx.ParsedContent["p"]})";
            return default;
        }

        /// <summary>
        /// <b>P50（中位数）</b>：等同于 percentile(field, 50)。
        /// <para>SQL：<c>p50(field)</c></para>
        /// </summary>
        public static double P50(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"p50({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>P90（第 90 百分位）</b>：等同于 percentile(field, 90)。
        /// <para>SQL：<c>p90(field)</c></para>
        /// </summary>
        public static double P90(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"p90({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>P95（第 95 百分位）</b>：等同于 percentile(field, 95)。
        /// <para>SQL：<c>p95(field)</c></para>
        /// </summary>
        public static double P95(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"p95({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>P99（第 99 百分位）</b>：等同于 percentile(field, 99)。
        /// <para>SQL：<c>p99(field)</c></para>
        /// </summary>
        public static double P99(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"p99({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>去重计数（DistinctCount）</b>：统计不重复值的数量（HyperLogLog 近似）。
        /// <para>SQL：<c>distinct_count(field)</c></para>
        /// </summary>
        public static long DistinctCount(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"distinct_count({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>统计字符串 FIELD 列的不重复值数量，生成 <c>distinct_count(field)</c>。</summary>
        public static long DistinctCount(string field)
        {
            var ctx = context.Value;
            ctx.Result = $"distinct_count({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>统计布尔 FIELD 列的不重复值数量，生成 <c>distinct_count(field)</c>。</summary>
        public static long DistinctCount(bool field)
        {
            var ctx = context.Value;
            ctx.Result = $"distinct_count({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>频率直方图</b>：将值域按 <paramref name="binWidth"/> 分桶，
        /// 返回每个桶的计数，结果为 JSON 数组。
        /// <para>SQL：<c>histogram(field, binWidth)</c></para>
        /// </summary>
        /// <param name="field">FIELD 列。</param>
        /// <param name="binWidth">每个桶的宽度。</param>
        public static string Histogram(double field, double binWidth)
        {
            var ctx = context.Value;
            ctx.Result = $"histogram({ctx.ParsedContent["field"]}, {ctx.ParsedContent["binWidth"]})";
            return default;
        }

        /// <summary>
        /// <b>TDigest 聚合</b>：返回当前窗口的 TDigest JSON 摘要。
        /// <para>SQL：<c>tdigest_agg(field)</c></para>
        /// </summary>
        public static string TDigestAgg(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"tdigest_agg({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// TDigest 聚合的简写入口，生成与 <see cref="TDigestAgg"/> 相同的 SQL。
        /// </summary>
        public static string TDigest(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"tdigest_agg({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>向量质心</b>：计算窗口内 VECTOR 列逐维平均值。
        /// <para>SQL：<c>centroid(field)</c></para>
        /// </summary>
        public static object Centroid(object field)
        {
            var ctx = context.Value;
            ctx.Result = $"centroid({GetVectorSql(ctx, "field")})";
            return default;
        }

        /// <summary>
        /// <b>异常检测</b>旧版兼容入口。SonnetDB 3.1 实际返回布尔值，
        /// 新代码请使用 <see cref="IsAnomaly"/>。
        /// <para>SQL：<c>anomaly(field, method, k)</c></para>
        /// </summary>
        /// <param name="field">FIELD 列。</param>
        /// <param name="method">检测方法：<c>'zscore'</c>（标准分数）、<c>'mad'</c>（绝对中位差）或
        /// <c>'iqr'</c>（四分位距）。</param>
        /// <param name="k">敏感度系数（标准分数阈值、MAD 阈值或 IQR 倍数，通常取 2~3）。</param>
        /// <returns>不返回结果；调用时会提示改用布尔入口。</returns>
        [Obsolete("SonnetDB 3.1 的 anomaly 返回布尔值，请改用 IsAnomaly。")]
        public static int Anomaly(double field, string method, double k)
        {
            throw new NotSupportedException(
                "SonnetDB 3.1 的 anomaly 返回布尔值，旧版 int 入口会导致结果类型不匹配；请改用 IsAnomaly。");
        }

        /// <summary>
        /// <b>异常检测</b>：对每个样本返回是否异常的布尔值。
        /// <para>SQL：<c>anomaly(field, method, threshold)</c>；method 必须是字符串字面量，
        /// threshold 必须是正数值字面量。</para>
        /// <para>非空样本不足或输入值为 NULL 时，SonnetDB 可能返回 NULL；如需保留该语义，
        /// 请在 DTO 中使用可空布尔类型。</para>
        /// </summary>
        public static bool IsAnomaly(double field, string method, double threshold)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "anomaly");
            ctx.Result = $"anomaly({ctx.ParsedContent["field"]}, {ctx.ParsedContent["method"]}, {ctx.ParsedContent["threshold"]})";
            return default;
        }

        /// <summary>
        /// <b>变点检测</b>旧版兼容入口。SonnetDB 3.1 实际返回布尔值，
        /// 新代码请使用 <see cref="IsChangepoint(double, string, double)"/>。
        /// <para>SQL：<c>changepoint(field, method, k, drift)</c></para>
        /// </summary>
        /// <param name="field">FIELD 列。</param>
        /// <param name="method">检测方法，目前支持 <c>'cusum'</c>。</param>
        /// <param name="k">允许的参考偏差（CUSUM 的松弛参数）。</param>
        /// <param name="drift">漂移阈值，超过此值时发出变点信号。</param>
        /// <returns>不返回结果；调用时会提示改用布尔入口。</returns>
        [Obsolete("SonnetDB 3.1 的 changepoint 返回布尔值，请改用 IsChangepoint。")]
        public static double Changepoint(double field, string method, double k, double drift)
        {
            throw new NotSupportedException(
                "SonnetDB 3.1 的 changepoint 返回布尔值，旧版 double 入口会导致结果类型不匹配；请改用 IsChangepoint。");
        }

        /// <summary>
        /// <b>变点检测</b>：使用 CUSUM 算法返回当前样本是否为变点。
        /// <para>SQL：<c>changepoint(field, method, threshold)</c>；method 必须为
        /// <c>'cusum'</c> 字符串字面量，threshold 必须是正数值字面量。</para>
        /// </summary>
        public static bool IsChangepoint(double field, string method, double threshold)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "changepoint");
            ctx.Result = $"changepoint({ctx.ParsedContent["field"]}, {ctx.ParsedContent["method"]}, {ctx.ParsedContent["threshold"]})";
            return default;
        }

        /// <summary>
        /// <b>变点检测</b>：使用指定漂移参数的 CUSUM 算法返回当前样本是否为变点。
        /// <para>SQL：<c>changepoint(field, method, threshold, drift)</c>；method 必须为
        /// <c>'cusum'</c> 字符串字面量，threshold 必须为正数值字面量，drift 必须为非负数值字面量。</para>
        /// </summary>
        public static bool IsChangepoint(double field, string method, double threshold, double drift)
        {
            var ctx = context.Value;
            EnsureWindowFunctionSupported(ctx, "changepoint");
            ctx.Result = $"changepoint({ctx.ParsedContent["field"]}, {ctx.ParsedContent["method"]}, " +
                         $"{ctx.ParsedContent["threshold"]}, {ctx.ParsedContent["drift"]})";
            return default;
        }

        /// <summary>
        /// <b>余弦距离</b>：1 - cosine_similarity(a, b)，范围 [0, 2]；0 表示完全相同方向。
        /// <para>SQL：<c>cosine_distance(a, b)</c>（等价运算符 <c>&lt;=&gt;</c>）</para>
        /// </summary>
        public static double CosineDistance(object a, object b)
        {
            var ctx = context.Value;
            ctx.Result = $"cosine_distance({GetVectorSql(ctx, "a")}, {GetVectorSql(ctx, "b")})";
            return default;
        }

        /// <summary>
        /// <b>欧氏距离（L2 距离）</b>：√( Σ(ai - bi)² )。
        /// <para>SQL：<c>l2_distance(a, b)</c>（等价运算符 <c>&lt;-&gt;</c>）</para>
        /// </summary>
        public static double L2Distance(object a, object b)
        {
            var ctx = context.Value;
            ctx.Result = $"l2_distance({GetVectorSql(ctx, "a")}, {GetVectorSql(ctx, "b")})";
            return default;
        }

        /// <summary>
        /// <b>内积</b>：Σ ai·bi。
        /// <para>SQL：<c>inner_product(a, b)</c></para>
        /// <para>SonnetDB 返回普通内积；同维归一化向量的结果越大表示方向越相近。</para>
        /// </summary>
        public static double InnerProduct(object a, object b)
        {
            var ctx = context.Value;
            ctx.Result = $"inner_product({GetVectorSql(ctx, "a")}, {GetVectorSql(ctx, "b")})";
            return default;
        }

        /// <summary>
        /// <b>向量 L2 范数</b>：√( Σ ai² )。
        /// <para>SQL：<c>vector_norm(a)</c></para>
        /// </summary>
        public static double VectorNorm(object a)
        {
            var ctx = context.Value;
            ctx.Result = $"vector_norm({GetVectorSql(ctx, "a")})";
            return default;
        }

        /// <summary>读取向量检索结果的距离伪列。</summary>
        public static double VectorDistance()
        {
            var ctx = context.Value;
            ctx.Result = "vector_distance()";
            return default;
        }

        /// <summary>读取向量检索结果的归一化分数伪列。</summary>
        public static double VectorScore()
        {
            var ctx = context.Value;
            ctx.Result = "vector_score()";
            return default;
        }

        /// <summary>读取文档全文检索结果的 BM25 分数伪列。</summary>
        public static double Bm25Score()
        {
            var ctx = context.Value;
            ctx.Result = "bm25_score()";
            return default;
        }

        /// <summary>读取文档全文与向量融合结果的综合分数伪列。</summary>
        public static double HybridScore()
        {
            var ctx = context.Value;
            ctx.Result = "hybrid_score()";
            return default;
        }

        /// <summary>读取时序测量 KNN 与文档融合结果的时序测量距离伪列。</summary>
        public static double MeasurementDistance()
        {
            var ctx = context.Value;
            ctx.Result = "measurement_distance()";
            return default;
        }

        /// <summary>读取时序测量 KNN 与文档融合结果的时序测量得分伪列。</summary>
        public static double MeasurementScore()
        {
            var ctx = context.Value;
            ctx.Result = "measurement_score()";
            return default;
        }

        /// <summary>读取文档向量距离伪列。</summary>
        public static double DocumentVectorDistance()
        {
            var ctx = context.Value;
            ctx.Result = "document_vector_distance()";
            return default;
        }

        /// <summary>读取文档向量分数伪列。</summary>
        public static double DocumentVectorScore()
        {
            var ctx = context.Value;
            ctx.Result = "document_vector_score()";
            return default;
        }

        /// <summary>读取融合检索结果的文本分数伪列。</summary>
        public static double TextScore()
        {
            var ctx = context.Value;
            ctx.Result = "text_score()";
            return default;
        }

        /// <summary>
        /// 将两个 16 位 Modbus 寄存器按指定字节序解码为有符号 32 位整数。
        /// <para>SQL：<c>modbus_int32(first_register, second_register, byte_order)</c>。</para>
        /// <para>字节序支持 <c>ABCD</c>、<c>BADC</c>、<c>CDAB</c> 和 <c>DCBA</c>；
        /// 输入为 NULL 时数据库返回 NULL。</para>
        /// </summary>
        public static long ModbusInt32(object firstRegister, object secondRegister, string byteOrder)
        {
            var ctx = context.Value;
            ctx.Result = $"modbus_int32({ctx.ParsedContent["firstRegister"]}, " +
                         $"{ctx.ParsedContent["secondRegister"]}, {ctx.ParsedContent["byteOrder"]})";
            return default;
        }

        /// <summary>
        /// 将两个 16 位 Modbus 寄存器按指定字节序解码为无符号 32 位整数。
        /// <para>SQL：<c>modbus_uint32(first_register, second_register, byte_order)</c>；
        /// 结果以 SonnetDB 的 64 位整数返回。</para>
        /// </summary>
        public static long ModbusUInt32(object firstRegister, object secondRegister, string byteOrder)
        {
            var ctx = context.Value;
            ctx.Result = $"modbus_uint32({ctx.ParsedContent["firstRegister"]}, " +
                         $"{ctx.ParsedContent["secondRegister"]}, {ctx.ParsedContent["byteOrder"]})";
            return default;
        }

        /// <summary>
        /// 将两个 16 位 Modbus 寄存器按指定字节序解码为 IEEE-754 单精度值。
        /// <para>SQL：<c>modbus_float32(first_register, second_register, byte_order)</c>；
        /// 结果以 SonnetDB 的 64 位浮点数返回。</para>
        /// </summary>
        public static double ModbusFloat32(object firstRegister, object secondRegister, string byteOrder)
        {
            var ctx = context.Value;
            ctx.Result = $"modbus_float32({ctx.ParsedContent["firstRegister"]}, " +
                         $"{ctx.ParsedContent["secondRegister"]}, {ctx.ParsedContent["byteOrder"]})";
            return default;
        }

        static string GetVectorSql(ExpressionCallContext ctx, string name)
        {
            string sql = null;
            if (ctx == null || ctx.ParsedContent.TryGetValue(name, out sql) == false || sql == null)
                return sql;

            if (ctx.RawExpression.TryGetValue(name, out var expression) == false || IsVectorExpression(expression) == false)
                return sql;

            var trimmed = sql.Trim();
            if (trimmed.StartsWith("(", StringComparison.Ordinal) && trimmed.EndsWith(")", StringComparison.Ordinal))
            {
                // 参数化数组没有 SonnetDB 3.1 的 ADO.NET 绑定协议，不能把它误写成普通字符串。
                if (trimmed.IndexOf('@') >= 0 || trimmed.IndexOf('?') >= 0)
                    throw new NotSupportedException(
                        "SonnetDB 3.1 的 ADO.NET 参数绑定不支持 VECTOR 参数；请启用 UseNoneCommandParameter(true) 生成向量字面量，或升级 SonnetDB。" );
                var elements = trimmed.Substring(1, trimmed.Length - 2);
                return "[" + Regex.Replace(elements, @",\s*", ", ") + "]";
            }
            return sql;
        }

        static bool IsVectorExpression(Expression expression)
        {
            while (expression is UnaryExpression unary &&
                   (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked || unary.NodeType == ExpressionType.Quote))
                expression = unary.Operand;
            var type = expression?.Type;
            return type == typeof(float[]) || type == typeof(Memory<float>) ||
                type == typeof(ReadOnlyMemory<float>) || type == typeof(System.Collections.Generic.IReadOnlyList<float>);
        }

        /// <summary>
        /// 从关系表 JSON 列中提取 JSON 路径对应的标量值。
        /// <para>SQL：<c>json_value(json_column, '$.path')</c></para>
        /// <para><paramref name="path"/> 会经过 FreeSql 表达式解析和 SQL 转义；不要把用户输入直接拼接到 SQL。</para>
        /// </summary>
        /// <param name="json">关系表中的 JSON 文本列。</param>
        /// <param name="path">SonnetDB JSON 路径表达式，例如 <c>$.device.name</c>。</param>
        public static string JsonValue(object json, string path)
        {
            var ctx = context.Value;
            ctx.Result = $"json_value({ctx.ParsedContent["json"]}, {RequireJsonPathLiteral(ctx)})";
            return default;
        }

        /// <summary>字符串 JSON 列的 <see cref="JsonValue(object, string)"/> 重载。</summary>
        public static string JsonValue(string json, string path)
        {
            var ctx = context.Value;
            ctx.Result = $"json_value({ctx.ParsedContent["json"]}, {RequireJsonPathLiteral(ctx)})";
            return default;
        }

        /// <summary>调用 SonnetDB 的正则匹配标量函数。</summary>
        public static bool RegexpLike(string value, string pattern)
        {
            var ctx = context.Value;
            ctx.Result = $"regexp_like({ctx.ParsedContent["value"]}, {ctx.ParsedContent["pattern"]})";
            return default;
        }

        /// <summary>
        /// 调用 SonnetDB 文档集合的全文索引匹配谓词。
        /// <para>索引名必须是调用时可确定的单个标识符；<paramref name="mode"/> 仅支持
        /// <c>exact</c> 或 <c>fuzzy</c>。</para>
        /// </summary>
        /// <param name="indexName">全文索引名。</param>
        /// <param name="field">索引字段、JSON 路径或 <c>*</c>。</param>
        /// <param name="query">全文检索文本。</param>
        /// <param name="topK">候选结果上限；传入 <c>null</c> 或零时使用 SonnetDB 默认值。</param>
        /// <param name="mode">可选匹配模式：<c>exact</c> 或 <c>fuzzy</c>。</param>
        public static bool Match([RawValue] string indexName, string field, string query,
            [RawValue] int? topK = null, [RawValue] string mode = null)
        {
            var ctx = context.Value;
            var sql = $"match({QuoteMatchIndexName(indexName)}, {ctx.ParsedContent["field"]}, " +
                      $"{ctx.ParsedContent["query"]}";
            if (topK.GetValueOrDefault() > 0)
                sql += $", {topK.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            if (string.IsNullOrWhiteSpace(mode) == false)
            {
                var normalizedMode = mode.Trim().ToLowerInvariant();
                if (normalizedMode != "exact" && normalizedMode != "fuzzy")
                    throw new ArgumentException("SonnetDB match 的 mode 只支持 exact 或 fuzzy。", nameof(mode));
                sql += $", {ctx.FormatSql(normalizedMode)}";
            }
            ctx.Result = sql + ")";
            return default;
        }

        static string QuoteMatchIndexName(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("SonnetDB match 的全文索引名不能为空。", nameof(indexName));
            return $"\"{indexName.Trim().Replace("\"", "\"\"")}\"";
        }

        /// <summary>
        /// SonnetDB 3.1 要求 json_value 的路径参数必须是字符串字面量，不能使用列值或其他动态表达式。
        /// </summary>
        static string RequireJsonPathLiteral(ExpressionCallContext ctx)
        {
            if (ctx == null || !ctx.ParsedContent.TryGetValue("path", out var path)
                || IsSqlStringLiteral(path) == false)
            {
                throw new NotSupportedException(
                    "SonnetDB 3.1 的 json_value 路径必须是字符串字面量，不支持动态路径。" );
            }

            return path;
        }

        static bool IsSqlStringLiteral(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var text = value.Trim();
            if (text.Length >= 2 && text[0] == '\'' && text[text.Length - 1] == '\'') return true;
            return text.Length >= 3 && (text[0] == 'N' || text[0] == 'n')
                && text[1] == '\'' && text[text.Length - 1] == '\'';
        }

        /// <summary>
        /// <b>提取纬度</b>：从 GEOPOINT 列中解析出纬度值（十进制度）。
        /// <para>SQL：<c>lat(field)</c></para>
        /// </summary>
        public static double Lat(string field)
        {
            var ctx = context.Value;
            ctx.Result = $"lat({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>从强类型 GEOPOINT 表达式提取纬度。</summary>
        public static double Lat(GeoPoint field)
        {
            var ctx = context.Value;
            ctx.Result = $"lat({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>提取经度</b>：从 GEOPOINT 列中解析出经度值（十进制度）。
        /// <para>SQL：<c>lon(field)</c></para>
        /// </summary>
        public static double Lon(string field)
        {
            var ctx = context.Value;
            ctx.Result = $"lon({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>从强类型 GEOPOINT 表达式提取经度。</summary>
        public static double Lon(GeoPoint field)
        {
            var ctx = context.Value;
            ctx.Result = $"lon({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>球面距离</b>：使用半正矢公式计算两个 GEOPOINT 的距离，单位：米。
        /// <para>SQL：<c>geo_distance(point1, point2)</c></para>
        /// </summary>
        /// <param name="point1">第一个 GEOPOINT 表达式。</param>
        /// <param name="point2">第二个 GEOPOINT 表达式。</param>
        public static double GeoDistance(object point1, object point2)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_distance({ctx.ParsedContent["point1"]}, {ctx.ParsedContent["point2"]})";
            return default;
        }

        /// <summary>
        /// 旧版“字段到目标坐标”入口。目标坐标改为 SonnetDB 要求的
        /// <c>POINT(lat, lon)</c> 字面量，而不是字符串字面量。
        /// </summary>
        [Obsolete("SonnetDB 3.1 建议使用 GeoDistance(point1, point2) 重载。")]
        public static double GeoDistance(string field, double lat, double lon)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_distance({ctx.ParsedContent["field"]}, POINT({ctx.ParsedContent["lat"]}, {ctx.ParsedContent["lon"]}))";
            return default;
        }

        /// <summary>
        /// <b>方位角</b>：从第一个 GEOPOINT 指向第二个 GEOPOINT 的初始方向角
        /// （0~360 度，北为 0）。
        /// <para>SQL：<c>geo_bearing(point1, point2)</c></para>
        /// </summary>
        /// <param name="point1">起始 GEOPOINT 表达式。</param>
        /// <param name="point2">目标 GEOPOINT 表达式。</param>
        public static double GeoBearing(object point1, object point2)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_bearing({ctx.ParsedContent["point1"]}, {ctx.ParsedContent["point2"]})";
            return default;
        }

        /// <summary>旧版“字段到目标坐标”方位角入口，目标坐标生成 POINT 字面量。</summary>
        [Obsolete("SonnetDB 3.1 建议使用 GeoBearing(point1, point2) 重载。")]
        public static double GeoBearing(string field, double lat, double lon)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_bearing({ctx.ParsedContent["field"]}, POINT({ctx.ParsedContent["lat"]}, {ctx.ParsedContent["lon"]}))";
            return default;
        }

        /// <summary>
        /// <b>圆形地理围栏</b>旧版兼容入口。SonnetDB 3.1 实际返回布尔值，
        /// 新代码请使用 <see cref="IsGeoWithin"/>。
        /// <para>SQL：<c>geo_within(field, centerLat, centerLon, radiusM)</c></para>
        /// <para>典型用途：地理围栏告警、车辆进出场检测。</para>
        /// </summary>
        /// <param name="field">GEOPOINT 列。</param>
        /// <param name="centerLat">围栏中心纬度。</param>
        /// <param name="centerLon">围栏中心经度。</param>
        /// <param name="radiusM">围栏半径（米）。</param>
        [Obsolete("SonnetDB 3.1 的 geo_within 返回布尔值，请改用 IsGeoWithin。")]
        public static int GeoWithin(string field, double centerLat, double centerLon, double radiusM)
        {
            throw new NotSupportedException(
                "SonnetDB 3.1 的 geo_within 返回布尔值，旧版 int 入口会导致结果类型不匹配；请改用 IsGeoWithin。");
        }

        /// <summary>
        /// 判断 GEOPOINT 是否位于圆形围栏内。
        /// <para>SQL：<c>geo_within(point, lat, lon, radius)</c></para>
        /// </summary>
        public static bool IsGeoWithin(object point, double centerLat, double centerLon, double radiusM)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_within({ctx.ParsedContent["point"]}, {ctx.ParsedContent["centerLat"]}, " +
                         $"{ctx.ParsedContent["centerLon"]}, {ctx.ParsedContent["radiusM"]})";
            return default;
        }

        /// <summary>
        /// <b>矩形地理围栏</b>旧版兼容入口。SonnetDB 3.1 实际返回布尔值，
        /// 新代码请使用 <see cref="IsGeoBbox"/>。
        /// <para>SQL：<c>geo_bbox(field, minLat, minLon, maxLat, maxLon)</c></para>
        /// </summary>
        [Obsolete("SonnetDB 3.1 的 geo_bbox 返回布尔值，请改用 IsGeoBbox。")]
        public static int GeoBbox(string field, double minLat, double minLon, double maxLat, double maxLon)
        {
            throw new NotSupportedException(
                "SonnetDB 3.1 的 geo_bbox 返回布尔值，旧版 int 入口会导致结果类型不匹配；请改用 IsGeoBbox。");
        }

        /// <summary>
        /// 判断 GEOPOINT 是否位于经纬度矩形内。
        /// <para>SQL：<c>geo_bbox(point, min_lat, min_lon, max_lat, max_lon)</c></para>
        /// </summary>
        public static bool IsGeoBbox(object point, double minLat, double minLon, double maxLat, double maxLon)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_bbox({ctx.ParsedContent["point"]}, {ctx.ParsedContent["minLat"]}, " +
                         $"{ctx.ParsedContent["minLon"]}, {ctx.ParsedContent["maxLat"]}, {ctx.ParsedContent["maxLon"]})";
            return default;
        }

        /// <summary>
        /// <b>移动速度</b>：基于两个 GEOPOINT 样本之间的距离与毫秒时间差计算速度（米/秒）。
        /// <para>SQL：<c>geo_speed(point1, point2, elapsed_ms)</c></para>
        /// <para>典型用途：车辆/设备超速检测、轨迹平均速度分析。</para>
        /// </summary>
        /// <param name="point1">起始 GEOPOINT 表达式。</param>
        /// <param name="point2">结束 GEOPOINT 表达式。</param>
        /// <param name="elapsedMs">两点之间的时间间隔（毫秒），必须大于零。</param>
        public static double GeoSpeed(object point1, object point2, double elapsedMs)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_speed({ctx.ParsedContent["point1"]}, {ctx.ParsedContent["point2"]}, " +
                         $"{ctx.ParsedContent["elapsedMs"]})";
            return default;
        }

        /// <summary>
        /// 旧版单字段速度入口。SonnetDB 3.1 要求两个点和明确的时间间隔，无法
        /// 从单列调用安全推导 elapsed_ms，因此保留入口并在翻译时明确拒绝。
        /// </summary>
        [Obsolete("SonnetDB 3.1 的 GeoSpeed 需要两个 GEOPOINT 和 elapsed_ms，请改用三参数重载。")]
        public static double GeoSpeed(string field)
        {
            throw new NotSupportedException(
                "SonnetDB 3.1 的 geo_speed 需要两个 GEOPOINT 和大于零的 elapsed_ms，旧版单字段入口无法安全迁移。");
        }

        /// <summary>
        /// <b>坐标系转换</b>：在 WGS84、GCJ02、BD09 之间转换 GEOPOINT。
        /// <para>SQL：<c>geo_transform(point, from_system, to_system)</c></para>
        /// </summary>
        public static object GeoTransform(object point, string fromSystem, string toSystem)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_transform({ctx.ParsedContent["point"]}, " +
                         $"{ctx.ParsedContent["fromSystem"]}, {ctx.ParsedContent["toSystem"]})";
            return default;
        }

        /// <summary>将 WGS84 坐标转换为 GCJ02，生成 <c>geo_wgs84_to_gcj02</c>。</summary>
        public static object GeoWgs84ToGcj02(object point)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_wgs84_to_gcj02({ctx.ParsedContent["point"]})";
            return default;
        }

        /// <summary>将 GCJ02 坐标转换为 WGS84，生成 <c>geo_gcj02_to_wgs84</c>。</summary>
        public static object GeoGcj02ToWgs84(object point)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_gcj02_to_wgs84({ctx.ParsedContent["point"]})";
            return default;
        }

        /// <summary>将 GCJ02 坐标转换为 BD09，生成 <c>geo_gcj02_to_bd09</c>。</summary>
        public static object GeoGcj02ToBd09(object point)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_gcj02_to_bd09({ctx.ParsedContent["point"]})";
            return default;
        }

        /// <summary>将 BD09 坐标转换为 GCJ02，生成 <c>geo_bd09_to_gcj02</c>。</summary>
        public static object GeoBd09ToGcj02(object point)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_bd09_to_gcj02({ctx.ParsedContent["point"]})";
            return default;
        }

        /// <summary>将 WGS84 坐标转换为 BD09，生成 <c>geo_wgs84_to_bd09</c>。</summary>
        public static object GeoWgs84ToBd09(object point)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_wgs84_to_bd09({ctx.ParsedContent["point"]})";
            return default;
        }

        /// <summary>将 BD09 坐标转换为 WGS84，生成 <c>geo_bd09_to_wgs84</c>。</summary>
        public static object GeoBd09ToWgs84(object point)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_bd09_to_wgs84({ctx.ParsedContent["point"]})";
            return default;
        }

        /// <summary>
        /// PostGIS 风格距离别名，生成 <c>st_distance(point1, point2)</c>。
        /// </summary>
        public static double StDistance(object point1, object point2)
        {
            var ctx = context.Value;
            ctx.Result = $"st_distance({ctx.ParsedContent["point1"]}, {ctx.ParsedContent["point2"]})";
            return default;
        }

        /// <summary>
        /// PostGIS 风格圆形围栏别名，生成 <c>st_within(point, lat, lon, radius)</c>。
        /// </summary>
        public static bool StWithin(object point, double centerLat, double centerLon, double radiusM)
        {
            var ctx = context.Value;
            ctx.Result = $"st_within({ctx.ParsedContent["point"]}, {ctx.ParsedContent["centerLat"]}, " +
                         $"{ctx.ParsedContent["centerLon"]}, {ctx.ParsedContent["radiusM"]})";
            return default;
        }

        /// <summary>
        /// PostGIS 风格距离围栏别名，生成 <c>st_dwithin(point, lat, lon, radius)</c>。
        /// </summary>
        public static bool StDWithin(object point, double centerLat, double centerLon, double radiusM)
        {
            var ctx = context.Value;
            ctx.Result = $"st_dwithin({ctx.ParsedContent["point"]}, {ctx.ParsedContent["centerLat"]}, " +
                         $"{ctx.ParsedContent["centerLon"]}, {ctx.ParsedContent["radiusM"]})";
            return default;
        }

        /// <summary>
        /// <b>轨迹总长度（聚合）</b>：在 GROUP BY 时间窗口内，将所有 GEOPOINT 样本
        /// 依时间排序后，计算首尾相接的球面折线总长度（米）。
        /// <para>SQL：<c>trajectory_length(field)</c></para>
        /// </summary>
        public static double TrajectoryLength(string field)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_length({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>轨迹总长度（聚合）</b>的 GEOPOINT 入口。
        /// <para>SonnetDB 要求参数为 GEOPOINT FIELD；使用 <c>object</c> 形参可直接传入
        /// <c>SonnetDB.Model.GeoPoint</c> 实体属性，同时保留旧版字符串入口的源码兼容性。</para>
        /// </summary>
        public static double TrajectoryLength(object field)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_length({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>接受强类型 <c>GeoPoint</c> 的轨迹总长度入口。</summary>
        public static double TrajectoryLength(GeoPoint field)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_length({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>轨迹质心（聚合）</b>：返回时间窗口内所有 GEOPOINT 点的几何质心，
        /// 结果为 SonnetDB 原生 GEOPOINT。
        /// <para>SQL：<c>trajectory_centroid(field)</c></para>
        /// </summary>
        public static string TrajectoryCentroid(string field)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_centroid({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>轨迹质心（聚合）</b>的 GEOPOINT 入口。
        /// <para>SonnetDB 3.1 返回 <c>GeoPoint</c>；结果读取到实体时请使用
        /// <c>SonnetDB.Model.GeoPoint</c> 或 <c>object</c> 属性。</para>
        /// </summary>
        public static object TrajectoryCentroid(object field)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_centroid({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>接受强类型 <c>GeoPoint</c> 并保留原生返回类型的轨迹质心入口。</summary>
        public static GeoPoint TrajectoryCentroid(GeoPoint field)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_centroid({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>轨迹包围盒（聚合）</b>：返回窗口内轨迹的最小外接矩形 JSON。
        /// <para>SQL：<c>trajectory_bbox(field)</c></para>
        /// </summary>
        public static string TrajectoryBbox(object field)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_bbox({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>轨迹最大速度（聚合）</b>：按相邻点时间差计算窗口内最大速度。
        /// <para>SQL：<c>trajectory_speed_max(field, time)</c></para>
        /// </summary>
        public static double TrajectorySpeedMax(object field, object time)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_speed_max({ctx.ParsedContent["field"]}, {ctx.ParsedContent["time"]})";
            return default;
        }

        /// <summary>
        /// <b>轨迹平均速度（聚合）</b>：按相邻点时间差计算窗口内平均速度。
        /// <para>SQL：<c>trajectory_speed_avg(field, time)</c></para>
        /// </summary>
        public static double TrajectorySpeedAvg(object field, object time)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_speed_avg({ctx.ParsedContent["field"]}, {ctx.ParsedContent["time"]})";
            return default;
        }

        /// <summary>
        /// <b>轨迹 P95 速度（聚合）</b>：返回窗口内速度的第 95 百分位。
        /// <para>SQL：<c>trajectory_speed_p95(field, time)</c></para>
        /// </summary>
        public static double TrajectorySpeedP95(object field, object time)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_speed_p95({ctx.ParsedContent["field"]}, {ctx.ParsedContent["time"]})";
            return default;
        }

        static void EnsureWindowFunctionSupported(ExpressionCallContext ctx, string functionName)
        {
            if (ctx == null) return;

            foreach (var expression in ctx.RawExpression.Values)
            {
                if (ContainsRelationshipEntity(ctx, expression) == false) continue;
                throw new NotSupportedException(
                    $"SonnetDB 3.1 的窗口函数 {functionName} 仅支持时序测量，关系表不支持窗口函数；" +
                    "请改写为聚合/标量查询或在应用层处理。");
            }
        }

        static bool ContainsRelationshipEntity(ExpressionCallContext ctx, Expression expression)
        {
            if (expression == null) return false;
            var visitor = new RelationshipEntityVisitor(ctx);
            visitor.Visit(expression);
            return visitor.IsRelationship;
        }

        sealed class RelationshipEntityVisitor : ExpressionVisitor
        {
            readonly ExpressionCallContext _context;

            public RelationshipEntityVisitor(ExpressionCallContext context)
            {
                _context = context;
            }

            public bool IsRelationship { get; private set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (IsRelationship == false && node.Type != typeof(string) && node.Type != typeof(object))
                {
                    var table = _context.Utility.GetTableByEntity(node.Type);
                    IsRelationship = table != null && SonnetDBModel.IsTable(table);
                }
                return base.VisitParameter(node);
            }
        }

        /// <summary>
        /// <b>时间桶对齐</b>在 SonnetDB 3.1 中不是标量函数。请调用
        /// <c>GroupByRaw("time(1m)")</c>，而不是在 Select Lambda 中调用此方法。
        /// </summary>
        /// <param name="duration">保留的兼容参数。</param>
        /// <param name="time">保留的兼容参数。</param>
        /// <exception cref="NotSupportedException">SonnetDB 3.1 没有对应的标量 SQL 函数。</exception>
        public static long TimeBucket(string duration, long time)
        {
            throw new NotSupportedException(
                "SonnetDB 3.1 不支持 time_bucket 标量函数；请使用 GroupByRaw(\"time(1m)\") 进行时序分桶。");
        }
    }
}
