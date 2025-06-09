using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    public class GameEventHandler
    {
        protected EventTable EventTable { get; private set; } = new EventTable();
        
        /// <summary>
        /// 注意Fire后事件对象会被回收
        /// 响应方注意不要持有该事件对象
        /// </summary>
        public void Fire<V>(V evt)where V : GameEvent
        {
            var key = GetTypeKey(evt.GetType());
#if UNITY_EDITOR
            //Debug.Log("GlobalEvents Fire : " + key);
            if (evt.GetType() != typeof(V))
            {
                Debug.LogError("必须保证类型一致" + evt.GetType() + " , " + typeof(V));
            }
#endif
            EventTable.SendEvent(key,evt);/*EventTable中是用泛型去处理的 所以要保证传入的evt表类型就要正确*/
        }
        
        public void GRegist<V>(Action<V> onEvt)where V : GameEvent
        {
            EventTable.RegisterEvent(GetTypeKey(typeof(V)), onEvt);
        }
        public void GUnRegist<V>(Action<V> onEvt)where V : GameEvent
        {
            EventTable.UnregisterEvent(GetTypeKey(typeof(V)), onEvt);
        }

        public static Dictionary<Type, string> Type2KeyMap = new Dictionary<Type, string>();

        protected static string GetTypeKey(Type type)
        {
            if (Type2KeyMap.TryGetValue(type, out string key))
            {
                return key;
            }
            key = type.Name;
            Type2KeyMap.Add(type,key);
            return key;
        }
    }


}