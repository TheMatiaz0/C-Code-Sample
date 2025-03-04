namespace Telegraphist.Lifecycle
{
    public class SongSingleton<T> : SongBehaviour where T : SongSingleton<T>
    {
        public static T Current { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            if (Current)
            {
                Destroy(gameObject);
                return;
            }
            
            Current = (T)this;
        }
    }
}