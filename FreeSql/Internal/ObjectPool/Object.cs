using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FreeSql.Internal.ObjectPool
{

    public class Object<T> : IDisposable
    {
        public static Object<T> InitWith(IObjectPool<T> pool, int id, T value)
        {
            return new Object<T>
            {
                Pool = pool,
                Id = id,
                Value = value,
                LastGetThreadId = Thread.CurrentThread.ManagedThreadId,
                LastGetTime = DateTime.Now
            };
        }

        /// <summary>
        /// 所属对象池
        /// </summary>
        public IObjectPool<T> Pool { get; internal set; }

        /// <summary>
        /// 在对象池中的唯一标识
        /// </summary>
        public int Id { get; internal set; }
        /// <summary>
        /// 资源对象
        /// </summary>
        public T Value { get; internal set; }

        internal long _getTimes;
        /// <summary>
        /// 被获取的总次数
        /// </summary>
        public long GetTimes => _getTimes;

        /// 最后获取时的时间
        public DateTime LastGetTime { get; internal set; }

        /// <summary>
        /// 最后归还时的时间
        /// </summary>
        public DateTime LastReturnTime { get; internal set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; internal set; } = DateTime.Now;

        /// <summary>
        /// 最后获取时的线程id
        /// </summary>
        public int LastGetThreadId { get; internal set; }

        /// <summary>
        /// 最后归还时的线程id
        /// </summary>
        public int LastReturnThreadId { get; internal set; }

        public override string ToString()
        {
            return $"{this.Value}, Times: {this.GetTimes}, ThreadId(R/G): {this.LastReturnThreadId}/{this.LastGetThreadId}, Time(R/G): {this.LastReturnTime.ToString("yyyy-MM-dd HH:mm:ss:ms")}/{this.LastGetTime.ToString("yyyy-MM-dd HH:mm:ss:ms")}";
        }

        /// <summary>
        /// 重置 Value 值
        /// </summary>
        public void ResetValue()
        {
            if (this.Value != null)
            {
                try { this.Pool.Policy.OnDestroy(this.Value); } catch { }
                try { (this.Value as IDisposable)?.Dispose(); } catch { }
            }
            T value = default(T);
            try { value = this.Pool.Policy.OnCreate(); } catch { }
            this.Value = value;
            this.LastReturnTime = DateTime.Now;
        }

        internal bool _isReturned = false;
        public void Dispose()
        {
            Pool?.Return(this);
        }
    }
}