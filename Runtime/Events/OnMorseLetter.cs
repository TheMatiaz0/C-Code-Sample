using UnityEngine;

namespace Telegraphist.Events
{
    public record OnMorseLetterStart;
    public record OnMorseLetterEnd(float CurrentAccuracy, int TotalPresses, Vector3 MorseLetterPos);
}