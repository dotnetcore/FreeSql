// SonnetDBFunctions.cs
// SonnetDB 专有 SQL 函数扩展类，供 FreeSql Lambda 表达式中调用。
//
// ─────────────────────────────────────────────────────────────────────────────
// SonnetDB 核心概念速查
// ─────────────────────────────────────────────────────────────────────────────
// • MEASUREMENT  相当于关系型数据库中的"表"（Table）。
// • TAG          带索引的字符串维度列，用于过滤和分组（GROUP BY），不存储数值观测。
// • FIELD        数值或布尔类型的观测列（Observation），是实际分析的数据列。
// • time         每行隐式携带的时间戳列（Unix 毫秒整数），不需要在 Schema 中单独声明。
// • series       measurement + 所有 tag 值的有序组合，形成唯一的逻辑时间序列。
//
// ─────────────────────────────────────────────────────────────────────────────
// 使用方法
// ─────────────────────────────────────────────────────────────────────────────
// 1. 在 FreeSql Select 的 Where / ToList 的 Lambda 表达式里直接调用本类方法。
// 2. 调用形式：SonnetDBFunctions.Pid(row.Temperature, 25.0, 0.5, 0.1, 0.05)
//    FreeSql 会借助 [ExpressionCall] 机制将其翻译为对应的 SonnetDB SQL 片段。
// 3. 聚合函数（如 pid / spread / percentile）须配合 GROUP BY time(1m) 子句使用。
// 4. 窗口函数（如 difference / moving_average）会逐行输出结果，无需 GROUP BY。
//
// ─────────────────────────────────────────────────────────────────────────────
// 示例
// ─────────────────────────────────────────────────────────────────────────────
// // 查询温度的30秒移动平均，按1分钟分桶
// fsql.Select<Sensor>()
//     .GroupByRaw("time(1m)")
//     .ToList(row => new {
//         Bucket   = SonnetDBFunctions.TimeBucket("1m", row.Time),
//         MAvg     = SonnetDBFunctions.MovingAverage(row.Temperature, 30),
//         PidOut   = SonnetDBFunctions.Pid(row.Temperature, 25.0, 0.5, 0.1, 0.05)
//     });
// ─────────────────────────────────────────────────────────────────────────────

