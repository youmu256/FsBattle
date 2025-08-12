using System;
using System.Collections.Generic;
using UnityEngine;
namespace FrameSyncBattle
{
    public class UnitAIFsm :IFsm<FsUnitAI>
    {
        public FsUnitAI Context;
        public IFsmState<FsUnitAI> Current;
        public string CurrentStateName { get; private set; }
        public Dictionary<string, FsUnitAIStateBase> StateMap = new();

        public void AddState(string key,IFsmState<FsUnitAI> state)
        {
            if (StateMap.ContainsKey(key)) return;
            StateMap.Add(key,state as FsUnitAIStateBase);
        }
        public IFsmState<FsUnitAI> GetState(string stateName)
        {
            if (StateMap.ContainsKey(stateName)) return StateMap[stateName];
            return null;
        }
        public void ChangeState(string stateName,bool sameChange = false)
        {
            var next = GetState(stateName);
            if (next == null) return;
            if (sameChange == false && next == Current) return;
            Current?.Exit(Context);
            Current = next;
            CurrentStateName = stateName;
            Current.Enter(Context);
        }

        public void UpdateFsm(float deltaTime)
        {
            Current?.Update(Context,deltaTime);
        }
    }
    public class AIMiddleState
    {
        public const string AttackMove = "AttackMove";//等于A地板
        public const string AttackEntity = "AttackEntity"; //追击并且攻击单位 完成后进入Stop
        public const string AttackGround = "AttackGround";//一直攻击地面 如果有最小射程 会自动调整攻击位置来满足
        public const string MoveToPosition = "MoveToPosition"; //移动到指定位置
        public const string Follow = "Follow";//跟随移动
        public const string Stop = "Stop";
        public const string Hold = "Hold";
        public const string Skill = "Skill";
        public const string Death = "Death";
    }
    public class AIBaseState
    {
        public const string Move = "Move";
        public const string MoveToEntity = "MoveToEntity";
        public const string Attack = "Attack";
        public const string Idle = "Idle";
        public const string Death = "Death";
        public const string Skill = "Skill";
    }
    public abstract class FsUnitAIStateBase: IFsmState<FsUnitAI>
    {
        //应该设计状态优先级来控制转换吗
        
        public abstract void Enter(FsUnitAI content);

        public abstract void Update(FsUnitAI content, float deltaTime);

        public abstract void Exit(FsUnitAI content);
    }

    public class BState_Death : FsUnitAIStateBase
    {
        public override void Enter(FsUnitAI content)
        {
            content.Me.PlayAnimation(new PlayAnimParam(AnimationConstant.Death,0,1,true));
        }
        public override void Update(FsUnitAI content, float deltaTime)
        {
            
        }

        public override void Exit(FsUnitAI content)
        {
            
        }
    }
    public class BState_Idle : FsUnitAIStateBase
    {
        public override void Enter(FsUnitAI content)
        {
            content.Me.PlayAnimation(new PlayAnimParam(AnimationConstant.Idle,0,1,true));
        }
        public override void Update(FsUnitAI content, float deltaTime)
        {
            
        }

        public override void Exit(FsUnitAI content)
        {
            
        }
    }
    public class BState_Skill : FsUnitAIStateBase
    {
        
        public FsUnitLogic TargetEntity;
        public Vector3 TargetPosition;
        public string SkillId;
        public SkillBase CastingSkill;
        
        public override void Enter(FsUnitAI content)
        {
            TargetEntity = content.PM_TargetEntity;
            TargetPosition = content.PM_TargetPosition;
            SkillId = content.PM_SkillId;
            content.Me.PlayAnimation(new PlayAnimParam(AnimationConstant.Idle,0,1,true));
        }
        
        public override void Update(FsUnitAI content, float deltaTime)
        {
            if (CastingSkill != null)
            {
                //正在施法的话 等技能释放接受后进入普通状态
                if (CastingSkill.State is SkillFlow.None or SkillFlow.Finish)
                {
                    content.RequestChangeBase(AIBaseState.Idle);
                    return;
                }
                return;
            }
            
            
            var skill = content.Me.SkillHandler.FindById(SkillId);
            if (skill != null)
            {
                bool castResult = false;
                if (skill.IsReadyToCast())
                {
                    switch (skill.Data.TargetType)
                    {
                        case SkillTargetType.None:
                            castResult = skill.TryCast(content.Battle);
                            break;
                        case SkillTargetType.Point:
                            castResult = skill.TryCast(content.Battle,TargetPosition);
                            break;
                        case SkillTargetType.Unit:
                            castResult = skill.TryCast(content.Battle,TargetEntity);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (castResult == false)
                    {
                        content.RequestChangeBase(AIBaseState.Idle);
                        return;
                    }
                    else
                    {
                        CastingSkill = skill;
                    }
                }
            }
            else
            {
                content.RequestChangeBase(AIBaseState.Idle);
            }
        }
        
        public override void Exit(FsUnitAI content)
        {
            CastingSkill = null;
            SkillId = null;
            TargetEntity = null;
        }
    }
    
