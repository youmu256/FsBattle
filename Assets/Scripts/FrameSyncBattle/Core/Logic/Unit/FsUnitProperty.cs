using System;
using System.Collections.Generic;

namespace FrameSyncBattle
{

    public enum FsUnitPropertyType
    {
        HpMax = 1,
        MpMax,
        Attack,
        AttackSpdPct,
        AttackRange,
        Defend,
        MoveSpeed,
    }

    public enum FsPropertyLevel
    {
        Lv1 = 1,
        Lv2,
    }

    public struct FsUnitPropertyInitData
    {
        public int HpMax;
        public int MpMax;
        public int Attack;
        public int AttackRange;
        public int Defend;
        public int MoveSpeed;
    }

    public delegate void OnPropertyChangeEvent(FsUnitPropertyType propertyType, int last, int value);

    public class FsUnitProperty : INumericPropertiesLimiter
    {
        //NumericProperties 用来处理单纯量级的属性比较合适，比如最大生命值，攻击力等，处理攻击间隔则比较反习惯
        protected NumericProperties Properties { get; private set; }
        protected OnPropertyChangeEvent PropertyChangeCallBack { get; private set; }

        public FsUnitProperty(OnPropertyChangeEvent onPropertyChange)
        {
            Properties = new NumericProperties(3, this, OnPropertyChange);
            PropertyChangeCallBack = onPropertyChange;
        }

        public static float SpeedToTimePct(float p)
        {
            if (p == 0) return 1f;
            float abp = Math.Abs(p);
            if (p > 0)
            {
                p = 1 / (1 + abp);
            }
            else
            {
                p = 1 + abp;
            }

            return p;
        }
        
        public float CurrentAttackTimeScaler { get; private set; } = 1f;

        private void OnPropertyChange(int numericType, int last, int value)
        {
            //value 没有进行数值限制 是比较危险的原数值 对外来说，要取得经过限制的安全数值才行
            PropertyChangeCallBack?.Invoke((FsUnitPropertyType) numericType, last, value);
        }

        private void Modify(int numericType, NumericOperation op, int level, int changeValue)
        {
            //Properties管理的
            Properties.Modify(numericType, op, level, changeValue);
        }

        public void Modify(FsUnitPropertyType numericType, NumericOperation op, FsPropertyLevel level, int changeValue)
        {
            Modify((int) numericType, op, (int) level, changeValue);
        }

        public int Get(FsUnitPropertyType type)
        {
            return Properties.GetResult((int) type);
        }

        public int GetRaw(FsUnitPropertyType type)
        {
            return Properties.GetRawValue((int) type);
        }

        public void SetPropertyBase(FsUnitPropertyType type, int value)
        {
            int index = (int) type;
            Properties.SetBase(index, value);
        }

        int INumericPropertiesLimiter.Limit(int numericType, int value)
        {
            FsUnitPropertyType type = (FsUnitPropertyType) numericType;
            //一些数值取值限制
            if (type == FsUnitPropertyType.HpMax || type == FsUnitPropertyType.MpMax)
            {
                value = Math.Max(0, value);
            }

            return value;
        }
    }
}