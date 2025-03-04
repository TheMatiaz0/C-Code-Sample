using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Analytics;
using Telegraphist.Helpers.LevelStore;
using Telegraphist.Helpers.Router;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Onboarding.Calibration;
using Telegraphist.Scriptables;
using UnityEngine;

namespace Telegraphist.Gameplay
{
    public record GameplaySceneArgs(LevelScriptable Level, float SongStartDelay = 0f)
        : LoadSceneArgs(SceneType.Gameplay, Delay: LoadSceneDelay), IRouteWithAnalytics
    {
        private const float LoadSceneDelay = 1f;

        public override string RouteParams => Level.name;
        public string AnalyticsName => "PlayLevel";
        public Dictionary<string, string> AnalyticsParams => new() { { "LevelName", Level.name }, { "LevelID", Level.levelID.ToString() } };
    }

    public class GameplayEntrypoint : SceneEntrypoint<GameplayEntrypoint, GameplaySceneArgs>
    {
        [SerializeField] private float songStartDelay = 0.5f;

        public bool EnableAutoplay { get; set; } = true;

        private bool ShouldAutoplay => EnableAutoplay 
                                       && !SceneLoader.IsSceneActive(SceneType.LevelEditor)
                                       && SceneLoader.GetSceneArguments<CalibrationSceneArgs>() == null;

        protected override async UniTask<GameplaySceneArgs> GetDefaultSceneArgsAsync()
        {
            await LevelRepository.WaitForLevels();
            return new GameplaySceneArgs(LevelRepository.GetByName(EditorSongProvider.CurrentLevelName), songStartDelay);
        }

        protected override void InitScene()
        {
            if (!ShouldAutoplay) return; // hand control over to LevelEditorState

            var (level, delay) = SceneArgs;
            
            if (level == null)
            {
                Debug.LogError("Gameplay scene loaded with null level!");
                GlobalRouter.Current.ReplaceTopLevel(new LoadSceneArgs(SceneType.LevelSelect)).Forget();
                return;
            }

            PrepareGameplay(level);
            EditorSongProvider.CurrentLevelName = level.name;

            UniTask.Void(async () =>
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
                if (ShouldAutoplay)
                {
                    SongController.Current.Play();
                }
            });
        }

        public static void LoadSceneAndPlay(LevelScriptable level)
        {
            GlobalRouter.Current.Push(new GameplaySceneArgs(level));
        }

        public static void PlayLevelManual(LevelScriptable level)
        {
            PrepareGameplay(level);
            SongController.Current.Play();
        }

        public void ReloadSceneWithCurrentSong()
        {
            GlobalRouter.Current.Replace(new GameplaySceneArgs(SceneArgs.Level));   
        }

        private static void PrepareGameplay(LevelScriptable level)
        {
            SongContext.Current.LoadSong(level.SongPack);
            LevelContext.Current.SetLevel(level);
        }
    }
}