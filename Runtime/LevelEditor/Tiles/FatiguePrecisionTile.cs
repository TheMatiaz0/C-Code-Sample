using System;
using Telegraphist.Helpers.Provider;
using Telegraphist.TileSystem;
using Telegraphist.Gameplay.QTE.PrecisionZone;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("Fatigue Precision Zone", ColorHtml = "#f0f08d"), Serializable]
    public record FatiguePrecisionZoneTile(
        float SleepTime = 1.5f
) : PrecisionZoneTile;

    public class FatiguePrecisionZoneTileBehaviour : AbstractPrecisionTileBehaviour<FatiguePrecisionZoneTile>
    {
        private readonly FatigueDisplayer fatigueDisplayer;

        public FatiguePrecisionZoneTileBehaviour(FatiguePrecisionZoneTile tile, int index, IProvider provider) : base(tile, index, provider)
        {
            fatigueDisplayer = provider.Get<FatigueDisplayer>();
        }

        protected override void DisplayInitialization()
        {
            base.DisplayInitialization();
            fatigueDisplayer.SquintEyelids();
        }

        protected override void DisplaySuccessResult()
        {
            base.DisplaySuccessResult();
            fatigueDisplayer.OpenEyelids();
        }

        protected override void DisplayFailResult()
        {
            base.DisplayFailResult();
            fatigueDisplayer.Sleep(Tile.SleepTime);
        }
    }
}
