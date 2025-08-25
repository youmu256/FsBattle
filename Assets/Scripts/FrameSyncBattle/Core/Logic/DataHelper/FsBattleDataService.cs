using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace FrameSyncBattle
{
    
    public class FsBattleDataService
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

        #region 配置
        
        public void RecordTeamIndexPoint(int team, int index, Vector3 position, Vector3 euler,bool cover = false)
        {
            string key = team + "_" + index;
            var map = _datas;
            if (map.ContainsKey(key))
            {
                if(cover)
                    map[key] = new FsAnglePoint(position,euler);
            }
            else
            {
                map.Add(key,new FsAnglePoint(position,euler));
            }
        }
        private readonly Dictionary<string, FsAnglePoint> _datas = new();

        public FsAnglePoint GetTeamIndexPoint(int team,int index)
        {
            string key = team + "_" + index;
            if (!_datas.TryGetValue(key, out var data))
            {
                FsDebug.LogError($"Not Find Match AnglePoint Data {key}");
            }
            return data;
        }

        private void InitTeamIndexPoints(FsBattleLogic battle)
        {
            Vector3 team1Euler = new Vector3(0, 90, 0);
            Vector3 team2Euler = new Vector3(0, -90, 0);
            //两边都是9个格子可放置初始单位
            var index = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    index++;
                    //都保证index369是前排 147是后排
                    
                    Vector3 local1Pos = new Vector3(j * 1, 0, i * 1);
                    Vector3 world1Pos = local1Pos + Vector3.left * 2;
                    RecordTeamIndexPoint(FsBattleLogic.PlayerTeam,index,world1Pos,team1Euler);
                    
                    Vector3 local2Pos = new Vector3(-j * 1, 0, i * 1);
                    Vector3 world2Pos = local2Pos + Vector3.right * 2;
                    RecordTeamIndexPoint(FsBattleLogic.EnemyTeam,index,world2Pos,team2Euler);
                }
            }
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
                        AttackModel = "test_missile",
                        AttackModelScale = 1f,
                        DamagePct = 1f,
                        DamageRange = 0f,
                        IsMelee = false,
                        LockTarget = true,
                        MeleeHitFx = null,
                    }
                },
            }
        };
        
        public void Init(FsBattleLogic battle)
        {
            //TODO 后面应该改成读配置的形式
            InitTeamIndexPoints(battle);
            RecordSkillData(TestSkill.TestData());
            RecordBuffData(Buff_Stun.CommonData());
            RecordAttackData(new UnitAttackData(){Id = "test_attack",AttackDatas = TestAttack});
            RecordFactoryType("test1",typeof(TestSkill));
            RecordFactoryType(Buff_Stun.CommonId,typeof(Buff_Stun));
        }
    }
}