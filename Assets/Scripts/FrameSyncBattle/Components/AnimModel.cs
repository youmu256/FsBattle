using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FrameSyncBattle
{

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
    /// 抽象一层 让上层与具体负责播放的组件解耦
    /// </summary>
    public interface IAnimationPlayer : IAnimationPlayable
    {
    }

    /// <summary>
    /// 可以播放动画的接口
    /// </summary>
    public interface IAnimationPlayable
    {
        public void Play(PlayAnimParam animParam);
    }
    
    /// <summary>
    /// 代表这个预制体是动画模型预制体
    /// </summary>
    public class AnimModel : MonoBehaviour
    {
        public IAnimationPlayer Player;
        private void Awake()
        {
            Player = GetComponent<IAnimationPlayer>();
        }
        public void PlayAnimation(PlayAnimParam animParam)
        {
            Player?.Play(animParam);
        }
    }
}