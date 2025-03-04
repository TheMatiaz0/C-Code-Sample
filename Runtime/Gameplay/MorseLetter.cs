using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Telegraphist.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.Gameplay.HandPaperVariant
{
    public struct MorseUnderlinePaper
    {
        public Transform tileTransform;
        public Transform parentTransform;
        public Vector2 startPosition;
        public Vector2 endPosition;
        public bool isTileLong;
        
        public MorseUnderlinePaper(Transform tileTransform, Transform parentTransform, Vector2 startPosition, Vector2 endPosition, bool isTileLong)
        {
            this.tileTransform = tileTransform;
            this.parentTransform = parentTransform;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.isTileLong = isTileLong;
        }
    }
    
    public class MorseLetter : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private Text textRight;
        [SerializeField] private Transform textRightRoot;
        [SerializeField] private Vector2 offsetRightText = new Vector2(0.3f, 0);
        [SerializeField] private Transform mainTextRoot;
        [SerializeField] private Text textUnderline;
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private MMF_Player pressFeedback;
        [SerializeField] private MMF_Player endFeedback;
        [Header("Morse Underline")]
        [SerializeField] private SpriteRenderer morseUnderlinePrefab;
        [SerializeField] private SpriteRenderer morseUnderlineLongPrefab;
        [Header("Press Animation")]
        [SerializeField] private float textPressedColorDuration = 0.2f;
        [SerializeField] private Ease textPressedColorEase = Ease.OutCubic;
        [SerializeField] private float underlineSwitchDuration = 0.1f;
        [SerializeField] private Ease underlineSwitchEase = Ease.InCubic;


        private List<SpriteRenderer> underlines = new();
        private Tween textSeq;
        private int totalPresses;
        
        public void Setup(string letter, Vector2 myPosition, int presses, List<MorseUnderlinePaper> morseUnderlines, bool isHighlighted)
        {
            text.text = letter;
            textUnderline.text = letter;
            textRight.text = letter;
            transform.localPosition = myPosition;
            
            // letter shown on the right of the morse sequence
            var lastMorseUnderline = morseUnderlines[^1];
            textRightRoot.SetParent(lastMorseUnderline.parentTransform);
            textRightRoot.localPosition = lastMorseUnderline.endPosition + offsetRightText;

            if (!isHighlighted)
            {
                mainTextRoot.gameObject.SetActive(false); //hide the paper letter
                return;
            }
            
            foreach (var morseUnderline in morseUnderlines)
            {
                bool isTileLong = morseUnderline.isTileLong;
                Transform parent = morseUnderline.tileTransform;
                Vector3 moveBy = morseUnderline.endPosition - morseUnderline.startPosition;
                var underline = Instantiate(isTileLong ? morseUnderlineLongPrefab : morseUnderlinePrefab, parent.position, Quaternion.identity, parent);
                underline.transform.localPosition += (Vector3)moveBy / 2; //the full length / 2 to get the middle point
                underlines.Add(underline);
                underline.gameObject.ReplaceObjectTween(underline.DOFade(1, underlineSwitchDuration).SetEase(underlineSwitchEase).SetLink(underline.gameObject));
            }
            
            initFeedback.PlayFeedbacks();
            totalPresses = presses;
        }

        public void CorrectPress(float intensity)
        {
            if (textSeq != null && textSeq.IsActive())
            {
                textSeq.Kill(true);
            }

            if (!text || !pressFeedback) return;
            var newColor = new Color(text.color.r, text.color.g, text.color.b, text.color.a + intensity/totalPresses);
             textSeq = text.DOColor(newColor, textPressedColorDuration).SetEase(textPressedColorEase).SetLink(text.gameObject);
            
            pressFeedback.FeedbacksIntensity = intensity;
            pressFeedback.PlayFeedbacks();
        }

        public void Close()
        {
            if(pressFeedback.IsPlaying) pressFeedback.StopFeedbacks();
            Dispose();
            endFeedback.PlayFeedbacks();
        }

        private void Dispose()
        {
            foreach (var underline in underlines)
            {
                underline.gameObject.ReplaceObjectTween(underline.DOFade(0, underlineSwitchDuration).SetEase(underlineSwitchEase).SetLink(underline.gameObject)
                    .OnComplete(()=>Destroy(underline.gameObject)));
            }
        }
    }
}