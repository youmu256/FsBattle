using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    public partial class FsUnitLogic
    {
        public FsUnitStateFlags StateFlags { get; private set; }
        public FsUnitProperty Property { get; private set; }
        

        public void InitStatus(FsUnitPropertyInitData basic)
        {
            StateFlags = new FsUnitStateFlags();
            
            Property = new FsUnitProperty(PropertyChangeCallback);
            Property.SetPropertyBase(FsUnitPropertyType.HpMax,basic.HpMax);
            Property.SetPropertyBase(FsUnitPropertyType.MpMax,basic.MpMax);
            Property.SetPropertyBase(FsUnitPropertyType.Attack,basic.Attack);
            Property.SetPropertyBase(FsUnitPropertyType.AttackSpdPct,100);
            Property.SetPropertyBase(FsUnitPropertyType.AttackRange,basic.AttackRange);
            Property.SetPropertyBase(FsUnitPropertyType.Defend,basic.Defend);
            Property.SetPropertyBase(FsUnitPropertyType.MoveSpeed,basic.MoveSpeed);
            HpPercent = 1f;
            MpPercent = 1f;
        }

        private void PropertyChangeCallback(FsUnitPropertyType propertyType, int last, int value)
        {
            //资源类最大值属性变化时 应该保留当前百分比
            switch (propertyType)
            {
                case FsUnitPropertyType.HpMax:
                    if (last != 0)
                    {
                        var hpPct = _hpCurrent/last;
                        HpPercent = hpPct;
                    }
                    break;
                case FsUnitPropertyType.MpMax:
                    if (last != 0)
                    {
                        var mpPct = _mpCurrent/last;
                        MpPercent = mpPct;
                    }
                    break;
                case FsUnitPropertyType.MoveSpeed:
                    this.MoveService?.UpdateMoveSpeed(value);
                    break;
            }
        }

        #region Resources&Property

        private int _hpCurrent = 0;

        public int HpCurrent
        {
            get { return _hpCurrent; }
            set { _hpCurrent = Mathf.Clamp(value,0,HpMax); }
        }

        public float HpPercent
        {
            get
            {
                if (HpMax == 0) return 0;
                return HpCurrent*1f/HpMax;
            }
            set
            {
                value = Mathf.Clamp(value,0f,1f);
                HpCurrent = (int) (value * HpMax);
            }
        }
        
        private int _mpCurrent = 0;

        public int MpCurrent
        {
            get { return _mpCurrent; }
            set { _mpCurrent = Mathf.Clamp(value, 0, MpMax); }
        }
        public float MpPercent
        {
            get
            {
                if (MpMax == 0) return 0;
                return MpCurrent*1f/MpMax;
            }
            set
            {
                value = Mathf.Clamp(value,0f,1f);
                MpCurrent = (int) (value * MpMax);
            }
        }
        public int HpMax => Property.Get(FsUnitPropertyType.HpMax);

        public int MpMax => Property.Get(FsUnitPropertyType.MpMax);

        #endregion
    }
}