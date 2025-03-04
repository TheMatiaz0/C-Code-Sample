using UnityEngine;

namespace Telegraphist.UI.Buttons
{
    public class ButtonQuitGame : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button quitGameButton;

        private void Awake()
        {
            quitGameButton.onClick.RemoveAllListeners();
            quitGameButton.onClick.AddListener(QuitGame);
        }

        private void QuitGame()
        {
            Application.Quit(0);
        }
    }
}
