using DG.Tweening;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using Telegraphist.Utils;
using UnityEngine;

namespace Telegraphist.Gameplay.HandPaperVariant.Tiles
{
    public class PaperDot : PaperSymbolBase
    {
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private ParticleSystem correctParticles;

        public override Vector2 EndPosition => Root.localPosition;
        public override Vector2 StartPosition => Root.localPosition;

        protected override void Init()
        {
            sprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            sprite.color = Colors.ColorDefault;
        }

        public override void SetPhantom(bool initiallyVisible)
        {
            sprite.color = Colors.ColorPhantom;
            
            if (!initiallyVisible)
            {
                sprite.color = sprite.color.CopyWith((ref Color c) => c.a = 0);
            }
        }

        public override void SetStatus(StatusWithTile status)
        {
            var p = correctParticles.main;
            p.loop = false;

            sprite.color = Colors.GetFromStatus(status) ?? sprite.color;
        }
        
        public override Tween DOFade(float endValue, float duration) => 
            sprite.DOFade(Colors.ColorPhantom.a * endValue, duration);
    }
}