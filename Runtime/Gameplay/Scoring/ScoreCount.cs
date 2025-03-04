using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.Gameplay.TileInput
{
    public class ScoreCount : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private int numbersCount = 6;
        [SerializeField] private bool colorizeLeadingZeros;
        [SerializeField] private Color leadingZerosColor;

        private void Start()
        {
            ScoringSystem.Current.OnUpdateScore += UpdateScore;
            UpdateScore(ScoringSystem.Current.Score);
        }

        private void OnDestroy()
        {
            ScoringSystem.Current.OnUpdateScore -= UpdateScore;
        }

        private void UpdateScore(float newScore)
        {
            var text = Mathf.CeilToInt(newScore).ToString();

            if (text.Length < numbersCount)
            {
                text = ColorizeZeros(new string('0', numbersCount - text.Length)) + text;
            }
            
            scoreText.text = text;
        }
        
        private string ColorizeZeros(string text) => colorizeLeadingZeros 
            ? $"<color=#{ColorUtility.ToHtmlStringRGBA(leadingZerosColor)}>{text}</color>" 
            : text;
    }
}
