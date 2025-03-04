using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Eflatun.SceneReference;
using Telegraphist.UI.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Telegraphist.Helpers.Scenes
{
    public static class SceneLoader
    {
        private const string Path = nameof(SceneListLoadout);

        private static SceneListLoadout loadout;

        private static Dictionary<SceneType, SceneReference> sceneDictionary = new();
        
        private static object sceneArguments = null;
        public static bool IsLoading { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Loadout()
        {
            loadout = Resources.Load(Path) as SceneListLoadout;
            sceneDictionary = loadout.Scenes.ToDictionary(key => key.SceneType, value => value.Scene);
            sceneArguments = null;
        }

        public static void LoadScene(SceneType sceneType, LoadSceneMode mode = LoadSceneMode.Single, bool withTransition = true, float delay = 0) =>
            LoadScene(new LoadSceneArgs(sceneType, withTransition, delay), mode);
        
        public static void LoadScene<T>(T args, LoadSceneMode mode = LoadSceneMode.Single) where T : LoadSceneArgs
        {
            LoadSceneAsync(args, mode).Forget();
        }

        public static async UniTask LoadSceneAsync(SceneType sceneType, LoadSceneMode mode = LoadSceneMode.Single, bool withTransition = true)
            => await LoadSceneAsync(new LoadSceneArgs(sceneType, withTransition), mode);
        
        public static async UniTask LoadSceneAsync<T>(T args, LoadSceneMode mode = LoadSceneMode.Single) where T : LoadSceneArgs
        {
            if (IsLoading)
            {
                var time = Time.time;
                Debug.LogWarning("Another scene is already loading!");
                await UniTask.WaitUntil(()=> !IsLoading || Time.time - time > 2f);
            }
            
            sceneArguments = args;
            IsLoading = true;

            if (args.WithTransition)
            {
                await DoSceneTransition(async () =>
                {
                    await InternalLoadSceneAsync(args, mode);
                }, args.Delay);
            }
            else
            {
                await InternalLoadSceneAsync(args, mode);
            }
        }
        
        public static AsyncOperation UnloadSceneAsync(SceneType sceneType) => 
            SceneManager.UnloadSceneAsync(GetSceneName(sceneType));

        public static bool IsSceneActive(SceneType sceneType)
        {
            if (!sceneDictionary.TryGetValue(sceneType, out SceneReference scene))
            {
                return false;
            }
            return scene.LoadedScene == SceneManager.GetActiveScene();
        }

        public static SceneType GetSceneType(Scene scene)
        {
            foreach (var (type, sceneRef) in sceneDictionary)
            {
                if (sceneRef.LoadedScene == scene)
                {
                    return type;
                }
            }

            throw new Exception("Scene is not in the scene list");
        }

        public static SceneType GetActiveScene() => GetSceneType(SceneManager.GetActiveScene());

        public static T GetSceneArguments<T>() => 
            sceneArguments is T arguments 
                ? arguments 
                : default;
        
        private static string GetSceneName(SceneType sceneType) => sceneDictionary[sceneType].Name;
        
        private static async UniTask InternalLoadSceneAsync(LoadSceneArgs args, LoadSceneMode mode = LoadSceneMode.Single)
        {
            try
            {
                await SceneManager.LoadSceneAsync(GetSceneName(args.SceneType), mode);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private static async UniTask DoSceneTransition(Func<UniTask> loadSceneCallback, float delay)
        {
            await SceneTransition.Current.FadeIn(showLoading: delay != 0).AsyncWaitForCompletion();
            await loadSceneCallback();
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            SceneTransition.Current.FadeOut();
        }
    }
}
