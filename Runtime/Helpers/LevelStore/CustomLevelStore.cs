using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Telegraphist.Scriptables;
using Telegraphist.Structures;
using Telegraphist.Utils;
using UnityEngine;

namespace Telegraphist.Helpers.LevelStore
{
    public class CustomLevelStore : ILevelStore
    {
        public string Name { get; }

        public string Path { get; private set; }

        public CustomLevelStore(string name, string path)
        {
            Name = name;
            Path = System.IO.Path.GetFullPath(path);
        }

        public async UniTask<List<LevelScriptable>> LoadAll()
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }

            var files = Directory.GetFiles(Path).Where(x => x.EndsWith(SongHelper.SongDataExtension));
            var levels = (
                await UniTask.WhenAll(files.Select(LoadLevelFromJsonPath))
            ).Where(x => x != null).ToList();

            return levels;
        }

        public void Save(LevelScriptable level)
        {
            // todo support different song name and file name
            SongHelper.SaveSong(Path, level.SongPack.Data);
        }

        public void Delete(LevelScriptable level)
        {
            SongHelper.DeleteSong(Path, level.SongPack.Data);
        }

        private async UniTask<LevelScriptable> LoadLevelFromJsonPath(string jsonPath)
        {
            try
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(jsonPath);
                var content = await File.ReadAllTextAsync(jsonPath);
                var songData = JsonConvert.DeserializeObject<SongData>(content);
                var audio = await AudioClipUtils.LoadAudioFromFile(SongHelper.GetSongAudioPath(Path, songData));

                return LevelScriptable.CreateRaw(name, songData, audio, this);
            }
            catch (Exception e)
            {
                Debug.LogError($"failed to load custom level at {jsonPath}: {e}");

                return null;
            }
        }
    }
}