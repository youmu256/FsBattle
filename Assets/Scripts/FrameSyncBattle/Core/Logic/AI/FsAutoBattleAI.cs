using System.Collections.Generic;

namespace FrameSyncBattle
{
    
    public class FsAutoBattleAI
    {
        protected FsUnitLogic Me { get; private set; }
        private bool TargetFilter(FsBattleLogic battle, FsUnitLogic target)
        {
            return battle.EntityService.IsEntityValidTobeTargeted(Me,target) && battle.EntityService.IsEnemy(Me,target);
        }
        
        public void ProcessUnitAI(FsBattleLogic battle, FsUnitLogic unit)
        {
            if (unit.IsDead || unit.IsRemoved) return;
            this.Me = unit;
            //-----auto battle logic------
            var unitAi = Me.UnitAI;
            if (Me.UnitAI.CurrentMiddleState != AIMiddleState.Skill)
            {
                var skillCastOrder = Me.SkillHandler.AIAutoCastCheck(battle);
                if (skillCastOrder != null)
                {
                    //释放技能
                    unitAi.PM_SkillId = skillCastOrder.Id;
                    unitAi.PM_TargetEntity = skillCastOrder.CastTarget;
                    unitAi.PM_TargetPosition = skillCastOrder.CastPoint;
                    unitAi.RequestChangeMiddle(AIMiddleState.Skill);
                }
                else
                {
                    //可以攻击的情况下 找到周围最近的敌人 转入M追击目标状态
                    if (Me.CanAttack() && Me.NormalAttack.AttackReady(false))
                    {
                        List<FsUnitLogic> targets = new List<FsUnitLogic>();
                        battle.EntityService.CollectUnits(targets, TargetFilter);
                        if (targets.Count > 0)
                        {
                            targets.Sort(((a, b) =>
                            {
                                var disA = DistanceUtils.DistanceBetween2D(Me, a, true);
                                var disB = DistanceUtils.DistanceBetween2D(Me, b, true);
                                if (disA > disB)
                                    return 1;
                                return -1;
                            }));
                            unitAi.PM_TargetEntity = targets[0];
                            unitAi.RequestChangeMiddle(AIMiddleState.AttackEntity);
                        }
                    }
                }
            }
        }
    }
}