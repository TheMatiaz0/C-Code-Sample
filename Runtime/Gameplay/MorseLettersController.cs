using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Telegraphist.Events;
using Telegraphist.Gameplay.Combo;
using Telegraphist.Gameplay.HandPaperVariant.Tiles;
using Telegraphist.Gameplay.QTE;
using Telegraphist.Gameplay.TileInput;
using Telegraphist.Helpers.Provider;
using Telegraphist.Scriptables;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.HandPaperVariant
{
    public class MorseLettersController : MonoBehaviour, IInjectable
    {
        [SerializeField] private TileSpawnerPaper tileSpawner;
        [SerializeField] private MorseLetter morseLetterPrefab;
        [SerializeField] private Vector2 letterPositionOffsetFromTile;
        [Header("Points display")]
        [SerializeField] private Vector2 pointsPositionOffsetFromLetter;
        [SerializeField] private QtePointsDisplayer pointsDisplay;
        [SerializeField] private float animationDurationMultiplier = 0.4f;

        private void Start()
        {
            MessageBroker.Default.Receive<OnMorseLetterEnd>()
                .Subscribe(OnMorseLetterEnd)
                .AddTo(this);
        }

        public (MorseLetter, int) SpawnLetter(string letter, List<(int index, Tile tile)> relatedTiles, bool isHighlighted)
        {
            int middleIndex = relatedTiles.Count / 2;
            PaperTile middleTileObject = null;
            var lastTileIndex = relatedTiles[^1].index;
            var lastTileObject = (PaperTile)tileSpawner.TileObjects[lastTileIndex]; 

            int totalPresses = 0;
            int realIndex = 0;
            List<MorseUnderlinePaper> underlines = new ();
            foreach (var (index, tile) in relatedTiles)
            {
                var tileObject = (PaperTile)tileSpawner.TileObjects[index];
                tileObject.SetLetterCompleted();
                var isTileLong = BalanceScriptable.Current.IsTileLong(tile);
                totalPresses += isTileLong ? 2 : 1;
                
                underlines.Add(new (tileObject.GetSymbol(), tileObject.transform.parent, tileObject.GetStartPosition(), tileObject.GetEndPosition(), isTileLong));

                if (realIndex == middleIndex) middleTileObject = tileObject;
                realIndex++;
            }

            if (middleTileObject == null) middleTileObject = lastTileObject;
            
            var morseLetter = Instantiate(morseLetterPrefab, middleTileObject.transform.parent);
            morseLetter.Setup(letter, middleTileObject.GetMiddlePosition() + letterPositionOffsetFromTile, totalPresses, underlines, isHighlighted);
            return (morseLetter, totalPresses);
        }

        private void OnMorseLetterEnd(OnMorseLetterEnd data)
        {
            (float accuracy, float totalPresses, Vector2 position) = data;
            var letterPoints = BalanceScriptable.Current.AdditionalPointsPerMorseLetterPress;
            var curve = BalanceScriptable.Current.MorseLetterPointsCurve;
            
            var maxPoints = letterPoints * totalPresses;
            var points = (int)(curve.Evaluate(accuracy) * maxPoints);
            var status = BalanceScriptable.Current.GetAccuracyStatus(points / maxPoints);
            
            ScoringSystem.Current.Score += points;
            pointsDisplay.ShowPointsText(points, status, animationDurationMultiplier, position + pointsPositionOffsetFromLetter, false);
        }
    }
}