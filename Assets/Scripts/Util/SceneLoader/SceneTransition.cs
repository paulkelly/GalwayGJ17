using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Billygoat
{
    public abstract class SceneTransition : MonoBehaviour
    {
        public abstract void TransitionAnimationFinished(Action callback);

        public abstract void TransitionIn(Action callback);
        public abstract void TransitionOut(Action callback);
    }
}