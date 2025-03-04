using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Telegraphist.Utils
{
    public static class AudioClipUtils
    {
        public static readonly string[] AudioExtensions = { "acc", "aiff", "it", "mod", "mp3", "mp2", "ogg", "s3m", "wav", "xm", "xma", "vag" };
        
        public static AudioClip Clone(this AudioClip audioClip, string newName)
        {
            AudioClip newAudioClip = AudioClip.Create(newName, audioClip.samples, audioClip.channels, audioClip.frequency, false);
            float[] copyData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(copyData, 0);
            newAudioClip.SetData(copyData, 0);
            return newAudioClip;
        }
        
        public static void TrimFromStart(this AudioClip clip, float offsetSeconds)
        {
            var offsetSamples = Mathf.FloorToInt(offsetSeconds * clip.frequency);
            var newClipData = new float[clip.samples - offsetSamples];
            clip.GetData(newClipData, offsetSamples);
            clip.SetData(newClipData, 0);
        }
        
        public static AudioType GetAudioTypeFromExtension(string extension) => extension.Replace(".", "") switch
        {
            "acc" => AudioType.ACC,
            "aiff" => AudioType.AIFF,
            "it" => AudioType.IT,
            "mod" => AudioType.MOD,
            "mp3" => AudioType.MPEG,
            "mp2" => AudioType.MPEG,
            "ogg" => AudioType.OGGVORBIS,
            "s3m" => AudioType.S3M,
            "wav" => AudioType.WAV,
            "xm" => AudioType.XM,
            "xma" => AudioType.XMA,
            "vag" => AudioType.VAG,
            _ => AudioType.UNKNOWN
        };
        
        public static async UniTask<AudioClip> LoadAudioFromFile(string filePath)
        {
            var extension = System.IO.Path.GetExtension(filePath);
            using (var uwr = UnityWebRequestMultimedia.GetAudioClip($"file://{filePath}", GetAudioTypeFromExtension(extension)))
            {
                var handler = (DownloadHandlerAudioClip)uwr.downloadHandler;
                handler.streamAudio = false;
                handler.compressed = false;
                
                await uwr.SendWebRequest();

                if (!handler.isDone)
                {
                    throw new Exception($"failed to load audio file: {handler.error}");
                }
                
                return handler.audioClip;
            }
        }
    }
}