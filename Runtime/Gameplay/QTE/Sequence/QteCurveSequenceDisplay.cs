using System.Collections.Generic;
using System.Linq;
using Telegraphist.Gameplay.TileInput;
using Telegraphist.Gameplay.Tiles;
using Telegraphist.Helpers.Provider;
using Telegraphist.Input;
using Telegraphist.Scriptables;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using Telegraphist.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Telegraphist.Gameplay.QTE
{
    public class QteCurveSequenceDisplay : MonoBehaviour, IInjectable
    {
        [Header("Values")]
        [Header("Player")]
        [SerializeField]
        private float playerSpeed;
        [SerializeField]
        private float accelerationRate = 2f;
        [SerializeField]
        private float maxSpeed = 200f;
        [Header("Scoring")]
        [SerializeField]
        private float maxScore = 100f;
        [SerializeField]
        private float tolerance = 10f;
        [SerializeField]
        private float validationCooldown = 0.5f;
        [Header("Visual")]
        [SerializeField]
        private float distanceMultiplier = 15f;

        [Header("References")]
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private CurvedUILineRenderer linePrefab;
        [SerializeField]
        private RectTransform player;
        [SerializeField]
        private RectTransform left;
        [SerializeField]
        private RectTransform right;
        [SerializeField]
        private Transform snapPointsParent;
        [SerializeField]
        private RectTransform lineParent;
        [SerializeField]
        private QtePointsDisplayer pointsDisplayer;
        [SerializeField]
        private RadioEffect radioEffect;

        [Header("Debug")]
        [SerializeField]
        private bool enableMovement = true;
        [SerializeField]
        private bool enableSmoothing = true;

        private QteDirection currentDirection;
        private List<CurvedUILineRenderer> lineRenderers = new();
        private Vector2 cachedLinePosition;
        private bool isSequenceActive = false;
        private BalanceScriptable balance;
        private CurveSequenceParentTile parentTile;
        private float validationTime;
        private int scoreSum;
        private float accuracy;
        private float holdTime;
        private float currentSpeed;
        private CurveSequenceChildTileBehaviour lastChildTile;
        private List<RectTransform> snapPoints = new();
        private float linePositionX;

        public Transform Root => this.transform;

        private void Awake()
        {
            SetElementsActive(false);

            balance = BalanceScriptable.Current;
            cachedLinePosition = lineParent.localPosition;

            snapPoints = new();
            foreach (RectTransform rect in snapPointsParent)
            {
                snapPoints.Add(rect);
            }
        }

        private void SetElementsActive(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0;
            isSequenceActive = visible;
        }

        private void StartMove(QteDirection direction)
        {
            currentDirection = direction;
            holdTime = 0f;
        }

        private bool CanPlayerMove()
        {
            return currentDirection != QteDirection.None && isSequenceActive;
        }

        private void Update()
        {
            if (CanPlayerMove())
            {
                ApplyPlayerAcceleration();
            }
        }

        private void ApplyPlayerAcceleration()
        {
            holdTime += Time.deltaTime;
            currentSpeed = Mathf.Min(playerSpeed + holdTime * accelerationRate, maxSpeed);
            MovePlayer();
        }

        private void MovePlayer()
        {
            var newXPos = player.localPosition.x + (currentDirection.ToVector().x * currentSpeed * Time.deltaTime);
            var positionX = Mathf.Clamp(newXPos, left.localPosition.x, right.localPosition.x);

            player.localPosition = new(positionX, player.localPosition.y);
        }

        private void EndMove()
        {
            currentDirection = QteDirection.None;
            currentSpeed = playerSpeed;
            holdTime = 0f;
        }

        public void Initialize(CurveSequenceParentTile tile)
        {
            ResetValues();
            GenerateLines(tile);

            GameInputHandler.Current.OnQteKeyPressEnded
                .Subscribe(_ => EndMove())
                .AddTo(this);

            GameInputHandler.Current.OnQteKeyPressStarted
               .Where(x => x == QteDirection.Left || x == QteDirection.Right)
               .Subscribe(x => StartMove(x))
               .AddTo(this);

            SetElementsActive(true);
        }

        private void ResetValues()
        {
            SetElementsActive(false);

            radioEffect.ResetValues();
            scoreSum = 0;
            accuracy = 0;
            lineParent.localPosition = cachedLinePosition;
            player.localPosition = new(snapPoints[Mathf.FloorToInt((snapPoints.Count - 1) / 2)].localPosition.x, player.localPosition.y);

            foreach (Transform transform in lineParent)
            {
                Destroy(transform.gameObject);
            }

            lineRenderers = new List<CurvedUILineRenderer>();
        }

        private bool IsStartOfLine(int index)
        {
            return index % 2 == 0;
        }

        private void GenerateLines(CurveSequenceParentTile parentTile)
        {
            this.parentTile = parentTile;

            var children = TileController.Current.GetChildTiles(parentTile);
            var childBehaviours = children.ConvertAll(x => x as CurveSequenceChildTileBehaviour);
            childBehaviours.RemoveAll(x => x == null);

            int index = 0;

            foreach (var childTile in childBehaviours)
            {
                var t = childTile.BaseTile as CurveSequenceChildTile;

                var snapPointPosition = snapPoints[t.PositionIndex].localPosition.x;

                var startBeat = childTile.OverrideStartBeat - parentTile.StartBeat;
                var endBeat = childTile.OverrideEndBeat - parentTile.StartBeat;

                if (IsStartOfLine(index))
                {
                    var createdLine = Instantiate(linePrefab, lineParent);
                    var lineRenderer = createdLine.Line;
                    lineRenderer.LineThickness = t.Thickness;
                    lineRenderers.Add(createdLine);

                    GetLineRenderer(index).Points = new[] { new Vector2(snapPointPosition, startBeat * distanceMultiplier) };
                }
                else
                {
                    var lastChild = childBehaviours[index - 1];
                    var lastChildEndBeat = lastChild.OverrideEndBeat - parentTile.StartBeat;
                    var timeBetweenChildren = startBeat - lastChildEndBeat;

                    // DuplicateLastPointY(index, snapPointPosition);
                    AddToLastPoint(index, new(snapPointPosition, timeBetweenChildren * distanceMultiplier));
                }

                AddToLastPoint(index, new(snapPointPosition, childTile.OverrideDurationBeats * distanceMultiplier));
                index += 1;
                lastChildTile = childTile;
            }

            if (enableSmoothing)
            {
                foreach (var lineRenderer in lineRenderers)
                {
                    lineRenderer.SmoothPointsToBezier();
                }
            }
        }

        private UILineRenderer GetLineRenderer(int index)
        {
            return lineRenderers[Mathf.FloorToInt(index / 2)].Line;
        }

        private void AddToLastPoint(int index, Vector2 position)
        {
            var lineRenderer = GetLineRenderer(index);
            AddPoint(lineRenderer, new(position.x, lineRenderer.Points[^1].y + position.y));
        }

        private void AddPoint(UILineRenderer line, Vector2 position)
        {
            var points = new List<Vector2>(line.Points)
            {
                position
            };

            line.Points = points.ToArray();
        }

        private void DuplicateLastPointY(int index, float positionX)
        {
            var lineRenderer = GetLineRenderer(index);
            AddPoint(lineRenderer, new(positionX, lineRenderer.Points[^1].y));
        }

        public void UpdateLogic(float CurrentBeat)
        {
            if (!isSequenceActive || !enableMovement)
            {
                return;
            }

            var startBeat = parentTile.StartBeat;
            var durationBeats = lastChildTile.OverrideEndBeat - parentTile.StartBeat;

            var linePositionY = GetPositionYFromTime(CurrentBeat, startBeat, durationBeats);

            lineParent.localPosition = new Vector2(lineParent.localPosition.x, -linePositionY);
            var (linePositionX, line) = GetPositionXFromY(linePositionY + player.rect.height);
            this.linePositionX = linePositionX;

            if (line != null)
            {
                Validate(linePositionX, line.LineThickness);
            }
        }

        public void AutomateMovement()
        {
            var delta = player.localPosition.x - linePositionX;

            if (delta < 0)
            {
                currentDirection = QteDirection.Right;
            }
            else if (delta > 0)
            {
                currentDirection = QteDirection.Left;
            }
            else
            {
                currentDirection = QteDirection.None;
            }
        }

        private float GetPositionYFromTime(float currentBeat, float startBeat, float durationBeats)
        {
            var progress = (currentBeat - startBeat) / durationBeats;
            var destination = lineRenderers[^1].Line.Points[^1].y;

            return progress * destination;
        }

        private (float, UILineRenderer) GetPositionXFromY(float positionY)
        {
            foreach (var lineRenderer in lineRenderers)
            {
                var points = lineRenderer.Line.Points;

                for (int i = 0; i < points.Length - 1; i++)
                {
                    var start = points[i];
                    var end = points[i + 1];

                    if (positionY >= start.y && positionY <= end.y)
                    {
                        float t = (positionY - start.y) / (end.y - start.y);
                        var currentLineXPos = Mathf.Lerp(start.x, end.x, t);

                        return (currentLineXPos, lineRenderer.Line);
                    }
                }
            }

            return (0, null);
        }

        private void Validate(float linePositionX, float thickness)
        {
            if (Time.time > validationTime)
            {
                var distance = Mathf.Abs(player.localPosition.x - linePositionX);
                var adjustedTolerance = tolerance * (1 + Mathf.Log(thickness + 1, 2));
                var score = CalculateScore(distance, adjustedTolerance);

                scoreSum += score;
                accuracy = score / maxScore;

                radioEffect.ControlValue = accuracy;

                var accuracyStatus = accuracy == 0 ? AccuracyStatus.Invalid : balance.GetAccuracyStatus(accuracy);
                PublishStatus(new QteInputStatus(accuracyStatus));

                pointsDisplayer.ShowPointsText(scoreSum, accuracy > 0.5f ? AccuracyStatus.Perfect : AccuracyStatus.Invalid);

                validationTime = Time.time + validationCooldown;
            }
        }

        private void PublishStatus(QteInputStatus status)
        {
            MessageBroker.Default.Publish(status);
        }

        private int CalculateScore(float distance, float tolerance)
        {
            return distance < tolerance ? Mathf.CeilToInt(maxScore * (1 - (distance / tolerance))) : 0;
        }

        public void FinishSequence(out float accuracyValue)
        {
            ScoringSystem.Current.Score = scoreSum;
            accuracyValue = accuracy;
            ResetValues();
        }
    }
}
