using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Scriptables;
using UnityEngine;

namespace Telegraphist.Helpers.LevelStore
{
    public static class LevelRepository
    {
        private const string LevelsPath = "Levels";
        private const string SongsPath = "Songs";
        
        public static List<LevelScriptable> Levels { get; private set; }
        public static List<LevelScriptable> PlayableOrderedLevels { get; private set; }
        public static List<ILevelStore> Stores { get; private set; }

        private static UniTaskCompletionSource loadLevelsCompletion;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Stores = new List<ILevelStore>()
            {
                new BuiltinLevelStore(),
#if UNITY_EDITOR
                new DraftDevLevelStore(),
#endif
                new CustomLevelStore("Custom", Path.Join(Application.persistentDataPath, "CustomLevels"))
            };

            loadLevelsCompletion = new UniTaskCompletionSource();
            
            Scan().Forget();
        }

        public static async UniTask Scan()
        {
            var levels = await UniTask.WhenAll(Stores.Select(SafeLoad));
            Levels = levels.SelectMany(x => x).ToList();
            PlayableOrderedLevels = Levels.Where(l => l.isVisible).OrderBy(x => x.levelID).ToList();

            loadLevelsCompletion.TrySetResult();
        }

        public static async UniTask WaitForLevels()
        {
            await loadLevelsCompletion.Task;
        }

        public static List<LevelScriptable> GetFromStore(string storeName)
        {
            return Levels.Where(l => l.LevelStore.Name == storeName).ToList();
        }
        
        public static LevelScriptable GetByName(string name)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Levels == null || Levels.Count == 0)
            {
                Debug.LogError("Levels list took too long to load!");
                SceneLoader.LoadScene(SceneType.LevelSelect);
                return null;
            }
#endif
            var level = Levels.FirstOrDefault(l => l.name == name);
            return level;
        }

        public static bool CanSave(LevelScriptable level) => level.LevelStore != null;
        
        public static void Save(LevelScriptable level)
        {
            if (!CanSave(level))
            {
                throw new Exception("Cannot save level as it does not have any LevelStore associated");
            }
            
            level.SongPack.Data.Name = level.name;
            level.LevelStore.Save(level);
            
            Scan().Forget();
        }

        public static void SaveAs(LevelScriptable level, ILevelStore store, string name)
        {
            level.LevelStore = store;
            level.name = name;
            
            Save(level);
        }

        public static void Delete(LevelScriptable level)
        {
            level.LevelStore.Delete(level);
        }

        private static async UniTask<List<LevelScriptable>> SafeLoad(ILevelStore store)
        {
            try
            {
                return await store.LoadAll();
            }
            catch (Exception e)
            {
                Debug.LogError($"failed to load levels from store '{store.Name}': {e}");
                return new List<LevelScriptable>();
            }
        }
    }
}