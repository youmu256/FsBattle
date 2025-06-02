using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

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

    /// <summary>
    /// 如果类对象的构造器非public就会报错
    /// </summary>
    public static class ClassCreateHelper
    {
        static Dictionary<Type, Func<object>> CreatorMap = new Dictionary<Type, Func<object>>();

        public static object Create(Type type)
        {
            Func<object> emitCreator = null;
            CreatorMap.TryGetValue(type, out emitCreator);
            //IL emit方法 
            if (emitCreator == null)
            {
                //type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder) null, new Type[] { }, (ParameterModifier[]) null);
                ConstructorInfo cons = type.GetConstructor(new Type[] { });
                DynamicMethod dynMet = new DynamicMethod(
                    name: string.Format("_{0:N}", Guid.NewGuid()), returnType: type, parameterTypes: null);
                var gen = dynMet.GetILGenerator();
                gen.Emit(OpCodes.Newobj, cons);
                gen.Emit(OpCodes.Ret);
                //记录下这个方法
                emitCreator = dynMet.CreateDelegate(typeof(Func<object>)) as Func<object>;
                CreatorMap.Add(type, emitCreator);
            }
            return emitCreator.Invoke();
        }
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
                Debug.LogError("Not Find Pool!" + key);
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
                Debug.LogError("Not Find Pool!" + obj.GetType());
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

    #region Unity对象池
    
    public class ComponentObjectPool<T> : Pool<T> where T : UnityEngine.Component
    {
        public string PrefabKey { get; private set; }
        readonly Action<T> resetMethod;
        readonly Action<T> allocateMethod;
        public ComponentObjectPool(T prefabIns,string key, Action<T> allocateMethod = null,Action < T> resetMethod = null)
        {
            Factory = new ComponentObjectCreateFactory<T>(PrefabKey,prefabIns);
            this.PrefabKey = key;
            this.resetMethod = resetMethod;
            this.allocateMethod = allocateMethod;
        }

        public override T Allocate()
        {
            var obj = base.Allocate();
            if(allocateMethod!=null)
                allocateMethod.Invoke(obj);
            return obj;
        }
        public override bool Recycle(T obj)
        {
            if(resetMethod!=null)
                resetMethod.Invoke(obj);
            CacheStack.Push(obj);
            return true;
        }

        public void Clear()
        {
            Factory = null;
            while (CacheStack.Count>0)
            {
                GameObject.Destroy(CacheStack.Pop().gameObject);
            }
        }
    }
    
    public class PrefabInsPool : Pool<GameObject>
    {
        public string PrefabKey { get; private set; }

        readonly Action<GameObject> resetMethod;
        readonly Action<GameObject> allocateMethod;

        public PrefabInsPool(GameObject prefabIns,string key, Action<GameObject> allocateMethod = null,Action <GameObject> resetMethod = null)
        {
            Factory = new PrefabCreateFactory(key,prefabIns);
            this.PrefabKey = key;
            this.resetMethod = resetMethod;
            this.allocateMethod = allocateMethod;
        }

        public override GameObject Allocate()
        {
            var obj = base.Allocate();
            if(allocateMethod!=null)
                allocateMethod.Invoke(obj);
            return obj;
        }
        public override bool Recycle(GameObject obj)
        {
            if(resetMethod!=null)
                resetMethod.Invoke(obj);
            CacheStack.Push(obj);
            return true;
        }

        public void Clear()
        {
            Factory = null;
            while (CacheStack.Count>0)
            {
                GameObject.Destroy(CacheStack.Pop());
            }
        }
    }

    public class PrefabInsPoolMap
    {
        public Dictionary<string,PrefabInsPool> PoolMap { get; private set; } = new Dictionary<string, PrefabInsPool>();

        readonly Action<string,GameObject> resetMethod;
        readonly Action<string,GameObject> allocateMethod;

        public PrefabInsPoolMap(Action<string,GameObject> allocateMethod = null, Action<string,GameObject> resetMethod = null)
        {
            this.allocateMethod = allocateMethod;
            this.resetMethod = resetMethod;
        }

        public bool HasPool(string key)
        {
            return PoolMap.ContainsKey(key);
        }
        
        public PrefabInsPool GetPool(string key)
        {
            if(PoolMap.ContainsKey(key))
                return PoolMap[key];
            return null;
        }

        public void RegistPool(string key, PrefabInsPool pool)
        {
            if (PoolMap.ContainsKey(key))
            {
                Debug.LogError("对象池注册失败 - Key重复");
                return;
            }
            PoolMap.Add(key,pool);
        }

        public GameObject Allocate(string key)
        {
            PrefabInsPool pool;
            GameObject obj = null;
            if (PoolMap.TryGetValue(key, out pool))
            {
                obj = pool.Allocate();
                if(allocateMethod!=null)
                    allocateMethod.Invoke(key,obj);
            }
            return obj;
        }

        public bool Recycle(string key, GameObject obj)
        {
            PrefabInsPool pool;
            if (PoolMap.TryGetValue(key, out pool))
            {
                if(resetMethod!=null)
                    resetMethod.Invoke(key,obj);
                pool.Recycle(obj);
                return true;
            }
            return false;
        }

        public void ClearAll()
        {
            foreach (var pair in PoolMap)
            {
                pair.Value.Clear();
            }
            PoolMap.Clear();
            /*删掉对象就行了
            resetMethod = null;
            allocateMethod = null;
            */
        }
    }

    #endregion

}
