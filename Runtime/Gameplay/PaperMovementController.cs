using System.Collections.Generic;
using Telegraphist.Dialogue;
using Telegraphist.Events;
using Telegraphist.Helpers;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.HandPaperVariant
{
    public class PaperMovementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HandPaperHelpers helpers;
        [SerializeField] private PaperPhantomTiles phantomTiles;
        [SerializeField] private PageFlipAnimation pageAnimation;
        [SerializeField] private ArmController arm;
        [SerializeField] private NewLineIndicator newLineIndicator;
        [SerializeField] private PaperPointer pointer;
        [SerializeField] private PaperCameraRotation cameraRotation;
        [Header("Hand movement")]
        [SerializeField] private Transform hand;
        [SerializeField] private float earlyNewLineIndicationOffset = 0.5f;
        [SerializeField] private float earlyPageSkip = 0.3f;
        [SerializeField] private float handEarlyOffset = 0.3f;

        private float EarlyNewLineIndicationOffsetBeats => TempoUtils.TimeToBeat(earlyNewLineIndicationOffset);
        private float EarlyPageSkipBeats => TempoUtils.TimeToBeat(earlyPageSkip);
        
        public float HandEarlyOffsetBeats => TempoUtils.TimeToBeat(handEarlyOffset);
        
        private List<PageObject> pages;
        private int lastPageIndex;
        private float lastEarlyY, lastY;
        private int pageIndexOverride = -1;
        private int frequencyChangeIndex;

        private void Start()
        {
            MessageBroker.Default.Receive<OnFrequencyChangeTileEnd>()
                .Subscribe(OnFrequencyChangeEnd)
                .AddTo(this);
            
            DialogueController.Current.OnDialogueStart.Subscribe(_ => SetMovementActive(false)).AddTo(this);
            DialogueController.Current.OnDialogueEnd.Subscribe(_ => SetMovementActive(true)).AddTo(this);
        }

        public void Setup(List<PageObject> pages)
        {
            this.pages = pages;
            lastPageIndex = 0;
            lastEarlyY = 0;
            lastY = 0;
            pageIndexOverride = -1;
            frequencyChangeIndex = 0;
        }
        
        public void MoveHandAndPage(float time)
        {
            var beat = TempoUtils.TimeToBeat(time);
            var (handPosition, handPageIndex) = GetHandPositionForBeat(beat);
            var (linePosition, linePageIndex) = helpers.BeatToFinalPositionEarlyNextRow(beat, EarlyNewLineIndicationOffsetBeats, pageIndexOverride);
            var (_, earlySkipPageIndex) = helpers.BeatToFinalPositionEarlyNextRow(beat, EarlyPageSkipBeats, pageIndexOverride);
            
            MoveHandAndPointer(handPosition, HandEarlyOffsetBeats);
            IndicateNewLineIfNeeded(linePosition, HandEarlyOffsetBeats, handPageIndex != linePageIndex);
            FlipPageIfNeeded(earlySkipPageIndex);
            FadePhantomTiles(beat, handPageIndex);
            cameraRotation.Rotate(beat, pageIndexOverride);
        }

        public (Vector2 position, int pageIndex) GetHandPositionForBeat(float beat) => 
            helpers.BeatToFinalPositionEarlyNextRow(beat, HandEarlyOffsetBeats, pageIndexOverride);

        private void OnFrequencyChangeEnd(OnFrequencyChangeTileEnd e)
        {
            // Debug.Log($"Frequency change end: {frequencyChangeIndex}");
            
            pageIndexOverride = helpers.GetPageForBeat(helpers.FirstTilePositionsAfterFrequencyChange[frequencyChangeIndex]);
            frequencyChangeIndex++;
        }
        
        private void SetMovementActive(bool isActive)
        {
            arm.SetArmActive(isActive);
            pointer.gameObject.SetActive(isActive);
            newLineIndicator.gameObject.SetActive(isActive);
        }
        
        private void MoveHandAndPointer(Vector2 position, float offset)
        {
            var isNewLine = position.y != lastY;
            
            arm.PointAt(position, isNewLine);
            pointer.MovePointer(position);
            
            var lineEndPositionX = helpers.PageExtents.x - helpers.BeatDurationToWidth(offset);
            pointer.SetLineProgress(position.x / lineEndPositionX);
            
            lastY = position.y;
        }
        
        private void IndicateNewLineIfNeeded(Vector2 earlyPosition, float offsetBeats, bool isNewPage)
        {
            if (isNewPage) return;
            if (earlyPosition.y == lastEarlyY) return;
            if (earlyPosition.y == helpers.BeatToFinalPosition(0).position.y) return;
            if (pageIndexOverride > lastPageIndex) return;
            
            earlyPosition.x += helpers.BeatDurationToWidth(offsetBeats);
            newLineIndicator.IndicateNewLine(earlyNewLineIndicationOffset, earlyPosition).Forget();
            lastEarlyY = earlyPosition.y;
        }
        
        private void FlipPageIfNeeded(int pageIndex)
        {
            if (lastPageIndex == pageIndex) return;
            
            pageAnimation.FlipPage(pages[lastPageIndex], pages[pageIndex]);
            lastPageIndex = pageIndex;
        }

        private void FadePhantomTiles(float beat, int page)
        {
            var relativeBeat = helpers.RelativePageBeat(beat, page);

            if (relativeBeat < 0) return;
            
            var position = helpers.BeatToPosition(relativeBeat);
            var row = helpers.GetBeatRow(relativeBeat);
            var minimumWidth = helpers.BeatDurationToWidth((1 - phantomTiles.AppearThreshold) * helpers.BeatsInRow);

            if (position.x > minimumWidth)
            {
                phantomTiles.FadeIn(page, row);
            }
            else
            {
                var previousPage = row - 1 < 0 ? page - 1 : page;
                var previousRow = row - 1 < 0 ? 0 : row - 1;
                phantomTiles.FadeOut(previousPage, previousRow);
            }
        }
    }
}