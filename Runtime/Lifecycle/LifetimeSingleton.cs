using System;
using UnityEngine;

namespace Telegraphist.Lifecycle
{
    public abstract class LifetimeSingleton : MonoBehaviour
    {
        public abstract void Setup(); 
        public abstract int Priority { get; }
    }
    
    /// <summary>
    /// Important: add this script to LifetimeSingletonsPack prefab
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LifetimeSingleton<T> : LifetimeSingleton where T : LifetimeSingleton<T>
    {
        public static T Current { get; private set; }
        
        public override int Priority => 0;
        
        [Obsolete("Use Setup instead. This method will still be called even if another singleton instance exists.")]
        protected void Awake() { }
        
        public override void Setup()
        {
            if (Current)
            {
                Debug.LogError($"{GetType().Name} singleton already exists, this shouldn't happen!");
                return;
            }
            
            Current = (T)this;
        }
    }
}