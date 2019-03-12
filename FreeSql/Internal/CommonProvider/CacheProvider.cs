using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider {
	class CacheProvider : ICache {

		public IDistributedCache Cache { get; private set; }
		private bool CacheSupportMultiRemove = false;
		private static DateTime dt1970 = new DateTime(1970, 1, 1);

		public CacheProvider(IDistributedCache cache, ILogger log) {
			if (cache == null) cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions { }));
			Cache = cache;
			var key1 = $"testCacheSupportMultiRemoveFreeSql{Guid.NewGuid().ToString("N")}";
			var key2 = $"testCacheSupportMultiRemoveFreeSql{Guid.NewGuid().ToString("N")}";
			Cache.Set(key1, new byte[] { 65 });
			Cache.Set(key2, new byte[] { 65 });
			try { Cache.Remove($"{key1}|{key2}"); } catch { } // redis-cluster 不允许执行 multi keys 命令
			CacheSupportMultiRemove = Cache.Get(key1) == null && cache.Get(key2) == null;
			if (CacheSupportMultiRemove == false) {
				//log.LogWarning("FreeSql Warning: 低性能, IDistributedCache 没实现批量删除缓存 Cache.Remove(\"key1|key2\").");
				Remove(key1, key2);
			}
		}

		public Func<object, string> Serialize { get; set; }
		public Func<string, Type, object> Deserialize { get; set; }

		Func<JsonSerializerSettings> JsonSerializerSettings = () => {
			var st = new JsonSerializerSettings();
			st.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
			st.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			st.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
			return st;
		};
		string SerializeObject(object value) {
			if (Serialize != null) return Serialize(value);
			return JsonConvert.SerializeObject(value, this.JsonSerializerSettings());
		}
		T DeserializeObject<T>(string value) {
			if (Deserialize != null) return (T) Deserialize(value, typeof(T));
			return JsonConvert.DeserializeObject<T>(value, this.JsonSerializerSettings());
		}

		public void Set<T>(string key, T data, int timeoutSeconds = 0) {
			if (string.IsNullOrEmpty(key)) return;
			Cache.Set(key, Encoding.UTF8.GetBytes(this.SerializeObject(data)), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(timeoutSeconds) });
		}
		public T Get<T>(string key) {
			if (string.IsNullOrEmpty(key)) return default(T);
			var value = Cache.Get(key);
			if (value == null) return default(T);
			return this.DeserializeObject<T>(Encoding.UTF8.GetString(value));
		}
		public string Get(string key) {
			if (string.IsNullOrEmpty(key)) return null;
			var value = Cache.Get(key);
			if (value == null) return null;
			return Encoding.UTF8.GetString(value);
		}

		async public Task SetAsync<T>(string key, T data, int timeoutSeconds = 0) {
			if (string.IsNullOrEmpty(key)) return;
			await Cache.SetAsync(key, Encoding.UTF8.GetBytes(this.SerializeObject(data)), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(timeoutSeconds) });
		}
		async public Task<T> GetAsync<T>(string key) {
			if (string.IsNullOrEmpty(key)) return default(T);
			var value = await Cache.GetAsync(key);
			if (value == null) return default(T);
			return this.DeserializeObject<T>(Encoding.UTF8.GetString(value));
		}
		async public Task<string> GetAsync(string key) {
			if (string.IsNullOrEmpty(key)) return null;
			var value = await Cache.GetAsync(key);
			if (value == null) return null;
			return Encoding.UTF8.GetString(value);
		}

		public void Remove(params string[] keys) {
			if (keys == null || keys.Length == 0) return;
			var keysDistinct = keys.Distinct();
			if (CacheSupportMultiRemove) Cache.Remove(string.Join("|", keysDistinct));
			else foreach (var key in keysDistinct) Cache.Remove(key);
		}

		async public Task RemoveAsync(params string[] keys) {
			if (keys == null || keys.Length == 0) return;
			var keysDistinct = keys.Distinct();
			if (CacheSupportMultiRemove) await Cache.RemoveAsync(string.Join("|", keysDistinct));
			else foreach (var key in keysDistinct) await Cache.RemoveAsync(key);
		}

		public T Shell<T>(string key, int timeoutSeconds, Func<T> getData) {
			if (timeoutSeconds <= 0) return getData();
			if (Cache == null) throw new Exception("缓存实现 IDistributedCache 为 null");
			var cacheValue = Cache.Get(key);
			if (cacheValue != null) {
				try {
					var txt = Encoding.UTF8.GetString(cacheValue);
					return DeserializeObject<T>(txt);
				} catch {
					Cache.Remove(key);
					throw;
				}
			}
			var ret = getData();
			Cache.Set(key, Encoding.UTF8.GetBytes(SerializeObject(ret)), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(timeoutSeconds) });
			return ret;
		}

		public T Shell<T>(string key, string field, int timeoutSeconds, Func<T> getData) {
			if (timeoutSeconds <= 0) return getData();
			if (Cache == null) throw new Exception("缓存实现 IDistributedCache 为 null");
			var hashkey = $"{key}:{field}";
			var cacheValue = Cache.Get(hashkey);
			if (cacheValue != null) {
				try {
					var txt = Encoding.UTF8.GetString(cacheValue);
					var value = DeserializeObject<(T, long)>(txt);
					if (DateTime.Now.Subtract(dt1970.AddSeconds(value.Item2)).TotalSeconds <= timeoutSeconds) return value.Item1;
				} catch {
					Cache.Remove(hashkey);
					throw;
				}
			}
			var ret = (getData(), (long) DateTime.Now.Subtract(dt1970).TotalSeconds);
			Cache.Set(hashkey, Encoding.UTF8.GetBytes(SerializeObject(ret)));
			return ret.Item1;
		}

		async public Task<T> ShellAsync<T>(string key, int timeoutSeconds, Func<Task<T>> getDataAsync) {
			if (timeoutSeconds <= 0) return await getDataAsync();
			if (Cache == null) throw new Exception("缓存实现 IDistributedCache 为 null");
			var cacheValue = await Cache.GetAsync(key);
			if (cacheValue != null) {
				try {
					var txt = Encoding.UTF8.GetString(cacheValue);
					return DeserializeObject<T>(txt);
				} catch {
					await Cache.RemoveAsync(key);
					throw;
				}
			}
			var ret = await getDataAsync();
			await Cache.SetAsync(key, Encoding.UTF8.GetBytes(SerializeObject(ret)), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(timeoutSeconds) });
			return ret;
		}

		async public Task<T> ShellAsync<T>(string key, string field, int timeoutSeconds, Func<Task<T>> getDataAsync) {
			if (timeoutSeconds <= 0) return await getDataAsync();
			if (Cache == null) throw new Exception("缓存实现 IDistributedCache 为 null");
			var hashkey = $"{key}:{field}";
			var cacheValue = await Cache.GetAsync(hashkey);
			if (cacheValue != null) {
				try {
					var txt = Encoding.UTF8.GetString(cacheValue);
					var value = DeserializeObject<(T, long)>(txt);
					if (DateTime.Now.Subtract(dt1970.AddSeconds(value.Item2)).TotalSeconds <= timeoutSeconds) return value.Item1;
				} catch {
					await Cache.RemoveAsync(hashkey);
					throw;
				}
			}
			var ret = (await getDataAsync(), (long) DateTime.Now.Subtract(dt1970).TotalSeconds);
			await Cache.SetAsync(hashkey, Encoding.UTF8.GetBytes(SerializeObject(ret)));
			return ret.Item1;
		}
	}
}
