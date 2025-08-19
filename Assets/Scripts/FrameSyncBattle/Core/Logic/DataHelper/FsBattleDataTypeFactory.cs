using System;
using System.Collections.Generic;

namespace FrameSyncBattle
{
    
    public class FsBattleDataTypeFactory
    {
        #region Data Process
        
        protected Dictionary<string, BuffData> BuffDataMap = new();

        protected Dictionary<string, SkillData> SkillDataMap = new();

        public void RecordData(BuffData data,bool cover = false)
        {
            var map = BuffDataMap;
            var key = data.Id;
            if (map.ContainsKey(key))
            {
                if(cover)
                    map[key] = data;
            }
            else
            {
                map.Add(key,data);
            }
        }

        public void RecordData(SkillData data,bool cover = false)
        {
            var map = SkillDataMap;
            var key = data.Id;
            if (map.ContainsKey(key))
            {
                if(cover)
                    map[key] = data;
            }
            else
            {
                map.Add(key,data);
            }
        }

        public BuffData GetBuffData(string key)
        {
            if (!BuffDataMap.TryGetValue(key, out var data))
            {
                FsDebug.LogError($"NotFind BuffData {key}");
            }
            return data;
        }
        public SkillData GetSkillData(string key)
        {
            if (!SkillDataMap.TryGetValue(key, out var data))
            {
                FsDebug.LogError($"NotFind SkillData {key}");
            }
            return data;
        }


        #endregion

        #region Factory Create

        protected Dictionary<string, Type> FactoryTypeMaps = new();

        protected Object Create(string key)
        {
            if (FactoryTypeMaps.TryGetValue(key, out var type))
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
        
        protected void RecordFactoryType(string key, Type type)
        {
            if (FactoryTypeMaps.ContainsKey(key))
            {
                FsDebug.LogError($"Repeat Key Type {key} {type.Name}");
                return;
            }
            FactoryTypeMaps.Add(key,type);
        }
        
        public SkillBase CreateSkill(SkillData data)
        {
            return Create(data.Id) as SkillBase;
        }

        public BuffBase CreateBuff(BuffData data)
        {
            return Create(data.Id) as BuffBase;
        }

        public SkillBase CreateSkill(string key)
        {
            return Create(key) as SkillBase;
        }

        public BuffBase CreateBuff(string key)
        {
            return Create(key) as BuffBase;
        }

        #endregion
        
        public void Init(FsBattleLogic battle)
        {
            RecordData(TestSkill.TestData());
            RecordData(Buff_Stun.TestData());
            
            RecordFactoryType("test1",typeof(TestSkill));
            RecordFactoryType("stun",typeof(Buff_Stun));
        }

    }
}