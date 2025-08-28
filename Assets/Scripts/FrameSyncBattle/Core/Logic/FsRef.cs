
#define UseRef //是否开启引用形式
using System;
using System.Collections.Generic;
#if UseRef
#else
using GameUnitRef = FrameSyncBattle.GameUnit; //C# 10才支持全局别名 目前要使用宏来切换类型指向的话  每个文件中都要处理非常麻烦
#endif

namespace FrameSyncBattle
{
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

    public class TestMissile
    {
        public FsUnitLogic Target;
        public FsEntityLogic Entity;
        public void SetTarget(FsUnitLogic target,FsEntityLogic entity)
        {
            //开启了引用形式 对象会被包装成Ref 如果没有开启AsRef就会返回对象自己
            Target = target.AsRef();
            Entity = entity.AsRef();
        }

        public void Frame()
        {
            var target = Target.Position;
            //往target飞
        }
        
    }


    public class GameEntity
    {
        public void AsRef<T>(ref GameEntityRef<T> entityRef) where T:GameEntity
        {
            if (entityRef == null)
            {
                entityRef = new GameEntityRef<T>(this);
            }
            else
            {
                entityRef.Set(this as T);
            }
        }
    }


    public class GameUnit : GameEntity
    {
        public void Attack(){}
    }

    #if UseRef
    
    public class GameUnitRef : GameEntityRef<GameUnit>
    {
        public GameUnitRef(GameEntity target) : base(target)
        {
            
        }
    }
    #endif
    


    public class GameEntityRef<T> where T : GameEntity 
    {
        private readonly WeakReference<GameEntity> _reference;

        public T Get()
        {
            //在Entity实体被移除后 实际上就应该报空 这样来提前暴露出技能业务等逻辑的问题
            this._reference.TryGetTarget(out var v);
            return v as T;
        }
        
        public void Set(T target)
        {
            this._reference.SetTarget(target);
        }

        public GameEntityRef(GameEntity target)
        {
            _reference = new WeakReference<GameEntity>(target);
        }
    }

    public static class GameEntityRefExtend
    {
        public static void AssignTo(this GameUnit unit, ref GameUnitRef unitRef)
        {
            if (unitRef == null)
            {
                unitRef = new GameUnitRef(unit);
            }
            else
            {
                unitRef.Set(unit);
            }
        }

        //在不开启Ref形式的时候 GameUnitRef变成别名实际上还是GameUnit 用来兼容编译
        public static GameUnit Get(this GameUnit unit)
        {
            return unit;
        }
    }

    public class GameRefTest
    {
        public GameUnitRef Unit;
        public void Start(GameUnit unit)
        {
            unit.AssignTo(ref Unit);
        }

        public void Update()
        {
            Unit.Get().Attack();
        }

        public void Exit()
        {
            Unit?.Set(null);
        }
    }

    
    /*
     * Q:为什么要存在单位引用的概念？
     * A:比如一个单位被删除了，不用了，但是其他逻辑中还引用着这个对象。
     * 比如一个攻击弹道朝着目标飞行，但是这个目标已经被删除。
     * 此时 弹道逻辑中无法得知单位已经被移除，还是会朝着目标飞行，但是此时目标单位的数据已经异常了，最终造成奇怪的游戏表现。
     * 但是这整个情况是不会报错的， 只能靠人去识别异常然后去定位原因。非常麻烦
     * 但是有单位引用的概念的话，弹道逻辑中每次去读取或者操作单位，都是通过引用，这样就能有很清晰地控制，可以主动报空提前发现处理问题
     *
     * 最简单的说法就是，像Unity的Mono一样，对象被删除后，其他引用Mono对象的脚步里，这个变量也会变成空，从而报出空指针
     *
     * 最终思考结论
     * 如果要实现 单位在移除后，其他引用单位的代码中，尝试读取修改单位时都应该报空
     * 也就真的存在弱引用这个概念
     * 实现方式应该是使用代理模式，编写一个代理类作为引用的实现
     * 在实体处于IsDestroy 或者 IsRemove 的状态时 就应该在代理调用实体方法时中报空
     * 在代理中使用弱引用WeakReference更加合适
     *
     * 在当前项目中
     * 先不考虑在这个问题。
     * 首先代理比较麻烦，而且也会占用性能。
     * 
     */
    
    
}