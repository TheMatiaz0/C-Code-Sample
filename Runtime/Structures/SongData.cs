using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Telegraphist.TileSystem;

namespace Telegraphist.Structures
{
    [Serializable]
    public record SongData
    {
        public string Name { get; set; }
        public string AudioFileName { get; set; }
        
        public float Bpm { get; set; }
        public float AudioOffset { get; set; }
        public float SongEnd { get; set; }
        
        public int MaxScore { get; set; }

        public float BeatsInRow { get; set; } = 8;
        public float MovementBeatSnap { get; set; } = 1;
        public float BeatsRequiredForLongTile { get; set; } = 0.75f;
   
        // TODO use wrapper class for level editor
        [JsonIgnore] public Dictionary<Guid, Tile> TilesDict { get; set; } = new();
        public List<Tile> Tiles { get; set; }
        
        public void Bake()
        {
            Tiles = TilesDict.Values.OrderBy(x => x.StartBeat).ToList();
        }
        
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            TilesDict = Tiles.ToDictionary(x => x.Guid);
            
            if (AudioFileName == null)
            {
                AudioFileName = $"{Name}.wav"; // backwards compatibility
            }
        }
        
        [OnSerializing]
        private void OnSerializing(StreamingContext context) => Bake();
    }
}
