using DG.Tweening;
using Telegraphist.Utils.UniRxUtils;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.QTE.Frequency
{
    public class FrequencyKnobView : MonoBehaviour
    {
        [SerializeField] private KnobInputController knob;
        [SerializeField, Tooltip("smaller = faster")] 
        private float lerpSmoothing = 2f;
        [SerializeField] private bool useLerp;
        

        private Tween tween;
        
        private void Start()
        {
            var observable = useLerp
                ? knob.OnAngleChange.Lerp(Mathf.LerpAngle, lerpSmoothing)
                : knob.OnAngleChange;
                
            observable.Subscribe(OnKnobAngleChange).AddTo(this);
        }

        private void OnKnobAngleChange(float angle)
        {
            transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}