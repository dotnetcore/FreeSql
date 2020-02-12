using System.Linq;

namespace FreeSql.Internal
{
    public static class StringUtils
    {
        /// <summary>
        /// 将帕斯卡命名字符串转换为下划线分隔字符串
        /// <para></para>
        /// BigApple -> Big_Apple
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string PascalCaseToUnderScore(string str)
        {
            return string.Concat(str.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));
        }
    }
}