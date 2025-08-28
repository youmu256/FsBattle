namespace FrameSyncBattle
{
    public class FsFxEntityLogic : FsEntityLogic
    {
        public override void Init(FsBattleLogic battle, int team, FsEntityType entityType, object initData)
        {
            base.Init(battle, team, entityType, initData);
            DeadRemoveTime = 3f;//默认3s
        }

        /// <summary>
        /// 销毁特效 播放死亡动画
        /// 持续时间后删除
        /// </summary>
        /// <param name="time"></param>
        public FsFxEntityLogic Destroy(float time)
        {
            SetDead();
            DeadRemoveTime = time;
            return this;
        }
        public FsFxEntityLogic Destroy()
        {
            SetDead();
            return this;
        }

        /// <summary>
        /// 直接删除特效
        /// </summary>
        public void Delete()
        {
            Remove();
        }
        
    }
}