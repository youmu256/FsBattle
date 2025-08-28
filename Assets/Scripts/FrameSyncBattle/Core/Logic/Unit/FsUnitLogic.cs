using UnityEngine;

namespace FrameSyncBattle
{
    public partial class FsUnitLogic : FsEntityLogic
    {
        private FsUnitInitData Data => InitData as FsUnitInitData;

        public FsUnitTypeData TypeData { get; protected set; }

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
        
        
        public override void Init(FsBattleLogic battle, int team, FsEntityType entityType, object initData)
        {
            base.Init(battle, team, entityType, initData);
            SetModel(Data.Model, Data.ModelScale);
            DeadRemoveTime = 3f;//单位统一3s死亡移除时间 后续可能考虑不同单位时间不同
            InitStatus(Data.PropertyData);
            //--attack
            if (Data.AttackDataId != null)
            {
                var unitAttackData = battle.DataService.GetAttackData(Data.AttackDataId);
                if(unitAttackData!=null)
                    NormalAttack = new NormalAttackHandler(this, Data.PropertyData.AttackInterval,unitAttackData);
            }
            //--move
            MoveService = new FsSimpleMoveService(this);
            MoveService.UpdateMoveSpeed(Property.Get(FsUnitPropertyType.MoveSpeed));
            
            UnitAI = new FsUnitAI(battle,this);
            GameAI = battle.AutoBattleAI;

            BuffHandler = new BuffHandler(this);
            SkillHandler = new SkillHandler(this);

            //添加初始技能
            if (Data.InitSkills != null)
            {
                foreach (var skillId in Data.InitSkills)
                {
                    SkillHandler.AddSkill(battle,skillId);
                }
            }
        }

        protected override void LogicUpdate(FsBattleLogic battle, FsCmd cmd)
        {
            base.LogicUpdate(battle, cmd);
            if (IsTotalDead()) return;
            //AI SKILL ETC...
            UnitAI?.OnEntityFrame(battle, this, battle.FrameLength, cmd);
            GameAI?.ProcessUnitAI(battle, this);
            MoveService.OnEntityFrame(battle, this, battle.FrameLength, cmd);
            NormalAttack?.OnEntityFrame(battle, this, battle.FrameLength, cmd);
            BuffHandler.OnEntityFrame(battle, this, battle.FrameLength, cmd);
            SkillHandler.OnEntityFrame(battle, this, battle.FrameLength, cmd);
        }

        public FsAutoBattleAI GameAI { get; protected set; }
        public FsUnitAI UnitAI { get; protected set; }
        public SkillHandler SkillHandler { get; protected set; }
        public BuffHandler BuffHandler { get; protected set; }
        public IAttackHandler NormalAttack{ get; protected set; }
        public IMoveService MoveService { get; protected set; }

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

        /// <summary>
        /// 彻底死亡无法复活了
        /// </summary>
        /// <returns></returns>
        public bool IsTotalDead()
        {
            return IsRemoved;
        }
        
        /// <summary>
        /// 设置复活 如果已经被移除了则无法复活
        /// 只有单位能复活
        /// </summary>
        /// <param name="battleLogic"></param>
        public void SetRelive(FsBattleLogic battleLogic)
        {
            if (IsDead == false || IsTotalDead()) return;
            IsDead = false;
            HpCurrent = 1;
            PlayAnimation(new PlayAnimParam(){Animation = AnimationConstant.Idle,IgnoreRepeat = true});
        }

        public override void SetDead()
        {
            base.SetDead();
            HpCurrent = 0;
        }
        
        /// <summary>
        /// 设置为被击杀
        /// 只有单位才能被击杀
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="damageInfo"></param>
        public void SetKilled(FsBattleLogic battle,FsDamageInfo damageInfo = null)
        {
            //可以记录最后完成击杀的DamageInfo对象
            SetDead();
        }
        #endregion
        
        
        public override string DebugMsg()
        {
            return $"id:{this.Id},pos:{this.Position},euler:{this.Euler},hp:{this.HpCurrent}";
        }
    }
}