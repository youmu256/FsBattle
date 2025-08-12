using System;
using System.Collections.Generic;

namespace FrameSyncBattle
{
    public interface IAutoCasterAI
    {
        public SkillCastOrder TryCast(FsBattleLogic battleLogic, FsUnitLogic caster, SkillBase skill);
    }
    
    public class SkillAICastHelper : IAutoCasterAI
    {
        protected SkillAITargetRx Rx { get; private set; }
        protected SkillAITarget Condition { get; private set; }
        protected FsUnitLogic Caster { get; private set; }

        public SkillCastOrder TryCast(FsBattleLogic battleLogic, FsUnitLogic caster, SkillBase skill)
        {
            if (skill.IsReadyToStartCast() == false) return null;
            Caster = caster;
            Rx = skill.Data.AIRx;
            Condition = skill.Data.AITarget;
            
            var units = battleLogic.EntityService.Units;
            var filteredUnits = new List<FsUnitLogic>();
            if (Rx == SkillAITargetRx.None || Condition == SkillAITarget.None)
            {
                return new SkillCastOrder();
            }

            foreach (var unit in units)
            {
                var rxMatch = true;
                switch (Rx)
                {
                    case SkillAITargetRx.Friend:
                        if (battleLogic.EntityService.IsFriend(caster, unit) == false)
                            rxMatch = false;
                        break;
                    case SkillAITargetRx.Enemy:
                        if (battleLogic.EntityService.IsEnemy(caster, unit) == false)
                            rxMatch = false;
                        break;
                    default:
                        break;
                }

                if (rxMatch == false)
                    continue;
                filteredUnits.Add(unit);
            }

            if (filteredUnits.Count <= 0) return null;

            FsUnitLogic target = null;
            switch (Condition)
            {
                case SkillAITarget.AnyOne:
                    target = filteredUnits[0];
                    break;
                case SkillAITarget.Near:
                    filteredUnits.Sort(DistanceComparison);
                    target = filteredUnits[0];
                    break;
                case SkillAITarget.Far:
                    filteredUnits.Sort(DistanceComparison);
                    target = filteredUnits[^1];
                    break;
                case SkillAITarget.HpLow:
                    filteredUnits.Sort(HpComparison);
                    target = filteredUnits[0];
                    break;
                case SkillAITarget.HpHigh:
                    filteredUnits.Sort(HpComparison);
                    target = filteredUnits[^1];
                    break;
                case SkillAITarget.AttackLow:
                    filteredUnits.Sort(AtkComparison);
                    target = filteredUnits[0];
                    break;
                case SkillAITarget.AttackHigh:
                    filteredUnits.Sort(AtkComparison);
                    target = filteredUnits[^1];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (target != null)
            {
                switch (skill.Data.TargetType)
                {
                    case SkillTargetType.None:
                        return new SkillCastOrder(){Id = skill.Id};
                    case SkillTargetType.Point:
                        return new SkillCastOrder(){Id = skill.Id,CastPoint = target.Position};
                    case SkillTargetType.Unit:
                        return new SkillCastOrder(){Id = skill.Id,CastTarget = target};
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return null;
        }
        private int DistanceComparison(FsUnitLogic x, FsUnitLogic y)
        {
            var d1 = DistanceUtils.DistanceBetween2D(Caster, x, true);
            var d2 = DistanceUtils.DistanceBetween2D(Caster, y, true);
            var dd = d1 - d2;
            if (dd > 0)
                return 1;
            if (dd < 0)
                return -1;
            return 0;
        }
        private int HpComparison(FsUnitLogic x, FsUnitLogic y)
        {
            var dd = x.HpCurrent - y.HpCurrent;
            if (dd > 0)
                return 1;
            if (dd < 0)
                return -1;
            return 0;
        }
        
        private int AtkComparison(FsUnitLogic x, FsUnitLogic y)
        {
            var dd = x.Property.Get(FsUnitPropertyType.Attack) - y.Property.Get(FsUnitPropertyType.Attack);
            if (dd > 0)
                return 1;
            if (dd < 0)
                return -1;
            return 0;
        }
    }

    public enum SkillAITargetRx
    {
        None,
        Friend,
        Enemy,
    }
    
    public enum SkillAITarget
    {
        None,
        AnyOne,
        Near,
        Far,
        HpLow,
        HpHigh,
        AttackLow,
        AttackHigh
    }

}