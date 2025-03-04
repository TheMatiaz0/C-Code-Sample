using DG.Tweening;
using Telegraphist.Helpers;
using UnityEngine;

namespace Telegraphist.Gameplay.HandPaperVariant
{
    public class ArmController : MonoBehaviour
    {
        [SerializeField] private Transform armRoot;
        [SerializeField] private Transform ikTarget;
        [SerializeField] private Transform idlePosition;
        [SerializeField] private float offsetScale = 0.5f;
        [SerializeField] private float newLineDuration = 0.1f;
        [SerializeField] private Ease newLineEase;
        [SerializeField] private float idleDuration = 0.5f;
        [SerializeField] private Ease idleEase;
        [SerializeField] private PaperMovementController paperMovementController;

        private Vector3 initialArmPosition;
        private bool isArmAnimating;
        private bool isArmActive;

        private void Start()
        {
            initialArmPosition = armRoot.localPosition;
            isArmAnimating = false;
            isArmActive = true;
        }

        // TODO refactor
        public void PointAt(Vector3 earlyPosition, bool isEarlyNewLine)
        {
            if (!isArmActive) return;
            
            if (isArmAnimating) return;
            
            if (isEarlyNewLine)
            {
                AnimateArm(earlyPosition, newLineDuration);
            }
            else
            {
                armRoot.localPosition = GetTargetArmPosition(earlyPosition);
                ikTarget.localPosition = earlyPosition;
            }
        }

        private Vector2 GetTargetArmPosition(Vector2 earlyPosition)
            => new Vector2(initialArmPosition.x,
                initialArmPosition.y + Mathf.Min(earlyPosition.y, 0) * offsetScale);

        private Tween AnimateArm(Vector2 earlyPosition, float duration)
        {
            isArmAnimating = true;
            return DOTween.Sequence()
                .Append(armRoot.DOLocalMove(GetTargetArmPosition(earlyPosition), duration))
                .Join(ikTarget.DOLocalMove(earlyPosition, duration))
                .OnComplete(() => isArmAnimating = false)
                .OnKill(() => isArmAnimating = false)
                .SetEase(newLineEase)
                .SetLink(gameObject);
        }

        public void SetArmActive(bool active)
        {
            isArmActive = active;

            if (active)
            {
                isArmAnimating = false;
                var (earlyPosition, _) = paperMovementController
                    .GetHandPositionForBeat(SongController.Current.CurrentBeat + TempoUtils.TimeToBeat(idleDuration)); // todo [variable bpm] change to DurationTimeToBeat
                AnimateArm(earlyPosition, idleDuration);
            }
            else
            {
                DOTween.Sequence()
                    .Append(armRoot.DOLocalMove(initialArmPosition, idleDuration))
                    .Join(ikTarget.DOMove(idlePosition.position, idleDuration))
                    .SetEase(idleEase)
                    .SetLink(gameObject);
            }
        }
    }
}