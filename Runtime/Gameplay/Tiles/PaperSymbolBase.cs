using DG.Tweening;
using Telegraphist.TileSystem;
using UnityEngine;

namespace Telegraphist.Gameplay.HandPaperVariant.Tiles
{
    public abstract class PaperSymbolBase : MonoBehaviour
    {
        protected Transform Root { get; private set; }
        protected PaperTileData Data { get; private set; }
        protected PaperTileColors Colors { get; private set; }

        public void Init(Transform root, PaperTileData data, PaperTileColors colors)
        {
            (Root, Data, Colors) = (root, data, colors);
            Init();
        }

        protected abstract void Init();
        
        public abstract Vector2 EndPosition { get; }
        public abstract Vector2 StartPosition { get; }
        public abstract void SetPhantom(bool initiallyVisible);
        public abstract void SetStatus(StatusWithTile status);
        public abstract Tween DOFade(float endValue, float duration);
        public virtual void SetHoldProgress(float progress) { }
        public virtual void SetLetterCompleted() { }
    }
}