using System;
using Telegraphist.Scriptables;
using UnityEngine;
using UnityEngine.Localization;

namespace Telegraphist.Structures
{
    [Serializable]
    public class Rank
    {
        [SerializeField] private string rankText;
        [SerializeField] private LocalizedString rankLocalizedText;
        [SerializeField] private Sprite sprite;
        [SerializeField] private DialogueScriptable.DialogueLine gameOverDialogueLine;
        [SerializeField] private int percentage;
        [SerializeField] private int index;
        
        public int Index => index;
        public string RankText => rankLocalizedText != null ? rankLocalizedText.GetLocalizedString() : rankText;
        public Sprite Sprite => sprite;
        public int Percentage => percentage;
        public bool IsPassed => Index >= BalanceScriptable.Current.MinimumRankIDToPassLevel;

        public Rank(string text, int percentage, int index) =>
            (rankText, this.percentage, this.index) = (text, percentage, index);
        
        public static Rank UnknownRank => new Rank("?", 0, 0);

        public override string ToString()
        {
            return RankText;
        }
    }
}