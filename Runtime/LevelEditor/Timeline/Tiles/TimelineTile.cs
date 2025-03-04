using Telegraphist.LevelEditor.Timeline.Tools;
using Telegraphist.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Telegraphist.LevelEditor.Timeline.Tiles
{
    public class TimelineTile : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerUpHandler
    {
        [SerializeField] private GameObject border;
        [SerializeField] private TimelineTileResizeHandle leftHandle;
        [SerializeField] private TimelineTileResizeHandle rightHandle;
        
        public TimelineTileBuilder TileBuilder { get; set; }
        public RectTransform Rt { get; private set; }
        public RawImage RawImage { get; private set; }
        public bool IsResizing { get; set; }
        
        private Timeline timeline;
        private TimelineToolsController toolsController;
        private Selectable selectable;

        private void Awake()
        {
            Rt = GetComponent<RectTransform>();
            RawImage = GetComponent<RawImage>();
            selectable = GetComponent<Selectable>();
        }

        // we need to use OnEnable due to object pooling
        private void OnEnable()
        {
            leftHandle.gameObject.SetActive(false);
            rightHandle.gameObject.SetActive(false);
            EnableBorder(false);
            EnableRaycast(true);
        }

        public void Setup(Timeline timeline, TimelineToolsController toolsController, TimelineTileBuilder tileBuilder)
        {
            this.timeline = timeline;
            this.toolsController = toolsController;
            TileBuilder = tileBuilder;

            Rt.anchorMin = Vector2.zero;
            Rt.anchorMax = Vector2.zero;
            Rt.pivot = Vector2.zero;
        }
        
        public void EnableRaycast(bool allow)
        {
            RawImage.raycastTarget = allow;
            selectable.interactable = allow;
        }

        public void EnableBorder(bool active)
        {
            border.SetActive(active);
        }

        #region Pointer event handlers
        public void OnPointerDown(PointerEventData eventData)
        {
            toolsController.ActiveTool.OnTilePointerDown(eventData, this);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            toolsController.ActiveTool.OnTilePointerUp(eventData, this);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            toolsController.ActiveTool.OnTilePointerClick(eventData, this);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            toolsController.ActiveTool.OnTilePointerEnter(eventData, this);
            
            leftHandle.gameObject.SetActive(true);
            rightHandle.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (IsResizing) return;
            
            toolsController.ActiveTool.OnTilePointerExit(eventData, this);
            
            leftHandle.gameObject.SetActive(false);
            rightHandle.gameObject.SetActive(false);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            toolsController.ActiveTool.OnTileBeginDrag(eventData, this);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            toolsController.ActiveTool.OnTileDrag(eventData, this);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            toolsController.ActiveTool.OnTileEndDrag(eventData, this);
        }
        
        public void OnHandleBeginDrag(PointerEventData eventData, HandleSide side)
        {
            toolsController.ActiveTool.OnTileHandleBeginDrag(eventData, this, side);
        }

        public void OnHandleDrag(PointerEventData eventData, HandleSide side)
        {
            toolsController.ActiveTool.OnTileHandleDrag(eventData, this, side);
        }

        public void OnHandleEndDrag(PointerEventData eventData, HandleSide side)
        {
            toolsController.ActiveTool.OnTileHandleEndDrag(eventData, this, side);
        }
        #endregion
    }
}
