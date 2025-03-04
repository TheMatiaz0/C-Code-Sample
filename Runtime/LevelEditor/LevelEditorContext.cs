using Cysharp.Threading.Tasks;
using Telegraphist.Helpers;
using Telegraphist.Helpers.LevelStore;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Structures;
using Telegraphist.Utils.State;
using UnityEngine;

namespace Telegraphist.LevelEditor
{
    public record LevelEditorSceneArgs(string SongName) : LoadSceneArgs(SceneType.LevelEditor);

    public class LevelEditorContext : SceneEntrypoint<LevelEditorContext, LevelEditorSceneArgs>
    {
        [SerializeField] private ValueContainer<SongData> songState;
        
        public ValueContainer<SongData> Song => songState;
        public SongData SongData => songState.Value;
        public AudioClip SongAudio { get; private set; }
        public SongPack SongPack => new(SongData, SongAudio);

        protected override LevelEditorSceneArgs DefaultSceneArgs => new(EditorSongProvider.CurrentLevelName);
        
        private UniTaskCompletionSource songLoadCompletion = new();

        protected override void InitScene()
        {
            var songName = SceneArgs.SongName;
            if (songName != null)
            {
                _ = LoadSong(songName);
            }
            else
            {
                SceneLoader.LoadScene(SceneType.LevelSelect);
            }
        }
        
        public async UniTask WaitForSong() => await songLoadCompletion.Task;

        public async UniTask LoadSong(string songName)
        {
            EditorSongProvider.CurrentLevelName = songName;
            
            songState = new ValueContainer<SongData>(new SongData(), maxHistory: 20);
            
            await LevelRepository.WaitForLevels();
            var level = LevelRepository.GetByName(songName);
            var (song, audio) = level.SongPack;

            if (audio == null)
            {
                Debug.LogError($"AudioClip is missing for current level. Moving to LevelList...");
                SceneLoader.LoadScene(SceneType.LevelSelect, withTransition: false);
                return;
            }

            songState.Value = song;
            SongAudio = audio;
            
            LevelContext.Current.SetLevel(level);
            
            songLoadCompletion.TrySetResult();
        }

        public void Bake()
        {
            songState.Update((ref SongData data) => data.Bake());
        }
    }
}