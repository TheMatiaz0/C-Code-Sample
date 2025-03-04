using Telegraphist.Helpers.Provider;
using UnityEngine;

namespace Telegraphist.Gameplay.QTE.Frequency
{
    public sealed class FrequencyKnobArrows : MonoBehaviour, IInjectable
    {
        [SerializeField] private GameObject leftArrow, rightArrow;
        [SerializeField] private GameObject mouseButton;

        private void Start()
        {
            HideArrows();
        }

        public void ShowLeftArrow()
        {
            leftArrow.SetActive(true);
            mouseButton.SetActive(true);
        }
        public void ShowRightArrow()
        {
            rightArrow.SetActive(true);
            mouseButton.SetActive(true);
        }

        public void HideArrows()
        {
            leftArrow.SetActive(false);
            rightArrow.SetActive(false);
            mouseButton.SetActive(false);
        }
    }
}