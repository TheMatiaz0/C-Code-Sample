using System.Collections;
using Telegraphist.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Telegraphist.Helpers.Video
{
    public class SRTSubtitleDisplayer : MonoBehaviour
    {
        private LocalizedTextAsset subtitleFile;

        [SerializeField]
        private Text text;

        [SerializeField]
        private VideoPlayer videoPlayer;

        [SerializeField]
        private HorizontalLayoutGroup horizontalLayout;

        [SerializeField]
        private Image background;

        private SRTParser parser;
        private float cachedStartTime;
        private SubtitleBlock currentSubtitle;
        private RectTransform horizontalLayoutRect;

        private bool IsTextDisplayed => SubtitleBlock.Blank != currentSubtitle && !string.IsNullOrEmpty(currentSubtitle.Text);

        private void Awake()
        {
            background.enabled = false;
        }

        public void SetSubtitleAsset(LocalizedTextAsset subtitleAsset)
        {
            subtitleFile = subtitleAsset;
        }
        
        public void Run()
        {
            if (subtitleFile?.IsEmpty ?? true) return;

            horizontalLayoutRect = horizontalLayout.GetComponent<RectTransform>();
            parser = new SRTParser(subtitleFile.LoadAsset());
            cachedStartTime = (float)videoPlayer.time;
            StartCoroutine(RunSubtitle());
        }

        private IEnumerator RunSubtitle()
        {
            while (true)
            {
                var elapsed = (float)videoPlayer.time - cachedStartTime;
                var subtitleBlock = parser.GetForTime(elapsed);

                if (subtitleBlock != null)
                {
                    if (!subtitleBlock.Equals(currentSubtitle))
                    {
                        Setup(subtitleBlock);
                    }
                    yield return null;
                }
                else
                {
                    Clear();
                    yield break;
                }
            }
        }

        private void Setup(SubtitleBlock subtitleBlock)
        {
            currentSubtitle = subtitleBlock;
            text.text = currentSubtitle.Text.Trim();

            RefreshBackground();
        }

        private void RefreshBackground()
        {
            background.enabled = IsTextDisplayed;

            /*
            if (!IsTextDisplayed)
            {
                return;
            }

            // AFAIK: fix for refreshing horizontal layout, may come handy
            // RefreshHorizontalLayout();
            */
        }

        private void RefreshHorizontalLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(horizontalLayoutRect);
            horizontalLayout.enabled = false;
            horizontalLayout.enabled = true;
        }

        private void Clear()
        {
            background.enabled = false;
            currentSubtitle = null;
            text.text = string.Empty;
        }
    }
}
