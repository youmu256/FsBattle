using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{

    public enum SkillSubType
    {
        Other,
        HeroBaseSkill,
        HeroFinalSkill,
        HeroPassiveSkill,
    }

    public enum SkillTargetType
    {
        None,
        Point,
        Unit,
    }

    public class SkillData
    {
        public string Id;
        public string Name;
        public string Desc;
        public string Icon;
        public bool IsPassive;
        public bool IsAttack;
        public SkillTargetType TargetType;
        public SkillSubType SubType;
        public int CostMp = 0;
        //技能数据分为 基础技能设置 和 技能参数设置
        //通用技能参数 施法距离 冷却
        //特殊参数 技能自身的特殊参数
        
        //AI
        public SkillAITargetRx AIRx;
        public SkillAITarget AITarget;
    }

    
    public enum SkillFlow
    {
        None,
        StartCast,
        StartEffect,
        Affecting,
        EndEffect,
        EndCast,
        Finish,
    }

    public class SkillCastOrder
    {
        public string Id;
        //public SkillTargetType Type;
        public Vector3 CastPoint;
        public FsUnitLogic CastTarget;
    }
    
    
    public class SkillBase
    {
        public string Id => Data.Id;
        public SkillData Data { get; private set; }
        public SkillFlow State { get; protected set; }
        public SkillHandler Handler{ get; protected set; }
        public FsUnitLogic Owner => Handler.Owner;
        public virtual void OnInit(FsBattleLogic battle, SkillHandler handler,SkillData data)
        {
            Data = data;
            Handler = handler;
        }
        
        public virtual void OnAdd(FsBattleLogic battle)
        {

        }

        public virtual void OnRemove(FsBattleLogic battle)
        {

        }

        public virtual void Stop(FsBattleLogic battle)
        {
            if (State == SkillFlow.None || State == SkillFlow.Finish) return;
            State = SkillFlow.None;
            OnChangeFlowState(battle);
        }

        #region 技能对象属性
        public float CoolDown { get; protected set; }
        public float CoolDownTimer { get; protected set; }
        public float CoolDownPercent
        {
            get
            {
                if (CoolDownTimer <= 0 || CoolDown <= 0) return 0;
                return CoolDownTimer / CoolDown;
            }
            set => CoolDownTimer = value * CoolDown;
        }
        #endregion
        
        #region 释放条件

        public bool CheckCoolDown()
        {
            return CoolDownTimer <= 0;
        }
        public bool CheckResources()
        {
            return Owner.MpCurrent >=  Data.CostMp;
        }

        /// <summary>
        /// 是否能启动技能命令
        /// </summary>
        /// <returns></returns>
        public virtual bool IsReadyToCast()
        {
            if (Data.IsPassive) return false;
            return CheckCoolDown() && CheckResources() && Owner.StateFlags.HasAnyState(FsUnitStateFlag.Cast);
        }

        /// <summary>
        /// 命令是否有效 (目标是否匹配 距离是否满足等)
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOrderValid()
        {
            return true;
        }

        #endregion
        
        #region Cast
        public Vector3 CastPoint { get; protected set; }
        public FsUnitLogic CastTarget { get; protected set; }
        
        public virtual bool TryCastAuto(FsBattleLogic battle)
        {
            if (IsReadyToCast() == false) return false;
            //auto set target
            
            return true;
        }
        
        public virtual bool TryCast(FsBattleLogic battle)
        {
            ChangeFlowState(battle,SkillFlow.StartCast);
            return true;
        }
        public virtual bool TryCast(FsBattleLogic battle,Vector3 target)
        {
            CastPoint = target;
            ChangeFlowState(battle,SkillFlow.StartCast);
            return true;
        }
        
        public virtual bool TryCast(FsBattleLogic battle,FsUnitLogic target)
        {
            CastTarget = target;
            ChangeFlowState(battle,SkillFlow.StartCast);
            return true;
        }
        #endregion
        
        public float StateTimer { get; protected set; }
        public virtual void LogicFrame(FsBattleLogic battle, FsCmd cmd)
        {
            //running cool down
            if (CoolDownTimer > 0)
            {
                CoolDownTimer -= battle.FrameLength;
            }
            StateTimer += battle.FrameLength;
            //change state
            while (true)
            {
                var pre = State;
                var next = FlowStateFrame(battle, cmd);
                if (pre == next)
                    break;
                ChangeFlowState(battle,next);
            }
        }


        protected void ChangeFlowState(FsBattleLogic battle,SkillFlow state)
        {
            if (State != state)
            {
                State = state;
                StateTimer = 0;
                OnChangeFlowState(battle);
            }
        }

        protected virtual void OnChangeFlowState(FsBattleLogic battle)
        {
            
        }
        
        protected virtual SkillFlow FlowStateFrame(FsBattleLogic battle, FsCmd cmd)
        {
            return State;
        }
        
    }

}