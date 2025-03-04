using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.UI.Buttons
{
    [RequireComponent(typeof(Button))]
    public abstract class ActionButton : MonoBehaviour
    {
        public IObservable<Unit> OnClick => onClick;
        
        private Subject<Unit> onClick = new();
        
        protected virtual void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ButtonClick);
        }

        [Button("Test Button")]
        private void ButtonClick()
        {
            OnButtonClick();
            onClick.OnNext(Unit.Default);
        }
        
        protected abstract void OnButtonClick();
    }
}