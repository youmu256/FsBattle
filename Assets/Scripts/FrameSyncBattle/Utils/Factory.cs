using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace FrameSyncBattle
{

    public interface IObjectFactory<T>
    {
        T Create();
    }
    public class NonPublicObjectFactory<T> : IObjectFactory<T> where T : class
    {
        public T Create()
        {
            var ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            //可能不能获得非公开的无参构造器?
            return ctor.Invoke(null) as T;
        }
    }
    public class DefaultObjectFactory<T> : IObjectFactory<T> where T : new()
    {
        public T Create()
        {
            return new T();
        }
    }

    public class CustomObjectFactory<T> : IObjectFactory<T>
    {
        public CustomObjectFactory(Func<T> factoryMethod)
        {
            mFactoryMethod = factoryMethod;
        }

        protected Func<T> mFactoryMethod;

        public T Create()
        {
            return mFactoryMethod();
        }
    }
    
    
    public class PrefabCreateFactory : IObjectFactory<GameObject>
    {
        public string PrefabKey { get; private set; }
        protected GameObject PrefabInstance { get; private set; }
        public PrefabCreateFactory(string key,GameObject prefabIns)
        {
            PrefabKey = key;
            PrefabInstance = prefabIns;
        }
        
        public GameObject Create()
        {
            GameObject obj= GameObject.Instantiate(PrefabInstance);
            obj.name = PrefabKey;
            return obj;
        }
    }
    
    public class ComponentObjectCreateFactory<T> : IObjectFactory<T> where T: Component
    {
        public string PrefabKey { get; private set; }
        protected T ComponentPrefab { get; private set; }
        public ComponentObjectCreateFactory(string key,T componentObject)
        {
            PrefabKey = key;
            ComponentPrefab = componentObject;
        }
        
        public T Create()
        {
            T obj= GameObject.Instantiate(ComponentPrefab);
            obj.gameObject.name = PrefabKey;
            return obj;
        }
    }
}
