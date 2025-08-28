using System;

namespace FrameSyncBattle
{
    #define UseRef //是否开启引用形式
    public class FsEntityRef
    {
        private readonly WeakReference<FsEntityLogic> _reference;
        public FsEntityRef(FsEntityLogic target)
        {
            _reference = new WeakReference<FsEntityLogic>(target);
        }
        public static implicit operator FsEntityLogic(FsEntityRef t)
        {
            return t.Value<FsEntityLogic>();
        }
        public static implicit operator FsUnitLogic(FsEntityRef t)
        {
            return t.Value<FsUnitLogic>();
        }
        private T Value<T>() where T : FsEntityLogic
        {
            this._reference.TryGetTarget(out var v);
            return v as T;
        }
    }

    public static class FsEntityRefExtends
    {
#if UseRef
        public static FsEntityRef AsRef(this FsEntityLogic logic)
        {
            return new FsEntityRef(logic);
        }
#else
        //为了让编译通过 每个FsEntity的类型都得实现一遍自身返回
        public static FsEntityLogic AsRef(this FsEntityLogic logic)
        {
            return logic;
        }
        public static FsUnitLogic AsRef(this FsUnitLogic logic)
        {
            return logic;
        }
#endif
    }

    public class TestFsRef
    {
        public FsUnitLogic Target;
        public FsEntityLogic Entity;
        public void SetTarget(FsUnitLogic target,FsEntityLogic entity)
        {
            //开启了引用形式 对象会被包装成Ref 如果没有开启AsRef就会返回对象自己
            Target = target.AsRef();
            Entity = entity.AsRef();
            //这对吗...传递进来的变量包装成了引用对象后 又立刻取出了原值，最后就是引用对象白白存在
        }
    }

}