using System;
using System.Collections.Generic;
using System.Linq;
using Cyberultimate.Unity;
using Telegraphist.Structures;
using UnityEngine;
using DG.Tweening;
using Telegraphist.Gameplay.TileInput;
using Telegraphist.Scriptables;

namespace Telegraphist.Gameplay.QTE 
{
    public struct QteKeyDurations
    {
        public float preDuration;
        public float moveDuration;
        public float postDuration;
    }
    
    public class QteSequenceDisplay : MonoBehaviour, IQteLogic
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("QTE Specific")]
        [SerializeField] private Transform qtePivot;
        [SerializeField] private Transform qteIndicatorsPivot;
        [SerializeField] private QteKeyBehaviour arrowPrefab;
        [SerializeField] private SequenceArrowIndicator arrowIndicatorPrefab;
        [SerializeField] private QtePointsDisplayer pointsDisplayer;
        [SerializeField] private GameObject horizontalLayout;
        [SerializeField] private GameObject verticalLayout;
        [SerializeField] private GameObject bothLayout;
        [Header("Arrow Tween")]
        [SerializeField] private Ease preAnimEase = Ease.InCubic;
        [SerializeField] private Ease moveAnimEase = Ease.Linear;
        [SerializeField] private Ease postAnimEase = Ease.InBounce;
        [SerializeField] private float preAnimDurationPercent = 0.2f;
        [SerializeField] private float postAnimDurationPercent = 0.1f;
        [Header("Arrow Transforms")]
        [SerializeField] private float prePositionOffset;
        [SerializeField] private float startPositionOffset;
        [SerializeField] private float postPositionOffset;

        private BalanceScriptable balance;
        private int scoreSum;
        private float currentAccuracy;
        private int requiredPresses;
        private Queue<QteKeyBehaviour> activeDirectionArrows = new ();

        public QteLogic LogicType => QteLogic.ArrowSequence;
        public Transform Root => this.transform;

        private void Start()
        {
            SetElementsActive(false);
            balance = BalanceScriptable.Current;
        }

        public void Initialize(QteLayout preferredLayout, int requiredPresses)
        {
            this.requiredPresses = requiredPresses;
            scoreSum = 0;
            currentAccuracy = 0;

            activeDirectionArrows = new();
            horizontalLayout.SetActive(preferredLayout == QteLayout.Horizontal);
            verticalLayout.SetActive(preferredLayout == QteLayout.Vertical);
            bothLayout.SetActive(preferredLayout == QteLayout.Both);

            SetElementsActive(true);
        }

        public void FinishSequence(out float accuracyValue)
        {
            accuracyValue = currentAccuracy / requiredPresses;
            pointsDisplayer.ShowPointsText(scoreSum, balance.GetAccuracyStatus(accuracyValue));

            SetElementsActive(false);
            qtePivot.KillAllChildren();
            qteIndicatorsPivot.KillAllChildren();
        }

        private void SetElementsActive(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0;
        }

        public void AddKeyObject(QteDirection direction, float timeToPress)
        {
            var arrowBehaviour = Instantiate(arrowPrefab, Vector3.zero, qtePivot.rotation, qtePivot);
            
            var guid = Guid.NewGuid();
            var directionVector = direction.ToCounterVector();
            
            var arrowIndicator = Instantiate(arrowIndicatorPrefab, Vector3.zero, qtePivot.rotation, qteIndicatorsPivot);
            arrowIndicator.Initialize(timeToPress, Vector3.zero, new Vector3(0,0,direction.ToRotationZ()));

            var durations = new QteKeyDurations()
            {
               preDuration  = timeToPress * preAnimDurationPercent,
               moveDuration = timeToPress - timeToPress * preAnimDurationPercent,
               postDuration = timeToPress * postAnimDurationPercent
            };
            
            var arrowMoveSequence = CreateArrowMovementSequence(arrowBehaviour, durations, directionVector).Pause();
            
            arrowBehaviour.Initialize(direction, guid, durations, arrowMoveSequence, () =>
            {
                HandleNextArrow(out _, guid);
                Destroy(arrowBehaviour.gameObject);
            });
            activeDirectionArrows.Enqueue(arrowBehaviour);
            if (activeDirectionArrows.Count == 1) arrowBehaviour.SetAsNextArrow();
        }

        private Sequence CreateArrowMovementSequence(QteKeyBehaviour arrowBehaviour, QteKeyDurations durations, Vector2Int arrowDirection)
        {
            var arrowTransform = arrowBehaviour.transform;
            var targetScale = arrowTransform.localScale;
            var prePositionOffsetVector = new Vector3(-prePositionOffset * arrowDirection.x,
                -prePositionOffset * arrowDirection.y, 0);
            
            arrowTransform.localPosition = new Vector3(startPositionOffset * arrowDirection.x, startPositionOffset * arrowDirection.y, 0) + prePositionOffsetVector;
            arrowTransform.localScale = new Vector3(0,0,0);

            var seq = DOTween.Sequence()
                .Append(
                    arrowTransform
                        .DOBlendableLocalMoveBy(
                            new Vector3(prePositionOffset * arrowDirection.x, prePositionOffset * arrowDirection.y, 0),
                            durations.preDuration).SetEase(preAnimEase))
                .Join( arrowTransform.DOScale(targetScale, durations.preDuration).SetEase(preAnimEase))
                .Append( arrowTransform.DOLocalMove(Vector2.zero, durations.moveDuration).SetEase(moveAnimEase))
                .Append(arrowTransform.DOBlendableLocalMoveBy(
                            new Vector3(postPositionOffset * -arrowDirection.x, postPositionOffset * -arrowDirection.y,
                                0), durations.postDuration)
                        .SetEase(postAnimEase))
                .SetLink(arrowBehaviour.gameObject);

            return seq;
        }

        private void HandleNextArrow(out QteKeyBehaviour activeArrow, Guid? arrowGuid = null)
        {
            activeArrow = null;
            if (activeDirectionArrows.Count <= 0) return;
            activeArrow =  activeDirectionArrows.Peek();
            if (arrowGuid != null && activeDirectionArrows.Peek().Guid != arrowGuid) return;
            activeDirectionArrows.Dequeue();
            if (activeDirectionArrows.Count <= 0) return;
            
            //found the next arrow in the queue
            var nextArr = activeDirectionArrows.Peek();
            nextArr.SetAsNextArrow();
        }

        public void KeyPressed(AccuracyStatus status)
        {
            if (status is AccuracyStatus.Invalid) return;

            var qteAccuracy = balance.DirectionQteScoring.Find(x => x.Status == status);
            currentAccuracy += qteAccuracy.Value;

            PressWithAccuracy(qteAccuracy);
        }

        private void PressWithAccuracy(BalanceScriptable.QteAccuracy qteAccuracy)
        {
            scoreSum += qteAccuracy.Score;
            ScoringSystem.Current.Score += qteAccuracy.Score;
            pointsDisplayer.ShowPointsText(qteAccuracy.Score, qteAccuracy.Status);

            HandleNextArrow(out var activeArrow);
            if (activeArrow == null) return;
            activeArrow.QteKeyPressed(qteAccuracy.Value);
        }
    }
}
