using System;
using System.Linq;
using Telegraphist.Gameplay.Tiles;
using Telegraphist.Helpers;
using Telegraphist.TileSystem;
using UnityEngine;

namespace Telegraphist.LevelEditor.Timeline.Tiles
{
    [Serializable]
    public struct TimelineTileBuilder
    {
        public int row;
        public float column;
        public float width;
        public string type;
        public Guid tileGuid;

        private Tile tile;

        public TimelineTileBuilder(Tile tile, float beatFraction)
        {
            this.tile = tile with {};
            row = tile.Lane;
            column = tile.StartBeat * beatFraction;
            width = GetDuration(tile) * beatFraction;
            type = tile.Type;
            tileGuid = tile.Guid;
        }

        public Tile Build(float beatFraction, string typeName = null, Guid guid = default)
        {
            var newType = string.IsNullOrWhiteSpace(typeName) ? Tile.DefaultType : typeName;
            tile ??= TileRegistry.CreateTile(newType);
            tile = tile with
            {
                Lane = row,
                StartBeat = column / beatFraction,
                Duration = width / beatFraction,
            };

            if (guid != default)
            {
                tile = tile with { Guid = guid };
            }
            
            return tile;
        }

        private static float GetDuration(Tile original)
        {
            // TODO make this generic and universal
            if (original is DialogueTile dialogueTile)
            {
                var dialogue = LevelContext.Current.Level.dialogues.ElementAtOrDefault(dialogueTile.DialogueId);
                if (dialogue == null)
                {
                    Debug.LogWarning($"Dialogue with id {dialogueTile.DialogueId} not found");
                    return original.Duration;
                }

                // TODO [variable bpm] change to DurationTimeToBeat
                return TempoUtils.TimeToBeat(dialogue.TotalDuration);
            }
            
            return original.Duration;
        }
    }
}
