using System;
using Telegraphist.Helpers.Provider;
using Telegraphist.TileSystem;
using UnityEngine.UI;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("Precision Zone", ColorHtml = "#b3b381"), Serializable]
    public record PrecisionZoneTile(
        float ZoneSize = 0.1f,
        float NeedleSpeed = 265f,
        float DelayAppearAndDisappear = 1.5f,
        bool UseRandomOrigin = true,
        Image.Origin360 TargetOrigin = Image.Origin360.Left,
        bool IsOriginClockwise = true
    ) : Tile;

    public class PrecisionZoneTileBehaviour : AbstractPrecisionTileBehaviour<PrecisionZoneTile>
    {
        public PrecisionZoneTileBehaviour(PrecisionZoneTile tile, int index, IProvider provider) : base(tile, index, provider)
        {
        }
    }
}
