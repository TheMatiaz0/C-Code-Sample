using System;
using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Router;
using UnityEngine;

namespace Telegraphist.UI.Buttons
{
    public class ButtonPopRoute : ActionButton
    {
        [SerializeField] private int popCount = 1;
        protected override void OnButtonClick()
        {
            PopCount().Forget();
        }

        private async UniTaskVoid PopCount()
        {
            for (int i = 0; i < popCount; i++)
            {
                await GlobalRouter.Current.Pop();
            }
        }
    }
}