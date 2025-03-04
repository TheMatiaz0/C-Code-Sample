using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Telegraphist.Gameplay.QTE;
using Telegraphist.Helpers.Provider;
using Telegraphist.Structures;
using Telegraphist.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Telegraphist.VFX
{ 
    public class LightController : MonoBehaviour, IInjectable
    {
        
        [Serializable]
        private class FocusGenericLight
        {
            public LightTarget LightTarget;
            public LightData LightData;
            
            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        [Serializable]
        private class LightData
        {
            public GenericLight Light;
            public Color TargetColor;
            public float FadeInDuration = 0.3f;
            public float FadeOutDuration = 1;
            public Ease FadeEase = Ease.InCubic;

            public LightData(GenericLight light, LightData data)
            {
                this.Light = light;
                this.TargetColor = data.TargetColor;
                this.FadeInDuration = data.FadeInDuration;
                this.FadeOutDuration = data.FadeOutDuration;
                this.FadeEase = data.FadeEase;
            }
        }
        
        [SerializeField] private List<FocusGenericLight> focusLights;
        [SerializeField] private LightData defaultLocalLight;
        
        private List<Color> cachedFocusLightsColors;
        private Tween lightTween;
        private LightData copiedLightData;
        private Color cachedDefaultColor;
        private GameObject copiedLight;

        private void Awake()
        {
            cachedFocusLightsColors = focusLights.Select(x => x.LightData.Light.Color).ToList();
            cachedDefaultColor = defaultLocalLight.Light.Color;
        }

        public void FadeLight(LightTarget lightTarget, bool shouldFadeOut, Color? targetColor = null, float? fadeDuration = null, Ease? fadeEase = null)
        {
            int i = focusLights.FindIndex(x => x.LightTarget == lightTarget);
            if (i == -1)
            {
                Debug.Log($"Couldn't find {lightTarget}! Setting default light...");

                CreateDefaultLight(lightTarget, shouldFadeOut);
                return;
            }

            var data = (FocusGenericLight)focusLights[i].Clone();
            var cachedColor = cachedFocusLightsColors[i];


            data.LightData.FadeOutDuration = fadeDuration ?? data.LightData.FadeOutDuration;
            data.LightData.FadeInDuration = fadeDuration ?? data.LightData.FadeInDuration;
            data.LightData.TargetColor = targetColor ?? data.LightData.TargetColor;
            data.LightData.FadeEase = fadeEase ?? data.LightData.FadeEase;
            
            Fade(data.LightData, cachedColor, shouldFadeOut);
        }

        private void CreateDefaultLight(LightTarget lightTarget, bool shouldFadeOut)
        {
            var pivot = GameplayElementPivotAssigner.Current.GetPivotTransform(lightTarget);
            var defaultLight = defaultLocalLight.Light;

            if (shouldFadeOut)
            {
                if (copiedLightData == null || copiedLight == null)
                {
                    return;
                }

                Fade(copiedLightData, cachedDefaultColor, shouldFadeOut)
                    .OnComplete(DisposeCopy)
                    .OnKill(DisposeCopy);
            }
            else
            {
                if (copiedLightData != null || copiedLight != null)
                {
                    return;
                }

                copiedLight = Instantiate(defaultLight.GameObject,
                    pivot.position, pivot.rotation, pivot);

                var lightType = defaultLight.Behaviour.GetType();
                var lightComponent = copiedLight.GetComponent(lightType);
                var copiedGenericLight = GenericLight.Create(lightComponent);

                if (copiedGenericLight == null)
                {
                    Destroy(copiedLight);
                    return;
                }

                copiedLightData = new LightData(copiedGenericLight, defaultLocalLight);

                Fade(copiedLightData, cachedDefaultColor, shouldFadeOut);
            }
        }

        private void DisposeCopy()
        {
            Destroy(copiedLight);
            copiedLight = null;
            copiedLightData = null;
        }

        private Tween Fade(LightData data, Color cachedColor, bool shouldFadeOut)
        {
            var duration = shouldFadeOut ? data.FadeOutDuration : data.FadeInDuration;
            var endValue = shouldFadeOut ? cachedColor : data.TargetColor;
            
            return data.Light.GameObject.ReplaceObjectTween(CreateFadeTween(data, duration, endValue));
        }

        private Tween CreateFadeTween(LightData lightData, float duration, Color endValue)
        {
            return lightData.Light.DOColor(endValue, duration)
                 .SetEase(lightData.FadeEase)
                 .SetLink(lightData.Light.GameObject);
        }
    }
}
