namespace FrameSyncBattle
{
    public struct FsUnitProperty
    {
        public int MaxHp;
        public int MaxMp;
        public int MoveSpeed;
        public int Attack;
        
        public static FsUnitProperty operator + (FsUnitProperty a, FsUnitProperty b)
        {
            return a;
        }
    }
}