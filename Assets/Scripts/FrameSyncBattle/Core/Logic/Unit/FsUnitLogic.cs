using UnityEngine;

namespace FrameSyncBattle
{
    public partial class FsUnitLogic : FsEntityLogic
    {
        private FsUnitInitData Data => InitData as FsUnitInitData;

        #region Property

        public float GetAttackRange()
        {
            return Property.Get(FsUnitPropertyType.AttackRange);
        }

        //受击体积半径
        public float Radius = 0.25f;
        
        public bool BeHitCheck(Vector3 hitPoint, float radius)
        {
            return DistanceUtils.DistanceBetween(this.GetBeHitPosition(),hitPoint) <= this.Radius + radius;
        }

        public Vector3 GetBeHitPosition()
        {
            return Position;
        }
        
        #endregion
        
        public bool IsRemoved { get; private set; }
        
        public float DeadTimer { get; private set; }
        
        public float DeadRemoveTime { get; private set; }
        
        public override void Init(FsBattleLogic battle, int team, string entityTypeId, object initData)
        {
            base.Init(battle, team, entityTypeId, initData);
            InitStatus(Data.PropertyInitData);
            //--attack
            NormalAttack = new NormalAttackHandler(this, Data.PropertyInitData.BaseAttackInterval,new AttackConfig[]
            {
                new AttackConfig()
                {
                    Anim = AnimationConstant.Attack,
                    AnimSuffix = null,
                    AnimTime = 1f,
                    NoFade = false,
                    HitDatas = new []{new AttackHitData()
                    {
                        HitTime = 0.1f,
                        AttackFireOffset = Vector3.up,
                        AttackFlyArc = 0.5f,
                        AttackFlySideSpin = 0,
                        AttackFlySpeed = 10,
                        AttackModel = "cube",
                        DamagePct = 1f,
                        DamageRange = 10f,
                        IsMelee = false,
                        LockTarget = true,
                        MeleeHitFx = null,
                    }},
                }
            });
            //--move
            MoveService = new FsSimpleMoveService(this);
            MoveService.UpdateMoveSpeed(Property.Get(FsUnitPropertyType.MoveSpeed));
            if (team == FsBattleLogic.EnemyTeam)
            {
                UnitAI = new FsUnitAI(battle,this);
                GameAI = battle.AutoBattleAI;
            }

            BuffHandler = new BuffHandler(this);
            SkillHandler = new SkillHandler(this);
            SkillHandler.AddSkill(battle,"test1");
        }

        protected override void LogicUpdate(FsBattleLogic battle, FsCmd cmd)
        {
            base.LogicUpdate(battle, cmd);
            if (IsDead)
            {
                DeadTimer += battle.FrameLength;
                //2s后移除单位
                if (DeadTimer >= DeadRemoveTime && DeadRemoveTime > 0)
                {
                    IsRemoved = true;
                    battle.RemoveEntity(this);
                    return;
                }
            }

            //AI SKILL ETC...
            UnitAI?.OnEntityFrame(battle, this, battle.FrameLength, cmd);
            GameAI?.ProcessUnitAI(battle, this);
            MoveService.OnEntityFrame(battle, this, battle.FrameLength, cmd);
            NormalAttack?.OnEntityFrame(battle, this, battle.FrameLength, cmd);
            BuffHandler.OnEntityFrame(battle, this, battle.FrameLength, cmd);
            SkillHandler.OnEntityFrame(battle, this, battle.FrameLength, cmd);
        }

        public FsAutoBattleAI GameAI { get; private set; }
        public FsUnitAI UnitAI { get; private set; }
        public SkillHandler SkillHandler { get; private set; }
        
        public BuffHandler BuffHandler { get; private set; }
        
        public IAttackHandler NormalAttack{ get; private set; }
        public IMoveService MoveService { get; private set; }

        #region GetSomeThing
        public bool CanCast()
        {
            return SkillHandler!=null && StateFlags.HasAnyState(FsUnitStateFlag.Cast);
        }
        public bool CanAttack()
        {
            return NormalAttack!=null && StateFlags.HasAnyState(FsUnitStateFlag.Attack);
        }

        public bool CanMove()
        {
            return StateFlags.HasAnyState(FsUnitStateFlag.Move);
        }
        
        #endregion
        
        /// <summary>
        /// 伤害结算后 响应死亡等行为
        /// </summary>
        /// <param name="battleLogic"></param>
        /// <param name="damageInfo"></param>
        public void OnDamagedPost(FsBattleLogic battleLogic, FsDamageInfo damageInfo)
        {
            if (HpCurrent <= 0)
            {
                SetKilled(battleLogic, damageInfo);
            }
        }

        #region Death Relive

        public bool IsDead { get; private set; }

        /// <summary>
        /// 彻底死亡无法复活了
        /// </summary>
        /// <returns></returns>
        public bool IsTotalDead()
        {
            return IsRemoved;
        }
        
        /// <summary>
        /// 设置复活
        /// </summary>
        /// <param name="battleLogic"></param>
        public void SetRelive(FsBattleLogic battleLogic)
        {
            if (IsDead == false || IsRemoved) return;
            IsDead = false;
            HpCurrent = 1;
            DeadRemoveTime = 0;
            PlayAnimation(new PlayAnimParam(){Animation = AnimationConstant.Idle,IgnoreRepeat = true});
        }
        
        /// <summary>
        /// 设置为被击杀
        /// </summary>
        /// <param name="battleLogic"></param>
        /// <param name="damageInfo"></param>
        public void SetKilled(FsBattleLogic battleLogic,FsDamageInfo damageInfo = null)
        {
            if (IsDead) return;
            IsDead = true;
            HpCurrent = 0;
            DeadRemoveTime = 2f;//移除延迟时间
            //Death View
            PlayAnimation(new PlayAnimParam(AnimationConstant.Death,0,1f,true));
        }
        #endregion
        
        
        public override string DebugMsg()
        {
            return $"id:{this.Id},pos:{this.Position},euler:{this.Euler},hp:{this.HpCurrent}";
        }
    }
}