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

        public static FsDamageInfo CreateAttackDamage(FsUnitLogic source, FsUnitLogic target)
        {
            FsDamageInfo info = new FsDamageInfo();
            info.Source = target;
            info.Target = target;
            info.Damage = source.Property.Get(FsUnitPropertyType.Attack);
            info.DamageType = FsDamageType.Physics;
            info.CriticalPct = 0;
            info.Tags |= FsDamageInfoTag.NormalAttack;
            return info;
        }
        
    }

    public enum FsDamageType
    {
        Physics,
        Magic,
    }
    
    [Flags]
    public enum FsDamageInfoTag
    {
        NormalAttack,//普通攻击
        SkillAttack,//技能攻击
        IsAddition,//是附加伤害
        IsDirect,//是直接伤害
    }
}