using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Telegraphist.Utils.Localization;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace Telegraphist.UI.Notifications
{
    public class NotificationBox : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private LocalizeStringEvent title, content;
        [SerializeField] private Image image;
        [SerializeField] private float transitionDuration = 0.2f;
        [SerializeField] private Ease transitionEase = Ease.InOutCubic;

        private void Awake()
        {
            FadeOut().Complete();
        }

        public async UniTask Show(NotificationData data)
        {
            title.SetStringReference(data.TitleLocalized);
            content.SetStringReference(data.ContentLocalized);

            if (data.Icon != null)
            {
                image.sprite = data.Icon;
            }
            else
            {
                image.enabled = false;
            }

            await FadeIn();
            await FadeOut().SetDelay(data.Timeout);
            Destroy(gameObject);
        }

        public Tween FadeIn() => 
            canvasGroup.DOFade(1, transitionDuration)
                .SetEase(transitionEase)
                .SetLink(gameObject);

        public Tween FadeOut() => 
            canvasGroup.DOFade(0, transitionDuration)
                .SetEase(transitionEase)
                .SetLink(gameObject);
    }
}