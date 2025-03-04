using System;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Analytics;
using UniRx;
using UnityEngine;

namespace Telegraphist.UI.Buttons
{
    public class ButtonTrackEvent : MonoBehaviour
    {
        [SerializeField, Self(Flag.Editable)] private ActionButton actionButton;
        [SerializeField] private AnalyticsEvent eventType;

        private void Start()
        {
            actionButton.OnClick.Subscribe(_ => AnalyticsController.Current.TrackEvent(eventType)).AddTo(this);
        }
    }
}