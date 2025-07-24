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
        public const string CastSkill = "CastSkill";
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
            content.Me.Play(new PlayAnimParam("Death",0,1,true));
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
            content.Me.Play(new PlayAnimParam("Idle",0,1,true));
        }
        public override void Update(FsUnitAI content, float deltaTime)
        {
            
        }

        public override void Exit(FsUnitAI content)
        {
            
        }
    }
    
    
    //基础攻击状态 只关心当前这一次攻击行为的控制
    public class BState_Attack : FsUnitAIStateBase
    {
        public FsUnitLogic Target;
        public override void Enter(FsUnitAI content)
        {
            Target = content.PB_TargetEntity;
            //停止移动 看向目标 发起攻击
            content.Me.NormalAttack.AttackTarget(Target);
        }

        public override void Update(FsUnitAI content, float deltaTime)
        {
            if (content.Me.StateFlags.HasAnyState(FsUnitStateFlag.Attack) == false)
            {
                content.RequestChangeBase(AIBaseState.Idle);
                return;
            }

            if (content.Me.NormalAttack.GetCurrentState() == AttackFlowState.FireEnd)
            {
                content.RequestChangeBase(AIBaseState.Idle);
                return;
            }
            
        }
        public override void Exit(FsUnitAI content)
        {
            //停止攻击
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
            content.Me.Play(new PlayAnimParam("Move",0,1,true));
        }
        public override void Update(FsUnitAI content, float deltaTime)
        {
            var goal = TargetEntity?.Position ?? TargetPosition;
            var goalRadius = TargetEntity?.Radius ?? 0;
            var reachCheck = ReachDistance + goalRadius;
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
            if (content.Battle.EntityService.IsEntityValidTobeTargeted(content.Me, TargetEntity) == false || content.Me.CanAttack() == false)
            {
                TargetEntity = null;
            }
            if (TargetEntity != null)
            {
                //处于攻击行动中时 应该不能让AI切换成其他动作 比如切换进入移动等
                if (content.Me.NormalAttack.GetCurrentState() == AttackFlowState.None)
                {
                    //chase and attack
                    var dis = DistanceUtils.DistanceBetween2D(content.Me, TargetEntity, true);
                    if (dis <= attackRange)
                    {
                        content.PB_TargetEntity = TargetEntity;
                        content.RequestChangeBase(AIBaseState.Attack);
                    }
                    else if(content.Me.CanMove())
                    {
                        //move close target
                        content.PB_TargetEntity = TargetEntity;
                        content.PB_MoveReachDistance = attackRange - 0.5f;//-0.5 保证处于攻击范围内，防止在攻击范围边缘反复鬼畜
                        content.RequestChangeBase(AIBaseState.Move);
                    }
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
    }
    #endregion
    
    
    public class FsUnitAI : IFsEntityFrame,IFsmContent
    {
        //基础行为AI 待机 死亡 攻击 施法 移动(静态目标) 移动(动态目标)
        //上层策略AI 判断技能释放时机 仇恨目标管理

        public FsUnitAI(FsBattleLogic battle,FsUnitLogic owner)
        {
            Me = owner;
            Battle = battle;
            BaseAIFsm = new UnitAIFsm();
            BaseAIFsm.AddState(AIBaseState.Idle,new BState_Idle());
            BaseAIFsm.AddState(AIBaseState.Attack,new BState_Attack());
            BaseAIFsm.AddState(AIBaseState.Move,new BState_Move());
            BaseAIFsm.AddState(AIBaseState.Death,new BState_Death());
            
            MiddleAIFsm = new UnitAIFsm();
            MiddleAIFsm.AddState(AIMiddleState.Death,new MState_Death());
            MiddleAIFsm.AddState(AIMiddleState.Hold,new MState_Hold());
            MiddleAIFsm.AddState(AIMiddleState.AttackEntity,new MState_AttackTarget());

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
                //搜索最近的目标去攻击
                if (Me.MpPercent >= 1)
                {
                    //try cast skill
                    Me.MpPercent = 0;
                    FsDebug.Log($"TODO AI CAST");
                }
                else
                {
                    //可以攻击的情况下 找到周围最近的敌人 转入M追击目标状态
                    if (Me.CanAttack() && Me.NormalAttack.GetCurrentState() == AttackFlowState.None)
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
        
        public float PB_MoveReachDistance { get; set; }
        public FsUnitLogic PB_TargetEntity { get; set; }
        public Vector3 PB_TargetPosition { get; set; }

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
        }
        public void ResetMiddleParam()
        {
            PM_TargetPosition = Vector3.zero;
            PM_TargetEntity = null;
            PM_MoveReachDistance = 0;
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