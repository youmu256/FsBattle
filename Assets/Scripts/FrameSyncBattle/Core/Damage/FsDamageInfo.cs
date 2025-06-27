using System;

namespace FrameSyncBattle
{
    /// <summary>
    /// 一次伤害
    /// </summary>
    public class FsDamageInfo
    {
        public FsUnitLogic Source;
        public FsUnitLogic Target;
        public FsDamageType DamageType;
        public int Damage;
        public int CriticalPct;
        public FsDamageInfoTag Tags;
    }

    public enum FsDamageType
    {
        Physics,
        Magic,
    }
    
    [Flags]
    public enum FsDamageInfoTag
    {
        IsAddition,//是附加伤害
        IsDirect,//是直接伤害
    }
}