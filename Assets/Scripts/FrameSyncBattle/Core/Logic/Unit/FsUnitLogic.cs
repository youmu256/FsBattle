namespace FrameSyncBattle
{
    public partial class FsUnitLogic : FsEntityLogic
    {
        private FsUnitInitData Data => InitData as FsUnitInitData;

        public float DeadTimer { get; private set; }
        
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
                if(DeadTimer<=0)
                    Play(new PlayAnimParam(){Animation = "Death",IgnoreRepeat = true});
                DeadTimer += battle.FrameLength;
                //2s后移除单位
                if (DeadTimer >= 2)
                {
                    battle.RemoveEntity(this);
                }
            }
            else
            {
                //AI SKILL ETC...
            }
        }
    }
}