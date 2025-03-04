using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Telegraphist.Helpers.Provider
{
    public class HierarchyProvider : MonoBehaviour, IProvider
    {
        private Dictionary<Type, IInjectable> injectables;
        
        private void Awake()
        {
            var objects = GetComponentsInChildren<IInjectable>(true);
            injectables = objects.ToDictionary(x => x.GetType());

            foreach (var injectable in injectables.Values)
            {
                injectable.Populate(this);
            }
        }
        
        public T Get<T>() where T : IInjectable
        {
            return (T)injectables[typeof(T)];
        }

        public List<T> GetAll<T>() where T : IInjectable
        {
            var interfaceType = typeof(T);
            return injectables
                .Where(kvp => interfaceType.IsAssignableFrom(kvp.Key))
                .Select(kvp => (T)kvp.Value)
                .ToList();
        }
    }
}