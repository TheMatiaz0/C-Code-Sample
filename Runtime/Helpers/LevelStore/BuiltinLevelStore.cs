using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Telegraphist.Scriptables;
using Telegraphist.UI.Notifications;
using Telegraphist.Utils.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Telegraphist.Helpers.LevelStore
{
    public class BuiltinLevelStore : ILevelStore
    {
        
        public string Name => "Builtin";
        public string Path =>
#if UNITY_EDITOR
            System.IO.Path.Join(Directory.GetCurrentDirectory(), SongHelper.ResourcesAbsoluteSongsPath);
#else
            Directory.GetCurrentDirectory();
#endif
        
        public async UniTask<List<LevelScriptable>> LoadAll()
        {
            var result = Resources.LoadAll<LevelScriptable>(SongHelper.ResourcesLevelsPath).ToList();
            
            foreach (var level in result)
            {
                try
                {
                    level.Init(this);
                }
                catch (Exception e)
                {
                    Debug.LogError($"failed to init level {level.levelID} {level.name} - {e}");
                }
            }

            return result;
        }
        
        public void Save(LevelScriptable level)
        {
#if UNITY_EDITOR
            SongHelper.SaveSong(SongHelper.ResourcesAbsoluteSongsPath, level.SongPack.Data);

            UnityEditor.AssetDatabase.Refresh();
#else
            // TODO text-only popup system
            NotificationsPanel.Current.Show(new NotificationData("Error", "Cannot save built-in levels in build version!")).Forget();
#endif
        }

        public void Delete(LevelScriptable level)
        {
#if UNITY_EDITOR
            SongHelper.DeleteSong(SongHelper.ResourcesAbsoluteSongsPath, level.SongPack.Data);
            
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}