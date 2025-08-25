using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    public class FsBulletLogic : FsEntityLogic
    {
        private FsBulletInitData Data => InitData as FsBulletInitData;

        public float Timer = 0;

        public FsUnitLogic Owner;
        
        public override void Init(FsBattleLogic battle, int team, FsEntityType entityType, object initData)
        {
            base.Init(battle, team, entityType, initData);
            this.Owner = Data.Owner;
        }

        protected override void LogicUpdate(FsBattleLogic battle, FsCmd cmd)
        {
            base.LogicUpdate(battle, cmd);
            bool remove = false;


            Timer += battle.FrameLength;
            if (Timer >= Data.LifeTime)
            {
                remove = true;
            }

            //move
            Vector3 vel = Quaternion.Euler(Data.MoveEuler) * Vector3.forward * (Data.FlySpeed * battle.FrameLength);
            
            //check hit
            Vector3 start = this.Position;
            
            List<FsUnitLogic> targets = new List<FsUnitLogic>();
            battle.EntityService.CollectUnits(targets);
            foreach (var unit in targets)
            {
                if (unit.Team == this.Team) continue;
                if (unit.IsDead) continue;
                if (CollisionUtil.RaySphereIntersect(start, vel, unit.Position, 0.5f, out var point))
                {
                    vel = point - start;
                    remove = true;
                    
                    //hit target
                    FsDamageInfo damageInfo = FsDamageInfo.CreateAttackDamage(Owner,unit,1f);
                    battle.ProcessDamage(damageInfo);
                    break;
                }
            }

            this.SetPosition(this.Position + vel);
            if (remove)
            {
                battle.RemoveEntity(this);
                return;
            }
        }
    }
}