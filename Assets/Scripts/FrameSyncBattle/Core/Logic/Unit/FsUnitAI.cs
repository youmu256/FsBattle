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
            if (content.Me.IsDead == false)
            {
                content.RequestChangeBase(AIBaseState.Idle);
            }
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
        public SkillBase OrderSkill;

        public override void Enter(FsUnitAI content)
        {
            TargetEntity = content.PB_TargetEntity;
            TargetPosition = content.PB_TargetPosition;
            SkillId = content.PB_SkillId;
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
            
            
            var skill = OrderSkill??content.Me.SkillHandler.FindById(SkillId);
            if (skill != null)
            {
                OrderSkill = skill;
                bool castResult = false;
                if (skill.IsReadyToStartCast())
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
            CastingSkill?.Stop(content.Battle);
            OrderSkill = null;
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
            if (content.Me.CanAttack() == false)
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
            if (content.Me.CanMove() == false)
            {
                content.RequestChangeBase(AIBaseState.Idle);
                return;
            }
            
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
        public SkillBase OrderSkill;
        
        public override void Enter(FsUnitAI content)
        {
            TargetEntity = content.PM_TargetEntity;
            TargetPosition = content.PM_TargetPosition;
            SkillId = content.PM_SkillId;
        }

        public override void Update(FsUnitAI content, float deltaTime)
        {
            if (CastingSkill != null)
            {
                //没有正常发动技能 或者技能已经结束都退出该AI状态
                if (CastingSkill.State is SkillFlow.None or SkillFlow.Finish)
                    content.RequestChangeMiddle(AIMiddleState.Hold);
                return;
            }

            var skill = OrderSkill??content.Me.SkillHandler.FindById(SkillId);
            if (skill != null)
            {
                OrderSkill = skill;
                float castRange = skill.CastRange;
                float chaseStopDistance = castRange - content.CasterRangeAdjust;
                //-0.5 保证处于攻击范围内，防止在攻击范围边缘反复鬼畜
                var dis = DistanceUtils.DistanceBetween2D(content.Me, TargetEntity, true);
                if (dis <= castRange)
                {
                    //cast
                    if (skill.IsReadyToStartCast())
                    {
                        //交由基础状态机去实际释放技能
                        content.PB_TargetEntity = TargetEntity;
                        content.PB_TargetPosition = TargetPosition;
                        content.PB_SkillId = SkillId;
                        content.RequestChangeBase(AIBaseState.Skill);
                        CastingSkill = skill;
                    }
                }
                else
                {
                    ChaseTarget(content,chaseStopDistance);
                }
            }
            else
            {
                //如果skill丢失 就直接停止
                content.RequestChangeMiddle(AIMiddleState.Hold);
            }
        }
        
        private void ChaseTarget(FsUnitAI content,float stopDistance)
        {
            if(content.Me.CanMove())
            {
                content.PB_TargetEntity = TargetEntity;
                content.PB_TargetPosition = TargetPosition;
                content.PB_MoveReachDistance = stopDistance;
                content.RequestChangeBase(AIBaseState.Move);
            }
        }

        public override void Exit(FsUnitAI content)
        {
            OrderSkill = null;
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
            float chaseStopDistance = attackRange - content.AttackRangeAdjust;
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

        public float CasterRangeAdjust = 0.5f;
        public float AttackRangeAdjust = 0.5f;
        
        public FsUnitAI(FsBattleLogic battle,FsUnitLogic owner)
        {
            
            /*
             * 总的来说
             * Base层主要负责控制最基本最具体的行动执行 比如攻击 移动 待机
             * Middle层主要是按照情况去切换Base层来达成复杂目的 追着敌人施法 追着敌人攻击 巡逻移动 警戒周围敌人 等等
             * 高层AI/玩家指令 控制Middle层的AI状态 比如一直尝试释放技能 按照仇恨优先级发布攻击目标等
             * 
             * Base层自身只关注自身行动是否因为特殊原因打断 不应该考虑更多
             * 关于行动切换/打断
             * 比如正在Base层攻击状态时 被上层控制要立刻进入Base层移动状态
             * 比如在War3中，正在攻击的单位，会被玩家发布新的移动命令直接打断
             * 但是在Middle层AI的实现中要避免出现随便打断的情况，比如Middle层攻击目标的状态中，只有攻击流程完成后才应该发布新的移动命令
             * 
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
            //Any Transition
            if (entity.IsDead)
            {
                MiddleAIFsm.ChangeState(AIMiddleState.Hold);//不单独搞Death状态了 都是空状态
                BaseAIFsm.ChangeState(AIBaseState.Death);
            }
            MiddleAIFsm.UpdateFsm(battle.FrameLength);
            BaseAIFsm.UpdateFsm(battle.FrameLength);
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

        public bool RequestChangeBase(string stateName, bool sameChange = false)
        {
            //Base状态的转换本身应该有限制 当前动作优先级以及转换优先级要进行比较才能考虑打断
            //ExitCheck来检查能否被新的状态切入
            this.BaseAIFsm.ChangeState(stateName, sameChange);
            ResetBaseParam();
            return true;
        }
        public bool RequestChangeMiddle(string stateName, bool sameChange = false)
        {
            this.MiddleAIFsm.ChangeState(stateName, sameChange);
            ResetMiddleParam();
            return true;
        }
        
        public string CurrentBaseState => BaseAIFsm.CurrentStateName;
        public string CurrentMiddleState => MiddleAIFsm.CurrentStateName;

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
}