    //基础攻击状态 只关心当前这一次攻击行为的控制
    public class BState_Attack : FsUnitAIStateBase
    {
        public FsUnitLogic Target;
        public override void Enter(FsUnitAI content)
        {
            //进入Attack状态就代表已经满足所有前提要求 这些条件在转换状态时候判断
            
            Target = content.PB_TargetEntity;
            //停止移动 看向目标 发起攻击
            //不管如何先进入idle
            content.Me.PlayAnimation(new PlayAnimParam(AnimationConstant.Idle,0,1,true));
            content.Me.NormalAttack.AttackTarget(Target);
        }

        public override void Update(FsUnitAI content, float deltaTime)
        {
            if (content.Me.StateFlags.HasAnyState(FsUnitStateFlag.Attack) == false)
            {
                content.RequestChangeBase(AIBaseState.Idle);
                return;
            }
        }
        public override void Exit(FsUnitAI content)
        {
            //强制停止攻击 攻击准备动画逻辑应该就在这个动作中实现
            content.Me.NormalAttack.StopAttack();
            Target = null;
        }
    }
    public class BState_Move : FsUnitAIStateBase
    {
        public float ReachDistance;
        public FsUnitLogic TargetEntity;
        public Vector3 TargetPosition;
        
        private float nextUpdateRemainTime = 0;

        public override void Enter(FsUnitAI content)
        {
            ReachDistance = content.PB_MoveReachDistance;
            TargetEntity = content.PB_TargetEntity;
            TargetPosition = content.PB_TargetPosition;
            nextUpdateRemainTime = 0;
            content.Me.PlayAnimation(new PlayAnimParam(AnimationConstant.Move,0,1,true));
        }
        public override void Update(FsUnitAI content, float deltaTime)
        {
            var goal = TargetEntity?.Position ?? TargetPosition;
            var goalRadius = TargetEntity?.Radius ?? 0;
            var reachCheck = ReachDistance + goalRadius;
            //FsDebug.Log($"AI CHECK REACH TARGET {DistanceUtils.DistanceBetween2D(content.Me,goal,true)} ? {reachCheck}");
            if (DistanceUtils.IsReachPosition2D(content.Me, true, goal, reachCheck))
            {
                content.RequestChangeBase(AIBaseState.Idle);
            }
            else
            {
                nextUpdateRemainTime -= deltaTime;
                if (nextUpdateRemainTime <= 0)
                {
                    nextUpdateRemainTime = 0.2f;
                    content.Me.MoveService.MoveToPosition(goal, ReachDistance);
                }
            }
        }

        public override void Exit(FsUnitAI content)
        {
            ReachDistance = 0;
            TargetEntity = null;
            TargetPosition = Vector3.zero;
            content.Me.MoveService.StopMove();
        }
    }
    #region MiddleAI
    public class MState_Death : FsUnitAIStateBase
    {
        public override void Enter(FsUnitAI content)
        {
            
        }

        public override void Update(FsUnitAI content, float deltaTime)
        {
            
        }

        public override void Exit(FsUnitAI content)
        {
            
        }
    }
    public class MState_Hold : FsUnitAIStateBase
    {
        public override void Enter(FsUnitAI content)
        {
            
        }

        public override void Update(FsUnitAI content, float deltaTime)
        {
            
        }

        public override void Exit(FsUnitAI content)
        {
            
        }
    }
    
    
    public class MState_Skill : FsUnitAIStateBase
    {
        //靠近去释放技能 但是现在默认技能都无限距离
        
        public FsUnitLogic TargetEntity;
        public Vector3 TargetPosition;
        public string SkillId;
        public SkillBase CastingSkill;
        
        
        public override void Enter(FsUnitAI content)
        {
            TargetEntity = content.PM_TargetEntity;
            TargetPosition = content.PM_TargetPosition;
            SkillId = content.PM_SkillId;
        }

