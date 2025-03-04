using System;
using System.Collections.Generic;
using System.Linq;
using Cyberultimate.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Telegraphist.LevelEditor.Timeline.Tiles
{
    [Serializable]
    public enum HandleSide
    {
        Left,
        Right
    }
    public class TimelineTileResizeHandle : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TimelineTile tile;
        [SerializeField] private HandleSide side;
        [SerializeField] private GameObject invisibleArea;
        [SerializeField] private List<SerializedTuple<float,float>> tileWidthToXScale;

        public void OnBeginDrag(PointerEventData eventData)
        {
            tile.OnHandleBeginDrag(eventData, side);
        }

        public void OnDrag(PointerEventData eventData)
        {
            tile.OnHandleDrag(eventData, side);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            tile.OnHandleEndDrag(eventData, side);
            invisibleArea.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            invisibleArea.SetActive(true);

            tile.OnPointerDown(eventData);
        }

        private void OnEnable()
        {
            var scaleTuple = tileWidthToXScale.FirstOrDefault(x => tile.Rt.rect.width < x.X);
            transform.localScale = new Vector3(scaleTuple.Y, transform.localScale.y, transform.localScale.z);
        }
    }
}
