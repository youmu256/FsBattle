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
            NormalAttack = new NormalAttackHandler(this, new AttackConfig[]
            {
                new AttackConfig()
                {
                    Anim = "Attack",
                    AnimSuffix = null,
                    AnimTime = 1f,
                    NoFade = false,
                    HitDatas = new []{new AttackHitData()
                    {
                        AttackFireOffset = Vector3.up,
                        AttackFlyArc = 0.5f,
                        AttackFlySideSpin = 0,
                        AttackFlySpeed = 10,
                        AttackModel = "cube",
                        DamagePct = 1f,
                        DamageRange = 10f,
                        HitTime = 0.3f,
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
                AI = new FsUnitAI(battle,this);
            }
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
                }
            }
            else
            {
                //AI SKILL ETC...
                AI?.OnEntityFrame(battle, this, battle.FrameLength, cmd);
                MoveService.OnEntityFrame(battle, this, battle.FrameLength, cmd);
                NormalAttack?.OnEntityFrame(battle, this, battle.FrameLength, cmd);
            }
        }

        public FsUnitAI AI { get; set; }
        
        public IAttackHandler NormalAttack{ get; set; }
        
        public IMoveService MoveService { get; set; }

        #region GetSomeThing
        
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
        /// 设置复活
        /// </summary>
        /// <param name="battleLogic"></param>
        public void SetRelive(FsBattleLogic battleLogic)
        {
            if (IsDead == false || IsRemoved) return;
            IsDead = false;
            HpCurrent = 1;
            DeadRemoveTime = 0;
            Play(new PlayAnimParam(){Animation = "Idle",IgnoreRepeat = true});
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
            Play(new PlayAnimParam("Death",0,1f,true));
        }
        #endregion
        
    }
}