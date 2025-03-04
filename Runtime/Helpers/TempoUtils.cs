using System;
using Telegraphist.Gameplay;
using Telegraphist.LevelEditor;
using Telegraphist.Structures;
using UnityEngine;

namespace Telegraphist.Helpers
{
    public static class TempoUtils
    {
        // TODO maybe some kind of ILevelContext implementation (or DI)
        private static SongData UniversalSong => SongContext.Current 
            ? SongContext.Current.SongData 
            : LevelEditorContext.Current
                ? LevelEditorContext.Current.SongData
                : throw new Exception("No song context found");
        
        public static float TimeToBeat(float time, float? bpm = null)
        {
            bpm ??= UniversalSong.Bpm;
            return time * (bpm.Value / 60);
        } 

        public static float BeatToTime(float beat, float? bpm = null)
        {
            bpm ??= UniversalSong.Bpm;
            return (beat * 60) / bpm.Value;
        }

        public static float Snap(float beat, float fraction, Func<float, float> roundFunc)
        {
            return roundFunc(beat / fraction) * fraction;
        }

        public static (float, float) SnapRange(float startBeat, float endBeat, float fraction, Func<float, float> roundFunc)
        {
            var snappedStart = Snap(startBeat, fraction, roundFunc);
            var snappedEnd = Snap(endBeat, fraction, roundFunc);
            if (Mathf.Abs(snappedStart - snappedEnd) <= Mathf.Epsilon)
            {
                snappedEnd += fraction;
            }
            
            return (snappedStart, snappedEnd);
        }
    }
}
