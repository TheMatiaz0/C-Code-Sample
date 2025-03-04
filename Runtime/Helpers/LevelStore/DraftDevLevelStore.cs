using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Telegraphist.Scriptables;
using Telegraphist.Structures;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Telegraphist.Helpers.LevelStore
{
    public class DraftDevLevelStore : ILevelStore
    {
        public string Name => "Draft Dev";
        public string Path => System.IO.Path.Join(Directory.GetCurrentDirectory(), SongHelper.ResourcesAbsoluteSongsPath);

        public async UniTask<List<LevelScriptable>> LoadAll()
        {
            var levels = Resources.LoadAll<LevelScriptable>(SongHelper.ResourcesLevelsPath);
            var songs = Resources.LoadAll<TextAsset>(SongHelper.ResourcesSongsPath);
            var draftSongs = songs.Where(song => !levels.Any(level => level.songAsset == song)).ToList();
            
            return draftSongs.Select(LoadLevelFromSongAsset).Where(x => x != null).ToList();
        }

        public void Save(LevelScriptable level)
        {
            SongHelper.SaveSong(SongHelper.ResourcesAbsoluteSongsPath, level.SongPack.Data);
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public void Delete(LevelScriptable level)
        {
            SongHelper.DeleteSong(SongHelper.ResourcesAbsoluteSongsPath, level.SongPack.Data);
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        private LevelScriptable LoadLevelFromSongAsset(TextAsset songAsset)
        {
            try
            {
                var audio = Resources.Load<AudioClip>(
                    System.IO.Path.Join(SongHelper.ResourcesSongsPath, songAsset.name));
                var data = JsonConvert.DeserializeObject<SongData>(songAsset.text);
                var level = LevelScriptable.CreateRaw(songAsset.name, data, audio, this);

                return level;
            }
            catch (Exception e)
            {
                Debug.LogError($"failed to load draft dev level {songAsset.name}: {e}");
                
                return null;
            }
        }
    }
}