using Telegraphist.Scriptables;
using Telegraphist.Structures;

namespace Telegraphist.Events
{
    public record OnLevelFinish(LevelScriptable Level, int Score, Rank Rank);
}