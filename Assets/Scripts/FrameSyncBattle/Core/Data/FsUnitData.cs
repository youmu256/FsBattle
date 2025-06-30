using UnityEngine;

namespace FrameSyncBattle
{
    public class FsEntityInitData
    {
        public Vector3 Position;
        public Vector3 Euler;
    }

    public class FsBulletInitData: FsEntityInitData
    {
        public FsUnitLogic Owner;
        public float FlySpeed;
        public float LifeTime;
    }
    
    public class FsUnitInitData : FsEntityInitData
    {
        public FsUnitPropertyInitData PropertyInitData;
    }
    
}