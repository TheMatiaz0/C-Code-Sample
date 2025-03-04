using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.LevelEditor.Timeline
{
    public class TimelineOptionsUI : MonoBehaviour
    {
        private const float ZoomScrollSensitivityMagic = 120;
        
        [SerializeField] private Dropdown beatFractionDropdown;
        [SerializeField, Tooltip("what fraction of a beat should one cell represent - 1/n-th")] 
        private int maxBeatFraction;

        [SerializeField] private Slider zoomSlider;
        [SerializeField] private Vector2 cellWidthRange;
        [SerializeField] private int initialCellWidth = 40;
        [SerializeField] private float scrollZoomSensitivity = 1;

        public ReactiveProperty<int> BeatFraction { get; private set; }
        public ReactiveProperty<int> CellWidth { get; private set; }
        public float Zoom => zoomSlider.value;

        private void Awake()
        {
            BeatFraction = new ReactiveProperty<int>(PlayerPrefs.GetInt("LEVEL_EDITOR_BEAT_FRACTION", 1));
            CellWidth = new ReactiveProperty<int>(PlayerPrefs.GetInt("LEVEL_EDITOR_CELL_WIDTH", initialCellWidth));
            
            BeatFraction.Subscribe((v) => PlayerPrefs.SetInt("LEVEL_EDITOR_BEAT_FRACTION", v));
            CellWidth.Subscribe((v) => PlayerPrefs.SetInt("LEVEL_EDITOR_CELL_WIDTH", v));

            SetupViewOptions();
        }

        private void SetupViewOptions()
        {
            // beat fraction
            var options = new List<Dropdown.OptionData>();
            for (var i = 0; i <= Mathf.Log(maxBeatFraction, 2); i++)
            {
                options.Add(new Dropdown.OptionData($"1/{Mathf.Pow(2, i)}"));
            }
            beatFractionDropdown.options = options;
            beatFractionDropdown.value = (int)Mathf.Log(BeatFraction.Value, 2);
            beatFractionDropdown.onValueChanged.AddListener(ChangeBeatFraction);

            // zoom
            zoomSlider.minValue = cellWidthRange.x;
            zoomSlider.maxValue = cellWidthRange.y;
            zoomSlider.value = CellWidth.Value;
            zoomSlider.onValueChanged.AddListener(v =>
            {
                Timeline.Current.WrapActionAndPreserveTimelinePosition(() => CellWidth.Value = Mathf.RoundToInt(v));
            });
        }

        public void OnBeatFractionScroll(float axis)
        {
            var currentPowerOf2 = (int)Mathf.Log(BeatFraction.Value, 2);
            var powerOf2 = axis switch
            {
                > 0 => Mathf.Min(maxBeatFraction, currentPowerOf2 + 1),
                < 0 => Mathf.Max(0, currentPowerOf2 - 1),
                _ => currentPowerOf2 
            };

            ChangeBeatFraction(powerOf2);
        }

        public void ChangeBeatFraction(int powerOf2)
        {
            Timeline.Current.WrapActionAndPreserveTimelinePosition(() =>
            {
                beatFractionDropdown.value = powerOf2;
                BeatFraction.Value = (int)Mathf.Pow(2, powerOf2);
            });
        }

        public void ChangeZoom(float amount)
        {
            zoomSlider.value += amount * scrollZoomSensitivity / ZoomScrollSensitivityMagic;
        }
    }
}