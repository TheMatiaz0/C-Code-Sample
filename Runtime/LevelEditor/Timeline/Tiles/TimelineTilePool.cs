using UniRx.Toolkit;
using UnityEngine;

namespace Telegraphist.LevelEditor.Timeline.Tiles
{
    public class TimelineTilePool : ObjectPool<TimelineTile>
    {
        private TimelineTile prefab;
        private Transform parent;

        public TimelineTilePool(TimelineTile prefab, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
        }
        
        protected override TimelineTile CreateInstance()
        {
            var tile = Object.Instantiate(prefab, parent);
            return tile;
        }

        protected override void OnBeforeReturn(TimelineTile instance)
        {
            base.OnBeforeReturn(instance);
            instance.Rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
        }
    }
}