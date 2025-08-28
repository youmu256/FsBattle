using System;

namespace FrameSyncBattle
{
    public enum FsDamageType
    {
        Physics,
        Magic,
    }
    
    [Flags]
    public enum FsDamageInfoTag
    {
        NormalAttack = 1<<0,//普通攻击
        SkillAttack = 1<<1,//技能攻击
        SaSkillAttack = 1<<2,//怒气技能攻击
        IsAddition = 1<<3,//是附加伤害
        IsDirect = 1<<4,//是直接伤害
        IsCritical = 1<<5,//是暴击伤害
    }
    /// <summary>
    /// 一次伤害信息
    /// </summary>
    public class FsDamageInfo
    {
        public FsUnitLogic Source;
        public FsUnitLogic Target;
        public FsDamageType DamageType;
        public int Damage;
        /**本次攻击的额外暴击率*/
        public int CriticalPct;
        /**本次攻击的额外爆伤*/
        public int CriticalBonus;
        public FsDamageInfoTag Tags;
        public int SourceAttackIndex;//攻击index
        public FsDamageInfo BindAttackIndex(int data)
        {
            this.SourceAttackIndex = data;
            return this;
        }

        private FsDamageInfo()
        {
            //TODO 防止被外部随便创建 后续走对象池控制
        }

        public FsDamageInfo GetCopy()
        {
            FsDamageInfo info = new FsDamageInfo();
            info.Source = this.Source;
            info.Target = this.Target;
            info.DamageType = this.DamageType;
            info.Damage = this.Damage;
            info.CriticalPct = this.CriticalPct;
            info.CriticalBonus = this.CriticalBonus;
            info.Tags = this.Tags;
            info.SourceAttackIndex = this.SourceAttackIndex;
            return info;
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

        public static FsDamageInfo CreateSkillDamage(FsUnitLogic source, FsUnitLogic target, float pct)
        {
            FsDamageInfo info = new FsDamageInfo();
            info.Source = target;
            info.Target = target;
            info.Damage = (int) (source.Property.Get(FsUnitPropertyType.Attack) * pct);
            info.DamageType = FsDamageType.Physics;
            info.CriticalPct = 0;
            info.Tags |= FsDamageInfoTag.SkillAttack;
            return info;
        }
    }
    
    public partial class FsBattleLogic
    {
        /// <summary>
        /// 处理伤害函数
        /// </summary>
        /// <param name="info"></param>
        public void ProcessDamage(FsDamageInfo info)
        {
            if (info.Target == null || info.Target.IsDead) return;
            //伤害结算PreEvent 角色身上的技能&Buff需要响应受伤事件
            
            //防御计算
            info.Damage -= info.Target.Property.Get(FsUnitPropertyType.Defend);
            
            //暴击计算
            var criticalPct = info.CriticalPct;
            var criticalBonus = info.CriticalBonus;
            if (info.Source != null)
            {
                criticalPct += info.Source.Property.Get(FsUnitPropertyType.CriticalPct);
                criticalBonus += info.Source.Property.Get(FsUnitPropertyType.CriticalBonus);
            }
            if (criticalPct > 0 && criticalBonus > 0 && RandomGen.Next(100) <= criticalPct)
            {
                info.Damage = (int) (info.Damage * criticalBonus/100f);
                info.Tags |= FsDamageInfoTag.IsCritical;
            }
            //伤害结算PostEvent
            
            //按照游戏公式等修正伤害
            info.Target.HpCurrent -= info.Damage;
            info.Target.OnDamagedPost(this,info);
        }
    }

}