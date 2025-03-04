using System;
using Telegraphist.Lifecycle;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Telegraphist.Input
{
    public class SkippableInputReceiver : SceneSingleton<SkippableInputReceiver>
    {
        private Subject<bool> skipSubject = new();
        private Subject<Unit> inputSubject = new();

        public IObservable<bool> OnSkipHeld => skipSubject;
        public IObservable<Unit> OnInputDetected => inputSubject;

        #region Controls [SendMessage()]

        public void OnSkip(InputValue value)
        {
            skipSubject.OnNext(value.isPressed);
        }

        public void OnFirstInput()
        {
            inputSubject.OnNext(Unit.Default);
        }

        #endregion
    }
}
