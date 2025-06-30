namespace FrameSyncBattle
{
    public partial class FsUnitLogic : FsEntityLogic
    {
        private FsUnitInitData Data => InitData as FsUnitInitData;

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
                battle.RemoveEntity(this);
            }
            else
            {
                //AI SKILL ETC...
            }
        }
    }
}