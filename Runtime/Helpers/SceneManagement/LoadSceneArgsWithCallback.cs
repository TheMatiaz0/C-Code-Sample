using System;

namespace Telegraphist.Helpers.Scenes
{
    public record LoadSceneArgsWithCallback(
        SceneType SceneType,
        Action Callback,
        bool WithTransition = true,
        float Delay = 0)
        : LoadSceneArgs(SceneType, WithTransition, Delay)
    {
        public void RunCallback()
        {
            Callback?.Invoke();
        }
    }
}