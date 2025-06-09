using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FrameSyncBattle
{

    /// <summary>
    /// 抽象一层 让上层与具体负责播放的组件解耦
    /// </summary>
    public interface IAnimationPlayer : IAnimationPlayable
    {
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