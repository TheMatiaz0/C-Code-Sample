using System;
using Telegraphist.Lifecycle;
using Telegraphist.Input;
using Telegraphist.LevelEditor.Playback;
using Telegraphist.LevelEditor.Timeline;
using UniRx;
using UnityEngine.InputSystem;
using UnityEngine;
using Telegraphist.Gameplay;
using Telegraphist.Gameplay.TileInput;

namespace Telegraphist.LevelEditor
{
    public class LevelEditorInputReceiver : SceneSingleton<LevelEditorInputReceiver>
    {
        public float ZoomScrollAxis { get; private set; } = 0;
        public float BeatFractionScrollAxis { get; private set; } = 0;
        private float pressTime;
        private float releaseTime;
        
        private Subject<TimelineToolType> onToolTypeChanged = new();
        private Subject<Unit> onDeleted = new();
        private Subject<Unit> onCopied = new();
        private Subject<Unit> onPasted = new();

        public IObservable<TimelineToolType> OnToolTypeChanged => onToolTypeChanged;
        public IObservable<Unit> OnDeleted => onDeleted;
        public IObservable<Unit> OnCopied => onCopied;
        public IObservable<Unit> OnPasted => onPasted;
        
        #region Controls [SendMessage()]
        private void OnUndo()
        {
            LevelEditorContext.Current.Song.Undo();
        }

        private void OnRedo()
        {
            LevelEditorContext.Current.Song.Redo();
        }
        
        private void OnCopy()
        {
            onCopied.OnNext(Unit.Default);
        }
        
        private void OnPaste()
        {
            onPasted.OnNext(Unit.Default);
        }
        
        private void OnZoom(InputValue dir)
        {
            ZoomScrollAxis = dir.Get<float>();
        }
        
        private void OnChangeBeatFraction(InputValue dir)
        {
            BeatFractionScrollAxis = dir.Get<float>();
        }
        
        private void OnPlayPause()
        {
            if (LevelEditorPlaybackBridge.Current != null)
            {
                LevelEditorPlaybackBridge.Current.InputPlayPause();
            }
        }
      
        private void OnPlaceTile(InputValue inputValue)
        {
            var value = inputValue.Get<float>();
            var isPressed = value != 0;
            if (isPressed)
            {
                pressTime = Time.time;
            }
            else
            {
                releaseTime = Time.time;
                AddTileAtCursorHandler.Current.AddTileAtCurrentPosition(releaseTime - pressTime);
            }
        }

        private void OnDelete()
        {
            onDeleted.OnNext(Unit.Default);
        }

        private void OnSelectionTool()
        {
            onToolTypeChanged.OnNext(TimelineToolType.Select);
        }

        private void OnEditTool()
        {
            onToolTypeChanged.OnNext(TimelineToolType.Edit);
        }

        private void OnToggleFullscreen()
        {
            LevelEditorPlaybackBridge.Current.ToggleFullscreen();
        }
        
        public void OnSeekForward()
        {
            LevelEditorPlaybackBridge.Current.SeekPlaybackForward();
        }

        public void OnSeekBackward()
        {
            LevelEditorPlaybackBridge.Current.SeekPlaybackBackward();
        }
        
        private void OnUnlockAllLevels()
        {
            DevCheatsController.Current.OnUnlockAllLevels();
        }
        
        public void OnAddPoints()
        {
#if UNITY_EDITOR
            ScoringSystem.Current.Score += 100;
#else
            Debug.LogError("Ah, ah! No cheating! Hehehe");
#endif
        }
        

        #endregion

    }
}
