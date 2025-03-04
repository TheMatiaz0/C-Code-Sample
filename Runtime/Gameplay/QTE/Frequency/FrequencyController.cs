using System.Collections.Generic;
using System.Linq;
using Telegraphist.Gameplay.Tiles;
using Telegraphist.Lifecycle;
using Telegraphist.TileSystem;
using Telegraphist.Utils;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.QTE.Frequency
{
    public sealed class FrequencyController : SongSingleton<FrequencyController>
    {
        [SerializeField, Header("Best if frequencyCount is odd")]
        private int frequencyCount = 3;
        [SerializeField] private int maxRandomFrequencyDelta = 2;
        
        private int frequency = 0;
        private readonly BehaviorSubject<int> frequencyChange = new(0);
        private readonly List<int> frequencyChanges = new();
        
        public int Frequency
        {
            get => frequency;
            set
            {
                var newValue = Mathf.Clamp(value, 0, MaxFrequency);
                if (newValue == frequency) return;

                frequency = newValue;
                frequencyChange.OnNext(newValue);
            }
        }

        public int MaxFrequency => frequencyCount - 1;
        public int FrequencyCount => frequencyCount;
        public int DefaultFrequency => Mathf.FloorToInt(MaxFrequency / 2f);
        
        private void Start()
        {
            Frequency = DefaultFrequency;
            TileController.Current.OnTilesLoad.Subscribe(_ => RestoreFrequency()).AddTo(this);
        }
        
        protected override void OnSongBeforeStart()
        {
            frequencyChanges.Clear();
            RegisterFrequencyTile(DefaultFrequency);
        }
        
        public void RegisterFrequencyTile(int target)
        {
            frequencyChanges.Add(target);
        }
        
        public int GetNextRandomFrequency()
        {
            var last = frequencyChanges.Last();
            var result = MathUtils.RandomIntExcept(
                Mathf.Max(last - maxRandomFrequencyDelta, 0), 
                Mathf.Min(last + maxRandomFrequencyDelta + 1, FrequencyCount),
                last);
            return result;
        }

        private void RestoreFrequency()
        {
            var currentBeat = SongController.Current.CurrentBeat;
            var frequencyTile = TileController.Current.TileHandles
                .TakeUntil(x => x.BaseTile.StartBeat > currentBeat)
                .OfType<FrequencyChangeTileBehaviour>()
                .LastOrDefault();
            Frequency = frequencyTile?.TargetFrequency ?? DefaultFrequency;
        }
    }
}
