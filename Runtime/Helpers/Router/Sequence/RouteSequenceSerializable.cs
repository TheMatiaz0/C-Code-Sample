using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Honey;
using Telegraphist.Helpers.Scenes;
using UnityEngine;

namespace Telegraphist.Helpers.Router.Sequence
{
    public enum SequenceCallbackMode
    {
        Scene,
        PopToRoot
    }
    
    [Serializable]
    public class RouteSequenceSerializable
    {
        [SerializeField] private List<RouteSequenceItemSerializable> items;
        
        // [SerializeField] private SequenceCallbackMode callbackMode;
        //
        // [SerializeField, HoneyRun, HShowIf(nameof(IsModeScene))]
        // private SceneType finalScene;
        // [SerializeField, HoneyRun, HShowIf(nameof(IsModeScene))]
        // private bool finalSceneAsReplacement;
        //
        // private bool IsModeScene => callbackMode == SequenceCallbackMode.Scene;

        public void Run()
        {
            new RouteSequence(items.Cast<ICallbackInvokable>().ToArray())
                .WithCallback(Callback)
                .Run();
        }

        private void Callback()
        {
            // switch (callbackMode)
            // {
            //     case SequenceCallbackMode.Scene:
            //         GlobalRouter.Current.PushOrReplace(new LoadSceneArgsWithCallback(finalScene, null), finalSceneAsReplacement).Forget();
            //         break;
            //     case SequenceCallbackMode.PopToRoot:
            //         GlobalRouter.Current.PopUntilRoot().Forget();
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }
        }
    }
}