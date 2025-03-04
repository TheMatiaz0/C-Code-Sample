using System;
using Telegraphist.TileSystem;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("First Frequency Page Tile"), Serializable]
    public record FirstFrequencyPageTile : Tile, IFrequencyChangeFirstTile;

    public class FirstFrequencyPageTileBehaviour : TileBehaviour<FirstFrequencyPageTile>
    {
        public FirstFrequencyPageTileBehaviour(FirstFrequencyPageTile tile, int index) : base(tile, index) { }
    }
}