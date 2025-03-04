using System.Collections;
using System.Collections.Generic;
using Telegraphist.Gameplay;
using Telegraphist.TileSystem;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;

namespace Telegraphist.FX
{
    public class OutOfTuneEffect : MonoBehaviour
    {
        public SongAudioController controller;

        public AudioMixerSnapshot DefaultSnapshot;
        public AudioMixerSnapshot MissedSnapshot;

        public float effectDuration = 0.5f;

        private bool isEffectActive = false;

        private void Start()
        {
            ResetAudio();
            MessageBroker.Default.Receive<TileInputStatus>()
                .Subscribe(OnTileStatus)
                .AddTo(this);
        }

        private void OnDestroy()
        {
            ResetAudio();
        }

        private void OnTileStatus(TileInputStatus status)
        {
            if (status is StatusMissed)
            {
                TriggerMistakeEffect();
            }
        }

        public void TriggerMistakeEffect()
        {
            if (!isEffectActive)
            {
                StartCoroutine(OutOfTuneCoroutine());
            }
        }

        private IEnumerator OutOfTuneCoroutine()
        {
            isEffectActive = true;

            MissedSnapshot.TransitionTo(0.1f);

            yield return new WaitForSeconds(effectDuration);

            DefaultSnapshot.TransitionTo(0.1f);
            isEffectActive = false;
        }

        private void ResetAudio()
        {
            isEffectActive = false;
            DefaultSnapshot.TransitionTo(0);
        }
    }
}