        public override void Update(FsUnitAI content, float deltaTime)
        {
            //本来要靠近在释放技能 现在先默认都无限距离
            if (CastingSkill != null)
            {
                if (CastingSkill.State is SkillFlow.None or SkillFlow.Finish)
                    content.RequestChangeMiddle(AIMiddleState.Hold);
                return;
            }

            var skill = content.Me.SkillHandler.FindById(SkillId);
            if (skill != null)
            {
                if (skill.IsReadyToCast())
                {
                    //交由基础状态机去实际释放技能
                    content.PB_TargetEntity = TargetEntity;
                    content.PB_TargetPosition = TargetPosition;
                    content.PB_SkillId = SkillId;
                    content.RequestChangeBase(AIBaseState.Skill,false);
                    CastingSkill = skill;
                }
            }
            else
            {
                //如果skill丢失 就直接停止
                content.RequestChangeMiddle(AIMiddleState.Hold);
            }
        }

        public override void Exit(FsUnitAI content)
        {
            CastingSkill = null;
            TargetEntity = null;
            SkillId = null;
        }
    }
    
    public class MState_AttackTarget : FsUnitAIStateBase
    {
        public FsUnitLogic TargetEntity;

        public override void Enter(FsUnitAI content)
        {
            TargetEntity = content.PM_TargetEntity;
        }
        public override void Update(FsUnitAI content, float deltaTime)
        {
            float attackRange = content.Me.GetAttackRange();
            float chaseStopDistance = attackRange - 0.5f;
            //-0.5 保证处于攻击范围内，防止在攻击范围边缘反复鬼畜
            if (content.Battle.EntityService.IsEntityValidTobeTargeted(content.Me, TargetEntity) == false || content.Me.CanAttack() == false)
            {
                TargetEntity = null;
            }
            if (TargetEntity != null)
            {
                //处于攻击行动中时 应该不能让AI切换成其他动作 比如切换进入移动等
                //可以攻击的情况下 尝试转入攻击行动状态
                if (content.Me.NormalAttack.AttackReady(false))
                {
                    var dis = DistanceUtils.DistanceBetween2D(content.Me, TargetEntity, true);
                    if (dis <= attackRange)
                    {
                        //可以重复进入攻击状态 来发起新的攻击
                        content.PB_TargetEntity = TargetEntity;
                        content.RequestChangeBase(AIBaseState.Attack,true);
                    }
                    else
                    {
                        ChaseTarget(content,chaseStopDistance);
                    }
                }else
                {
                    var inAttacking = content.Me.NormalAttack.GetCurrentState() != AttackFlowState.None;
                    //FsDebug.Log($"ATTACK CHASE  attacking:{inAttacking} {content.Me.NormalAttack.GetCurrentState()}");
                    if(inAttacking == false)
                        ChaseTarget(content,chaseStopDistance);
                }
            }
            else
            {
                content.RequestChangeMiddle(AIMiddleState.Hold);
            }
        }
        public override void Exit(FsUnitAI content)
        {
            TargetEntity = null;
        }

        private void ChaseTarget(FsUnitAI content,float stopDistance)
        {
            if(content.Me.CanMove())
            {
                content.PB_TargetEntity = TargetEntity;
                content.PB_MoveReachDistance = stopDistance;
                content.RequestChangeBase(AIBaseState.Move);
            }
        }
        
    }
    #endregion
    
    
    public class FsUnitAI : IFsEntityFrame,IFsmContent
    {
        //基础行为AI 待机 死亡 攻击 施法 移动(静态目标) 移动(动态目标)
        //上层策略AI 判断技能释放时机 仇恨目标管理

