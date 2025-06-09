namespace FrameSyncBattle
{
    //这个大类用来定义逻辑层对引擎渲染层的一些操作接口 纯逻辑运行时这些接口应该都是空实现的版本

    #region 播放动画
    
    public struct PlayAnimParam
    {
        public string Animation;
        public int Layer;
        public float NormalizedTime;
        /// <summary>
        /// 旧动画与要播放的动画相同时候忽略播放 一般每帧播放loop动画时用到
        /// </summary>
        public bool IgnoreRepeat;
    }
    
    /// <summary>
    /// 可以播放动画的接口
    /// </summary>
    public interface IAnimationPlayable
    {
        public void Play(PlayAnimParam animParam);
    }


    #endregion
    
    
    /// <summary>
    /// 逻辑层中需要能直接尝试调用一些View层行为 这是统一接口
    /// 比如播放动画 音效 特效 等等
    /// </summary>
    public interface IFsEntityView : IAnimationPlayable
    {
        
    }

}