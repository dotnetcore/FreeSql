using System;
using System.Threading.Tasks;

namespace FreeSql.Interface {
	public interface ICache {

		/// <summary>
		/// 缓存数据时序列化方法，若无设置则默认使用 Json.net
		/// </summary>
		Func<object, string> Serialize { get; set; }
		/// <summary>
		/// 获取缓存数据时反序列化方法，若无设置则默认使用 Json.net
		/// </summary>
		Func<string, Type, object> Deserialize { get; set; }

		/// <summary>
		/// 缓存可序列化数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">缓存键</param>
		/// <param name="data">可序列化数据</param>
		/// <param name="timeoutSeconds">缓存秒数，&lt;=0时永久缓存</param>
		void Set<T>(string key, T data, int timeoutSeconds = 0);
		/// <summary>
		/// 循环或批量获取缓存数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		T Get<T>(string key);
		/// <summary>
		/// 循环或批量获取缓存数据
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		string Get(string key);
		/// <summary>
		/// 循环或批量删除缓存键
		/// </summary>
		/// <param name="keys">缓存键[数组]</param>
		void Remove(params string[] keys);
		/// <summary>
		/// 缓存壳
		/// </summary>
		/// <typeparam name="T">缓存类型</typeparam>
		/// <param name="key">缓存键</param>
		/// <param name="timeoutSeconds">缓存秒数</param>
		/// <param name="getData">获取源数据的函数</param>
		/// <returns></returns>
		T Shell<T>(string key, int timeoutSeconds, Func<T> getData);
		/// <summary>
		/// 缓存壳(哈希表)
		/// </summary>
		/// <typeparam name="T">缓存类型</typeparam>
		/// <param name="key">缓存键</param>
		/// <param name="field">字段</param>
		/// <param name="timeoutSeconds">缓存秒数</param>
		/// <param name="getData">获取源数据的函数</param>
		/// <returns></returns>
		T Shell<T>(string key, string field, int timeoutSeconds, Func<T> getData);

		/// <summary>
		/// 缓存可序列化数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">缓存键</param>
		/// <param name="data">可序列化数据</param>
		/// <param name="timeoutSeconds">缓存秒数，&lt;=0时永久缓存</param>
		Task SetAsync<T>(string key, T data, int timeoutSeconds = 0);
		/// <summary>
		/// 循环或批量获取缓存数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		Task<T> GetAsync<T>(string key);
		/// <summary>
		/// 循环或批量获取缓存数据
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		Task<string> GetAsync(string key);
		/// <summary>
		/// 循环或批量删除缓存键
		/// </summary>
		/// <param name="keys">缓存键[数组]</param>
		Task RemoveAsync(params string[] keys);
		/// <summary>
		/// 缓存壳
		/// </summary>
		/// <typeparam name="T">缓存类型</typeparam>
		/// <param name="key">缓存键</param>
		/// <param name="timeoutSeconds">缓存秒数</param>
		/// <param name="getDataAsync">获取源数据的函数</param>
		/// <returns></returns>
		Task<T> ShellAsync<T>(string key, int timeoutSeconds, Func<Task<T>> getDataAsync);
		/// <summary>
		/// 缓存壳(哈希表)
		/// </summary>
		/// <typeparam name="T">缓存类型</typeparam>
		/// <param name="key">缓存键</param>
		/// <param name="field">字段</param>
		/// <param name="timeoutSeconds">缓存秒数</param>
		/// <param name="getDataAsync">获取源数据的函数</param>
		/// <returns></returns>
		Task<T> ShellAsync<T>(string key, string field, int timeoutSeconds, Func<Task<T>> getDataAsync);
	}
}
