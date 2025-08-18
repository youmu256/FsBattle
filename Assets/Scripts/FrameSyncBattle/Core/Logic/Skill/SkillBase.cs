using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{

    public enum SkillSubType
    {
        Other,
        /**普通攻击*/
        HeroAttackSkill,
        /**大招*/
        HeroFinalSkill,
        /**被动*/
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
        public float CastRange = 0;
        public float CoolDown = 0;
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
            CastRange = data.CastRange;
            CoolDown = data.CoolDown;
        }
        
        public virtual void OnAdd(FsBattleLogic battle)
        {

        }

        public virtual void OnRemove(FsBattleLogic battle)
        {

        }

        public virtual void Stop(FsBattleLogic battle)
        {
            //TODO 需要考虑技能打断情况
            if (State == SkillFlow.None || State == SkillFlow.Finish) return;
            ChangeFlowState(battle,SkillFlow.None);
        }

        #region 技能对象属性 基础属性来自Data 但是技能在游戏过程中属性还会变化
        
        public float CastRange { get; protected set; }
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
        public virtual bool IsReadyToStartCast()
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

        public void SetCastCool()
        {
            this.CoolDownTimer = this.CoolDown;
        }
        
        public virtual bool TryCastAuto(FsBattleLogic battle)
        {
            if (IsReadyToStartCast() == false) return false;
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
        public void LogicFrame(FsBattleLogic battle, FsCmd cmd)
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
                var next = OnFlowStateFrame(battle, cmd);
                if (pre == next)
                    break;
                ChangeFlowState(battle,next);
            }
        }


        protected void ChangeFlowState(FsBattleLogic battle,SkillFlow state)
        {
            if (State != state)
            {
                var pre = State;
                State = state;
                StateTimer = 0;
                OnEnterFlowState(battle,pre);
            }
        }

        protected virtual void OnEnterFlowState(FsBattleLogic battle,SkillFlow preState)
        {
            //FsDebug.Log($"{Data.Name} Skill {preState} -> {State}");
        }
        
        protected virtual SkillFlow OnFlowStateFrame(FsBattleLogic battle, FsCmd cmd)
        {
            return State;
        }
    }
}