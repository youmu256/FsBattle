using System.Collections.Generic;

namespace FrameSyncBattle
{
    
    public interface IFsmContent
    {
        //为啥多此一举要一个Content呢
        //有时候比如状态机的持有者不想暴露太多的东西给状态 那此时这个Content就相当于起到了规范的作用
    }
    
    public interface IFsmState<in T> where T:IFsmContent
    {
        void Enter(T context);

        void Update(T context, float deltaTime);

        void Exit(T context);
    }

    public interface IFsm<T> where T : IFsmContent
    {
        void AddState(string key, IFsmState<T> state);
        IFsmState<T> GetState(string stateName);
        void ChangeState(string stateName,bool sameChange = false);
        void UpdateFsm(float deltaTime);
    }
    
    public class SimpleFsm<T> :IFsm<T> where T:IFsmContent
    {
        public T Context;
        public IFsmState<T> Current;
        public string CurrentStateName;
        public Dictionary<string, IFsmState<T>> StateMap = new Dictionary<string, IFsmState<T>>();

        public SimpleFsm (T content)
        {
            this.Context = content;
        }
        
        public void AddState(string key,IFsmState<T> state)
        {
            if (StateMap.ContainsKey(key)) return;
            StateMap.Add(key,state);
        }
        public IFsmState<T> GetState(string stateName)
        {
            if (StateMap.ContainsKey(stateName)) return StateMap[stateName];
            return null;
        }
        
        public void ChangeState(string stateName,bool sameChange = false)
        {
            var next = GetState(stateName);
            if (next == null) return;
            if (sameChange == false && next == Current) return;
            if (Current != null)
                Current.Exit(Context);
            Current = next;
            Current.Enter(Context);
            CurrentStateName = stateName;
        }

        public void UpdateFsm(float deltaTime)
        {
            if(Current!=null)
                Current.Update(Context,deltaTime);
        }
    }
}