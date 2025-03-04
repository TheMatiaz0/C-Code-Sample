using System;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using UnityEngine;

namespace Telegraphist.Gameplay.HandPaperVariant.Tiles
{
    [Serializable]
    public class PaperTileColors
    {
        [field: SerializeField] public Color ColorDefault { get; private set; } = Color.gray;
        [field: SerializeField] public Color ColorOk { get; private set; } = Color.black;
        [field: SerializeField] public Color ColorInProgress { get; private set; } = Color.black;
        [field: SerializeField] public Color ColorInvalid { get; private set; } = Color.red;
        [field: SerializeField] public Color ColorMissed { get; private set; } = Color.gray;
        [field: SerializeField] public Color ColorPhantom { get; private set; } = Color.blue;

        public Color? GetFromStatus(TileInputStatus status) => status switch
        {
            StatusPressStarted => ColorOk,
            StatusPressEnded => ColorOk,
            StatusMissed => ColorMissed,
            StatusWithAccuracy { Accuracy: AccuracyStatus.Invalid } => ColorInvalid,
            _ => null
        };
    }
}