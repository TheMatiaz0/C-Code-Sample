using System.Collections.Generic;
using DG.Tweening;
using Telegraphist.Structures;
using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.Gameplay.QTE
{
    public class QtePointsDisplayer : MonoBehaviour
    {
        [SerializeField] private Transform pointsTextRoot;
        [SerializeField] private GameObject pointsTextPrefab;
        [SerializeField] private ParticleSystem textParticles;

        [Header("Text animation")] 
        [SerializeField] private List<Color> accuracyColors;
        [SerializeField] private List<int> accuracyParticleEmmision;
        [SerializeField] private float movingDuration = 1f;

        [SerializeField] private float moveYBy = 200f;
        [SerializeField] private float scalingDuration = 0.2f;
        [SerializeField] private float startScale = 1.2f;
        [SerializeField] private float endScale = 1;
        [SerializeField] private float fadeDuration = 1;
        [SerializeField] private float movingFadeDuration = 1;
        [SerializeField] private Ease fadeEase;

        private RectTransform previousRt;
        private Text previousText;
        private Sequence previousSequence;

        public void ShowPointsText(int points, AccuracyStatus accuracy, float durationMultiplier=1, Vector2? textPosition = null, bool killPreviousText = true)
        {
            if (!pointsTextRoot || !textParticles)
            {
                return;
            }
            
            var go = Instantiate(pointsTextPrefab, pointsTextRoot);
            
            var pos = textPosition ?? go.transform.position;
            go.transform.position = pos;
            textParticles.transform.position = pos;
            
            var text = go.GetComponent<Text>();
            var rt = go.GetComponent<RectTransform>();

            text.text = $"+{points}";
            text.color = accuracyColors[(int)accuracy];
            var main = textParticles.main;
            main.startColor = accuracyColors[(int)accuracy];
            var emmision = textParticles.emission;
            emmision.rateOverTime = accuracyParticleEmmision[(int)accuracy];
            textParticles.Play();

            if (previousText && killPreviousText)
            {
                previousSequence.Kill();
                previousRt.DOAnchorPosY(rt.anchoredPosition.y + moveYBy, movingDuration).SetLink(go);
                previousText.DOFade(0, movingFadeDuration).SetEase(fadeEase).SetLink(go);
            }

            previousRt = rt;
            previousText = text;
            previousSequence = AnimateText(text, durationMultiplier).Play();
        }

        private Sequence AnimateText(Text text, float durationMultiplier)
        {
            var seq = DOTween.Sequence().SetLink(text.gameObject)
                .Append(text.transform.DOScale(startScale, 0))
                .Append(text.transform.DOScale(endScale, scalingDuration*durationMultiplier))
                .Append(text.DOFade(0,fadeDuration*durationMultiplier).SetEase(fadeEase));
            return seq;
        }
    }
}