using FreeSql.DataAnnotations;
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
        // FreeSql ExpressionCall 必须字段，用于在翻译时传递上下文。
        // 每次表达式翻译在独立线程中完成，ThreadLocal 保证线程安全。
        static readonly ThreadLocal<ExpressionCallContext> context =
            new ThreadLocal<ExpressionCallContext>();

        // =====================================================================
        // 一、PID 工业过程控制函数
        // SonnetDB 1.1.0 新增，面向工业控制场景，实现增量式 PID 控制律。
        // PID 控制输出：u(t) = Kp·e(t) + Ki·∫e dt + Kd·de/dt
        // =====================================================================

        /// <summary>
        /// <b>PID 聚合函数</b>（SonnetDB 独有）。
        /// <para>在 GROUP BY time(...) 时间窗口内，基于 <paramref name="field"/> 与
        /// <paramref name="setpoint"/> 的误差历史，计算增量式 PID 控制律输出 u(t)。</para>
        /// <para>SQL：<c>pid(field, setpoint, kp, ki, kd)</c></para>
        /// <para>典型用途：实时闭环控制仿真、控制性能分析。</para>
        /// </summary>
        /// <param name="field">过程变量（Process Variable），如传感器测量值。</param>
        /// <param name="setpoint">目标设定值（Setpoint）。</param>
        /// <param name="kp">比例增益 Kp（Proportional）。</param>
        /// <param name="ki">积分增益 Ki（Integral）。</param>
        /// <param name="kd">微分增益 Kd（Derivative）。</param>
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
            ctx.Result = $"pid_series({ctx.ParsedContent["field"]}, {ctx.ParsedContent["setpoint"]}, " +
                         $"{ctx.ParsedContent["kp"]}, {ctx.ParsedContent["ki"]}, {ctx.ParsedContent["kd"]})";
            return default;
        }

        /// <summary>
        /// <b>PID 参数自动整定函数</b>（SonnetDB 独有）。
        /// <para>对 <paramref name="field"/> 列的阶跃响应数据进行 FOPDT 模型辨识，
        /// 然后按指定方法自动计算最优 Kp/Ki/Kd，返回 JSON 字符串
        /// <c>{"kp":...,"ki":...,"kd":...}</c>。</para>
        /// <para>SQL：<c>pid_estimate(field, method)</c></para>
        /// </summary>
        /// <param name="field">过程变量列（阶跃响应历史数据）。</param>
        /// <param name="method">整定方法：<c>'zn'</c>（Ziegler-Nichols）、
        /// <c>'cc'</c>（Cohen-Coon）、<c>'imc'</c>（Internal Model Control）。</param>
        /// <returns>JSON 字符串（含 kp/ki/kd 参数），FreeSql 翻译结果。</returns>
        public static string PidEstimate(double field, string method)
        {
            var ctx = context.Value;
            ctx.Result = $"pid_estimate({ctx.ParsedContent["field"]}, {ctx.ParsedContent["method"]})";
            return default;
        }

        // =====================================================================
        // 二、时序差分与变化率函数
        // 用于计算相邻数据点之间的变化，是时序分析的基础算子。
        // =====================================================================

        /// <summary>
        /// <b>差分</b>：返回当前值与上一个值的差 <c>value[t] - value[t-1]</c>。
        /// <para>SQL：<c>difference(field)</c></para>
        /// <para>注意：第一行无前驱值，结果为 NULL。</para>
        /// </summary>
        public static double Difference(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"difference({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>非负差分</b>：与 <see cref="Difference"/> 相同，但负值结果强制置 NULL。
        /// <para>适用于单调递增计数器（如网络流量字节数）发生溢出回绕时的处理。</para>
        /// <para>SQL：<c>non_negative_difference(field)</c></para>
        /// </summary>
        public static double NonNegativeDifference(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"non_negative_difference({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>变化率</b>（每秒）：<c>difference(field) / elapsed_seconds</c>。
        /// <para>SQL：<c>derivative(field)</c></para>
        /// </summary>
        public static double Derivative(double field)
        {
            var ctx = context.Value;
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
            ctx.Result = $"irate({ctx.ParsedContent["field"]})";
            return default;
        }

        // =====================================================================
        // 三、时序累积与积分函数
        // =====================================================================

        /// <summary>
        /// <b>累积和</b>：从第一行到当前行的滚动前缀和。
        /// <para>SQL：<c>cumulative_sum(field)</c></para>
        /// </summary>
        public static double CumulativeSum(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"cumulative_sum({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>梯形积分</b>：计算时间轴下方的面积（∫field dt），单位为 field 单位·秒。
        /// <para>SQL：<c>integral(field)</c></para>
        /// </summary>
        public static double Integral(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"integral({ctx.ParsedContent["field"]})";
            return default;
        }

        // =====================================================================
        // 四、时序平滑函数
        // =====================================================================

        /// <summary>
        /// <b>简单移动平均（SMA）</b>：对最近 <paramref name="n"/> 个样本求均值。
        /// <para>SQL：<c>moving_average(field, n)</c></para>
        /// </summary>
        /// <param name="field">待平滑的 FIELD 列。</param>
        /// <param name="n">滑动窗口大小（样本数）。</param>
        public static double MovingAverage(double field, int n)
        {
            var ctx = context.Value;
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
            ctx.Result = $"holt_winters({ctx.ParsedContent["field"]}, {ctx.ParsedContent["alpha"]}, {ctx.ParsedContent["beta"]})";
            return default;
        }

        // =====================================================================
        // 五、缺失值填充函数
        // 时序数据常因设备离线、采集延迟出现空缺，以下函数用于插值/填充。
        // =====================================================================

        /// <summary>
        /// <b>常数填充</b>：用指定常数替换 NULL 值。
        /// <para>SQL：<c>fill(field, value)</c></para>
        /// </summary>
        public static double Fill(double field, double value)
        {
            var ctx = context.Value;
            ctx.Result = $"fill({ctx.ParsedContent["field"]}, {ctx.ParsedContent["value"]})";
            return default;
        }

        /// <summary>
        /// <b>最近值前向填充（LOCF，Last Observation Carried Forward）</b>：
        /// 用最近一次非 NULL 值填充当前 NULL 行。
        /// <para>SQL：<c>locf(field)</c></para>
        /// </summary>
        public static double Locf(double field)
        {
            var ctx = context.Value;
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
            ctx.Result = $"interpolate({ctx.ParsedContent["field"]})";
            return default;
        }

        // =====================================================================
        // 六、状态分析函数
        // 用于分析布尔/离散状态序列的变化情况。
        // =====================================================================

        /// <summary>
        /// <b>状态变更标记</b>：当前行的值与上一行不同时输出 1，相同时输出 0。
        /// <para>SQL：<c>state_changes(field)</c></para>
        /// </summary>
        public static long StateChanges(double field)
        {
            var ctx = context.Value;
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
            ctx.Result = $"state_duration({ctx.ParsedContent["field"]})";
            return default;
        }

        // =====================================================================
        // 七、扩展统计聚合函数
        // SonnetDB 在标准 COUNT/SUM/AVG/MIN/MAX 之外提供的额外聚合函数。
        // 这些函数在 GROUP BY time(...) 聚合查询中使用。
        // =====================================================================

        /// <summary>
        /// <b>极差（Spread）</b>：窗口内最大值 - 最小值，反映数据波动范围。
        /// <para>SQL：<c>spread(field)</c></para>
        /// </summary>
        public static double Spread(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"spread({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>标准差（StdDev）</b>：样本标准差 √( Σ(xi-x̄)² / (n-1) )。
        /// <para>SQL：<c>stddev(field)</c></para>
        /// </summary>
        public static double Stddev(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"stddev({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>方差（Variance）</b>：样本方差。
        /// <para>SQL：<c>variance(field)</c></para>
        /// </summary>
        public static double Variance(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"variance({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>众数（Mode）</b>：出现频次最多的值；若多个值并列则返回最小者。
        /// <para>SQL：<c>mode(field)</c></para>
        /// </summary>
        public static double Mode(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"mode({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>中位数（Median）</b>：相当于 percentile(field, 50)。
        /// <para>SQL：<c>median(field)</c></para>
        /// </summary>
        public static double Median(double field)
        {
            var ctx = context.Value;
            ctx.Result = $"median({ctx.ParsedContent["field"]})";
            return default;
        }

        /// <summary>
        /// <b>任意分位数（Percentile）</b>：返回第 <paramref name="p"/> 百分位的值。
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

        /// <summary>
        /// <b>频率直方图（Histogram）</b>：将值域按 <paramref name="binWidth"/> 分桶，
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

        // =====================================================================
        // 八、预测与异常检测函数（SonnetDB 独有）
        // =====================================================================

        /// <summary>
        /// <b>异常检测（Anomaly）</b>：对每个样本打标（0=正常 / 1=异常）。
        /// <para>SQL：<c>anomaly(field, method, k)</c></para>
        /// </summary>
        /// <param name="field">FIELD 列。</param>
        /// <param name="method">检测方法：<c>'zscore'</c>（Z-Score）或 <c>'iqr'</c>（四分位距）。</param>
        /// <param name="k">敏感度系数（Z-Score 阈值或 IQR 倍数，通常取 2~3）。</param>
        /// <returns>0 或 1（正常 / 异常标记）。</returns>
        public static int Anomaly(double field, string method, double k)
        {
            var ctx = context.Value;
            ctx.Result = $"anomaly({ctx.ParsedContent["field"]}, {ctx.ParsedContent["method"]}, {ctx.ParsedContent["k"]})";
            return default;
        }

        /// <summary>
        /// <b>变点检测（Changepoint）</b>：使用 CUSUM 算法识别均值漂移点。
        /// <para>SQL：<c>changepoint(field, method, k, drift)</c></para>
        /// </summary>
        /// <param name="field">FIELD 列。</param>
        /// <param name="method">检测方法，目前支持 <c>'cusum'</c>。</param>
        /// <param name="k">允许的参考偏差（CUSUM 的 slack 参数）。</param>
        /// <param name="drift">漂移阈值，超过此值时发出变点信号。</param>
        /// <returns>CUSUM 统计量，超过漂移阈值的点即为变点。</returns>
        public static double Changepoint(double field, string method, double k, double drift)
        {
            var ctx = context.Value;
            ctx.Result = $"changepoint({ctx.ParsedContent["field"]}, {ctx.ParsedContent["method"]}, " +
                         $"{ctx.ParsedContent["k"]}, {ctx.ParsedContent["drift"]})";
            return default;
        }

        // =====================================================================
        // 九、向量距离函数（配合 VECTOR(N) 列使用）
        // SonnetDB 原生支持高维浮点向量，兼容 pgvector 运算符语义。
        // VECTOR(N) 列用于存储 N 维嵌入向量（Embedding），支持 KNN 近邻检索。
        // =====================================================================

        /// <summary>
        /// <b>余弦距离</b>：1 - cosine_similarity(a, b)，范围 [0, 2]；0 表示完全相同方向。
        /// <para>SQL：<c>cosine_distance(a, b)</c>（等价运算符 <c>&lt;=&gt;</c>）</para>
        /// </summary>
        public static double CosineDistance(object a, object b)
        {
            var ctx = context.Value;
            ctx.Result = $"cosine_distance({ctx.ParsedContent["a"]}, {ctx.ParsedContent["b"]})";
            return default;
        }

        /// <summary>
        /// <b>欧氏距离（L2 距离）</b>：√( Σ(ai - bi)² )。
        /// <para>SQL：<c>l2_distance(a, b)</c>（等价运算符 <c>&lt;-&gt;</c>）</para>
        /// </summary>
        public static double L2Distance(object a, object b)
        {
            var ctx = context.Value;
            ctx.Result = $"l2_distance({ctx.ParsedContent["a"]}, {ctx.ParsedContent["b"]})";
            return default;
        }

        /// <summary>
        /// <b>内积（负相似度）</b>：-( Σ ai·bi )。
        /// <para>SQL：<c>inner_product(a, b)</c>（等价运算符 <c>&lt;#&gt;</c>）</para>
        /// <para>注意：SonnetDB 返回的是负内积，数值越小表示越相似。</para>
        /// </summary>
        public static double InnerProduct(object a, object b)
        {
            var ctx = context.Value;
            ctx.Result = $"inner_product({ctx.ParsedContent["a"]}, {ctx.ParsedContent["b"]})";
            return default;
        }

        /// <summary>
        /// <b>向量 L2 范数</b>：√( Σ ai² )。
        /// <para>SQL：<c>vector_norm(a)</c></para>
        /// </summary>
        public static double VectorNorm(object a)
        {
            var ctx = context.Value;
            ctx.Result = $"vector_norm({ctx.ParsedContent["a"]})";
            return default;
        }

        // =====================================================================
        // 十、地理空间函数（配合 GEOPOINT 列使用）
        // GEOPOINT 列存储 (latitude, longitude) 坐标对，格式为 "lat,lon"（字符串）。
        // 距离计算使用 Haversine 球面公式，单位为米。
        // =====================================================================

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

        /// <summary>
        /// <b>球面距离</b>：计算 GEOPOINT 列中坐标与目标点之间的 Haversine 距离，单位：米。
        /// <para>SQL：<c>geo_distance(field, lat, lon)</c></para>
        /// </summary>
        /// <param name="field">GEOPOINT 列。</param>
        /// <param name="lat">目标点纬度（十进制度）。</param>
        /// <param name="lon">目标点经度（十进制度）。</param>
        public static double GeoDistance(string field, double lat, double lon)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_distance({ctx.ParsedContent["field"]}, {ctx.ParsedContent["lat"]}, {ctx.ParsedContent["lon"]})";
            return default;
        }

        /// <summary>
        /// <b>方位角</b>：从 GEOPOINT 列中坐标到目标点的方向角（0~360 度，北为 0）。
        /// <para>SQL：<c>geo_bearing(field, lat, lon)</c></para>
        /// </summary>
        public static double GeoBearing(string field, double lat, double lon)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_bearing({ctx.ParsedContent["field"]}, {ctx.ParsedContent["lat"]}, {ctx.ParsedContent["lon"]})";
            return default;
        }

        /// <summary>
        /// <b>圆形地理围栏</b>：判断 GEOPOINT 列坐标是否在以 (centerLat, centerLon)
        /// 为圆心、<paramref name="radiusM"/> 米为半径的圆形区域内，满足条件返回 1。
        /// <para>SQL：<c>geo_within(field, centerLat, centerLon, radiusM)</c></para>
        /// <para>典型用途：地理围栏告警、车辆进出场检测。</para>
        /// </summary>
        /// <param name="field">GEOPOINT 列。</param>
        /// <param name="centerLat">围栏中心纬度。</param>
        /// <param name="centerLon">围栏中心经度。</param>
        /// <param name="radiusM">围栏半径（米）。</param>
        public static int GeoWithin(string field, double centerLat, double centerLon, double radiusM)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_within({ctx.ParsedContent["field"]}, {ctx.ParsedContent["centerLat"]}, " +
                         $"{ctx.ParsedContent["centerLon"]}, {ctx.ParsedContent["radiusM"]})";
            return default;
        }

        /// <summary>
        /// <b>矩形地理围栏</b>：判断 GEOPOINT 列坐标是否在给定经纬度矩形内，满足条件返回 1。
        /// <para>SQL：<c>geo_bbox(field, minLat, minLon, maxLat, maxLon)</c></para>
        /// </summary>
        public static int GeoBbox(string field, double minLat, double minLon, double maxLat, double maxLon)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_bbox({ctx.ParsedContent["field"]}, {ctx.ParsedContent["minLat"]}, " +
                         $"{ctx.ParsedContent["minLon"]}, {ctx.ParsedContent["maxLat"]}, {ctx.ParsedContent["maxLon"]})";
            return default;
        }

        /// <summary>
        /// <b>移动速度</b>：基于相邻两个 GEOPOINT 样本之间的距离与时间差计算速度（米/秒）。
        /// <para>SQL：<c>geo_speed(field)</c></para>
        /// <para>典型用途：车辆/设备超速检测、轨迹平均速度分析。</para>
        /// </summary>
        public static double GeoSpeed(string field)
        {
            var ctx = context.Value;
            ctx.Result = $"geo_speed({ctx.ParsedContent["field"]})";
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
        /// <b>轨迹质心（聚合）</b>：返回时间窗口内所有 GEOPOINT 点的几何质心，
        /// 结果格式同 GEOPOINT（"lat,lon" 字符串）。
        /// <para>SQL：<c>trajectory_centroid(field)</c></para>
        /// </summary>
        public static string TrajectoryCentroid(string field)
        {
            var ctx = context.Value;
            ctx.Result = $"trajectory_centroid({ctx.ParsedContent["field"]})";
            return default;
        }

        // =====================================================================
        // 十一、时间函数
        // SonnetDB 的 time 列存储 Unix 毫秒整数，以下函数用于时间对齐与提取。
        // =====================================================================

        /// <summary>
        /// <b>时间桶对齐（TimeBucket）</b>：将时间戳按固定步长 <paramref name="duration"/> 对齐，
        /// 常用于 SELECT 子句中生成等间隔时间序列，与 PostgreSQL <c>date_trunc</c> 行为一致。
        /// <para>SQL：<c>time_bucket(duration, time)</c></para>
        /// <para>示例：<c>time_bucket('1m', time)</c> 将时间戳截断到分钟边界。</para>
        /// </summary>
        /// <param name="duration">时间桶步长，支持单位：<c>ms</c>（毫秒）、<c>s</c>（秒）、
        /// <c>m</c>（分钟）、<c>h</c>（小时）、<c>d</c>（天）。字符串字面量需加单引号，
        /// 例如 <c>"'1m'"</c>。</param>
        /// <param name="time">时间戳列或表达式（Unix 毫秒整数）。</param>
        /// <returns>对齐后的时间桶起始时间戳（Unix 毫秒整数）。</returns>
        public static long TimeBucket(string duration, long time)
        {
            var ctx = context.Value;
            ctx.Result = $"time_bucket({ctx.ParsedContent["duration"]}, {ctx.ParsedContent["time"]})";
            return default;
        }
    }
}
