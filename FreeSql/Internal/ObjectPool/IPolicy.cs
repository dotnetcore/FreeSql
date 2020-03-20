using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.ObjectPool
{
    public interface IPolicy<T>
    {

        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 池容量
        /// </summary>
        int PoolSize { get; set; }

        /// <summary>
        /// 默认获取超时设置
        /// </summary>
        TimeSpan SyncGetTimeout { get; set; }

        /// <summary>
        /// 空闲时间，获取时若超出，则重新创建
        /// </summary>
        TimeSpan IdleTimeout { get; set; }

        /// <summary>
        /// 异步获取排队队列大小，小于等于0不生效
        /// </summary>
        int AsyncGetCapacity { get; set; }

        /// <summary>
        /// 获取超时后，是否抛出异常
        /// </summary>
        bool IsThrowGetTimeoutException { get; set; }

        /// <summary>
        /// 监听 AppDomain.CurrentDomain.ProcessExit/Console.CancelKeyPress 事件自动释放
        /// </summary>
        bool IsAutoDisposeWithSystem { get; set; }

        /// <summary>
        /// 后台定时检查可用性间隔秒数
        /// </summary>
        int CheckAvailableInterval { get; set; }

        /// <summary>
        /// 对象池的对象被创建时
        /// </summary>
        /// <returns>返回被创建的对象</returns>
        T OnCreate();

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="obj">资源对象</param>
        void OnDestroy(T obj);

        /// <summary>
        /// 从对象池获取对象超时的时候触发，通过该方法统计
        /// </summary>
        void OnGetTimeout();

        /// <summary>
        /// 从对象池获取对象成功的时候触发，通过该方法统计或初始化对象
        /// </summary>
        /// <param name="obj">资源对象</param>
        void OnGet(Object<T> obj);
#if net40
#else
        /// <summary>
        /// 从对象池获取对象成功的时候触发，通过该方法统计或初始化对象
        /// </summary>
        /// <param name="obj">资源对象</param>
        Task OnGetAsync(Object<T> obj);
#endif

        /// <summary>
        /// 归还对象给对象池的时候触发
        /// </summary>
        /// <param name="obj">资源对象</param>
        void OnReturn(Object<T> obj);

        /// <summary>
        /// 检查可用性
        /// </summary>
        /// <param name="obj">资源对象</param>
        /// <returns></returns>
        bool OnCheckAvailable(Object<T> obj);

        /// <summary>
        /// 事件：可用时触发
        /// </summary>
        void OnAvailable();
        /// <summary>
        /// 事件：不可用时触发
        /// </summary>
        void OnUnavailable();
    }
}