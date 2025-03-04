using Telegraphist.Events;
using Telegraphist.Gameplay;
using Telegraphist.Helpers;
using Telegraphist.Structures;
using UniRx;
using UnityEngine;

namespace Telegraphist.Lifecycle
{
    public class SongBehaviour : MonoBehaviour
    {
        protected CompositeDisposable DisposablesPerPlay { get; private set; }
        
        protected SongController SongController => SongController.Current;
        protected SongContext SongContext => SongContext.Current;
        protected SongData CurrentSong => SongContext.Song;

        protected virtual void Awake()
        {
            MessageBroker.Default.Receive<OnSongBeforeStart>().Subscribe(_ => SongBeforeStart()).AddTo(this);
            MessageBroker.Default.Receive<OnSongStart>().Subscribe(_ => OnSongStart()).AddTo(this);
            MessageBroker.Default.Receive<OnSongTimeUpdate>().Subscribe(SongUpdate).AddTo(this);
            MessageBroker.Default.Receive<OnSongStop>().Subscribe(_ => OnSongStop()).AddTo(this);
            MessageBroker.Default.Receive<OnSongEnd>().Subscribe(_ => OnSongEnd()).AddTo(this);
            MessageBroker.Default.Receive<OnSongPlayStateChange>().Subscribe(SongPlayStateChange).AddTo(this);
        }

        protected virtual void OnDestroy()
        {
            DisposablesPerPlay?.Dispose();
        }

        protected virtual void OnSongLoad() { }

        private void SongBeforeStart()
        {
            DisposablesPerPlay?.Dispose();
            DisposablesPerPlay = new CompositeDisposable();
            
            OnSongBeforeStart();
        }
        
        protected virtual void OnSongBeforeStart() { }
        
        protected virtual void OnSongStart() { }
        
        private void SongUpdate(OnSongTimeUpdate evt)
        {
            OnSongUpdate(evt.Time);
        }
        protected virtual void OnSongUpdate(float time) { }
        
        protected virtual void OnSongStop() { }

        protected virtual void OnSongEnd() { }

        private void SongPlayStateChange(OnSongPlayStateChange evt)
        {
            OnSongPlayStateChange(evt.IsPlaying);
            if (evt.IsPlaying)
            {
                OnSongResume();
            }
            else
            {
                OnSongPause();
            }
        }

        protected virtual void OnSongPlayStateChange(bool isPlaying) { }
        protected virtual void OnSongPause() { }
        protected virtual void OnSongResume() { }
    }
}