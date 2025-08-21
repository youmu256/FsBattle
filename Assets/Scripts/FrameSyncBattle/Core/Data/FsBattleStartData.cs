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