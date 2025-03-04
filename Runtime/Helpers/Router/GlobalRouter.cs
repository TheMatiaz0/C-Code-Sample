using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Router.Segments;
using Telegraphist.Lifecycle;
using UniRx;
using UnityEngine;

namespace Telegraphist.Helpers.Router
{
    public class GlobalRouter : LifetimeSingleton<GlobalRouter>
    {
        public IObservable<List<IRouteSegment>> OnChange => onChange;
        public IReadOnlyList<IRouteSegment> History => routerHistory.ToList();
        public IRouteSegment CurrentSegment => routerHistory.Peek();
        public string Path => string.Join(" -> ", routerHistory.Select(item => item.RouteName).Reverse());
        
        private bool CanPop => routerHistory.Count > 1;
        
        private Stack<IRouteSegment> routerHistory = new();
        private Subject<List<IRouteSegment>> onChange = new();
        
        public async UniTask Push(IRouteSegment item)
        {
            if (!item.CanEnter())
            {
                return;
            }
            
            if (routerHistory.TryPeek(out var previous))
            {
                previous.OnBlur().Forget();
            }

            routerHistory.Push(item);
            NotifyChange();
            
            await item.OnEnter();
        }
        
        public async UniTask PushNoHistory(IRouteSegment item)
        {
            // TODO proper push without history working only for scenes
            // await Push(new TempRouteSegment(item, this));
            await Push(item);
        }
        
        public async UniTask Pop()
        {
            if (!CanPop) return;
            
            var previous = routerHistory.Pop();
            previous.OnExit().Forget();
            NotifyChange();
            
            if (routerHistory.TryPeek(out var item))
            {
                await item.OnFocus();
            }
        }
        
        public async UniTask Replace(IRouteSegment item)
        {
            if (!item.CanEnter())
            {
                return;
            }
            
            if (routerHistory.TryPop(out var previous))
            {
                previous.OnExit().Forget();
            }
            
            routerHistory.Push(item);
            NotifyChange();
            
            await item.OnEnter();
        }

        public async UniTask PushOrReplace(IRouteSegment item, bool replace)
        {
            if (replace) 
            {
                await Replace(item);
            } 
            else
            {
                await Push(item);
            }
        }
        
        public async UniTask ReplaceTopLevel(IRouteSegment item)
        {
            while (CanPop)
            {
                var previous = routerHistory.Pop();
                previous.OnExit().Forget();
            }

            var top = routerHistory.Peek();
            if (item.Equals(top))
            {
                item = top;
            }
            else
            {
                routerHistory.Push(item);
            }

            NotifyChange();
            
            await item.OnEnter();
        }
        
        public async UniTask PopUntil(Func<IRouteSegment, bool> predicate)
        {
            var item = Dig(predicate);
            if (item != null)
            {
                await item.OnFocus();
            }
        }
        
        public async UniTask PopUntilInclusive(Func<IRouteSegment, bool> predicate)
        {
            var item = Dig(predicate, notify: false);
            if (item != null)
            {
                await Pop();
            }
        }

        public async UniTask Pop(IRouteSegment segment)
        {
            await PopUntilInclusive(x => x == segment);
        }
        
        public async UniTask PopUntilRoot()
        {
            while (CanPop)
            {
                var previous = routerHistory.Pop();
                previous.OnExit().Forget();
            }
            
            var current = routerHistory.Peek();
            await current.OnFocus();
            
            NotifyChange();
        }
        
        public async UniTask PopUntilAndReplace(Func<IRouteSegment, bool> predicate, IRouteSegment replacement)
        {
            var target = Dig(predicate, notify: false);
            if (target == null)
            {
                await Push(replacement);
                return;
            }

            var last = routerHistory.Pop();
            last.OnExit().Forget();
            
            routerHistory.Push(replacement);
            
            await replacement.OnEnter();
            
            NotifyChange();
        }

        public void PushManual(IRouteSegment segment)
        {
            if (routerHistory.TryPeek(out var current))
            {
                if (current.Equals(segment)) return;
                
                current.OnBlur();
            }
            
            routerHistory.Push(segment);
            NotifyChange();
        }

        public void RemoveManual(Func<IRouteSegment, bool> predicate, bool notify = true)
        {
            var newHistory = routerHistory.ToList();
            var index = newHistory.FindIndex(new Predicate<IRouteSegment>(predicate));
            if (index == -1) return;

            newHistory.RemoveAt(index);
            newHistory.Reverse();
            routerHistory.Clear();

            foreach (var item in newHistory)
            {
                routerHistory.Push(item);
            }
            
            if (notify) NotifyChange();
        }

        public void RemoveManualCascade(Func<IRouteSegment, bool> predicate)
        {
            var target = Dig(predicate, notify: false);
            if (target == null) return;

            routerHistory.Pop();
            NotifyChange();
        }

        /// <summary>
        /// Digs through the router stack and pops all segments until the target is found.
        /// </summary>
        /// <returns>Found IRouteSegment or null</returns>
        private IRouteSegment Dig(Func<IRouteSegment, bool> predicate, bool notify = true)
        {
            var target = routerHistory.FirstOrDefault(predicate);
            if (target == null) return null;

            while (CanPop)
            {
                var item = routerHistory.Peek();
                if (item == target)
                {
                    break;
                }
                
                item.OnExit().Forget();
                routerHistory.Pop();
            }
            
            if (notify) NotifyChange();
            return target;
        }
        
        private void NotifyChange()
        {
            onChange.OnNext(routerHistory.ToList());
            Debug.Log($"Router changed: {Path}");
        }
    }
}