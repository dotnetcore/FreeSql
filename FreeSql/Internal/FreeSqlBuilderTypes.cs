using System;

namespace FreeSql.Internal
{
    /// <summary>
    /// 映射优先级，默认： Attribute > FluentApi > Aop
    /// </summary>
    public enum MappingPriorityType
    {
        /// <summary>
        /// 实体特性<para></para>
        /// [Table(Name = "tabname")]<para></para>
        /// [Column(Name = "table_id")]
        /// </summary>
        Attribute = 0,

        /// <summary>
        /// 流式接口<para></para>
        /// fsql.CodeFirst.ConfigEntity(a => a.Name("tabname"))<para></para>
        /// fsql.CodeFirst.ConfigEntity(a => a.Property(b => b.Id).Name("table_id"))
        /// </summary>
        FluentApi,

        /// <summary>
        /// AOP 特性 https://github.com/dotnetcore/FreeSql/wiki/AOP<para></para>
        /// fsql.Aop.ConfigEntity += (_, e) => e.ModifyResult.Name = "public.tabname";<para></para>
        /// fsql.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = "table_id";<para></para>
        /// </summary>
        Aop
    }

    public enum NameConvertType
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

        ///// <summary>
        ///// 将下划线分隔字符串命名字符串转换为帕斯卡
        ///// <para></para>
        ///// big_apple -> BigApple
        ///// </summary>
        //UnderscoreToPascalCase,

        /// <summary>
        /// 将字符串转换为大写
        /// <para></para>
        /// BigApple -> BIGAPPLE
        /// </summary>
        ToUpper,

        /// <summary>
        /// 将字符串转换为小写
        /// <para></para>
        /// BigApple -> bigapple
        /// </summary>
        ToLower
    }

}