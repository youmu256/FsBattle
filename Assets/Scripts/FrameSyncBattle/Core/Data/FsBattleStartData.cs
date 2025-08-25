using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    public class FsAnglePoint
    {
        public Vector3 Position;
        public Vector3 Euler;

        public FsAnglePoint(Vector3 position, Vector3 euler)
        {
            this.Position = position;
            this.Euler = euler;
        }
    }
    
    public class FsBattleStartUnitData
    {
        /**TypeId指向一个单位的默认配置 如美术方面*/
        public string TypeId;
        public int InitPosId;
        public FsUnitInitData UnitInitData;
    }
    
    public class FsBattleStartData
    {
        public List<FsBattleStartUnitData> PlayerTeamUnits = new();

        public List<FsBattleStartUnitData> EnemyTeamUnits = new();
    }
}