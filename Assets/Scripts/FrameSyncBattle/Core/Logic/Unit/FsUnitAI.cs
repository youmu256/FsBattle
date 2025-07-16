using System.Collections.Generic;
using System.Numerics;

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
        public FsUnitLogic Me;
        public FsUnitAI UnitAi;
        public UnitAIFsm BaseAIFsm;
        public UnitAIFsm MiddleAIFsm;
        //---这里是临时参数--用来在Command M-AI B-AI中传递参数而已 接收的状态Enter时就要自己记录参数 并且ResetParam
        public float MoveReachDistance { get; set; }
        public FsUnitLogic Target { get; set; }
        public Vector3 TargetPosition { get; set; }
        public string Order { get; set; }
        public void ResetParam()
        {
            Order = null;
            TargetPosition = Vector3.Zero;
            Target = null;
            MoveReachDistance = 0;
        }
        
    }
    
    public class FsUnitAIBaseState
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

    public class BState_Idle : FsUnitAIStateBase
    {
        public override void Enter(UnitAIContent content)
        {
            //STOP OTHER ACTION
            content.Me.Play(new PlayAnimParam("Idle",0,0,true));
        }
        public override void Update(UnitAIContent content, float deltaTime)
        {
            
        }

        public override void Exit(UnitAIContent content)
        {
            
        }
    }
    
    
    
    public class BState_Attack : FsUnitAIStateBase
    {
        public FsUnitLogic Target;
        public override void Enter(UnitAIContent content)
        {
            Target = content.Target;
            //停止移动 看向目标 发起攻击
        }

        public override void Update(UnitAIContent content, float deltaTime)
        {
            if (content.Me.StateFlags.HasAnyState(FsUnitStateFlag.Attack) == false)
            {
                content.BaseAIFsm.ChangeState(FsUnitAIBaseState.Idle);
                return;
            }
        }
        public override void Exit(UnitAIContent content)
        {
            //stop
        }
    }
    
    
    public class FsUnitAI
    {
        //基础行为AI 待机 死亡 攻击 施法 移动(静态目标) 移动(动态目标)
        //上层策略AI 判断技能释放时机 仇恨目标管理
    }
}