using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace FrameSyncBattle
{
    public interface IPoolable
    {
        void OnRecycled();
        bool IsRecycled { get; set; }
        void OnDelete();//删除时 释放资源，一般对象不需要操作，没引用了自然被释放
    }
    public interface IPool<T>
    {
        T Allocate();
        bool Recycle(T obj);
    }

    public abstract class Pool<T> : IPool<T>
    {
        public int CurCount
        {
            get { return CacheStack.Count; }
        }

        protected IObjectFactory<T> Factory;

        protected readonly Stack<T> CacheStack = new Stack<T>();


        public virtual T Allocate()
        {
            return CacheStack.Count == 0
                ? Factory.Create()
                : CacheStack.Pop();
        }

        public abstract bool Recycle(T obj);
    }

    #region SimplePool

    public class SimpleObjectPool<T> : Pool<T> where T:new()
    {
        readonly Action<T> resetMethod;

        public SimpleObjectPool(Action<T> resetMethod = null)
        {
            Factory = new DefaultObjectFactory<T>();
            this.resetMethod = resetMethod;
        }

        public override bool Recycle(T obj)
        {
            if(resetMethod!=null)
                resetMethod.Invoke(obj);
            CacheStack.Push(obj);
            return true;
        }
    }
    public class SimplePool<T> : Pool<T>
    {
        readonly Action<T> resetMethod;

        public SimplePool(Func<T> factoryMethod , Action<T> resetMethod = null, int initCount = 0)
        {
            Factory = new CustomObjectFactory<T>(factoryMethod);
            this.resetMethod = resetMethod;

            for (int i = 0; i < initCount; i++)
            {
                CacheStack.Push(Factory.Create());
            }
        }
        
        public override bool Recycle(T obj)
        {
            if(resetMethod!=null)
                resetMethod.Invoke(obj);
            CacheStack.Push(obj);
            return true;
        }
    }

    #endregion

    #region 父子类模糊对象池 / ParentClass - ChildClass[1]Pool - ChildClass[N]Pool

    /// <summary>
    /// 有继承的池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MultiExtendObjectPool<T> 
    {
        public Action<T> PoolObjectResetMethod { get; private set; }
        public MultiExtendObjectPool(Action<T> resetMethod = null, bool initPools = false)
        {
            if (initPools)
                PoosInit();
            PoolObjectResetMethod = resetMethod;
        }
        public void PoosInit()
        {
            //可以初始化池
            //反射--可能比较慢
            Assembly ab = Assembly.GetAssembly(typeof(T));
            var types = ab.GetTypes().Where((type => type.IsSubclassOf(typeof(T)) && type != typeof(T)));
            foreach (var type in types)
            {
                var obj = Allocate(type);
                Recycle(obj);
            }
        }

        public Dictionary<Type, SimplePool<T>> PoolMap = new Dictionary<Type, SimplePool<T>>();

        public void Recycle(T obj)
        {
            SimplePool<T> pool;
            var key = obj.GetType();
            if (PoolMap.TryGetValue(key, out pool))
            {
                pool.Recycle(obj);
            }
            else
            {
                FsDebug.LogError("Not Find Pool!" + key);
            }
        }

        public T Allocate(Type type)
        {
            SimplePool<T> pool = null;
            if (!PoolMap.ContainsKey(type))
            {
                pool = new SimplePool<T>(Activator.CreateInstance<T>, PoolObjectResetMethod);
                PoolMap.Add(type, pool);
            }
            pool = PoolMap[type];
            return pool.Allocate();
        }

        public V Allocate<V>() where V : class, T,new()
        {
            return Allocate(typeof(V)) as V;
        }
    }


    #endregion



    #region MultiPool

    public class MultiPool<T> where T:class 
    {
        public Dictionary<Type, SimplePool<T>> PoolMap = new Dictionary<Type, SimplePool<T>>();

        public void Recycle(T obj)
        {
            SimplePool<T> pool;
            if (PoolMap.TryGetValue(obj.GetType(), out pool))
            {
                pool.Recycle(obj);
            } else
            {
                FsDebug.LogError("Not Find Pool!" + obj.GetType());
            }
        }
        public V Allocate<V>() where V: class,T, new()
        {
            SimplePool<T> pool;
            Type key = typeof(V);
            if (!PoolMap.ContainsKey(key))
            {
                pool = new SimplePool<T>(() => new V());
                PoolMap.Add(key,pool);
            }
            pool = PoolMap[key];
            return pool.Allocate() as V;
        }
    }

    #endregion

    #region SafePool


    /// <summary>
    /// Object pool.
    /// 自定义对象构造
    /// 限制缓存量
    /// </summary>
    public class SafeObjectPool<T> : Pool<T> where T : IPoolable, new()
    {
        //单例？
        public SafeObjectPool()
        {
            Factory = new DefaultObjectFactory<T>();
        }
        /// <summary>
        /// Init the specified maxCount and initCount.
        /// </summary>
        /// <param name="maxCount">Max Cache count.</param>
        /// <param name="initCount">Init Cache count.</param>
        public void Init(int maxCount, int initCount)
        {
            if (maxCount > 0)
            {
                initCount = Math.Min(maxCount, initCount);
            }

            if (CurCount < initCount)
            {
                for (int i = CurCount; i < initCount; ++i)
                {
                    Recycle(Factory.Create());
                }
            }
        }
        protected int MaxCount = 12;

        /// <summary>
        /// Gets or sets the max cache count.
        /// </summary>
        /// <value>The max cache count.</value>
        public int MaxCacheCount
        {
            get { return MaxCount; }
            set
            {
                MaxCount = value;
                if (CacheStack != null)//更新最大池对象数量
                {
                    if (MaxCount > 0)
                    {
                        if (MaxCount < CacheStack.Count)
                        {
                            int removeCount = MaxCount - CacheStack.Count;
                            while (removeCount > 0)
                            {
                                CacheStack.Pop().OnDelete();//删除
                                --removeCount;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allocate T instance.
        /// </summary>
        public override T Allocate()
        {
            var result = base.Allocate();
            result.IsRecycled = false;
            return result;
        }

        /// <summary>
        /// Recycle the T instance
        /// </summary>
        /// <param name="t">T.</param>
        public override bool Recycle(T t)
        {
            if (t == null || t.IsRecycled)
            {
                return false;
            }

            if (MaxCount > 0)
            {
                if (CacheStack.Count >= MaxCount)
                {
                    //t.OnRecycled();
                    t.OnDelete();//删除
                    return false;
                }
            }
            t.IsRecycled = true;
            t.OnRecycled();
            CacheStack.Push(t);

            return true;
        }
    }

    #endregion

}
