using System.Linq;
using UnityEngine;

namespace Telegraphist.Lifecycle
{
    public class LifetimeSingletonManager : MonoBehaviour
    {
        public static LifetimeSingletonManager Current { get; private set; }
        
        private void Awake()
        {
            if (Current)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
            DontDestroyOnLoad(gameObject);
            
            Setup();
        }

        private void Setup()
        {
            var singletons = GetComponentsInChildren<LifetimeSingleton>();
            foreach (var singleton in singletons.OrderByDescending(x => x.Priority))
            {
                singleton.Setup();
            }
        }
    }
}