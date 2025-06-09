using UnityEngine;

namespace FrameSyncBattle
{
    public class FsBulletLogic : FsEntityLogic
    {
        public FsBulletInitData Data => InitData as FsBulletInitData;

        public float Timer = 0;
        
        public override void Init(int team,string entityTypeId, object initData)
        {
            base.Init(team,entityTypeId, initData);
            this.Position = Data.Position;
            this.Euler = Data.Euler;
        }

        public override void LogicFrame(FsBattleLogic battle, FsCmd cmd)
        {
            base.LogicFrame(battle, cmd);
            Vector3 vel = Quaternion.Euler(Data.Euler) * Vector3.forward * (Data.FlySpeed * battle.FrameLength);
            this.Position += vel;
            Timer += battle.FrameLength;
            if (Timer >= Data.LifeTime)
            {
                battle.RemoveEntity(this);
                return;
            }
            
            
        }
    }
}