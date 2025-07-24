using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{

    public interface IFsEntityFrame
    {
        void OnEntityFrame(FsBattleLogic battle, FsUnitLogic entity, float deltaTime,FsCmd cmd);
    }

    public enum AttackFlowState
    {
        None,
        Start,
        FirstFired,
        FireEnd,
    }

    public class AttackTempData
    {
        public AttackConfig Attack;
        public int AttackHitIndex;
        public int AttackId;
    }
    public interface IAttackHandlerCaller
    {
        float GetAttackRangeBuffer();
        AttackFlowState GetCurrentState();
        int CommitOverrideAttack(AttackConfig attackConfig, int priority);
        void AttackTarget(FsUnitLogic target);
        void StopAttack();
    }
    public interface IAttackHandler: IAttackHandlerCaller,IFsEntityFrame
    {
        
    }

    public class EmptyAttackHandler : IAttackHandler
    {
        public float GetAttackRangeBuffer()
        {
            return 0;
        }

        public AttackFlowState GetCurrentState()
        {
            return AttackFlowState.None;
        }

        public int CommitOverrideAttack(AttackConfig attackConfig, int priority)
        {
            return 0;
        }

        public void AttackTarget(FsUnitLogic target)
        {
            
        }

        public void StopAttack()
        {
            
        }

        public void OnEntityFrame(FsBattleLogic battle, FsUnitLogic entity, float deltaTime, FsCmd cmd)
        {
            
        }
    }
    
    public class NormalAttackHandler : IAttackHandler
    {
        //攻击速度归一到统一攻击间隔 如果动画时间大于攻击间隔，则要带默认速度修正
        public const float ExtraAttackRangeBuffer = 1.5f;
        public AttackConfig[] NormalAttack { get; private set; }

        public float GetAttackRangeBuffer()
        {
            float attackRange = Owner.GetAttackRange();
            if (FlowState == AttackFlowState.Start || FlowState == AttackFlowState.FirstFired)
                return ExtraAttackRangeBuffer + attackRange;
            return attackRange;
        }

        public AttackFlowState GetCurrentState()
        {
            return FlowState;
        }
        public FsUnitLogic Owner { get; private set; }

        public NormalAttackHandler(FsUnitLogic owner,AttackConfig[] attackConfigs)
        {
            this.Owner = owner;
            AttackId = 0;
            NormalAttack = attackConfigs;
        }
        
        //--攻击相关--
        /// <summary>
        /// 默认是1 攻速+100%后 这个数值会为2
        /// </summary>
        protected float CurrentAttackTimeScale { get; private set; } = 1f;
        /// <summary>
        /// 攻击冷却
        /// </summary>
        protected float CurrentAttackCoolDown{ get; private set; }
        protected AttackFlowState FlowState { get; private set; }
        protected AttackConfig CurrentAttack { get; private set; }
        protected int AttackId { get; private set; }
        protected int CurrentAttackHitIndex { get; private set; }
        protected float CurrentAttackTimer { get; private set; }
        protected FsUnitLogic CurrentTarget { get; private set; }
        protected bool AttackActive { get; private set; }
        protected AttackConfig GetOneAttack(FsUnitLogic target)
        {
            //按照目标类型 距离等情况 可以考虑取出不同的攻击行为去攻击目标
            var length = NormalAttack.Length;
            var atk =  NormalAttack[UnityEngine.Random.Range(0,length)];
            
            //--发起事件--
            var evt = GameEvent.New<GE_AnyUnitPrepareAttack>();
            evt.Source = Owner;
            evt.Target = target;
            evt.OriginAttack = atk;
            evt.RaiseToGlobal();
            evt.Recycle();
            //--
            
            if (CommittedAttack != null)
            {
                atk = CommittedAttack;
                ResetCommittedAttack();
            }
            return atk;
        }

        /// <summary>
        /// 下一次攻击
        /// </summary>
        protected AttackConfig CommittedAttack { get; private set; }
        protected int CommittedPriority { get; private set; }
        void ResetCommittedAttack()
        {
            CommittedAttack = null;
            CommittedPriority = 0;
        }
        public int CommitOverrideAttack(AttackConfig attackConfig,int priority)
        {
            if (CommittedAttack == null || priority >= CommittedPriority)
            {
                CommittedAttack = attackConfig;
                CommittedPriority = priority;
                AttackId++;
                return AttackId;
            }
            return 0;
        }
        
        public void AttackTarget(FsUnitLogic target)
        {
            CurrentTarget = target;
            AttackActive = true;
        }

        public void StopAttack()
        {
            AttackActive = false;
            if (FlowState == AttackFlowState.None) return;
            FlowState = AttackFlowState.None;
            CurrentTarget = null;
            CurrentAttackTimer = 0;
            CurrentAttackHitIndex = 0;
            CurrentAttack = null;
            CurrentAttackTimeScale = 1f;
        }

        
        private void OnAttackObjectEnd(FsBattleLogic battle,FsMissileLogic missileObject,bool valid)
        {
            var target = missileObject.Target;
            var position = missileObject.MissileResultPosition;
            var data = missileObject.BindData as AttackTempData;
            if (data == null) return;
            DoAttackDamage(battle,0,data,position,target);
        }
        
        private bool HitFilter(FsBattleLogic battle,FsUnitLogic target)
        {
            if (target.IsDead) return false;
            if (battle.EntityService.IsEnemy(Owner, target) == false) return false;
            return true;
        }

        private void MeleeHitEnd(FsBattleLogic battle,AttackTempData data)
        {
            bool hit = DoAttackDamage(battle,Owner.GetAttackRange(),data,Owner.Position,CurrentTarget);
            if (hit)
            {
                //todo create fx
                //var attackHitData = data.Attack.HitDatas[data.AttackHitIndex];
                //AnimateModel.Create(attackHitData.MeleeHitFx).SetPosition(CurrentTarget.GetBeHitPosition()).SetAutoRemove();
            }
        }

        private bool DoAttackDamage(FsBattleLogic battle,float baseRange,AttackTempData data,Vector3 position,FsUnitLogic target)
        {
            var hit = false;
            var attackHitData = data.Attack.HitDatas[data.AttackHitIndex];
            var damageRange = attackHitData.DamageRange;
            if (damageRange <= 0)
            {
                if (HitFilter(battle,target) && target.BeHitCheck(position, baseRange+ExtraAttackRangeBuffer))
                {
                    FsDamageInfo damageInfo = FsDamageInfo.CreateAttackDamage(Owner,target,attackHitData.DamagePct).BindAttackIndex(data.AttackId);
                    battle.ProcessDamage(damageInfo);
                    hit = true;
                }
            }
            else
            {
                List<FsUnitLogic> list = new List<FsUnitLogic>();
                battle.EntityService.CollectUnitsInPositionRange2D(list,position,damageRange,HitFilter);
                foreach (var aoeTarget in list)
                {
                    FsDamageInfo damageInfo = FsDamageInfo.CreateAttackDamage(Owner,aoeTarget,attackHitData.DamagePct).BindAttackIndex(data.AttackId);
                    battle.ProcessDamage(damageInfo);
                    hit = true;
                }
            }
            return hit;
        }
        
        
        public void OnEntityFrame(FsBattleLogic battle, FsUnitLogic entity, float deltaTime, FsCmd cmd)
        {
            //cool down timer
            if (CurrentAttackCoolDown > 0)
                CurrentAttackCoolDown -= deltaTime * CurrentAttackTimeScale;
            
            //try to start
            if (CurrentAttackCoolDown <= 0 && FlowState == AttackFlowState.None && AttackActive)
            {
                ChangeState(battle,AttackFlowState.Start);
            }
            
            if (FlowState == AttackFlowState.None) return;
            CurrentAttackTimer += deltaTime * CurrentAttackTimeScale;
            if (FlowState == AttackFlowState.Start)
            {
                //todo face target
                //if(CurrentTarget!=null)
                    //Owner.LookAtTarget2D(CurrentTarget.Position);
            }
            if (FlowState == AttackFlowState.Start || FlowState == AttackFlowState.FirstFired)
            {
                var hit = CurrentAttack.GetCurrentHit(CurrentAttackTimer,CurrentAttackHitIndex);
                if (hit != null)
                {
                    ChangeState(battle,AttackFlowState.FirstFired,hit);
                    CurrentAttackHitIndex++;
                }
                if (CurrentAttack.IsLastHitIndex(CurrentAttackHitIndex))
                {
                    ChangeState(battle,AttackFlowState.FireEnd);
                }
            }
            if (FlowState == AttackFlowState.FireEnd && CurrentAttackTimer >= CurrentAttack.AnimTime)
            {
                ChangeState(battle,AttackFlowState.None);
            }
        }

        protected void ChangeState(FsBattleLogic battle,AttackFlowState state,object param = null)
        {
            if (FlowState == state && state!= AttackFlowState.FirstFired) return;
            if (state == AttackFlowState.Start)
            {
                //真正开始攻击流程 
                AttackId++;
                CurrentAttackTimeScale = Owner.Property.CurrentAttackTimeScaler;
                CurrentAttack = GetOneAttack(CurrentTarget);
                
                CurrentAttackHitIndex = 0;
                Owner.PlayAnimation(new PlayAnimParam(){Animation = CurrentAttack.Anim,IgnoreRepeat = false,Speed = CurrentAttackTimeScale,});
                //Owner.PlayAnimation(CurrentAttack.Anim,CurrentAttack.AnimSuffix,CurrentAttackTimeScale,CurrentAttack.NoFade?0f:0.15f);
            }
            else if (state == AttackFlowState.FirstFired)
            {
                if (CurrentAttackHitIndex == 0)
                {
                    CurrentAttackCoolDown = CurrentAttack.AnimTime;
                }
                if (param is AttackHitData attack)
                {
                    if (attack.IsMelee)
                    {
                        AttackTempData bindData = new AttackTempData();
                        bindData.Attack = CurrentAttack;
                        bindData.AttackHitIndex = CurrentAttackHitIndex;
                        bindData.AttackId = AttackId;
                        MeleeHitEnd(battle,bindData);
                    } else
                    {
                        AttackTempData bindData = new AttackTempData();
                        bindData.Attack = CurrentAttack;
                        bindData.AttackHitIndex = CurrentAttackHitIndex;
                        bindData.AttackId = AttackId;
                        Vector3 start = Owner.Position + Quaternion.Euler(Owner.Euler) * attack.AttackFireOffset;
                        
                        var missile = battle.AddEntity<FsMissileLogic>(Owner.Team,"missile",new FsEntityInitData(){Euler = Owner.Euler,Position = start});
                        missile.SetBase(attack.AttackModel,attack.AttackFlySpeed,attack.AttackFlyArc, attack.AttackFlySideSpin)
                            .AimTarget(start,CurrentTarget,attack.LockTarget)
                            .Fire(bindData,OnAttackObjectEnd);
                        
                    }
                }
            }
            else if (state == AttackFlowState.FireEnd)
            {
                
            }
            else if (state == AttackFlowState.None)
            {
                StopAttack();
            }
            FlowState = state;
        }

    }
}