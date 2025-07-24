namespace FrameSyncBattle
{
    //这个大类用来定义逻辑层对引擎渲染层的一些操作接口 纯逻辑运行时这些接口应该都是空实现的版本

    #region 播放动画
    
    public struct PlayAnimParam
    {
        public float Speed;
        public string Animation;
        public float NormalizedTime;
        /// <summary>
        /// 旧动画与要播放的动画相同时候忽略播放 一般每帧播放loop动画时用到
        /// </summary>
        public bool IgnoreRepeat;

        public PlayAnimParam(string animation,float normalizedTime = 0f,float speed = 1f,bool ignoreRepeat = false)
        {
            Speed = speed;
            Animation = animation;
            NormalizedTime = normalizedTime;
            IgnoreRepeat = ignoreRepeat;
        }

        public static PlayAnimParam Null = new PlayAnimParam();

    }

    
    /// <summary>
    /// 可以播放动画的接口
    /// </summary>
    public interface IAnimationPlayable
    {
        public void Play(PlayAnimParam animParam);
    }


    #endregion
    
    public interface IFsEntityView
    {
        
    }

}