using System.Collections.Generic;
using DG.Tweening;
using Telegraphist.Gameplay.QTE;
using Telegraphist.Scriptables;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.Gameplay.TileInput
{
    // TODO: Remove all GetComponent, use local components inside referenced and cache everything into it
    public class StatusTextDisplayer : MonoBehaviour
    {
        [SerializeField] private GameObject statusTextRoot;
        [SerializeField] private GameObject statusTextPrefab;
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

        private void Start()
        {
            MessageBroker.Default.Receive<TileInputStatus>()
                .Subscribe(OnTileStatusChange)
                .AddTo(this);
            
            MessageBroker.Default.Receive<QteInputStatus>()
                .Subscribe(OnQteStatusChange)
                .AddTo(this);
        }

        private void OnTileStatusChange(TileInputStatus status)
        {
            // Short Tile OR Long tile and we don't count points from Tile end 
            if (status is StatusPressEnded statusEnd
                && (!BalanceScriptable.Current.IsTileLong(statusEnd.Tile)
                    || BalanceScriptable.Current.LongTileScoring != BalanceScriptable.LongTileScoringMode.StartAndEnd))
            {
                return;
            }

            if (status is StatusWithAccuracy { Accuracy: AccuracyStatus.Invalid }) return;
            if (status is StatusDummy) return;

            /*
            var prefix = status switch
            {
                StatusPressStarted => "Start: ",
                StatusPressEnded => "End: ",
                _ => ""
            };

            var suffix = status switch
            {
                StatusWithAccuracy a => $" {status.GetContext()}",
                _ => ""
            };
            */

            var accuracy = status switch
            {
                StatusWithAccuracy s => s.Accuracy,
                _ => AccuracyStatus.Invalid
            };
            
            var accuracyString = status.GetAccuracyString();

            ShowStatusText($"{accuracyString}", accuracy);
        }
        
        private void OnQteStatusChange(QteInputStatus status)
        {
            //if (status.Accuracy == AccuracyStatus.Invalid) return;
            
            var accuracyString = status.Accuracy.GetAccuracyString();

            ShowStatusText($"{accuracyString}", status.Accuracy);
        }

        private void ShowStatusText(string message, AccuracyStatus accuracyStatus)
        {
            // TODO: Implement object pooling if this will get out of hand
            var go = Instantiate(statusTextPrefab, statusTextRoot.transform);
            var text = go.GetComponent<Text>();
            var rt = go.GetComponent<RectTransform>();

            text.text = message;
            text.color = accuracyColors[(int)accuracyStatus];
            var main = textParticles.main;
            main.startColor = accuracyColors[(int)accuracyStatus];
            var emmision = textParticles.emission;
            emmision.rateOverTime = accuracyParticleEmmision[(int)accuracyStatus];
            textParticles.Play();

            if (previousText)
            {
                previousSequence.Kill();
                previousRt.DOAnchorPosY(rt.anchoredPosition.y + moveYBy, movingDuration).SetLink(go);
                previousText.DOFade(0, movingFadeDuration).SetEase(fadeEase).SetLink(go);
            }

            previousRt = rt;
            previousText = text;
            previousSequence = AnimateText(text, go).Play();
        }

        private Sequence AnimateText(Text text, GameObject textObject)
        {
            var seq = DOTween.Sequence().SetLink(text.gameObject)
                .Append(text.transform.DOScale(startScale, 0))
                .Append(text.transform.DOScale(endScale, scalingDuration))
                .Append(text.DOFade(0,fadeDuration).SetEase(fadeEase))
                .AppendCallback(() => Destroy(textObject));
            return seq;
        }
    }
}