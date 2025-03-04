using System;
using System.Linq;
using Telegraphist.Gameplay.Combo;
using Telegraphist.Lifecycle;
using Telegraphist.Scriptables;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.TileInput
{
    public class ScoringSystem : SceneSingleton<ScoringSystem>
    {
        // TODO switch to UniRx
        public event Action<float> OnUpdateScore = delegate { };

        public int Score
        {
            get => score;
            set { score = value; OnUpdateScore(score); }
        }

        public int MaxScore => SongController.Current.CurrentSong.MaxScore;

        public Rank Rank => BalanceScriptable.Current.CalculateRank(score, MaxScore);
        public Rank MaxRank => BalanceScriptable.Current.GetMaxRank();

        private int score;

        private bool isHoldingDown;
        private Tile holdingTile;
        private float longTileScoreTimer;

        private bool IsIncrementalScoring =>
            BalanceScriptable.Current.LongTileScoring == BalanceScriptable.LongTileScoringMode.Incremental;
        private bool IsStartEndScoring =>
            BalanceScriptable.Current.LongTileScoring == BalanceScriptable.LongTileScoringMode.StartAndEnd;

        private void Start()
        {
            MessageBroker.Default.Receive<TileInputStatus>()
                .Subscribe(OnTileStatusChange)
                .AddTo(this);
        }

        private void Update()
        {
            if (IsIncrementalScoring)
            {
                LongTilesIncrementalScore();
            }
        }

        public void OnTileStatusChange(TileInputStatus status)
        {
            switch (status)
            {
                case StatusPressStarted statusStart:
                    isHoldingDown = true;
                    AddScoreByAccuracy(statusStart.Accuracy);

                    if (IsIncrementalScoring)
                    {
                        holdingTile = statusStart.Tile;
                        longTileScoreTimer = BalanceScriptable.Current.LongTileIncrementInterval;
                    }
                    break;
                case StatusPressEnded statusEnd:
                    isHoldingDown = false;

                    if (IsStartEndScoring)
                    {
                        AddScoreByAccuracy(statusEnd.Accuracy);
                    }
                    break;
            }
        }

        private void AddScoreByAccuracy(AccuracyStatus accuracy)
        {
            if (accuracy == AccuracyStatus.Invalid) return;

            Score += BalanceScriptable.Current.GetTileScore(accuracy);
        }

        private void LongTilesIncrementalScore()
        {
            longTileScoreTimer -= Time.deltaTime;
            if (CanTrackLongTile() && longTileScoreTimer <= 0)
            {
                Score += BalanceScriptable.Current.LongTileScorePerInterval;
                longTileScoreTimer = BalanceScriptable.Current.LongTileIncrementInterval;
            }
        }

        private bool CanTrackLongTile()
        {
            return isHoldingDown && BalanceScriptable.Current.IsTileLong(holdingTile) &&
                   SongController.Current.CurrentBeat <= holdingTile.EndBeat;
        }
    }
}
