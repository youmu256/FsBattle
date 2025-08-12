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
        public int SourceAttackIndex;//攻击index
        public FsDamageInfo BindAttackIndex(int data)
        {
            this.SourceAttackIndex = data;
            return this;
        }
        /// <summary>
        /// 创建基础攻击伤害
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="pct"></param>
        /// <returns></returns>
        public static FsDamageInfo CreateAttackDamage(FsUnitLogic source, FsUnitLogic target,float pct)
        {
            FsDamageInfo info = new FsDamageInfo();
            info.Source = target;
            info.Target = target;
            info.Damage = (int) (source.Property.Get(FsUnitPropertyType.Attack) * pct);
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