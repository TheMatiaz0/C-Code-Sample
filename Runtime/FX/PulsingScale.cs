using DG.Tweening;
using UnityEngine;

namespace Telegraphist.VFX
{
    public class PulsingScale : MonoBehaviour
    {
        [SerializeField] private float scaleMin = 0;
        [SerializeField] private float scaleMax = 1;
        [SerializeField] private float scalingTime = 0.1f;
        [SerializeField] private Ease scalingEase;

        private void OnEnable()
        {
            StartPulsing();
        }

        private void StartPulsing()
        {
            var sequence = DOTween.Sequence()
                .Append(transform.DOScale(scaleMax, scalingTime).SetEase(scalingEase))
                .Append(transform.DOScale(scaleMin, scalingTime).SetEase(scalingEase))
                .SetLink(gameObject)
                .SetLoops(-1);

            sequence.Play();
        }
    }
}