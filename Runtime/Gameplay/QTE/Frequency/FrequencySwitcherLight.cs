using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Telegraphist.Utils.CysharpUtils;
using UnityEngine;

namespace Telegraphist.Gameplay.QTE.Frequency
{
    public sealed class FrequencySwitcherLight : MonoBehaviour
    {
        [SerializeField] private Color lightBlinkingColor;
        [SerializeField] private Color lightDefaultColor;
        [SerializeField] private SpriteRenderer lightSprite;
        
        private bool isLightBlinking = false;
        private CancellationTokenSource cancellationTokenSource;
        private Tween lightTween;
        
        public void StartBlinking(float blinkingFrequency, float blinkingOnDuration, float lightFadeDuration)
        {
            if (isLightBlinking)
            {
                return;
            }
            isLightBlinking = true;

            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            CoroutineUtility.DoEvery(
                (int)((blinkingFrequency + blinkingOnDuration) * 1000), async () =>
                {
                    await FadeLight(true, lightFadeDuration).AsyncWaitForCompletion();
                    await UniTask.Delay((int)(blinkingOnDuration * 1000));
                    await FadeLight(false, lightFadeDuration).AsyncWaitForCompletion();
                }, cancellationToken: cancellationToken)();
        }

        private Tween FadeLight(bool on, float lightFadeDuration)
        {
            return (lightTween = lightSprite.DOColor(on ? lightBlinkingColor : lightDefaultColor, lightFadeDuration)
                .SetLink(gameObject));
        }

        public void DisableLight()
        {
            Cleanup();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            cancellationTokenSource?.Cancel();
            if (lightTween.IsActive())
            {
                lightTween.Kill(true);
            }

            lightSprite.color = lightDefaultColor;
            isLightBlinking = false;
        }
    }
}
