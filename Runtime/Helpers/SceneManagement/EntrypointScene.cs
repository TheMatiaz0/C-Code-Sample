using System;
using Telegraphist.Helpers.Router;
using Telegraphist.Helpers.Router.Sequence;
using UnityEngine;

namespace Telegraphist.Helpers.Scenes
{
    public class EntrypointScene : MonoBehaviour
    {
        [SerializeField] private RouteSequenceSerializable routeSequence;
        
        
        private void Awake()
        {
            routeSequence.Run();
        }
    }
}