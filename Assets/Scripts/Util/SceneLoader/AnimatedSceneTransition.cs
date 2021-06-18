using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Billygoat
{
    public class AnimatedSceneTransition : SceneTransition
    {
        [SerializeField]
        private Animator Animator;

        private bool Loading
        {
            get { return Animator.GetBool("Loading"); }
            set { Animator.SetBool("Loading", value); }
        }

        private bool FadeInFinished()
        {
            return Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
        }

        private bool FadeOutFinished()
        {
            return Animator.GetCurrentAnimatorStateInfo(0).IsName("Loading");
        }

        private bool AnimationFinished()
        {
            return Animator.GetCurrentAnimatorStateInfo(0).IsName("OutEnd");
        }

        public override void TransitionAnimationFinished(Action callback)
        {
            if (AnimationFinished())
            {
                callback.Invoke();
            }
            else
            {
                StartCoroutine(WaitForTransitionAndDoCallback(AnimationFinished, callback));
            }
        }

        public override void TransitionIn(Action callback)
        {
            Loading = false;

            if (FadeInFinished())
            {
                callback.Invoke();
            }
            else
            {
                StartCoroutine(WaitForTransitionAndDoCallback(FadeInFinished, callback));
            }
        }

        public override void TransitionOut(Action callback)
        {
            Loading = true;

            if (FadeOutFinished())
            {
                callback.Invoke();
            }
            else
            {
                StartCoroutine(WaitForTransitionAndDoCallback(FadeOutFinished, callback));
            }
        }

        private IEnumerator WaitForTransitionAndDoCallback(Func<bool> condition, Action callback)
        {
            while (!condition.Invoke())
            {
                yield return null;
            }

            callback.Invoke();
        }

    }
}