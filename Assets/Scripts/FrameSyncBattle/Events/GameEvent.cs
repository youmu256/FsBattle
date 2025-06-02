using System;

namespace FrameSyncBattle
{
    public static class GameEventExtend
    {
        //不使用泛型的形式 就会有问题
        public static V RaiseTo<V>(this V evt, GameEventHandler handler) where V : GameEvent,new()
        {
            handler.Fire<V>(evt);
            return evt;
        }
        public static V RaiseToGlobal<V>(this V evt) where V : GameEvent,new()
        {
            RaiseTo(evt,GlobalEvents.GameEventHandler);
            return evt;
        }
        public static void Recycle(this GameEvent ge)
        {
            GameEvent.Recycle(ge);
        }
    }
    public abstract class GameEvent
    {
        protected GameEvent(){}
        /**重置事件参数 按规范必须全部重置*/
        public abstract void Reset();
        #region Pool
        /*让GameEvent自己管理池子 和 事件发起*/
        protected static MultiExtendObjectPool<GameEvent> Pool { get; private set; } = new MultiExtendObjectPool<GameEvent>(ResetMethod);
        private static void ResetMethod(GameEvent obj)
        {
            obj.Reset();
        }
        public static V New<V>() where V : GameEvent,new()
        {
            return Pool.Allocate<V>();
        }
        public static void Recycle(GameEvent ge)
        {
            Pool.Recycle(ge);
        }
        #endregion
        /* 错误代码..
        public void RaiseTo(GameEventHandler handler)
        {
            //Raise 发起会有问题 应该要完整带 泛型 this传递过去的话，handler里只能认得GameEvent..
            handler.Fire(this);
            Pool.Recycle(this);
        }
        public void RaiseToGlobal()
        {
            RaiseTo(GlobalEvents.GameEventHandler);
        }
        */
    }
}