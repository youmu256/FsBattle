using UnityEngine;

namespace FrameSyncBattle
{
    public class FsPlayerLogic : FsUnitLogic
    {
        public float FireInterval = 0.1f;

        public float NextFireTime = 0;
        
        protected override void LogicUpdate(FsBattleLogic battle, FsCmd cmd)
        {
            base.LogicUpdate(battle, cmd);
            int xInput = 0;
            int yInput = 0;
            if (cmd != null)
            {
                xInput += cmd.ButtonContains(FsButton.A) ? -1 : 0;
                xInput += cmd.ButtonContains(FsButton.D) ? 1 : 0;
                yInput += cmd.ButtonContains(FsButton.S) ? -1 : 0;
                yInput += cmd.ButtonContains(FsButton.W) ? 1 : 0;
            }
            if (xInput != 0 || yInput != 0)
            {
                var speed = Property.Get(FsUnitPropertyType.MoveSpeed);
                Vector3 vel = new Vector3(xInput, 0, yInput).normalized * (speed * battle.FrameLength);
                this.Position += vel;
                this.Euler = Quaternion.LookRotation(vel).eulerAngles;
                this.Play(new PlayAnimParam(){Animation = "Move",IgnoreRepeat = true});
            }
            else
            {
                this.Play(new PlayAnimParam(){Animation = "Idle",IgnoreRepeat = true});
            }
            
            
            //移动后再处理射击 让逻辑中的单位位置和发射位置对上
            if (cmd != null && cmd.ButtonContains(FsButton.Fire))
            {
                //一次逻辑帧时间可能也会发射多次子弹
                while (battle.LogicTime >= NextFireTime)
                {
                    NextFireTime = battle.LogicTime + FireInterval;
                    //显示上会对不上 因为view层是落后一逻辑帧的...而且新增的逻辑对象 在下一帧才会执行到逻辑看起来会停在原地一会
                    Vector3 euler = new Vector3(0, cmd.FireYaw, 0);
                    Vector3 firePosition = this.Position;
                    battle.AddEntity<FsBulletLogic>(this.Team, "bullet",
                        new FsBulletInitData()
                            {Owner = this,Euler = euler, Position = firePosition, FlySpeed = 50, LifeTime = 1f});
                }
            }
        }
    }
}