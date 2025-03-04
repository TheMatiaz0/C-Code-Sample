using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KBCore.Refs;
using MoreMountains.Feedbacks;
using Telegraphist.Gameplay.QTE;
using Telegraphist.Structures;
using UnityEngine;

namespace Telegraphist
{
    public class ChangeOrdersDisplay : MonoBehaviour, IQteContent
    {
        [Header("Pre Indication")]
        [SerializeField] private MMF_Player preIndicationFeedbacks;
        [SerializeField] private MMF_Player preIndicationEndFeedbacks;
        [SerializeField] private MMF_Player endFeedbacks;
        [Header("Other elements")]
        [SerializeField, Self] private AudioSource audioSource;
        [SerializeField] private AudioClip paperInitSound;
        [SerializeField] private AudioClip paperSound;

        public QteContent ContentType => QteContent.Orders;
        public Transform Root => this.transform;
        
        public void OnDirectorPreIndication()
        {
            preIndicationFeedbacks.PlayFeedbacks();
            endFeedbacks.StopFeedbacks();
        }

        public void OnDirectorEnter()
        {
            Initialize();
        }

        public void OnDirectionEnter(QteDirection direction)
        {
        }

        public void OnDirectionPress(AccuracyStatus accuracyStatus)
        {
            audioSource.PlayOneShot(paperSound);
        }

        public void OnDirectorExit(bool isQtePassed)
        {
            endFeedbacks.PlayFeedbacks();
        }

        private void Initialize()
        {
            endFeedbacks.StopFeedbacks();
            preIndicationFeedbacks.StopFeedbacks();
            preIndicationEndFeedbacks.PlayFeedbacks();

            audioSource.PlayOneShot(paperInitSound);
            audioSource.Play();
        }

    }
}
