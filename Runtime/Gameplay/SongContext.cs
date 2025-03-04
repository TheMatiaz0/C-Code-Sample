using System;
using Telegraphist.Lifecycle;
using Telegraphist.Structures;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay
{
    public class SongContext : SceneSingleton<SongContext>
    {
        public SongPack SongPack { get; private set; }
        public string SongName => SongData.Name;
        public SongData SongData => SongPack.Data;
        public AudioClip SongAudio => SongPack.Audio;
        public static SongData Song => Current.SongData;
        
        private Subject<Unit> onSongLoad = new();
        public IObservable<Unit> OnSongLoad => onSongLoad;
        
        public void LoadSong(SongPack songPack)
        {
            SongPack = songPack;
            
            onSongLoad.OnNext(Unit.Default);
        }
    }
}