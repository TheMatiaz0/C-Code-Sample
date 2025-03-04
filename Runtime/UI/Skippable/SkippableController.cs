using System;
using DG.Tweening;
using Telegraphist.Input;
using Telegraphist.Lifecycle;
using Telegraphist.Pauseable;
using Telegraphist.UI.Menus;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.UI.Skippable
{
    public class SkippableController : SceneSingleton<SkippableController>, IPauseable
    {
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        [SerializeField] private Image fillProgressImage;

        [SerializeField] private float startFlashDuration = 1f;

        [SerializeField] private float inputFlashDuration = 2.5f;

        [SerializeField] private float flashLoopDecay = 0.5f;

        [SerializeField] private bool flashDoLoop = true;

        [SerializeField] private float holdDurationRequired = 2;

        [SerializeField] private float minFade = 0.5f;

        [SerializeField] private bool visibleOnStart = false;

        private Sequence flashSequence;
        private Sequence flashLoopSequence;
        private Tween holdTween;
        private Tween fadeTween;
        private ISkippable skippable;
        private CompositeDisposable disposables = new();

        protected override void Awake()
        {
            base.Awake();
            fillProgressImage.fillAmount = 0;
            fadeCanvasGroup.alpha = 0f;
        }

        // TODO: add stack implementation for Skippable history
        public IDisposable Register(ISkippable skippable)
        {
            ResetController();
            disposables = new();

            this.skippable = skippable;
            SkippableInputReceiver.Current.OnSkipHeld.Subscribe(Skip).AddTo(disposables);
            SkippableInputReceiver.Current.OnInputDetected.Subscribe(_ => InputFlash()).AddTo(disposables);
            disposables.AddTo(this);

            if (visibleOnStart)
            {
                FlashAtStart();
            }

            if (PauseMenuController.Current)
            {
                PauseMenuController.Current.Register(this);
            }

            return disposables;
        }

        private void FlashAtStart()
        {
            StartFlash(startFlashDuration);
        }

        private void InputFlash()
        {
            StartFlash(inputFlashDuration);
        }

        private void Skip(bool isHolding)
        {
            if (isHolding)
            {
                flashSequence.Pause();
                flashLoopSequence.Pause();
                fadeTween = FadeTween(1, flashLoopDecay);
                holdTween = fillProgressImage.DOFillAmount(1, holdDurationRequired)
                    .SetEase(Ease.Linear)
                    .SetLink(this.gameObject)
                    .OnComplete(FinishSkip);
            }
            else
            {
                fadeTween.Kill();
                holdTween.Kill();
                fillProgressImage.fillAmount = 0;

                flashSequence.Play();
                flashLoopSequence.Play();
            }
        }

        private void FinishSkip()
        {
            ResetController();
            skippable?.Skip();
            if (PauseMenuController.Current != null)
            {
                PauseMenuController.Current.Unregister(this);
            }
        }

        private void OnDestroy()
        {
            if (PauseMenuController.Current != null)
            {
                PauseMenuController.Current.Unregister(this);
            }
        }

        private void ResetController()
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }

            fadeTween.Kill();
            holdTween.Kill();
            flashSequence.Kill();
            flashLoopSequence.Kill();
            fillProgressImage.fillAmount = 0;
            fadeCanvasGroup.alpha = 0f;
        }

        private void StartFlash(float duration)
        {
            if (flashSequence.IsActive())
            {
                flashSequence.Kill();
                flashLoopSequence.Kill();
            }

            flashSequence = DOTween.Sequence();
            flashSequence.InsertCallback(0, () => LoopFlash())
                .AppendInterval(duration)
                .AppendCallback(() => flashLoopSequence.Kill(true))
                .Append(FadeTween(0, flashLoopDecay / 2))
                .SetLink(this.gameObject);
        }

        private void LoopFlash()
        {
            if (flashLoopSequence.IsActive())
            {
                flashLoopSequence.Kill();
            }

            flashLoopSequence = DOTween.Sequence().SetLink(this.gameObject);;

            if (flashDoLoop)
            {
                flashLoopSequence.Insert(0, FadeTween(1, flashLoopDecay / 2))
                    .Append(FadeTween(minFade, flashLoopDecay / 2))
                    .SetLoops(-1);
            }
            else
            {
                flashLoopSequence.Insert(0, FadeTween(1, flashLoopDecay / 2));
            }
        }

        private Tween FadeTween(float fade, float duration)
        {
            return fadeCanvasGroup.DOFade(fade, duration);
        }

        public void Pause()
        {
            ResetController();
        }

        public void Resume()
        {
            Register(skippable);
        }
    }
}