using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.Gameplay.UI
{
    [RequireComponent(typeof(Slider))]
    public class SliderView : MonoBehaviour
    {
        private Slider slider;

        private void Awake()
        {
            slider = GetComponent<Slider>();
        }

        public void Initialize(float max, float startValue = 0)
        {
            slider.value = startValue;
            slider.maxValue = max;
        }

        public void UpdateValue(float value)
        {
            slider.value = value;
        }
    }
}
