using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Telegraphist.Gameplay;
using Telegraphist.Helpers.Router;
using Telegraphist.Helpers.Scenes;
using Telegraphist.LevelEditor.Playback;
using Telegraphist.Lifecycle;
using Telegraphist.Onboarding.Calibration;
using Telegraphist.Pauseable;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Telegraphist.UI.Menus
{
    public class PauseMenuController : SceneSingleton<PauseMenuController>
    {
        [SerializeField] private Canvas pauseMenuCanvas;
        [SerializeField] private Button resumeBtn;
        [SerializeField] private Button restartBtn;

        public string RouteName => "PauseMenu";
        public bool IsPauseActive => pauseMenuCanvas.enabled;
        public bool IsLocked { get; set; }

        private readonly List<IPauseable> registeredPauseables = new();

        private void Start()
        {
            resumeBtn.onClick.AddListener(Resume);

            var currentScene = SceneLoader.GetActiveScene();
            bool isGameplayOrLevelEditorScene = currentScene == SceneType.Gameplay || currentScene == SceneType.LevelEditor;

            restartBtn.gameObject.SetActive(isGameplayOrLevelEditorScene);
            if (isGameplayOrLevelEditorScene)
            {
                restartBtn.onClick.AddListener(Restart);
            }
        }

#if !UNITY_EDITOR
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                TryOpenPause();
            }
        }
#endif

        // closing is done via PauseRouterBinding (easier this way)
        public void TryOpenPause()
        {
            if ((!IsPauseActive && IsLocked) || IsPauseActive)
            {
                return;
            }
            
            SetPause(true);
        }

        public void ClosePause()
        {
            SetPause(false);
        }

        public async UniTask OnEnter()
        {
            SetPause(true);
        }

        public async UniTask OnExit()
        {
            SetPause(false);
        }

        
        public void Register(IPauseable pauseable)
        {
            if (registeredPauseables.Contains(pauseable))
            {
                return;
            }
            registeredPauseables.Add(pauseable);
        }

        public void Unregister(IPauseable pauseable)
        {
            if (!registeredPauseables.Contains(pauseable))
            {
                return;
            }
            registeredPauseables.Remove(pauseable);
        }

        private void SetPause(bool isActive)
        {
            UniTask.Void(async () =>
            {
                // prevent re-opening pause on escape
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
                pauseMenuCanvas.enabled = isActive;
            });

            if (isActive)
            {
                InnerPause();
            }
            else
            {
                InnerResume();
            }
        }

        private void InnerPause()
        {
            foreach (var pauseable in registeredPauseables)
            {
                pauseable.Pause();
            }
            resumeBtn.Select();
        }

        private void InnerResume()
        {
            EventSystem.current.SetSelectedGameObject(null);
            foreach (var pauseable in registeredPauseables)
            {
                pauseable.Resume();
            }
        }

        private void Resume()
        {
            SetPause(false);
        }

        private void Restart()
        {
            if (LevelEditorPlaybackBridge.Current)
            {
                LevelEditorPlaybackBridge.Current.StartPlayback();
            }
            else if (CalibrationController.Current)
            {
                CalibrationController.Current.Restart();
            }
            else
            {
                GameplayEntrypoint.Current.ReloadSceneWithCurrentSong();
            }
        }
    }
}
