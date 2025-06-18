using System.Collections.Generic;

namespace FrameSyncBattle
{
    /// <summary>
    /// 根据这个数据来创建一个具体的单位对象
    /// </summary>
    public class FsBattleStartUnitData
    {
        public string TypeId;
        public FsUnitInitData UnitInitData;
    }
    
    public class FsBattleStartData
    {
        public List<FsBattleStartUnitData> PlayerTeamUnits = new();

        public List<FsBattleStartUnitData> EnemyTeamUnits = new();
    }
}