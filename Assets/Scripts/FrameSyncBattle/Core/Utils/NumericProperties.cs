#define ACTIVE_COVER_OPERATE//是否启用Cover调整

using System;
using System.Collections.Generic;

namespace FrameSyncBattle
{
    public interface INumericPropertiesLimiter
    {
        int Limit(int numericType, int value);
    }

    public enum NumericOperation
    {
        Add=0,//加减
        Pct,//百分比调整
        #if ACTIVE_COVER_OPERATE
        Cover,//直接覆盖某一级属性
        #endif
    }
    /// <summary>
    /// 修改等级必须有序 并且不能大于maxLevel
    /// </summary>
    public class NumericProperties
    {
        protected static int _opcount = -1;
        public static int GetOpIndex(int level ,NumericOperation numericOperation)
        {
            if (_opcount == -1)
                _opcount = Enum.GetValues(typeof(NumericOperation)).Length;
            return (int) numericOperation + level* _opcount;
        }

        public static bool IsBaseOpIndex(int index)
        {
            return index < _opcount;
        }

        protected readonly Dictionary<int, int> NumericDic = new Dictionary<int, int>();
        protected const int BaseOffsetMul = 1000;//每一个属性所能占有的最大下标位。 EveryPropertyOffset/2 -1 = 可用修改等级上限
        protected readonly int MaxLevel = 0;
        protected INumericPropertiesLimiter Limiter;
        public NumericProperties(int maxLevel,INumericPropertiesLimiter limiter,Action<int, int, int> listener)
        {
            MaxLevel = maxLevel;
            this.Limiter = limiter;
            if (listener!=null)
                LimitedValueChangeEvent = listener;
        }
        protected Action<int, int, int> LimitedValueChangeEvent;
        
        #region Get/Set

        private static int BaseModifyType = GetOpIndex(0, NumericOperation.Add);
        public void SetBase(int numericType, int value)
        {
            Set(numericType, BaseModifyType, value);
        }

        public int GetBase(int numericType)
        {
            return GetRawValue((numericType+1) * BaseOffsetMul);
        }
        private void Set(int numericType, int modifyType, int value)
        {
            bool isBaseSet = IsBaseOpIndex(modifyType);//初始设置也调用回调
            int index = (numericType+1) * BaseOffsetMul;
            int modifyIndex = modifyType + index;
            if (NumericDic.ContainsKey(modifyIndex))
            {
                NumericDic[modifyIndex] = value;
            }
            else
            {
                NumericDic.Add(modifyIndex,value);
            }
            int result = Update(numericType,modifyType);//更新数值
            this.NumericDic[numericType] = result;//最终结果值
            
            //限制器处理后的
            int limitResult = Limiter?.Limit( numericType, result) ?? result;
            int lastLimitResult = GetResult(numericType);
            this.NumericDic[-BaseOffsetMul+numericType] = limitResult;
            if (lastLimitResult != limitResult || isBaseSet)
            {
                OnValueChange(numericType,lastLimitResult,limitResult);
                LimitedValueChangeEvent?.Invoke(numericType, lastLimitResult, limitResult);
            }
        }

        protected virtual void OnValueChange(int numericType, int lastValue, int result)
        {
            
        }

        /// <summary>
        /// 主要使用这个方法来对属性进行累加
        /// 注意某些属性要避免越界
        /// </summary>
        /// <param name="numericType"></param>
        /// <param name="modifyType"></param>
        /// <param name="changeValue"></param>
        private void Modify(int numericType, int modifyType, int changeValue)
        {
            if(changeValue==0)return;
            int index = (numericType+1) * BaseOffsetMul;
            int modifyIndex = modifyType + index;
            Set(numericType,modifyType,GetByKey(modifyIndex) +changeValue);
        }

        public void Modify(int numericType,NumericOperation op, int level, int changeValue)
        {
            Modify(numericType, GetOpIndex(level, op), changeValue);
        }
        #endregion

        /// <summary>
        /// 经过限制器处理的缓存值
        /// </summary>
        /// <param name="numericType"></param>
        /// <returns></returns>
        public int GetResult(int numericType)
        {
            //取反下标
            return GetByKey(-BaseOffsetMul + numericType);
        }
        
        /// <summary>
        /// 获取未经限制的结果值
        /// </summary>
        /// <param name="numericType"></param>
        /// <returns></returns>
        public int GetRawValue(int numericType)
        {
            return GetByKey(numericType);
        }
        protected int GetByKey(int key)
        {
            this.NumericDic.TryGetValue(key, out var value);
            return value;
        }
        protected virtual int Update(int numericType,int modifyType)
        {
            int result = 0;
            int start = (numericType+1) * BaseOffsetMul;
            for (int level = 0; level <= MaxLevel; level++)//maxLevel越大 性能越差
            {
#if ACTIVE_COVER_OPERATE
                int cover = start + GetOpIndex(level, NumericOperation.Cover);
                int coverValue = GetByKey(cover);
                if (coverValue != 0)
                {
                    result = coverValue;
                    continue;
                }
#endif
                int add = start + GetOpIndex(level, NumericOperation.Add);
                int pct = start + GetOpIndex(level, NumericOperation.Pct);
                result = result * (GetByKey(pct) + 100) / 100 + GetByKey(add);
            }
            return result;
        }
    }
}