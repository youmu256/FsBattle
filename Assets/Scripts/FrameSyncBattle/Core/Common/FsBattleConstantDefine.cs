namespace FrameSyncBattle
{
    public class AnimationConstant
    {
        //角色单位的动画名
        public const string Idle = "Idle";
        public const string Death = "Death";
        public const string Move = "Move";
        public const string Attack = "Attack";
        //--

        //出生动画
        public const string Birth = "Birth";
        public const string Stand = "Stand";

    }
    public enum FsEntityType
    {
        /**辅助体*/
        Dummy,
        /**投射物*/
        Missile,
        /**单位*/
        Unit,
        /**特效*/
        Fx,
    }
    
}