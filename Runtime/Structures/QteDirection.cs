using System;
using UnityEngine;

namespace Telegraphist.Structures
{
    public enum QteDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public static class QteDirectionExtension
    {
        private static readonly Vector2Int[] directionVectors = new Vector2Int[]
        {
            Vector2Int.zero,
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        private static readonly Vector2Int[] counterDirectionVectors = new Vector2Int[]
        {
            Vector2Int.zero,
            Vector2Int.down,
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.left,
        };
        
        private static readonly float[] directionRotations = new float[]
        {
            0,
            0,
            180,
            90,
            -90,
        };

        public static Vector2Int ToCounterVector(this QteDirection direction)
        {
            return counterDirectionVectors[(int)direction];
        }

        public static Vector2Int ToVector(this QteDirection direction)
        {
            return directionVectors[(int)direction];
        }
        
        public static float ToRotationZ(this QteDirection direction)
        {
            return directionRotations[(int)direction];
        }
    }

    [Flags]
    public enum QteLayout
    {
        None = 0,
        Horizontal = 1 << 0,
        Vertical = 1 << 1,
        Both = Horizontal | Vertical,
    }
}
