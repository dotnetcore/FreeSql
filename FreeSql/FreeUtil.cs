using SafeObjectPool;
using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public static class FreeUtil {

	private static DateTime dt1970 = new DateTime(1970, 1, 1);
	private static ThreadLocal<Random> rnd = new ThreadLocal<Random>(() => new Random());
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

	internal static void PrevReheatConnectionPool(ObjectPool<DbConnection> pool) {
		var initTestOk = true;
		var initStartTime = DateTime.Now;
		var initConns = new ConcurrentBag<Object<DbConnection>>();

		try {
			var conn = pool.Get();
			initConns.Add(conn);
			pool.Policy.OnCheckAvailable(conn);
		} catch {
			initTestOk = false; //预热一次失败，后面将不进行
		}
		for (var a = 1; initTestOk && a < pool.Policy.PoolSize; a += 10) {
			if (initStartTime.Subtract(DateTime.Now).TotalSeconds > 3) break; //预热耗时超过3秒，退出
			var b = Math.Min(pool.Policy.PoolSize - a, 10); //每10个预热
			var initTasks = new Task[b];
			for (var c = 0; c < b; c++) {
				initTasks[c] = Task.Run(() => {
					try {
						var conn = pool.Get();
						initConns.Add(conn);
						pool.Policy.OnCheckAvailable(conn);
					} catch {
						initTestOk = false;  //有失败，下一组退出预热
					}
				});
			}
			Task.WaitAll(initTasks);
		}
		while (initConns.TryTake(out var conn)) pool.Return(conn);
	}
}