using System;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Telegraphist.Structures;
using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.Gameplay.QTE
{
    public class QteKeyBehaviour : MonoBehaviour
    {
        [Serializable]
        private class QteDirectionImage
        {
            [SerializeField] private QteDirection direction;
            [SerializeField] private Sprite sprite;
            [SerializeField] private Sprite spritePressed;
            [SerializeField] private float rotationZ;
            
            public QteDirection Direction => direction;
            public Sprite Sprite => sprite;
            public Sprite SpritePressed => spritePressed;
            public float RotationZ => rotationZ;
        }

        [SerializeField] private Image arrowImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private List<QteDirectionImage> directionImages;
        [SerializeField] private GameObject selectedFX;
        [Header("On Pressed")]
        [SerializeField] private float pressedShakeStrength = 20;
        [SerializeField] private MMF_Player pressedFeedback;
        
        [Header("Colors")] 
        [SerializeField] private Color startColor;
        [SerializeField] private Color shouldBePressedColor;
        [SerializeField] private Color pressedColor;
        [SerializeField] private float pressedColorFadeDuration;
        [SerializeField] private Color inactiveColor;
        [SerializeField] private Ease colorFadeEase;

        private Sequence movementSequence;
        private Sequence colorSequence;
        private QteDirectionImage keyOutfit;
        public Guid Guid { get; private set; }

        public void Initialize(QteDirection direction, Guid guid, QteKeyDurations durations, Sequence arrowMoveSequence, Action completeAction)
        {
            selectedFX.SetActive(false);
            keyOutfit = directionImages.Find(x => x.Direction == direction);
            Guid = guid;
            if (keyOutfit == null) return;    
            
            if (keyOutfit.Sprite != null)
            {
                arrowImage.sprite = keyOutfit.Sprite;
            }

            backgroundImage.color = inactiveColor;
            rotatePivot.transform.rotation = Quaternion.Euler(0, 0, keyOutfit.RotationZ);

            colorSequence = CreateColorSequence(durations, completeAction);
            movementSequence = arrowMoveSequence;

            colorSequence.Play();
            movementSequence.Play();
        }

        private Sequence CreateColorSequence(QteKeyDurations durations, Action completeAction)
        {
            return DOTween.Sequence()
                .Append(backgroundImage.DOColor(startColor, durations.preDuration).SetEase(colorFadeEase))
                //.Join(arrowImage.DOColor(Color.black, postDuration).SetEase(colorFadeEase))
                .Append(backgroundImage.DOColor(shouldBePressedColor, durations.moveDuration)
                    .SetEase(colorFadeEase))
                .AppendInterval(durations.postDuration)
                .Append(backgroundImage.DOColor(inactiveColor, durations.postDuration)
                    .SetEase(colorFadeEase))
                .AppendInterval(0.1f)
                .AppendCallback(() => completeAction())
                //.Join(arrowImage.DOColor(inactiveColor, postDuration).SetEase(colorFadeEase))
                .SetLink(gameObject)
                .Pause();
        }

        public void QteKeyPressed(float intensity)
        {
            colorSequence.Pause();
            movementSequence.Pause();
            
            arrowImage.sprite = keyOutfit.SpritePressed;
            pressedFeedback.FeedbacksIntensity *= intensity * intensity;
            pressedFeedback.PlayFeedbacks();
            
            DOTween.Sequence()
                .Append(backgroundImage.DOColor(pressedColor, pressedColorFadeDuration).SetEase(colorFadeEase))
                .Join(transform.DOShakePosition(pressedColorFadeDuration, new Vector3(pressedShakeStrength, pressedShakeStrength , 0) * intensity))
                .OnComplete(() =>
                {
                    colorSequence.Play();
                })
                .SetLink(gameObject)
                .Play();
        }
        

        
        public void SetAsNextArrow()
        {
            selectedFX.SetActive(true);
        }
    }
}