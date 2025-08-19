using System;
using System.Collections.Generic;

namespace FrameSyncBattle
{

    public class BuffHandler : IFsEntityFrame
    {
        protected FsLinkedList<BuffBase> BuffList = new();
        protected Dictionary<string, BuffBase> BuffCacheMap = new();
        public FsUnitLogic Owner { get; private set; }

        public BuffHandler(FsUnitLogic owner)
        {
            this.Owner = owner;
        }

        private void HandleAddRequest(FsBattleLogic battle, AddBuffRequest request)
        {
            FsUnitLogic source = request.Source;
            FsUnitLogic target = request.Target;
            BuffData data = request.Data;
            string buffRuntimeKey = BuffBase.GetBuffRuntimeKey(request);
            if (BuffCacheMap.ContainsKey(buffRuntimeKey))
            {
                BuffBase buffBase = BuffCacheMap[buffRuntimeKey];
                BuffCoverOperate operate = buffBase.CoverCheck(source, target, data, request);
                if (operate.buffCoverType == BuffCoverType.Ignore)
                    return;
                int count = operate.GetCoverCount(buffBase.Count, request.AddCount);
                var lastTime = operate.GetCoverRemainTime(buffBase.LastTime, buffBase.GetRemainTime(), request.LastTime);
                switch (operate.buffCoverType)
                {
                    case BuffCoverType.UseOld:
                        buffBase.LastTime = lastTime;
                        buffBase.Refresh(battle, data, count - buffBase.Count);
                        break;
                    case BuffCoverType.UseNew:
                        RemoveBuff(battle,buffBase);
                        RealAddBuff(battle, buffRuntimeKey, request.OtherSource, source, target, data, count, lastTime);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                //无冲突 直接加新buff
                RealAddBuff(battle, buffRuntimeKey,
                    request.OtherSource, source, target, data, request.AddCount, request.LastTime);
            }
        }

        public void RemoveBuff(FsBattleLogic battle,BuffBase buffBase)
        {
            var r = BuffList.Remove(buffBase);
            if (!r) return;
            BuffCacheMap.Remove(buffBase.RuntimeKey);
            buffBase.SetDeAttach(battle);
        }

        protected BuffBase RealAddBuff(FsBattleLogic battle, string buffRuntimeKey,
            IBuffSource other, FsUnitLogic source, FsUnitLogic target, BuffData data, int count, float lastTime)
        {
            var buff = battle.DataTypeFactory.CreateBuff(data);
            buff.SetAttach(battle, buffRuntimeKey, other, source, target, data, count, lastTime);
            BuffCacheMap.Add(buffRuntimeKey, buff);
            BuffList.Add(buff);
            return buff;
        }
        public void AddBuff(FsBattleLogic battle, FsUnitLogic source, IBuffSource other, FsUnitLogic target,
            string id, int count, float lastTime)
        {
            AddBuff(battle,source,other,target,battle.DataTypeFactory.GetBuffData(id),count,lastTime);
        }
        public void AddBuff(FsBattleLogic battle, FsUnitLogic source, IBuffSource other, FsUnitLogic target,
            BuffData data, int count, float lastTime)
        {
            AddBuffRequest request = AddBuffRequest.Allocate();
            request.Source = source;
            request.OtherSource = other;
            request.Target = target;
            request.Data = data;
            request.AddCount = count;
            request.LastTime = lastTime;
            HandleAddRequest(battle, request);
            AddBuffRequest.Recycle(request);
        }

        public void OnEntityFrame(FsBattleLogic battle, FsUnitLogic entity, float deltaTime, FsCmd cmd)
        {
            var p = (battle, cmd);
            BuffList.RefForEach(ref p, (buff, param) =>
            {
                var (fsBattleLogic, fsCmd) = param;
                buff.LogicFrame(fsBattleLogic, fsCmd);
                if (buff.IsNeedToRemove())
                {
                    RemoveBuff(fsBattleLogic,buff);
                }
            });
        }
    }
}