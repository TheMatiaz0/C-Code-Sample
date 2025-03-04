using System.IO;
using Newtonsoft.Json;
using Telegraphist.Structures;
using UnityEngine;

namespace Telegraphist.Helpers.LevelStore
{
    public static class SongHelper
    {
        public const string ResourcesSongsPath = "Songs";
        public const string ResourcesLevelsPath = "Levels";
        public static readonly string ResourcesAbsoluteSongsPath = Path.Join("Assets", "Resources", ResourcesSongsPath);
        
        public const string SongDataExtension = "json";

        public static string GetSongDataPath(string basePath, SongData songData) => 
            Path.Join(basePath, $"{songData.Name}.{SongDataExtension}");
        
        public static string GetSongAudioPath(string basePath, SongData song) =>
            Path.Join(basePath, song.AudioFileName);

        public static SongPack InitializeSong(SongData data, AudioClip clip, string name = null)
        {
            data.Name ??= name;
            data.Bake();
            
            return new SongPack(data, clip);
        }

        public static SongPack InitializeSong(TextAsset asset, AudioClip clip, string name = null)
        {
            var data = JsonConvert.DeserializeObject<SongData>(asset.text);
            return InitializeSong(data, clip, name ?? asset.name);
        }
        
        public static void SaveSong(string path, SongData songData)
        {
            File.WriteAllText(GetSongDataPath(path, songData), JsonConvert.SerializeObject(songData));
        }

        public static void DeleteSong(string path, SongData songData)
        {
            File.Delete(GetSongDataPath(path, songData));
            File.Delete(GetSongAudioPath(path, songData));
        }
    }
}