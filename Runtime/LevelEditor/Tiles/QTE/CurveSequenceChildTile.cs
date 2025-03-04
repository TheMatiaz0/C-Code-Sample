using System;
using Telegraphist.Gameplay.QTE;
using Telegraphist.Helpers.Provider;
using Telegraphist.TileSystem;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("Curve Sequence Child", ColorHtml = "#ff322b"), Serializable]
    public record CurveSequenceChildTile(
        int PositionIndex = 3,
        float Thickness = 15f
    ) : Tile;

    public class CurveSequenceChildTileBehaviour : TileBehaviour<CurveSequenceChildTile>
    {
        public CurveSequenceChildTileBehaviour(CurveSequenceChildTile tile, int index, IProvider provider) : base(tile, index)
        {
        }
    }
}
