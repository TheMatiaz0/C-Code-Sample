using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.LevelEditor.Timeline.Grid
{
    public class GridBackground : MonoBehaviour
    {
        [SerializeField] private RawImage grid;
        [SerializeField] private RawImage cursorGrid;
        [SerializeField] private Color gridCellBorderColor, gridDividerColor, 
            cursorGridCellBorderColor, cursorGridDividerColor;
        [SerializeField] private int gridBorderSize = 1;
        [SerializeField] private int dividerEveryBeats = 4;

        public void DrawGrid(int cellWidth, float cellHeight, Vector2Int gridSize)
        {
            var cursorGridRect = cursorGrid.GetComponent<RectTransform>().rect;

            int cursorGridHeight = Mathf.RoundToInt(cursorGridRect.height);
            int cellHeightInt = Mathf.RoundToInt(cellHeight);

            GridTextureUtils.DrawGridTextureWithDivider(grid, cellWidth, cellHeightInt, gridBorderSize, 
                dividerEveryBeats, gridSize.x, gridSize.y, 
                gridCellBorderColor, gridDividerColor);
            
            GridTextureUtils.DrawGridTextureWithDivider(cursorGrid, cellWidth, cursorGridHeight, gridBorderSize, 
                dividerEveryBeats, gridSize.x, 1, 
                cursorGridCellBorderColor, cursorGridDividerColor);
        }
    }
}