using DG.Tweening;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using Telegraphist.Utils;
using UnityEngine;
using UnityEngine.U2D;

namespace Telegraphist.Gameplay.HandPaperVariant.Tiles
{
    public class PaperDash : PaperSymbolBase
    {
        private const float MinSplineDistance = 0.1f;
        
        [SerializeField] private float dashHeight = 1;
        [SerializeField] private SpriteShapeRenderer backgroundRenderer, fillRenderer;
        [SerializeField] private SpriteShapeController backgroundShape, fillShape;
        [SerializeField] private ParticleSystem correctParticles;

        public override Vector2 EndPosition =>
            (Vector2)Root.localPosition + new Vector2(Data.Width * Root.localScale.x, 0);
        public override Vector2 StartPosition => (Vector2)Root.localPosition;

        protected override void Init()
        {
            InitSpriteShape(backgroundShape, Data.Width);
            InitSpriteShape(fillShape, Data.Width);
            fillShape.spline.SetPosition(1, new Vector2(MinSplineDistance, 0));

            fillRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            backgroundRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            backgroundRenderer.color = Colors.ColorDefault;
            fillRenderer.color = Color.clear;
        }
        
        private void InitSpriteShape(SpriteShapeController shape, float scale)
        {
            shape.spline.SetPosition(0, Vector2.zero);
            shape.spline.SetPosition(1, new Vector2(scale / shape.transform.localScale.x, 0));
            shape.spline.SetHeight(0, dashHeight);
            shape.spline.SetHeight(1, dashHeight);
        }

        public override void SetStatus(StatusWithTile status)
        {
            var p = correctParticles.main;
            if (status is StatusPressStarted s && s.IsPositive())
            {
                p.loop = true;
                correctParticles.Play();
            }
            else
            {
                p.loop = false;
                correctParticles.Stop();
            }

            // not using GetFromStatus because we only want to change the background color in two cases
            backgroundRenderer.color = status switch
            {
                StatusMissed => Colors.ColorMissed,
                StatusWithAccuracy { Accuracy: AccuracyStatus.Invalid } => Colors.ColorInvalid,
                _ => backgroundRenderer.color
            };
        }

        public override void SetHoldProgress(float progress)
        {
            fillRenderer.color = Colors.ColorInProgress;
            
            var x = (progress * Data.Width) / fillShape.transform.localScale.x;
            x = Mathf.Max(x, MinSplineDistance);

            fillShape.spline.SetPosition(1, new Vector2(x, 0));
            SetupPositionToHoldProgress(progress);
        }

        private void SetupPositionToHoldProgress(float progress)
        {
            float posX = Mathf.Lerp(transform.position.x, EndPosition.x + 0.4f, progress);

            var correctParticlesPos = correctParticles.transform.position;
            correctParticlesPos.x = posX;
            correctParticles.transform.position = correctParticlesPos;
        }

        public override Tween DOFade(float endValue, float duration) =>
            backgroundRenderer.DOFade(Colors.ColorPhantom.a * endValue, duration);

        public override void SetPhantom(bool initiallyVisible)
        {
            backgroundRenderer.color = Colors.ColorPhantom;
            fillRenderer.color = Color.clear;

            if (!initiallyVisible)
            {
                backgroundRenderer.color = backgroundRenderer.color.CopyWith((ref Color c) => c.a = 0);
            }
        }
    }
}