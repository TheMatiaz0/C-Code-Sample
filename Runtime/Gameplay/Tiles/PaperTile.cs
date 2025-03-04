using DG.Tweening;
using Telegraphist.Gameplay.TileSpawner;
using Telegraphist.Scriptables;
using Telegraphist.TileSystem;
using UnityEngine;

namespace Telegraphist.Gameplay.HandPaperVariant.Tiles
{
    public class PaperTile : TileObjectBase
    {
        [SerializeField] private PaperDot dot;
        [SerializeField] private PaperDash dash;
        [SerializeField] private ParticleSystem correctParticles;
        
        private PaperSymbolBase symbol;
        private PaperTileData data;
        
        public void Init(Tile tile, Vector2 localPosition, Vector2 localScale)
        {
            foreach (var x in new PaperSymbolBase[] { dot, dash })
            {
                x.gameObject.SetActive(false);
            }

            transform.localPosition = localPosition;
            
            data = new PaperTileData(localScale.x);

            symbol = BalanceScriptable.Current.IsTileLong(tile) ? dash : dot;
            symbol.Init(transform, data, Colors);
            symbol.gameObject.SetActive(true);
            
            base.Init(tile);
        }

        public void SetPhantom(bool initiallyVisible)
        {
            gameObject.name = "Phantom tile";
            symbol.SetPhantom(initiallyVisible);
        }
        
        public void PhantomFadeIn(float duration, Ease ease) => symbol.DOFade(1, duration).SetEase(ease).SetLink(gameObject);

        public void PhantomFadeOut(float duration, Ease ease) => symbol.DOFade(0, duration).SetEase(ease).SetLink(gameObject);

        public Vector2 GetEndPosition() => symbol.EndPosition;
        public Vector2 GetStartPosition() => symbol.StartPosition;
        public Vector2 GetMiddlePosition() => (symbol.StartPosition+symbol.EndPosition)/2;
        public Transform GetSymbol() => symbol.transform;

        public void SetLetterCompleted() => symbol.SetLetterCompleted();
        
        public override void SetStatus(StatusWithTile status)
        {
            symbol.SetStatus(status);

            if (status is StatusPressStarted s && s.IsPositive())
            {
                correctParticles.Play();
            }
        }

        public override void SetHoldProgress(float progress)
        {
            symbol.SetHoldProgress(progress);
        }
    }
}
