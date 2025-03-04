using Telegraphist.Utils;
using UnityEngine;
using Telegraphist.Helpers;

namespace Telegraphist.Gameplay.TileInput
{
    public enum PressInRangeResult
    {
        InRange,
        TooLate,
        TooEarly,
    }

    public static class PressInRangeHelper
    {
        public static (PressInRangeResult result, float diff, float accuracy) CheckInRange(float currentBeat, float tileBeat, float maxInputOffsetBeats = -1)
        {
            if (maxInputOffsetBeats == -1)
            {
                maxInputOffsetBeats = TempoUtils.TimeToBeat(TimingValuesStore.MaxInputOffset);
            }

            var diff = currentBeat - tileBeat;
            var diffAbs = Mathf.Abs(diff);
            var isInRange = diffAbs <= maxInputOffsetBeats;

            var accuracy = 1 - diffAbs / maxInputOffsetBeats;

            if (!isInRange)
            {
                if (currentBeat > tileBeat)
                {
                    return (PressInRangeResult.TooLate, diff, accuracy);
                }
                else
                {
                    return (PressInRangeResult.TooEarly, diff, accuracy);
                }
            }

            return (PressInRangeResult.InRange, diff, accuracy);
        }

        public static string GetPressStatusContext(float diff)
        {
            if (Mathf.Abs(diff) <= 0.02) return "";

            var str = diff > 0 ? LocalizedStringsController.Current.tooLate : LocalizedStringsController.Current.tooEarly;
            return str.GetLocalizedString();
        }
    }
}