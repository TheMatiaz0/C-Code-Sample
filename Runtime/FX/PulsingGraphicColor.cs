using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Telegraphist.VFX
{
    public class PulsingGraphicColor : MonoBehaviour
    {
        [SerializeField] private Color colorStart = Color.white;
        [SerializeField] private Color colorEnd = Color.black;
        [SerializeField] private float fadeTime = 0.1f;
        [SerializeField] private Ease fadeEase;
        [FormerlySerializedAs("text")] [SerializeField] private MaskableGraphic graphic;

        private void OnEnable()
        {
            StartPulsing();
        }

        private void StartPulsing()
        {
            var sequence = DOTween.Sequence()
                .Append(graphic.DOColor(colorEnd, fadeTime).SetEase(fadeEase))
                .Append(graphic.DOColor(colorStart, fadeTime).SetEase(fadeEase))
                .SetLink(gameObject)
                .SetLoops(-1);

            sequence.Play();
        }
    }
}