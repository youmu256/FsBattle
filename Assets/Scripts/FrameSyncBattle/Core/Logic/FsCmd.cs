using System;

namespace FrameSyncBattle
{
    
    [Flags]
    public enum FsButton
    {
        W = 1<<0,
        S = 1<<1,
        A = 1<<2,
        D = 1<<3,
        Fire = 1<<4,
    }
    
    public class FsCmd
    {
        public int LogicFrameIndex;
        public FsButton Buttons = 0;
        public float FireYaw;
        public bool ButtonContains(FsButton button)
        {
            return (Buttons & button) > 0;
        }

        public bool IsEmpty()
        {
            return Buttons == 0;
        }
    }

}