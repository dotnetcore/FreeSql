using System;

namespace FreeSql.Provider.SonnetDB.Attributes
{
    /// <summary>
    /// 将实体标记为 SonnetDB 3.1 关系表。
    /// 未标记的实体继续使用提供程序历史上的时序测量映射；该模型请使用
    /// <see cref="SonnetDBTagAttribute"/> 和 <see cref="SonnetDBFieldAttribute"/>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class SonnetDBTableAttribute : Attribute
    {
    }
}
