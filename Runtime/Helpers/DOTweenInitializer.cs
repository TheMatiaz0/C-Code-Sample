using DG.Tweening;
using Telegraphist.Lifecycle;
using UnityEngine;

namespace Telegraphist.Helpers
{
    public class DOTweenInitializer : LifetimeSingleton<DOTweenInitializer>
    {
        [SerializeField]
        private int tweenersMaxCapacity = 30;
        [SerializeField]
        private int sequencesMaxCapacity = 15;
        [SerializeField]
        private bool shouldUseRecycling = false;

        private bool IsDebugging => Application.isEditor || Debug.isDebugBuild;

        public override void Setup()
        {
            base.Setup();
            
            DOTween.KillAll();
            DOTween.Init(recycleAllByDefault: shouldUseRecycling, useSafeMode: IsDebugging, IsDebugging ? LogBehaviour.Default : LogBehaviour.ErrorsOnly)
                .SetCapacity(tweenersMaxCapacity, sequencesMaxCapacity);

            DOTween.debugMode = IsDebugging;
        }
    }
}
