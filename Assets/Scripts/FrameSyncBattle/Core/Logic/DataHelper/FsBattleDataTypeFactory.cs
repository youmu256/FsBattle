using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace FrameSyncBattle
{
    
    public class FsBattleDataTypeFactory
    {
        #region Data Process
        protected Dictionary<string, UnitAttackData> AttackDataMap = new();

        protected Dictionary<string, BuffData> BuffDataMap = new();

        protected Dictionary<string, SkillData> SkillDataMap = new();

        public void RecordAttackData(UnitAttackData data,bool cover = false)
        {
            var map = AttackDataMap;
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
        public void RecordBuffData(BuffData data,bool cover = false)
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

        public void RecordSkillData(SkillData data,bool cover = false)
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

        public UnitAttackData GetAttackData(string key)
        {
            if (!AttackDataMap.TryGetValue(key, out var data))
            {
                FsDebug.LogError($"NotFind UnitAttackData {key}");
            }
            return data;
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

        private AttackData[] TestAttack = new[]
        {
            new AttackData()
            {
                Anim = AnimationConstant.Attack,
                AnimSuffix = null,
                AnimTime = 1f,
                NoFade = false,
                HitDatas = new[]
                {
                    new AttackHitData()
                    {
                        HitTime = 0.1f,
                        AttackFireOffset = Vector3.up,
                        AttackFlyArc = 0.5f,
                        AttackFlySideSpin = 0,
                        AttackFlySpeed = 10,
                        AttackModel = "cube",
                        DamagePct = 1f,
                        DamageRange = 10f,
                        IsMelee = false,
                        LockTarget = true,
                        MeleeHitFx = null,
                    }
                },
            }
        };
        
        public void Init(FsBattleLogic battle)
        {
            RecordSkillData(TestSkill.TestData());
            RecordBuffData(Buff_Stun.CommonData());
            RecordAttackData(new UnitAttackData(){Id = "test_attack",AttackDatas = TestAttack});
            RecordFactoryType("test1",typeof(TestSkill));
            RecordFactoryType(Buff_Stun.CommonId,typeof(Buff_Stun));
        }
    }
}