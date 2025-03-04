using UnityEngine;

namespace Telegraphist.FX
{
    public class RadioEffect : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioSource noiseSource;

        [SerializeField] private float controlValue = 1;
        [SerializeField] private float exponent = 2f;
        [SerializeField] private float minimumNoiseLevel = 0.1f;

        public float ControlValue
        {
            get => controlValue;
            set
            {
                if (controlValue != value)
                {
                    controlValue = value;
                    ApplyRadioExponentially();
                }
            }
        }

        private float cachedAudioVolume;
        private float cachedNoiseVolume;

        private void Start()
        {
            cachedAudioVolume = audioSource.volume;
            cachedNoiseVolume = noiseSource.volume;
            noiseSource.volume = 0;
            ControlValue = 0;
        }

        public void ResetValues()
        {
            ControlValue = 0;
            audioSource.volume = cachedAudioVolume;
            noiseSource.volume = 0;
        }

        public void ApplyRadioExponentially()
        {
            float adjustedControlValue = Mathf.Pow(controlValue, exponent);
            float musicVolume = adjustedControlValue * cachedAudioVolume;

            float noiseVolume = Mathf.Max((1 - adjustedControlValue) * cachedNoiseVolume, minimumNoiseLevel);

            audioSource.volume = musicVolume;
            noiseSource.volume = noiseVolume;
        }
    }
}