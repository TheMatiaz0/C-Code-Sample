using DG.Tweening;
using Telegraphist.Input;
using Telegraphist.Utils;
using UniRx;
using UnityEngine;
using Telegraphist.UI;
using Telegraphist.UI.DebugUtils;

namespace Telegraphist.Gameplay.HandPaperVariant
{
    public class HandTelegraphPresser : MonoBehaviour
    {
        [SerializeField] private Transform releasedState, pressedState;
        [SerializeField] private float duration = 0.1f;
        
        private void Start()
        {
            transform.DOLocalTransform(releasedState, 0);
            
            ToggleUIPresses(false);
            
            releasedState.gameObject.SetActive(false);
            pressedState.gameObject.SetActive(false);
            
            GameInputHandler.Current.OnPressStarted
                .Subscribe(_ => ChangeState(pressedState)).AddTo(this);
            GameInputHandler.Current.OnPressEnded
                .Subscribe(_ => ChangeState(releasedState)).AddTo(this);
        }

        private void OnEnable() => ToggleUIPresses(false);

        private void OnDisable() => ToggleUIPresses(true);

        private void ChangeState(Transform target)
        {
            transform.DOLocalTransform(target, duration).SetLink(gameObject);
        }

        private void ToggleUIPresses(bool active)
        {
            if (ShowPresses.Current)
            {
                ShowPresses.Current.gameObject.SetActive(active);  
            }
        }
    }
}