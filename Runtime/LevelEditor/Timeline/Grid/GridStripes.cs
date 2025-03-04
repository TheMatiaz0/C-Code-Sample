using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.LevelEditor.Timeline.Grid
{
    [RequireComponent(typeof(RawImage))]
    public class GridStripes : MonoBehaviour
    {
        [SerializeField] private Color color1, color2;

        private RawImage rawImage;

        private void Awake()
        {
            rawImage = GetComponent<RawImage>();
        }

        private void Start()
        {
            DrawStripedBackground(Timeline.Current.RowCount);
        }

        public void DrawStripedBackground(int rowCount)
        {
            var texture = new Texture2D(1, rowCount, TextureFormat.RGB24, false);
            for (var y = 0; y < rowCount; y++)
            {
                var color = y % 2 == 0 ? color1 : color2;
                texture.SetPixel(0, y, color);
            }
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();

            rawImage.texture = texture;
        }
    }
}