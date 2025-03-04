using UnityEngine;

namespace Telegraphist.Structures
{
    public record SongPack(SongData Data, AudioClip Audio)
    {
        public float Duration => Audio.length;
        public float SongEnd => Data.SongEnd <= 0 ? Audio.length : Data.SongEnd;
        public float AudioOffset => Data.AudioOffset;

        public float ClampToSongBounds(float value)
        {
            // Max value here is subtracted by 1 because skipping to song duration will lead to playback error
            return Mathf.Clamp(value, 0, Duration - 1);
        }
    }
}