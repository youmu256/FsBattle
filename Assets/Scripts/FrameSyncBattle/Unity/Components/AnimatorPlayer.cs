using System;
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

        public void Play(PlayAnimParam animParam)
        {
            if (animParam.IgnoreRepeat && LastAnim == animParam.Animation)
                return;
            var hash = Animator.StringToHash(animParam.Animation);
            Animator.speed = animParam.Speed;
            Animator.Play(hash,0,animParam.NormalizedTime);
            LastAnim = animParam.Animation;
        }
    }
}