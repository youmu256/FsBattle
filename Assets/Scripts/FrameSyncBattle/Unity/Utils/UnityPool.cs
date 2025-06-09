using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    
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