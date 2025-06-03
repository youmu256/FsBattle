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

        public void Play(string anim, int layer, float normalizedTime)
        {
            Animator.Play(anim,layer,normalizedTime);
        }
    }
}