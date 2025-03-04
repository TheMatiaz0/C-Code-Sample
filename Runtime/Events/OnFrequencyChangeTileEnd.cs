namespace Telegraphist.Events
{
    public record OnFrequencyChangeTileEnd(int TargetFrequency, int TileIndex, bool IsSuccess);
}