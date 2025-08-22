using System;

namespace FrameSyncBattle
{
    public class TestSkill : SkillBase
    {

        public static SkillData TestData()
        {
            var data =  new SkillData();
            data.Id = "test1";
            data.Icon = "test1_icon";
            data.Desc = "test1_desc";
            data.Name = "test1_name";
            data.CastRange = 10f;
            data.CoolDown = 5f;
            data.CostMp = 0;
            data.IsPassive = false;
            data.SubType = SkillSubType.HeroAttackSkill;
            data.TargetType = SkillTargetType.Unit;
            data.AIRx = SkillAITargetRx.Enemy;
            data.AITarget = SkillAITarget.Near;
            return data;
        }


        protected override void OnEnterFlowState(FsBattleLogic battle, SkillFlow preState)
        {
            base.OnEnterFlowState(battle, preState);
            switch (State)
            {
                case SkillFlow.None:
                    break;
                case SkillFlow.StartCast:
                    Owner.PlayAnimation(AnimationConstant.Attack);
                    break;
                case SkillFlow.StartEffect:
                    SetCastCool();
                    var target = CastTarget;
                    var start = Owner.Position;
                    for (int i = 0; i < 5; i++)
                    {
                        var lockMissile = battle.AddEntity<FsMissileLogic>(this.Owner.Team,"missile",new FsEntityInitData(){Euler = this.Owner.Euler,Position = start});
                        lockMissile.SetBase("cube", 10, 0.5f, battle.RandomGen.Next(-90, 90)).AimTarget(start,target,true).Fire(Owner,null, MissileCallback);
                    }
                    break;
                case SkillFlow.Affecting:
                    break;
                case SkillFlow.EndEffect:
                    break;
                case SkillFlow.EndCast:
                    break;
                case SkillFlow.Finish:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void MissileCallback(FsBattleLogic logic, FsMissileLogic missileLogic, bool valid)
        {
            if (valid)
            {
                FsDamageInfo damageInfo = FsDamageInfo.CreateAttackDamage(missileLogic.Source,missileLogic.Target,1f);
                logic.ProcessDamage(damageInfo);
                missileLogic.Target.BuffHandler.AddBuff(logic,missileLogic.Source,null,missileLogic.Target,Buff_Stun.CommonId,1,1);
            }
        }

        protected override SkillFlow OnFlowStateFrame(FsBattleLogic battle, FsCmd cmd)
        {
            switch (State)
            {
                case SkillFlow.None:
                    break;
                case SkillFlow.StartCast:
                    if (StateTimer >= 0.3f)
                    {
                        ChangeFlowState(battle,SkillFlow.StartEffect);
                    }
                    break;
                case SkillFlow.StartEffect:
                    ChangeFlowState(battle,SkillFlow.EndEffect);
                    break;
                case SkillFlow.Affecting:
                    //非持续性技能 不会进入该状态
                    break;
                case SkillFlow.EndEffect:
                    ChangeFlowState(battle,SkillFlow.EndCast);
                    break;
                case SkillFlow.EndCast:
                    //施法后摇1s 测试
                    if(StateTimer >= 1f)
                        ChangeFlowState(battle,SkillFlow.Finish);
                    break;
                case SkillFlow.Finish:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return State;
        }
    }
}