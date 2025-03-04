using System;
using Cyberultimate.Unity;
using Cysharp.Threading.Tasks;
using Telegraphist.Lifecycle;
using UniRx;
using UnityEngine;

namespace Telegraphist.UI.Notifications
{
    public class NotificationsPanel : LifetimeSingleton<NotificationsPanel>
    {
        [SerializeField] private NotificationBox prefab;
        [SerializeField] private Transform notificationsRoot;
        [SerializeField] private float defaultTimeout;

        private UniTask currentNotificationTask;

        public override void Setup()
        {
            base.Setup();
            
            notificationsRoot.KillAllChildren();
        }

        // TODO add a queue system or allow displaying multiple notifications at once
        public async UniTaskVoid Show(NotificationData notification)
        {
            if (notification.Timeout == 0)
            {
                notification = notification with { Timeout = defaultTimeout };
            }
            
            currentNotificationTask = ShowInternal(notification, currentNotificationTask);
            await currentNotificationTask;
        }

        private async UniTask ShowInternal(NotificationData notification, UniTask waitFor)
        {
            if (!waitFor.Status.IsCompleted())
            {
                await waitFor;
            }

            var box = Instantiate(prefab, notificationsRoot);
            
            var rectTransform = box.GetComponent<RectTransform>();
            // stretch it to parent
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            await box.Show(notification);
        }
    }
}