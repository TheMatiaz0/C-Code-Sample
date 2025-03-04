namespace Telegraphist.Events
{
    public struct OnSongBeforeStart { }

    public struct OnSongStart { }

    public struct OnSongStop { }

    public struct OnSongEnd { }

    public struct OnSongTimeUpdate
    {
        public float Time { get; init; }

        public OnSongTimeUpdate(float time) => Time = time;
    }

    public struct OnSongPlayStateChange
    {
        public bool IsPlaying { get; init; }
        
        public OnSongPlayStateChange(bool isPlaying) => IsPlaying = isPlaying;
    }
}