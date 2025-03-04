using System;

namespace Telegraphist.Structures
{
    public enum AccuracyStatus
    {
        Perfect = 0,
        Great = 1,
        Good = 2,
        Invalid = 3,
    }
    
    [Flags]
    public enum AccuracyStatusFilter : byte
    {
        Perfect = 1 << AccuracyStatus.Perfect,
        Great = 1 << AccuracyStatus.Great,
        Good = 1 << AccuracyStatus.Good,
        Invalid = 1 << AccuracyStatus.Invalid,
    }
    
    public static class AccuracyStatusExtensions
    {
        public static bool MatchesAccuracy(this AccuracyStatusFilter filter, AccuracyStatus flag) =>
            (filter & (AccuracyStatusFilter) (1 << (int) flag)) != 0;
    }
}
