using System.Collections;

public static partial class XuguTypesExtensions
{
   
    public static string To1010(this BitArray ba)
    {
        char[] ret = new char[ba.Length];
        for (int a = 0; a < ba.Length; a++) ret[a] = ba[a] ? '1' : '0';
        return new string(ret);
    }

    /// <summary>
    /// 将 1010101010 这样的二进制字符串转换成 BitArray
    /// </summary>
    /// <param name="_1010Str">1010101010</param>
    /// <returns></returns>
    public static BitArray ToBitArray(this string _1010Str)
    {
        if (_1010Str == null) return null;
        BitArray ret = new BitArray(_1010Str.Length);
        for (int a = 0; a < _1010Str.Length; a++) ret[a] = _1010Str[a] == '1';
        return ret;
    }
}