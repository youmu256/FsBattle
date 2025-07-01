using System;
using System.Collections.Generic;

namespace FrameSyncBattle
{
    
    /// <summary>
    /// 基础行为类型
    /// </summary>
    [Flags]
    public enum FsUnitStateFlag : long
    {
        Move = 1 << 0, //允许移动
        ExtraMove = 1 << 1, //允许额外移动 被击飞等等
        Attack = 1 << 2, //允许攻击
        Cast = 1 << 3, //允许施法
        Treatment = 1 << 4, //允许被治疗
    }

    public class FsUnitStateFlags
    {
        protected static List<FsUnitStateFlag> StateEnumCacheList;
        public FsUnitStateFlags()
        {
            StateCountMap = new Dictionary<FsUnitStateFlag, int>();
            if (StateEnumCacheList == null)
            {
                StateEnumCacheList = new List<FsUnitStateFlag>();
                foreach (FsUnitStateFlag sft in Enum.GetValues(typeof(FsUnitStateFlag)))
                {
                    StateEnumCacheList.Add(sft);
                }
            }
            //初始化所有标记为0 代表为可用状态
            foreach (var sft in StateEnumCacheList)
            {
                SetCounter(sft,0);
                StatesCache |= sft;
            }
        }
        
        protected FsUnitStateFlag StatesCache = 0;
        public void Modify(FsUnitStateFlag states, bool addState)
        {
            int index = 0;
            long it = (long) states;
            while (it>0)
            {
                if ((it & 1) == 1)
                {
                    long sub = 1 << index;
                    ModifySingle((FsUnitStateFlag)sub, addState);
                }
                it >>= 1;
                index++;
            }
        }

        public bool HasAnyState(FsUnitStateFlag states)
        {
            if ((StatesCache & states) > 0) return true;
            return false;
        }
        public bool HasAllState(FsUnitStateFlag states)
        {
            if ((StatesCache & states) == states) return true;
            return false;
        }

        private Dictionary<FsUnitStateFlag, int> StateCountMap { get; set; }

        private int ModifySingle(FsUnitStateFlag state, bool addOrRemove)
        {
            var oldCount = GetCounter(state);
            var newCount = oldCount + (addOrRemove ? 1 : -1);
            SetCounter(state,newCount);
            if (newCount >= 0)
            {
                StatesCache |= state;//add
            }
            else
            {
                StatesCache &= ~state;//remove
            }
            return newCount;
        }
        private void SetCounter(FsUnitStateFlag state,int count)
        {
            if (!StateCountMap.ContainsKey(state))
            {
                StateCountMap.Add(state,count);
            }
            StateCountMap[state] = count;
        }
        private int GetCounter(FsUnitStateFlag state)
        {
            return StateCountMap[state];
        }
    }
}