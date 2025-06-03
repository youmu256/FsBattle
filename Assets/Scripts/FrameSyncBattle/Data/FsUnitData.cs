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
        public float FlySpeed;
        public float LifeTime;
    }
    
    public class FsUnitInitData : FsEntityInitData
    {
        
    }
    
}