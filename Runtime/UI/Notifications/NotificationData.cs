using Telegraphist.Utils.Localization;
using UnityEngine;
using UnityEngine.Localization;

namespace Telegraphist.UI.Notifications
{
    public record NotificationData(
        LocalizedString TitleLocalized,
        LocalizedString ContentLocalized,
        Sprite Icon = null,
        float Timeout = 0f)
    {
        public NotificationData(string title, string content) : this(new FakeLocalizedString(title), new FakeLocalizedString(content)) { }
    }
}