using Telegraphist.Helpers.Provider;
using Telegraphist.Structures;
using UnityEngine;

namespace Telegraphist.Gameplay.QTE
{
    public enum QteLogic
    {
        ArrowSequence,
        CurveSequence
    }

    public interface IQteLogic : IInjectable
    {
        QteLogic LogicType { get; }
        Transform Root { get; }

        void Initialize(QteLayout preferredLayout, int requiredPresses);
        void AddKeyObject(QteDirection direction, float timeToPress);
        void KeyPressed(AccuracyStatus status);
        void FinishSequence(out float accuracyValue);
    }
}