        public FsUnitAI(FsBattleLogic battle,FsUnitLogic owner)
        {
            
            /*
             * 总的来说
             * Base层主要负责控制最基本的行动 比如攻击 移动 待机
             * Middle层主要是按照情况去切换Base层来达成目的
             * Base层自身只关注自身行动是否因为特殊原因打断 不应该考虑更多
             * Middle层需要保证在进行Base层状态转换的时候 已经完善考虑了，不要把问题过多留给Base层
             */
            
            Me = owner;
            Battle = battle;
            BaseAIFsm = new UnitAIFsm();
            BaseAIFsm.AddState(AIBaseState.Idle,new BState_Idle());
            BaseAIFsm.AddState(AIBaseState.Attack,new BState_Attack());
            BaseAIFsm.AddState(AIBaseState.Skill,new BState_Skill());
            BaseAIFsm.AddState(AIBaseState.Move,new BState_Move());
            BaseAIFsm.AddState(AIBaseState.Death,new BState_Death());
            
            MiddleAIFsm = new UnitAIFsm();
            MiddleAIFsm.AddState(AIMiddleState.Death,new MState_Death());
            MiddleAIFsm.AddState(AIMiddleState.Hold,new MState_Hold());
            MiddleAIFsm.AddState(AIMiddleState.AttackEntity,new MState_AttackTarget());
            MiddleAIFsm.AddState(AIMiddleState.Skill,new MState_Skill());

            BaseAIFsm.Context = this;
            MiddleAIFsm.Context = this;
            //start state
            BaseAIFsm.ChangeState(AIBaseState.Idle);
            MiddleAIFsm.ChangeState(AIMiddleState.Hold);
        }

        private bool StartFlag = false;
        
        public void OnEntityFrame(FsBattleLogic battle, FsUnitLogic entity, float deltaTime, FsCmd cmd)
        {
            if (StartFlag == false)
            {
                StartFlag = true;
            }
            
            if (entity.IsDead)
            {
                BaseAIFsm.ChangeState(AIBaseState.Death);
                MiddleAIFsm.ChangeState(AIMiddleState.Death);
            }
            else
            {

                var skillCastOrder = Me.SkillHandler.AIAutoCastCheck(battle);
                if (skillCastOrder != null)
                {
                    //释放技能
                    //TODO USE SKILL ORDER
                    PM_SkillId = skillCastOrder.Id;
                    PM_TargetEntity = skillCastOrder.CastTarget;
                    PM_TargetPosition = skillCastOrder.CastPoint;
                    RequestChangeMiddle(AIMiddleState.Skill);
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
                            PM_TargetEntity = targets[0];
                            RequestChangeMiddle(AIMiddleState.AttackEntity);
                        }
                    }
                }
            }
            MiddleAIFsm.UpdateFsm(battle.FrameLength);
            BaseAIFsm.UpdateFsm(battle.FrameLength);
        }

        private bool TargetFilter(FsBattleLogic battle, FsUnitLogic target)
        {
            return battle.EntityService.IsEntityValidTobeTargeted(Me,target) && battle.EntityService.IsEnemy(Me,target);
        }


        public FsBattleLogic Battle;
        public FsUnitLogic Me;
        protected UnitAIFsm BaseAIFsm;
        protected UnitAIFsm MiddleAIFsm;

        #region 状态机切换时的参数 接收的状态Enter时就要自己记录参数 并且ResetParam

        public float PM_MoveReachDistance { get; set; }
        public FsUnitLogic PM_TargetEntity { get; set; }
        public Vector3 PM_TargetPosition { get; set; }
        public string PM_SkillId { get; set; }
        public float PB_MoveReachDistance { get; set; }
        public FsUnitLogic PB_TargetEntity { get; set; }
        public Vector3 PB_TargetPosition { get; set; }

        public string PB_SkillId { get; set; }
        #endregion

        public void RequestChangeBase(string stateName, bool sameChange = false)
        {
            this.BaseAIFsm.ChangeState(stateName, sameChange);
            ResetBaseParam();
        }
        public void RequestChangeMiddle(string stateName, bool sameChange = false)
        {
            this.MiddleAIFsm.ChangeState(stateName, sameChange);
            ResetMiddleParam();
        }
        public void ResetBaseParam()
        {
            PB_TargetPosition = Vector3.zero;
            PB_TargetEntity = null;
            PB_MoveReachDistance = 0;
            PB_SkillId = null;
        }
        public void ResetMiddleParam()
        {
            PM_TargetPosition = Vector3.zero;
            PM_TargetEntity = null;
            PM_MoveReachDistance = 0;
            PM_SkillId = null;
        }
    }
    
    
    public class HState_Complex : FsUnitAIStateBase
    {
        public override void Enter(FsUnitAI content)
        {
            
        }
        public override void Update(FsUnitAI content, float deltaTime)
        {
            /*
             * 1.按照索敌逻辑(仇恨距离等因素) 分配一次仇恨目标 并让M进入目标攻击逻辑AI
             * 2.检查技能状态，并且检测技能AI前置，如果能释放技能则让M进入施法AI
             */
        }
        public override void Exit(FsUnitAI content)
        {
            
        }
    }
}