using System;
using System.Collections.Generic;
using System.Linq;
using Telegraphist.Helpers;
using Telegraphist.Lifecycle;
using Telegraphist.TileSystem;
using Telegraphist.Utils;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay
{
    public class TileEventDispatcher : SongBehaviour
    {
        public float offset;
        public List<string> tileTypeFilter = new();

        private Subject<(Tile, int)> TileStart = new();
        private Subject<(Tile, int)> TileEnd = new();

        public IObservable<(Tile tile, int index)> OnTileStart => TileStart;
        public IObservable<(Tile tile, int index)> OnTileEnd => TileEnd;

        private SongController Sc => SongController.Current;

        public int TileIndex { get; private set; }
        public Tile CurrentTile => Sc.CurrentSong.Tiles?.ElementAtOrDefault(TileIndex) ?? default;

        private float CurrentBeat => Sc.CurrentBeat - TempoUtils.TimeToBeat(offset);

        private bool tilePlayed;
        private HashSet<string> tileTypeFilterSet;

        private void Start()
        {
            tileTypeFilterSet = tileTypeFilter.ToHashSet();
        }
        
        protected override void OnSongStart()
        {
            TileIndex = FindNextTile();
            tilePlayed = false;
        }

        public void TryNext()
        {
            if (CurrentBeat < CurrentTile.EndBeat)
            {
                TileIndex++;
            }
        }

        private void Update()
        {
            if (!Sc.IsPlaying) return;

            if (CurrentTile == null || TileIndex >= Sc.CurrentSong.Tiles.Count) return;

            if (CurrentBeat >= CurrentTile.EndBeat) // note end
            {
                TileEnd.OnNext((CurrentTile, TileIndex));
                tilePlayed = false;
                TileIndex++;
            }
            else if (CurrentBeat >= CurrentTile.StartBeat && !tilePlayed) // note start
            {
                if (IsValidTileType(CurrentTile))
                {
                    TileStart.OnNext((CurrentTile, TileIndex));
                    tilePlayed = true;
                }
                else
                {
                    TileIndex++;
                }
            }
        }

        private int FindNextTile()
        {
            return Sc.CurrentSong.Tiles.FindIndex(x => x.StartBeat >= CurrentBeat);
        }

        private bool IsValidTileType(Tile tile)
        {
            return tileTypeFilterSet.Count == 0 || tileTypeFilterSet.Contains(tile.Type);
        }
    }
}
