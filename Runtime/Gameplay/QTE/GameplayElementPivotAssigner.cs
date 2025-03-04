using Cyberultimate.Unity;
using System;
using System.Collections.Generic;
using Telegraphist.Structures;
using UnityEngine;

namespace Telegraphist.Gameplay.QTE
{
    [Serializable]
    public class GameplayElementPivot
    {
        public FocusObject FocusObject;
        public LightTarget LightTarget;
        public Transform ContentTransform;
        public Transform DisplayTransform;
    }
    public class GameplayElementPivotAssigner : MonoSingleton<GameplayElementPivotAssigner>
    {
        [SerializeField]
        private List<GameplayElementPivot> pivots;

        public GameplayElementPivot GetPivot(FocusObject focusObject)
        {
            var pivot = pivots.Find(x => x.FocusObject == focusObject);
            return pivot;
        }
        
        public Transform GetPivotTransform(FocusObject focusObject)
        {
            var pivot = pivots.Find(x => x.FocusObject == focusObject);
            return pivot.ContentTransform;
        }
        
        public Transform GetPivotTransform(LightTarget lightTarget)
        {
            var pivot = pivots.Find(x => x.LightTarget == lightTarget);
            return pivot.ContentTransform;
        }
    }
}
