﻿namespace FrameSyncBattle
{
    public partial class FsUnitLogic : FsEntityLogic
    {
        private FsUnitInitData Data => InitData as FsUnitInitData;

        public bool IsRemoved { get; private set; }
        
        public float DeadTimer { get; private set; }
        
        public float DeadRemoveTime { get; private set; }
        
        public override void Init(int team, string entityTypeId, object initData)
        {
            base.Init(team, entityTypeId, initData);
            InitStatus(Data.PropertyInitData);
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
            }
        }

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
            Play(new PlayAnimParam(){Animation = "Death",IgnoreRepeat = true});
        }
        #endregion
        
    }
}