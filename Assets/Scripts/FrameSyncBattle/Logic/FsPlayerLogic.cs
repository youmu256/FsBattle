using UnityEngine;

namespace FrameSyncBattle
{
    public class FsPlayerLogic : FsEntityLogic
    {
        public float MoveSpeed { get; private set; } = 20;
        
        public override void LogicFrame(FsBattleLogic battle, FsCmd cmd)
        {
            base.LogicFrame(battle, cmd);
            if (cmd == null) return;
            int xInput = 0;
            xInput += cmd.ButtonContains(FsButton.A) ? -1 : 0;
            xInput += cmd.ButtonContains(FsButton.D) ? 1 : 0;
            int yInput = 0;
            yInput += cmd.ButtonContains(FsButton.S) ? -1 : 0;
            yInput += cmd.ButtonContains(FsButton.W) ? 1 : 0;
            if (xInput != 0 || yInput != 0)
            {
                Vector3 vel = new Vector3(xInput, 0, yInput).normalized * (MoveSpeed * battle.FrameLength);
                this.Position += vel;
                this.Euler = Quaternion.LookRotation(vel).eulerAngles;
                Debug.DrawRay(this.Position,Vector3.up,Color.red,battle.FrameLength);
                Debug.DrawRay(this.Position,vel*10,Color.green,battle.FrameLength);
            }

            if (cmd.ButtonContains(FsButton.Fire))
            {
                battle.AddEntity<FsBulletLogic>(this.Team,"",
                    new FsBulletInitData()
                        {Euler = this.Euler, Position = this.Position, FlySpeed = 50, LifeTime = 1f});
            }
        }
    }
}