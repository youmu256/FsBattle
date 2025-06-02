using System;
using System.Collections.Generic;
using System.Reflection;

namespace FrameSyncBattle
{

    /// <summary>
    /// 实际上就是一个静态的全局事件中心
    /// </summary>
    public class GlobalEvents
    {
        public static GameEventHandler GameEventHandler { get; private set; }= new GameEventHandler();
        public static void GRegist<V>(Action<V> onEvt)where V : GameEvent,new()
        {
            GameEventHandler.GRegist<V>(onEvt);
        }
        public static void GUnRegist<V>(Action<V> onEvt)where V : GameEvent,new()
        {
            GameEventHandler.GUnRegist<V>(onEvt);
        }
        public static void Fire<V>(V evt) where V : GameEvent, new()
        {
            GameEventHandler.Fire(evt);
        }
    }
    
}