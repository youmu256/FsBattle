using System;
using System.Collections.Generic;

namespace FrameSyncBattle
{

    public struct BuffAddData
    {
        public string TemplateId;
        public int AddCount;
        public float Duration;
    }

    public class BuffFactory
    {
        public static Buff Create(string templateId)
        {
            if(templateId == "buff_stun")
                return new Buff_Stun();
            FsDebug.LogError($"not find matched template , id : {templateId}");
            return new Buff();
        }
    }

    public class BuffHandler : IFsEntityFrame
    {
        protected FsLinkedList<Buff> BuffList = new();
        protected Dictionary<string, Buff> BuffCacheMap = new();
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
            string buffRuntimeKey = Buff.GetBuffRuntimeKey(request);
            if (BuffCacheMap.ContainsKey(buffRuntimeKey))
            {
                Buff buff = BuffCacheMap[buffRuntimeKey];
                BuffCoverOperate operate = buff.CoverCheck(source, target, data, request);
                if (operate.buffCoverType == BuffCoverType.Ignore)
                    return;
                int count = operate.GetCoverCount(buff.Count, request.AddCount);
                var lastTime = operate.GetCoverRemainTime(buff.LastTime, buff.GetRemainTime(), request.LastTime);
                switch (operate.buffCoverType)
                {
                    case BuffCoverType.UseOld:
                        buff.LastTime = lastTime;
                        buff.Refresh(battle, data, count - buff.Count);
                        break;
                    case BuffCoverType.UseNew:
                        RemoveBuff(battle,buff);
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

        public void RemoveBuff(FsBattleLogic battle,Buff buff)
        {
            var r = BuffList.Remove(buff);
            if (!r) return;
            BuffCacheMap.Remove(buff.RuntimeKey);
            buff.SetDeAttach(battle);
        }

        protected Buff RealAddBuff(FsBattleLogic battle, string buffRuntimeKey,
            IBuffSource other, FsUnitLogic source, FsUnitLogic target, BuffData data, int count, float lastTime)
        {
            var buff = BuffFactory.Create(data.TemplateKey);
            buff.SetAttach(battle, buffRuntimeKey, other, source, target, data, count, lastTime);
            BuffCacheMap.Add(buffRuntimeKey, buff);
            BuffList.Add(buff);
            return buff;
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