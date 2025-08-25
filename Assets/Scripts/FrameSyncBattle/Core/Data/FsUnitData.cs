using System;
using UnityEngine;

namespace FrameSyncBattle
{
    public class FsEntityInitData
    {
    }

    public class FsBulletInitData: FsEntityInitData
    {
        public FsUnitLogic Owner;
        public Vector3 MoveEuler;
        public float FlySpeed;
        public float LifeTime;
    }
    
    public class FsUnitInitData : FsEntityInitData
    {
        public string Model;
        public float ModelScale = 1f;
        public string AttackDataId;
        public string[] InitSkills;
        public FsUnitPropertyData PropertyData;
    }

    public class FsUnitPropertyData
    {
        public int HpMax;
        public int MpMax;
        public int Attack;
        public float AttackInterval;
        public int AttackRange;
        public int CriticalPct;
        public int CriticalBonus;
        public int Defend;
        /**cm/s*/
        public int MoveSpeed;
    }

    public class FsUnitTypeData
    {
        public string Id;
        public string Model;
        public string Name;
        public string Icon;
        //默认数据
        public string AttackDataId;
        public string[] InitSkills;
        public FsUnitPropertyData PropertyData;
    }
}