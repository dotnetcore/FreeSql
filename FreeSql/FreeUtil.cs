using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

public static class FreeUtil {

	private static DateTime dt1970 = new DateTime(1970, 1, 1);
	private static ThreadLocal<Random> rnd = new ThreadLocal<Random>();
	private static readonly int __staticMachine = ((0x00ffffff & Environment.MachineName.GetHashCode()) +
#if NETSTANDARD1_5 || NETSTANDARD1_6
			1
#else
			AppDomain.CurrentDomain.Id
#endif
			) & 0x00ffffff;
	private static readonly int __staticPid = Process.GetCurrentProcess().Id;
	private static int __staticIncrement = rnd.Value.Next();
	/// <summary>
	/// 生成类似Mongodb的ObjectId有序、不重复Guid
	/// </summary>
	/// <returns></returns>
	public static Guid NewMongodbId() {
		var now = DateTime.Now;
		var uninxtime = (int)now.Subtract(dt1970).TotalSeconds;
		int increment = Interlocked.Increment(ref __staticIncrement) & 0x00ffffff;
		var rand = rnd.Value.Next(0, int.MaxValue);
		var guid = $"{uninxtime.ToString("x8").PadLeft(8, '0')}{__staticMachine.ToString("x8").PadLeft(8, '0').Substring(2, 6)}{__staticPid.ToString("x8").PadLeft(8, '0').Substring(6, 2)}{increment.ToString("x8").PadLeft(8, '0')}{rand.ToString("x8").PadLeft(8, '0')}";
		return Guid.Parse(guid);
	}
}