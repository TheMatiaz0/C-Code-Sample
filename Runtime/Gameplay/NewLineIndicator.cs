using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Telegraphist.Gameplay.HandPaperVariant
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class NewLineIndicator : MonoBehaviour
    {
        public enum Mode
        {
            None,
            Blink,
            Scale
        }

        [SerializeField] private Mode mode;
        
        [Header("Blink")]
        [SerializeField] private float fadeDuration;
        [SerializeField] private Ease fadeEase;
        [SerializeField] private int blinks;
        [SerializeField] private float blinkActiveTime;
        
        [Header("Scale")]
        [SerializeField] private Ease scaleEase;
        [SerializeField] private float stayDuration;
        [SerializeField] private float fadeOutDuration;
        
        private SpriteRenderer spriteRenderer;
        private float defaultAlpha;
        private Vector3 defaultScale;
        
        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultAlpha = spriteRenderer.color.a;
            spriteRenderer.DOFade(0, 0);
            defaultScale = transform.localScale;
        }

        public async UniTaskVoid IndicateNewLine(float totalDuration, Vector2 position)
        {
            transform.localPosition = position;

            switch (mode)
            {
                case Mode.Blink:
                    await StartBlinking(totalDuration);
                    break;
                case Mode.Scale:
                    await Scale(totalDuration);
                    break;
            }
        }

        private async UniTask StartBlinking(float totalDuration)
        {
            var stepDuration = totalDuration / blinks;
            var blinkInactiveTime = stepDuration - blinkActiveTime;

            for (var i = 0; i < blinks; i++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(blinkInactiveTime));
                spriteRenderer.DOFade(defaultAlpha, fadeDuration).SetEase(fadeEase).SetLink(this.gameObject);
                await UniTask.Delay(TimeSpan.FromSeconds(blinkActiveTime));
                spriteRenderer.DOFade(0, fadeDuration).SetEase(fadeEase).SetLink(this.gameObject);
            }
        }

        private async UniTask Scale(float totalDuration)
        {
            transform.localScale = Vector3.zero;
            await DOTween.Sequence()
                .Append(spriteRenderer.DOFade(defaultAlpha, totalDuration).SetEase(scaleEase))
                .Join(transform.DOScale(defaultScale, totalDuration).SetEase(scaleEase))
                .AppendInterval(stayDuration)
                .Append(spriteRenderer.DOFade(0, fadeOutDuration))
                .SetLink(this.gameObject)
                .AsyncWaitForCompletion();
        }
    }
}