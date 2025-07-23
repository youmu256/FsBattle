using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    public class FsPlayerLogic : FsUnitLogic
    {
        public float FireInterval = 0.1f;

        public float NextFireTime = 0;

        public override void Init(FsBattleLogic battle, int team, string entityTypeId, object initData)
        {
            base.Init(battle, team, entityTypeId, initData);
        }

        protected override void LogicUpdate(FsBattleLogic battle, FsCmd cmd)
        {
            base.LogicUpdate(battle, cmd);
            if (IsDead) return;
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
                this.SetPosition(Position + vel).SetEuler(Quaternion.LookRotation(vel).eulerAngles);
                this.Play(new PlayAnimParam("Move",0,1f,true));
            }
            else
            {
                this.Play(new PlayAnimParam("Idle",0,1f,true));
            }
            
            
            //移动后再处理射击 让逻辑中的单位位置和发射位置对上
            if (cmd != null && cmd.ButtonContains(FsButton.Fire))
            {
                //一次逻辑帧时间可能也会发射多次子弹
                while (battle.LogicTime >= NextFireTime)
                {
                    NextFireTime = battle.LogicTime + FireInterval;
                    TestAttack(battle,cmd);
                    //TestMissile(battle,cmd);
                    //TestBullet(battle,cmd);
                }
            }
        }

        private void TestAttack(FsBattleLogic battle, FsCmd cmd)
        {
            List<FsUnitLogic> targets = new List<FsUnitLogic>();
            battle.EntityService.CollectUnits(targets, TargetFilter);
            if (targets.Count <= 0) return;
            var target = targets[battle.RandomGen.Next(targets.Count)];
            NormalAttack.AttackTarget(target);
        }

        
        private void TestBullet(FsBattleLogic battle,FsCmd cmd)
        {
            //显示上会对不上 因为view层是落后一逻辑帧的...而且新增的逻辑对象 在下一帧才会执行到逻辑看起来会停在原地一会
            Vector3 euler = new Vector3(0, cmd.FireYaw, 0);
            Vector3 firePosition = this.Position;
            battle.AddEntity<FsBulletLogic>(this.Team, "bullet",
                new FsBulletInitData()
                    {Owner = this,Euler = euler, Position = firePosition, FlySpeed = 50, LifeTime = 1f});

        }
        
        private void TestMissile(FsBattleLogic battle,FsCmd cmd)
        {
            Vector3 start = this.Position + Quaternion.Euler(this.Euler) * Vector3.up;
            /*
            var missile = battle.AddEntity<FsMissileLogic>(this.Team,"missile",new FsEntityInitData(){Euler = this.Euler,Position = start});
            missile.SetBase("cube",10,0.5f, 45)
                .AimTarget(start,Vector3.zero)
                .Fire(null,null);
            */
            
            List<FsUnitLogic> targets = new List<FsUnitLogic>();
            battle.EntityService.CollectUnits(targets, TargetFilter);
            if (targets.Count <= 0) return;
            var target = targets[battle.RandomGen.Next(targets.Count)];
            var lockMissile = battle.AddEntity<FsMissileLogic>(this.Team,"missile",new FsEntityInitData(){Euler = this.Euler,Position = start});
            lockMissile.SetBase("cube", 10, 0.5f, battle.RandomGen.Next(-90, 90)).AimTarget(start,target,true).Fire(null, (
                (logic, missileLogic, valid) =>
                {
                    if (valid)
                    {
                        FsDamageInfo damageInfo = FsDamageInfo.CreateAttackDamage(this,missileLogic.Target,1f);
                        logic.ProcessDamage(damageInfo);
                    }
                }));

        }
        
        private bool TargetFilter(FsBattleLogic battle, FsUnitLogic target)
        {
            return battle.EntityService.IsEntityValidTobeTargeted(this,target) && battle.EntityService.IsEnemy(this,target);
        }

        
    }
}