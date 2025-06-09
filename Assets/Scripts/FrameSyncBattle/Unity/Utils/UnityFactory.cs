using UnityEngine;

namespace FrameSyncBattle
{
    
    
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