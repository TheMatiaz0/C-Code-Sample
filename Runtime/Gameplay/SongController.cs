using Telegraphist.Lifecycle;
using System;
using Cysharp.Threading.Tasks;
using Telegraphist.Events;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Settings;
using Telegraphist.Pauseable;
using Telegraphist.Structures;
using UniRx;
using UnityEngine;
using Telegraphist.UI.Menus;

namespace Telegraphist.Gameplay
{
    public class SongController : SceneSingleton<SongController>, IPauseable
    {
        [SerializeField] private SongAudioController audioController;
        [SerializeField] private float songRestartDelay = 1f;

        public SongPack SongPack => SongContext.SongPack;
        public SongData CurrentSong => SongPack.Data;

        private double startDspTime;
        private float pausedPlaybackTime;
        private bool isPlaying;

        public bool IsPlaying
        {
            get => isPlaying;
            private set
            {
                isPlaying = value;
                MessageBroker.Default.Publish(new OnSongPlayStateChange(value));
            }
        }

        public float CurrentBeat => TempoUtils.TimeToBeat(SongPlaybackTime, CurrentSong.Bpm);

        public float SongPlaybackTime
        {
            get
            {
                if (audioController.AudioSource != null)
                {
                    // instead of playing the song earlier, we just do everything else later by subtracting AudioLatency (which is most often positive)
                    return (float)(AudioSettings.dspTime - startDspTime - SettingsController.Settings.AudioLatency) * audioController.Pitch;
                }
                else
                {
                    Debug.LogWarning($"AudioSource is null, this is bad!", this);
                    return 0;
                }
            }
        }

        private SongContext SongContext => SongContext.Current;

        public void Pause() => PauseSong();
        public void Resume() => ResumeSong();

        private void OnDestroy()
        {
            if (PauseMenuController.Current != null)
            {
                PauseMenuController.Current.Unregister(this);
            }
        }

        private void Update()
        {
            if (!IsPlaying) return;
            
            MessageBroker.Default.Publish(new OnSongTimeUpdate(SongPlaybackTime));

            CheckSongEnd(SongPlaybackTime);
        }

        public bool CheckSongEnd(float songTime)
        {
            var realTime = songTime + CurrentSong.AudioOffset;
            var songEnded = CurrentSong.SongEnd > 0 && realTime >= CurrentSong.SongEnd;
            
            if (realTime >= SongContext.SongAudio.length || songEnded)
            {
                MessageBroker.Default.Publish(new OnSongEnd());
                MessageBroker.Default.Publish(new OnSongStop());
                IsPlaying = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets song from SongContext and plays it immediately, skipping to startAtOffset if provided.
        /// </summary>
        public void Play(float startAtOffset = 0)
        {
            PauseMenuController.Current.Register(this);

            var songTime = SongPack.ClampToSongBounds(startAtOffset + CurrentSong.AudioOffset);

            if (audioController)
            {
                audioController.Setup(SongPack.Audio, songTime);
            }

            startDspTime = AudioSettings.dspTime - songTime + CurrentSong.AudioOffset;
            IsPlaying = true;
            
            MessageBroker.Default.Publish(new OnSongBeforeStart());
            MessageBroker.Default.Publish(new OnSongStart());
        }

        public void LoadSongFrame(float startAtOffset = 0)
        {
            startAtOffset = SongPack.ClampToSongBounds(startAtOffset + CurrentSong.AudioOffset);
            startDspTime = AudioSettings.dspTime - startAtOffset + CurrentSong.AudioOffset;
            
            MessageBroker.Default.Publish(new OnSongBeforeStart());
            MessageBroker.Default.Publish(new OnSongStart());
            MessageBroker.Default.Publish(new OnSongTimeUpdate(SongPlaybackTime));
        }

        public void RestartWithDelay()
        {
            UniTask.Void(async () =>
            {
                StopSong();
                await UniTask.Delay(TimeSpan.FromSeconds(songRestartDelay));
                Play();
            });
        }

        public void ResumeSong()
        {
            Play(pausedPlaybackTime);
        }

        public void PauseSong()
        {
            pausedPlaybackTime = SongPlaybackTime;
            IsPlaying = false;
            
            MessageBroker.Default.Publish(new OnSongStop());
        }

        public void StopSong()
        {
            startDspTime = 0;
            IsPlaying = false;
            MessageBroker.Default.Publish(new OnSongStop());
            MessageBroker.Default.Publish(new OnSongTimeUpdate(0));
        }
    }
}