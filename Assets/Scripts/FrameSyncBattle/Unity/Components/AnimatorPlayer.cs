using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    public class AnimatorPlayer : MonoBehaviour,IAnimationPlayer
    {
        public Animator Animator { get; private set; }
        private void Awake()
        {
            Animator = GetComponent<Animator>();
        }

        public string LastAnim;

        private void OnDisable()
        {
            LastAnim = null;
        }

        private static Dictionary<string, int> HashCache = new();

        public void Play(PlayAnimParam animParam)
        {
            if (animParam.IgnoreRepeat && LastAnim == animParam.Animation)
                return;

            if (HashCache.TryGetValue(animParam.Animation, out var hash) == false)
            {
                hash = Animator.StringToHash(animParam.Animation);
                HashCache.Add(animParam.Animation, hash);
            }
            if (Animator.HasState(0, hash) == false) return;
            Animator.speed = animParam.Speed;
            Animator.Play(hash, 0, animParam.NormalizedTime);
            LastAnim = animParam.Animation;
        }
    }
}