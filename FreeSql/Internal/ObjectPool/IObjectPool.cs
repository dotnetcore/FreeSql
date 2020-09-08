using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.ObjectPool
{
    public interface IObjectPool<T> : IDisposable
    {
        IPolicy<T> Policy { get; }
        /// <summary>
        /// 是否可用
        /// </summary>
        bool IsAvailable { get; }
        /// <summary>
        /// 不可用错误
        /// </summary>
        Exception UnavailableException { get; }
        /// <summary>
        /// 不可用时间
        /// </summary>
        DateTime? UnavailableTime { get; }

        /// <summary>
        /// 将对象池设置为不可用，后续 Get/GetAsync 均会报错，同时启动后台定时检查服务恢复可用
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>由【可用】变成【不可用】时返回true，否则返回false</returns>
        bool SetUnavailable(Exception exception);

        /// <summary>
        /// 统计对象池中的对象
        /// </summary>
        string Statistics { get; }
        /// <summary>
        /// 统计对象池中的对象（完整)
        /// </summary>
        string StatisticsFullily { get; }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
        Object<T> Get(TimeSpan? timeout = null);

#if net40
#else
        /// <summary>
        /// 获取资源
        /// </summary>
        /// <returns></returns>
        Task<Object<T>> GetAsync();
#endif

        /// <summary>
        /// 使用完毕后，归还资源
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="isReset">是否重新创建</param>
        void Return(Object<T> obj, bool isReset = false);
    }
}
