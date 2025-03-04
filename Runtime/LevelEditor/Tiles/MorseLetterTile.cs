using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Telegraphist.Events;
using Telegraphist.Gameplay.HandPaperVariant;
using Telegraphist.Gameplay.QTE;
using Telegraphist.Gameplay.TileInput;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Provider;
using Telegraphist.Helpers.Settings;
using Telegraphist.Hub.Features;
using Telegraphist.Scriptables;
using Telegraphist.TileSystem;
using Telegraphist.Utils;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("Morse letter", ColorHtml = "#7a7a7a"), Serializable]
    public record MorseLetterTile(
        float PreIndicationBeats=0.5f,
        float PostIndicationBeats=0.5f,
        bool IsHighlighted = true
        ) : Tile;

    public class MorseLetterTileBehaviour : TileBehaviour<MorseLetterTile>
    {
        public override float OverrideStartBeat => base.OverrideStartBeat - Tile.PreIndicationBeats;
        public override float OverrideEndBeat => base.OverrideEndBeat + Tile.PostIndicationBeats;
        
        private readonly MorseLettersController controller;
        private readonly TileController tileController;
        private List<TileBehaviour> tileChildren;
        private MorseLetter morseLetter;
        private Vector3 morseLetterPos;
        private float currentAccuracy = 0;
        private int totalPresses = 1;
        
        public MorseLetterTileBehaviour(MorseLetterTile tile, int index, IProvider provider) : base(tile, index)
        {
            controller = provider.Get<MorseLettersController>();
            tileController = TileController.Current;
        }

        protected override void OnTileStart()
        {
            base.OnTileStart();
            MessageBroker.Default.Receive<TileInputStatus>()
                .OfType<TileInputStatus, StatusWithAccuracy>()
                .Subscribe(OnTileStatus)
                .AddTo(Disposables);
            
            MessageBroker.Default.Publish(new OnMorseLetterStart());

            tileChildren = tileController.GetChildTiles(Tile).Where(x => x.BaseTile is MorseTile).ToList();

            var morseText = string.Join("",
                tileChildren.Select(x => BalanceScriptable.Current.IsTileLong(x.BaseTile) ? "-" : "."));
            var letter = MorseCodeUtil.DecodeMorse(morseText);
            
            (morseLetter, totalPresses) = controller.SpawnLetter(letter, tileChildren.Select(x => (x.TileIndex, x.BaseTile)).ToList(), Tile.IsHighlighted);
            morseLetterPos = morseLetter.transform.position;
        }
        
        private void OnTileStatus(StatusWithAccuracy status)
        {
            if (Tile.IsHighlighted && BalanceScriptable.Current.AccuracyDict.TryGetValue(status.Accuracy, out var accuracy))
            {
                var value = accuracy.ScoreModifier;
                morseLetter.CorrectPress(value);
                currentAccuracy += value;
            }
        }

        protected override void OnTileEnd()
        {
            base.OnTileEnd();
            if (Tile.IsHighlighted) MessageBroker.Default.Publish(new OnMorseLetterEnd(currentAccuracy, totalPresses, morseLetterPos));
            morseLetter.Close();
        }
    }
}