using System.Collections.Generic;
using UnityEngine;
namespace FrameSyncBattle
{
    public class UnitAIFsm :IFsm<UnitAIContent>
    {
        public UnitAIContent Context;
        public IFsmState<UnitAIContent> Current;
        public string CurrentStateName { get; private set; }
        public Dictionary<string, FsUnitAIStateBase> StateMap = new();

        public void AddState(string key,IFsmState<UnitAIContent> state)
        {
            if (StateMap.ContainsKey(key)) return;
            StateMap.Add(key,state as FsUnitAIStateBase);
        }
        public IFsmState<UnitAIContent> GetState(string stateName)
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
    public class UnitAIContent : IFsmContent
    {
        public FsBattleLogic Battle;
        public FsUnitLogic Me;
        public FsUnitAI UnitAi;
        public UnitAIFsm BaseAIFsm;
        public UnitAIFsm MiddleAIFsm;
        //---这里是临时参数--用来在Command M-AI B-AI中传递参数而已 接收的状态Enter时就要自己记录参数 并且ResetParam
        public float MoveReachDistance { get; set; }
        public FsUnitLogic TargetEntity { get; set; }
        public Vector3 TargetPosition { get; set; }
        public void ResetParam()
        {
            TargetPosition = Vector3.zero;
            TargetEntity = null;
            MoveReachDistance = 0;
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
    public abstract class FsUnitAIStateBase: IFsmState<UnitAIContent>
    {
        //应该设计状态优先级来控制转换吗
        
        public abstract void Enter(UnitAIContent content);

        public abstract void Update(UnitAIContent content, float deltaTime);

        public abstract void Exit(UnitAIContent content);
    }

    public class BState_Death : FsUnitAIStateBase
    {
        public override void Enter(UnitAIContent content)
        {
            content.Me.Play(new PlayAnimParam("Death",0,0,true));
        }
        public override void Update(UnitAIContent content, float deltaTime)
        {
            
        }

        public override void Exit(UnitAIContent content)
        {
            
        }
    }
    public class BState_Idle : FsUnitAIStateBase
    {
        public override void Enter(UnitAIContent content)
        {
            content.Me.Play(new PlayAnimParam("Idle",0,0,true));
        }
        public override void Update(UnitAIContent content, float deltaTime)
        {
            
        }

        public override void Exit(UnitAIContent content)
        {
            
        }
    }
    
    
    //基础攻击状态 只关心当前这一次攻击行为的控制
    public class BState_Attack : FsUnitAIStateBase
    {
        public FsUnitLogic Target;
        public override void Enter(UnitAIContent content)
        {
            Target = content.TargetEntity;
            //停止移动 看向目标 发起攻击
            content.Me.NormalAttack.AttackTarget(Target);
        }

        public override void Update(UnitAIContent content, float deltaTime)
        {
            if (content.Me.StateFlags.HasAnyState(FsUnitStateFlag.Attack) == false)
            {
                content.BaseAIFsm.ChangeState(AIBaseState.Idle);
                return;
            }
        }
        public override void Exit(UnitAIContent content)
        {
            //停止攻击
            content.Me.NormalAttack.StopAttack();
            Target = null;
        }
    }
    public class BState_Move : FsUnitAIStateBase
    {
        public float ReachRange;
        public FsUnitLogic TargetEntity;
        public Vector3 TargetPosition;
        public override void Enter(UnitAIContent content)
        {
            ReachRange = content.MoveReachDistance;
            TargetEntity = content.TargetEntity;
            TargetPosition = content.TargetPosition;
            content.Me.Play(new PlayAnimParam("Move",0,0,true));
        }
        public override void Update(UnitAIContent content, float deltaTime)
        {
            //如果抵达目的地就切换成B-Idle
            var goal = TargetEntity?.Position ?? TargetPosition;
            if (DistanceUtils.IsReachPosition2D(content.Me, true, goal, ReachRange))
            {
                //change to idle
            }
        }

        public override void Exit(UnitAIContent content)
        {
            
        }
    }
    public class HState_Complex : FsUnitAIStateBase
    {
        public override void Enter(UnitAIContent content)
        {
            
        }
        public override void Update(UnitAIContent content, float deltaTime)
        {
            /*
             * 1.按照索敌逻辑(仇恨距离等因素) 分配一次仇恨目标 并让M进入目标攻击逻辑AI
             * 2.检查技能状态，并且检测技能AI前置，如果能释放技能则让M进入施法AI
             */
        }
        public override void Exit(UnitAIContent content)
        {
            
        }
    }
    #region MiddleAI
    
    public class MState_Hold : FsUnitAIStateBase
    {
        public override void Enter(UnitAIContent content)
        {
            
        }

        public override void Update(UnitAIContent content, float deltaTime)
        {
            
        }

        public override void Exit(UnitAIContent content)
        {
            
        }
    }
    public class MState_AttackTarget : FsUnitAIStateBase
    {
        public FsUnitLogic TargetEntity;

        public override void Enter(UnitAIContent content)
        {
            
        }
        public override void Update(UnitAIContent content, float deltaTime)
        {
            float attackRange = content.Me.GetAttackRange();
            if (content.Battle.EntityService.IsEntityValidTobeTargeted(content.Me, TargetEntity) == false || content.Me.CanAttack() == false)
            {
                TargetEntity = null;
            }
            if (TargetEntity != null)
            {
                //chase and attack
                var dis = DistanceUtils.DistanceBetween2D(content.Me, TargetEntity, true);
                if (dis <= attackRange)
                {
                    //attack
                }
                else if(content.Me.CanMove())
                {
                    //move close target
                }
            }
            else
            {
                content.MiddleAIFsm.ChangeState(AIMiddleState.Stop);
            }
        }
        public override void Exit(UnitAIContent content)
        {
            
        }
    }
    #endregion
    
    
    public class FsUnitAI : IFsEntityFrame
    {
        //基础行为AI 待机 死亡 攻击 施法 移动(静态目标) 移动(动态目标)
        //上层策略AI 判断技能释放时机 仇恨目标管理

        public UnitAIFsm BaseFsm;
        public UnitAIFsm MiddleFsm;
        public UnitAIContent AiContent;
        public FsUnitAI(FsUnitLogic owner)
        {
            BaseFsm = new UnitAIFsm();
            
            AiContent = new UnitAIContent();
            AiContent.Me = owner;
            AiContent.BaseAIFsm = BaseFsm;

            BaseFsm.Context = AiContent;
        }
        
        public void OnEntityFrame(FsBattleLogic battle, FsUnitLogic entity, float deltaTime, FsCmd cmd)
        {
            if (entity.IsDead)
            {
                BaseFsm.ChangeState(AIBaseState.Death);
            }
            BaseFsm.UpdateFsm(battle.FrameLength);
            //顶层AI需求 找最近的人进攻 如果可以释放技能则释放技能 
        }
    }
}