namespace FreeSql.Internal
{
    public enum StringConvertType
    {
        /// <summary>
        /// 不进行任何处理
        /// </summary>
        None = 0,

        /// <summary>
        /// 将帕斯卡命名字符串转换为下划线分隔字符串
        /// <para></para>
        /// BigApple -> Big_Apple
        /// </summary>
        PascalCaseToUnderscore,

        /// <summary>
        /// 将帕斯卡命名字符串转换为下划线分隔字符串，且转换为全大写
        /// <para></para>
        /// BigApple -> BIG_APPLE
        /// </summary>
        PascalCaseToUnderscoreWithUpper,

        /// <summary>
        /// 将帕斯卡命名字符串转换为下划线分隔字符串，且转换为全小写
        /// <para></para>
        /// BigApple -> big_apple
        /// </summary>
        PascalCaseToUnderscoreWithLower,

        /// <summary>
        /// 将字符串转换为大写
        /// <para></para>
        /// BigApple -> BIGAPPLE
        /// </summary>
        Upper,

        /// <summary>
        /// 将字符串转换为小写
        /// <para></para>
        /// BigApple -> bigapple
        /// </summary>
        Lower
    }
}