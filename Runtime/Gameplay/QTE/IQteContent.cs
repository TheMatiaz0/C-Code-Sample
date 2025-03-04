using Telegraphist.Helpers.Provider;
using Telegraphist.Structures;
using UnityEngine;

namespace Telegraphist.Gameplay.QTE
{
    public enum QteContent
    {
        None,
        BrokenTelegraph,
        FrequencySwitcher,
        Orders
    }

    public interface IQteContent : IInjectable
    {
        QteContent ContentType { get; }
        Transform Root { get; }

        void OnDirectorPreIndication();
        void OnDirectorEnter();
        void OnDirectionEnter(QteDirection direction);
        void OnDirectionPress(AccuracyStatus accuracyStatus);
        void OnDirectorExit(bool isQtePassed);
    }
}
