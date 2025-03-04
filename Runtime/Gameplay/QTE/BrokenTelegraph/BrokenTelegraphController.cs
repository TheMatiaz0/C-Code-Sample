using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Telegraphist.Structures;
using Telegraphist.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.Gameplay.QTE.BrokenTelegraph
{
    public sealed class BrokenTelegraphController : MonoBehaviour, IQteContent
    {
        [Header("Pre Indication")]
        [SerializeField] private MMF_Player preIndicationFeedbacks;
        [SerializeField] private MMF_Player preIndicationEndFeedbacks;
        [Header("Change elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CanvasGroup outsideCanvasGroup;
        [SerializeField] private SliderView progressSlider;
        [SerializeField] private ParticleSystem brokenParticles;
        [Header("Other elements")]
        [SerializeField] private Image fixImage;
        [SerializeField] private float fixImageStayDuration;
        [Header("Audio")] 
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip brokenTelegraphLoopSound;
        [SerializeField] private AudioClip brokenTelegraphPressSound;
        [SerializeField] private AudioClip fixedCorrectSound;
        [SerializeField] private AudioClip fixedTimeoutSound;

        private Sequence fixImageSequence;
        private bool hideHud;

        public QteContent ContentType => QteContent.BrokenTelegraph;
        public Transform Root => this.transform;

        private void Start()
        {
            SetElementsActive(false);
            fixImage.DOFade(0,0);
        }
        
        public void OnDirectorPreIndication()
        {
            preIndicationFeedbacks.PlayFeedbacks();
        }

        public void OnDirectorEnter()
        {
            hideHud = true;
            StartBrokenTelegraph();
        }

        public void OnDirectorExit(bool isQtePassed)
        {
            if (isQtePassed)
            {
                BrokenTelegraphSuccess();
            }
            else
            {
                BrokenTelegraphTimeout();
            }

            hideHud = false;
        }

        public void OnDirectionEnter(QteDirection direction)
        {
        }

        public void OnDirectionPress(AccuracyStatus accuracyStatus)
        {
            UpdateSmashCount();
        }

        public void StartBrokenTelegraph(int requiredPresses)
        {
            if (progressSlider != null)
            {
                progressSlider.Initialize(requiredPresses);
            }

            StartBrokenTelegraph();
        }

        public void StartBrokenTelegraph()
        {
            preIndicationFeedbacks.StopFeedbacks();
            preIndicationEndFeedbacks.PlayFeedbacks();
            audioSource.clip = brokenTelegraphLoopSound;
            audioSource.loop = true;

            SetElementsActive(true);
        }

        public void BrokenTelegraphSuccess()
        {
            SetElementsActive(false);
            audioSource.PlayOneShot(fixedCorrectSound);
        }

        public void BrokenTelegraphTimeout()
        {
            SetElementsActive(false);
            audioSource.PlayOneShot(fixedTimeoutSound);
        }

        public void UpdateSmashCount()
        {
            AnimateFixImage();
            audioSource.PlayOneShot(brokenTelegraphPressSound);
        }

        public void UpdateSmashCount(int smashCount)
        {
            if (progressSlider)
            {
                progressSlider.UpdateValue(smashCount);
            }

            UpdateSmashCount();
        }

        private void AnimateFixImage()
        {
            fixImageSequence.Kill();
            fixImageSequence = DOTween.Sequence().SetLink(fixImage.gameObject)
                .Append(fixImage.DOFade(1,0))
                .AppendInterval(fixImageStayDuration)
                .Append(fixImage.DOFade(0,0));
            fixImageSequence.Play();
        }

        private void SetElementsActive(bool active)
        {
            if (progressSlider)
            {
                progressSlider.gameObject.SetActive(active && !hideHud);
            }

            canvasGroup.alpha = active && !hideHud ? 1 : 0;
            outsideCanvasGroup.alpha = active && hideHud ? 1 : 0;

            if (active)
            {
                audioSource.Play();
                brokenParticles.Play();
            }
            else
            {
                fixImageSequence.Kill();
                fixImage.DOFade(0, 0);
                audioSource.Stop();
                brokenParticles.Stop();
            }
        }
    }
}