using NpgsqlTypes;
using System;
using System.Collections;

namespace NpgsqlTypes {
	public static class FreeSqlExtensions {

		public static string To1010(this BitArray ba) {
			char[] ret = new char[ba.Length];
			for (int a = 0; a < ba.Length; a++) ret[a] = ba[a] ? '1' : '0';
			return new string(ret);
		}

		/// <summary>
		/// 将 1010101010 这样的二进制字符串转换成 BitArray
		/// </summary>
		/// <param name="_1010">1010101010</param>
		/// <returns></returns>
		public static BitArray ToBitArray(this string _1010Str) {
			if (_1010Str == null) return null;
			BitArray ret = new BitArray(_1010Str.Length);
			for (int a = 0; a < _1010Str.Length; a++) ret[a] = _1010Str[a] == '1';
			return ret;
		}

		public static NpgsqlRange<T> ToNpgsqlRange<T>(this string that) {
			var s = that;
			if (string.IsNullOrEmpty(s) || s == "empty") return NpgsqlRange<T>.Empty;
			string s1 = s.Trim('(', ')', '[', ']');
			string[] ss = s1.Split(new char[] { ',' }, 2);
			if (ss.Length != 2) return NpgsqlRange<T>.Empty;
			T t1 = default(T);
			T t2 = default(T);
			if (!string.IsNullOrEmpty(ss[0])) t1 = (T)Convert.ChangeType(ss[0], typeof(T));
			if (!string.IsNullOrEmpty(ss[1])) t2 = (T)Convert.ChangeType(ss[1], typeof(T));
			return new NpgsqlRange<T>(t1, s[0] == '[', s[0] == '(', t2, s[s.Length - 1] == ']', s[s.Length - 1] == ')');
		}
	}
}