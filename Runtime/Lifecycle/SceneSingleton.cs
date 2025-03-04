using Telegraphist.Lifecycle;
using Telegraphist.Utils;
using UnityEngine;

namespace Telegraphist.Lifecycle
{
    public class SceneSingleton<T> : MonoBehaviour where T : SceneSingleton<T>
    {
        public static T Current { get; private set; }
        protected bool IsCurrent => Current == this;
        
        protected virtual void Awake()
        {
            if (!IsCurrent && Current != null && Current.gameObject.activeInHierarchy)
            {
                Destroy(this);
                Debug.Log(
                    $"{typeof(T).Name} singleton already exists, destroying. Current = {Current.transform.GetPath()}, this = {transform.GetPath()}.", this);
            }
            else
            {
                Current = (T)this;
            }
        }

    }
}