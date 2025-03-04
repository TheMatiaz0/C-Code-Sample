using Telegraphist.Gameplay;
using Telegraphist.Helpers;
using Telegraphist.LevelEditor.Playback;
using Telegraphist.Progression;
using UnityEngine;
using UnityEngine.InputSystem;
using Telegraphist.Structures;

namespace Telegraphist.Input
{
    public class GameInputReceiver : MonoBehaviour
    {

        #region Controls [SendMessage()]

        private void OnTapTelegraph(InputValue input) => GameInputHandler.Current?.OnTapTelegraph(input);
        private void OnTapKeyUp(InputValue input) => GameInputHandler.Current?.OnTapKey(input, QteDirection.Up);
        private void OnTapKeyDown(InputValue input) => GameInputHandler.Current?.OnTapKey(input, QteDirection.Down);
        private void OnTapKeyLeft(InputValue input) => GameInputHandler.Current?.OnTapKey(input, QteDirection.Left);
        private void OnTapKeyRight(InputValue input) => GameInputHandler.Current?.OnTapKey(input, QteDirection.Right);
        private void OnPause() => GameInputHandler.Current?.OnPause();
        private void OnKnobRotate(InputValue input) => GameInputHandler.Current?.OnKnobRotate(input);

        private void OnTakeScreenshot()
        {
            ScreenshotController.Current.TakeScreenshot();
        }

        #endregion

        #region DEV CHEATS

        private void OnToggleCheats()
        {
            DevCheatsController.Current.CheatsEnabled = !DevCheatsController.Current.CheatsEnabled;
        }
        
        private void OnRestart()
        {
            if (!DevCheatsController.Current.CheatsEnabled) return;
            
            GameInputHandler.Current?.OnRestart();
        }

        public void OnSeekForward()
        {
            if (!DevCheatsController.Current.CheatsEnabled) return;

            if (LevelEditorPlaybackBridge.Current != null)
            {
                LevelEditorPlaybackBridge.Current.SeekPlaybackForward();
            }
            else
            {
                //SongController.Current.SeekSong(5);
                LevelEditorPlaybackBridge.Current.SeekSong(5);
            }
        }

        public void OnSeekBackward()
        {
            if (!DevCheatsController.Current.CheatsEnabled) return;

            if (LevelEditorPlaybackBridge.Current != null)
            {
                LevelEditorPlaybackBridge.Current.SeekPlaybackBackward();
            }
            else
            {
                //SongController.Current.SeekSong(-5);
                LevelEditorPlaybackBridge.Current.SeekSong(-5);
            } 
        }

        private void OnForceLevelFinish()
        {
            if (!DevCheatsController.Current.CheatsEnabled) return;
            
            DevCheatsController.Current.ForceFinishLevel();
        }

        #endregion
    }
}
