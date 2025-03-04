using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;
using Gradient = UnityEngine.Gradient;

namespace Telegraphist
{
    public class SequenceArrowIndicator : MonoBehaviour
    {
        [Header("Heat Animation")]
        [SerializeField] private Image heatImage;
        [SerializeField] private float durationPercent = 0.8f;
        [SerializeField] private Color heatStartColor;
        [SerializeField] private Color heatTargetColor;
        [SerializeField] private Ease heatColorEase = Ease.InCubic;
        [Header("Circle Options")]
        [SerializeField] private int segments;
        [SerializeField] private LineRenderer lineRenderer;
        [Header("Circle Animation")]
        [SerializeField] private float startRadius = 400;
        [SerializeField] private float targetRadius = 150;
        [SerializeField] private float startWidth = 1;
        [SerializeField] private float targetWidth = 2;
        [SerializeField] private Ease widthEase = Ease.InCubic;
        [SerializeField] private Color circleStartColor;
        [SerializeField] private Color circleTargetColor;
        [SerializeField] private Ease circleColorEase = Ease.InCubic;

        private float radius;
        private bool initialized = false;

        private void Update()
        {
            if(initialized) RenderCircle();
        }

        public void Initialize(float duration, Vector3 position, Vector3 rotation)
        {        
            transform.localPosition = position;
            transform.localEulerAngles = rotation;
            
            Destroy(gameObject, duration+0.05f);

            HeatTween(duration);
            CircleTween(duration);

            initialized = true;
        }

        private void RenderCircle()
        {
            float angleStep = 2 * Mathf.PI / segments;
            lineRenderer.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float xPos = radius * Mathf.Cos(angleStep * i);
                float zPos = radius * Mathf.Sin(angleStep * i);
                lineRenderer.SetPosition(i, new Vector3(xPos, 0, zPos));
            }
        }

        private void HeatTween(float duration)
        {
            DOVirtual.Color(heatStartColor, heatTargetColor, duration * durationPercent, x => heatImage.color = x)
                .SetEase(heatColorEase).SetLink(gameObject);
        }

        private void CircleTween(float duration)
        {
            DOVirtual.Float(startRadius, targetRadius, duration, x => radius = x)
                .SetEase(Ease.Linear).SetLink(lineRenderer.gameObject);
            DOVirtual.Float(startWidth, targetWidth, duration, x => lineRenderer.widthMultiplier = x)
                .SetEase(widthEase).SetLink(lineRenderer.gameObject);
            DOVirtual.Color(circleStartColor, circleTargetColor, duration, x =>
                {
                    var g = new Gradient();
                    g.SetKeys(new []{new GradientColorKey(x, 0) }, new []{new GradientAlphaKey(x.a, 0) });
                    lineRenderer.colorGradient = g;
                })
                .SetEase(circleColorEase).SetLink(lineRenderer.gameObject);;
        }
    }
}
