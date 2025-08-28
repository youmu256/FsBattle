using System;

namespace FrameSyncBattle
{
    public class TestRef
    {

    }



    public class TestUnitManager
    {
        public TestUnit GetUnit(int id)
        {
            return null;
        }
    }

    public class TestUnit
    {
        public int Id;
    }

    public class TestUnitRef
    {
        public TestUnitManager Manager;
        public int Id;

        public TestUnitRef(TestUnitManager manager, TestUnit unit)
        {
            this.Manager = manager;
            this.Id = unit.Id;
        }

        /*
        public static explicit operator TestUnit(TestUnitRef testUnitRef)
        {
            return testUnitRef.Manager.GetUnit(testUnitRef.Id);
        }
        */
        public static implicit operator TestUnit(TestUnitRef testUnitRef)
        {
            if (testUnitRef == null || testUnitRef.Manager == null) return null;
            return testUnitRef.Manager.GetUnit(testUnitRef.Id);
        }
    }

    public class TestAttackMissile
    {
        public TestUnitRef UnitRef;

        public TestUnit Unit => UnitRef;

        public void Frame()
        {
            if (Unit == null)
            {

            }
        }
    }


    public class MyTestMonoBehavior
    {
        public Action OnDestroy;

        public void Destroy()
        {
            OnDestroy?.Invoke();
        }
    }

    public class MyTestRef
    {
        private MyTestMonoBehavior Ref;
        public MyTestMonoBehavior Value => Ref;

        public MyTestRef(MyTestMonoBehavior target)
        {
            Ref = target;
            target.OnDestroy += OnDestroy;
        }

        private void OnDestroy()
        {
            Ref = null;
        }
    }

    public class Test
    {
        public void DoTest()
        {
            MyTestMonoBehavior b = new MyTestMonoBehavior();
            MyTestRef r = new MyTestRef(b);
            r = null;
            //b持有了r对象中的OnDestroy方法 导致r无法被释放 需要主动删除方法引用才行
        }
    }



    public class TestObject
    {
        public bool IsDestroyed { get; private set; }


        public void Destroy()
        {
            IsDestroyed = true;
        }

    }


    public interface IMyUnit
    {
        void Attack();
        void Run();
    }

    public class MyUnit : IMyUnit
    {
        public void Attack()
        {
        }

        public void Run()
        {
        }
    }

    //代理模式可以很好地处理这个引用被Destroy后 其他引用的地方自动变成null的新需求
    //缺点是代理类要完整地重新写一遍真实类的方法和属性
    public class MyUnitRef : IMyUnit
    {
        private IMyUnit Obj { get; set; }

        public void Destroy()
        {
            Obj = null;
        }

        public void Attack()
        {
            Obj.Attack();
        }

        public void Run()
        {
            Obj.Run();
        }

        public static bool operator ==(MyUnitRef a, object b)
        {
            return object.ReferenceEquals(a, b) || object.ReferenceEquals(a?.Obj, b);
        }

        public static bool operator !=(MyUnitRef a, object b)
        {
            return !(a == b);
        }
    }

}