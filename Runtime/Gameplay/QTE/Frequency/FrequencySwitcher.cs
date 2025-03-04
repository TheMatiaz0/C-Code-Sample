using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Telegraphist.Lifecycle;
using Telegraphist.Structures;
using Telegraphist.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Telegraphist.Gameplay.QTE.Frequency
{
    public sealed class FrequencySwitcher : SongBehaviour, IQteContent
    {
        [Header("Pre Indication")]
        [SerializeField] private MMF_Player preIndicationFeedbacks;
        [SerializeField] private MMF_Player preIndicationEndFeedbacks;

        [Header("Rotating arrow")] 
        [SerializeField] private Transform arrowPivot;
        [SerializeField] private KnobInputController knobInputController;
        [SerializeField] private float minAngle;
        [SerializeField] private float maxAngle;
        [SerializeField] private bool reverseAngle = true;
        
        [Header("Lights")]
        [SerializeField] private List<FrequencySwitcherLight> newLights;
        [SerializeField] private float lightFadeDuration;
        [SerializeField] private float blinkingFrequency = 0.5f;
        [SerializeField] private float blinkingOnDuration = 0.5f;
        [SerializeField] private Transform lightPivot;
        [SerializeField] private Transform lightPosition;
        [SerializeField] private FrequencySwitcherLight lightObject;
        
        [Header("Sounds")] 
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip switchIncomingClip;
        [SerializeField] private AudioClip buzzLoopClip;
        [SerializeField] private AudioClip correctFrequencyClip;
        [SerializeField] private AudioClip failFrequencyClip;
        [SerializeField] private AudioClip correctPressClip;

        private float arrowAngleSpan;
        private float knobAngleSpan;
        private bool wasAllowedToRotate;
        private int previousLightIndex;

        public QteContent ContentType => QteContent.FrequencySwitcher;
        public Transform Root => this.transform;
        private int LightCount => FrequencyController.Current.FrequencyCount - 1;
        private float AnglePerLight => arrowAngleSpan / LightCount;

        protected override void OnSongStart() => DisableAllLights();
        protected override void OnSongEnd() => DisableAllLights();

        private void Start()
        {
            audioSource.clip = buzzLoopClip;
            audioSource.loop = true;

            knobAngleSpan = knobInputController.MaxAngle - knobInputController.MinAngle;
            arrowAngleSpan = maxAngle - minAngle;

            newLights.Reverse();
        }
        
        public void OnDirectorPreIndication()
        {
            preIndicationFeedbacks.PlayFeedbacks();
        }

        public void OnDirectorEnter()
        {
            wasAllowedToRotate = knobInputController.AllowRotation;
            knobInputController.AllowRotation = false;
            Initialize();
        }

        public void Initialize()
        {
            preIndicationFeedbacks.StopFeedbacks();
            preIndicationEndFeedbacks.PlayFeedbacks();
            knobInputController.OnAngleChange
                .Subscribe(OnKnobRotate)
                .AddTo(this);

            audioSource.PlayOneShot(switchIncomingClip);
            audioSource.Play();
        }

        public void OnDirectionEnter(QteDirection direction)
        {
            Vector2Int vect = direction.ToCounterVector();
            int lightIndex = previousLightIndex + vect.x + vect.y;
            lightIndex = MathUtils.Wrap(lightIndex, 0, LightCount + 1);
            StartBlinking(lightIndex);
        }

        public void OnDirectionPress(AccuracyStatus accuracyStatus)
        {
            var newAngle = previousLightIndex * AnglePerLight;
            knobInputController.Angle = newAngle;
            OnKnobRotate(newAngle);
            audioSource.PlayOneShot(correctPressClip);
        }

        public void OnDirectorExit(bool isQtePassed)
        {
            Complete(isQtePassed);
            knobInputController.AllowRotation = wasAllowedToRotate;
        }

        public void StartBlinking(int lightIndex)
        {
            DisableAllLights();

            newLights[lightIndex].StartBlinking(blinkingFrequency, blinkingOnDuration, lightFadeDuration);
            previousLightIndex = lightIndex;
        }  

        public void Complete(bool isSuccess)
        {
            knobInputController.Angle = 0;
            DisableAllLights();

            audioSource.Stop();

            ShowResult(isSuccess);
        }

        private void DisableAllLights()
        {
            foreach (var light in newLights)
            {
                light.DisableLight();
            }
        }

        private void ShowResult(bool isSuccess)
        {
            if (isSuccess)
            {
                audioSource.PlayOneShot(correctFrequencyClip);
            }
            else
            {
                audioSource.PlayOneShot(failFrequencyClip);
            }
        }

        private void OnKnobRotate(float angle)
        {
            var arrowAngle = (reverseAngle ? -1 : 1) * KnobAngleToArrowAngle(angle);
            RotateArrow(arrowAngle);
        }

        private float KnobAngleToArrowAngle(float angle)
        {
            float percent = angle / knobAngleSpan;
            float arrowAngle = arrowAngleSpan * percent;

            return arrowAngle;
        }

        private void RotateArrow(float angle)
        {
            arrowPivot.localEulerAngles = new Vector3(0, 0, angle);
        }
    }
}