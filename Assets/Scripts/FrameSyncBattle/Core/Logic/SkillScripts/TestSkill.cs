using System;

namespace FrameSyncBattle
{
    public class TestSkill : SkillBase
    {

        public static SkillData GetTestData()
        {
            var data =  new SkillData();
            return data;
        }
        
        protected override SkillFlow FlowStateFrame(FsBattleLogic battle, FsCmd cmd)
        {
            switch (State)
            {
                case SkillFlow.None:
                    break;
                case SkillFlow.StartCast:
                    Owner.PlayAnimation(AnimationConstant.Attack);
                    if (StateTimer >= 0.5f)
                    {
                        ChangeFlowState(battle,SkillFlow.StartEffect);
                    }
                    break;
                case SkillFlow.StartEffect:
                    var target = CastTarget;
                    var start = Owner.Position;
                    var lockMissile = battle.AddEntity<FsMissileLogic>(this.Owner.Team,"missile",new FsEntityInitData(){Euler = this.Owner.Euler,Position = start});

                    for (int i = 0; i < 5; i++)
                    {
                        lockMissile.SetBase("cube", 10, 0.5f, battle.RandomGen.Next(-90, 90)).AimTarget(start,target,true).Fire(null, (
                            (logic, missileLogic, valid) =>
                            {
                                if (valid)
                                {
                                    FsDamageInfo damageInfo = FsDamageInfo.CreateAttackDamage(this.Owner,missileLogic.Target,1f);
                                    logic.ProcessDamage(damageInfo);
                                }
                            }));
                    }
                    
                    ChangeFlowState(battle,SkillFlow.EndEffect);
                    break;
                case SkillFlow.Affecting:
                    break;
                case SkillFlow.EndEffect:
                    ChangeFlowState(battle,SkillFlow.EndCast);
                    break;
                case SkillFlow.EndCast:
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