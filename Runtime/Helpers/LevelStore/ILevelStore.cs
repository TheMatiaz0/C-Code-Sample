using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Telegraphist.Scriptables;
using Telegraphist.Structures;

namespace Telegraphist.Helpers.LevelStore
{
    public interface ILevelStore
    {
        string Name { get; }
        string Path { get; }
        UniTask<List<LevelScriptable>> LoadAll();
        void Save(LevelScriptable level);
        void Delete(LevelScriptable level);
    }
}