using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.LevelEditor.Timeline.Grid
{
    public static class GridTextureUtils
    {
        public static Texture2D GenerateGridTexture(int width, int height, int borderSize)
        {
            var tex = new Texture2D(width, height, TextureFormat.Alpha8, false);
            
            tex.SetPixels(0, 0, width, height, Enumerable.Repeat(Color.clear, width * height).ToArray());
            
            tex.SetPixels(0, 0, width, borderSize, Enumerable.Repeat(Color.white, width * borderSize).ToArray()); // top
            tex.SetPixels(0, height - borderSize, width, borderSize, Enumerable.Repeat(Color.white, width * borderSize).ToArray()); // bottom
            tex.SetPixels(0, 0, borderSize, height, Enumerable.Repeat(Color.white, height * borderSize).ToArray()); // left
            tex.SetPixels(width - borderSize, 0, borderSize, height, Enumerable.Repeat(Color.white, height * borderSize).ToArray()); // right;
            
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            return tex;
        }
        
        public static Texture2D GenerateGridTextureWithDivider(int cellWidth, int height, int borderSize, int chunkColumns, Color cellColor, Color dividerColor)
        {
            var width = cellWidth * chunkColumns;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            tex.SetPixels(0, 0, width, height, Enumerable.Repeat(Color.clear, width * height).ToArray());

            for (var i = 0; i < chunkColumns; i++)
            {
                tex.SetPixels(i * cellWidth, 0, cellWidth, borderSize, Enumerable.Repeat(cellColor, width * borderSize).ToArray()); // top
                tex.SetPixels(i * cellWidth, height - borderSize, cellWidth, borderSize, Enumerable.Repeat(cellColor, width * borderSize).ToArray()); // bottom
                tex.SetPixels(i * cellWidth, 0, borderSize, height, Enumerable.Repeat(cellColor, height * borderSize).ToArray()); // left
                tex.SetPixels((i + 1) * cellWidth - borderSize, 0, borderSize, height, Enumerable.Repeat(cellColor, height * borderSize).ToArray()); // right;
            }
            
            // left part of divider
            tex.SetPixels(0, 0, borderSize, height, Enumerable.Repeat(dividerColor, height * borderSize).ToArray());
            // right part of divider
            tex.SetPixels(width - borderSize, 0, borderSize, height, Enumerable.Repeat(dividerColor, height * borderSize).ToArray());
            
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            return tex;
        }

        public static void DrawGridTextureWithDivider(RawImage rawImage, int cellWidth, int cellHeight, int borderSize, 
            int chunkColumns, int totalColumns, int rows, Color cellColor, Color dividerColor)
        {
            var tex = GenerateGridTextureWithDivider(cellWidth, cellHeight, borderSize, chunkColumns, cellColor, dividerColor);
            rawImage.texture = tex;
            rawImage.uvRect = new Rect(0, 0, (float)totalColumns / chunkColumns, rows);
        }
    }
}
