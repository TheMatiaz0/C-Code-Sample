using System;
using Eflatun.SceneReference;
using UnityEngine;

namespace Telegraphist.Helpers.Scenes
{
    [CreateAssetMenu(fileName = "SceneListLoadout", menuName = "Telegraphist/SceneListLoadout", order = 100)]
    public class SceneListLoadout : ScriptableObject
    {
        [Serializable]
        public class SceneWithType
        {
            [SerializeField]
            private SceneType sceneType;
            [SerializeField]
            private SceneReference scene;

            public SceneType SceneType => sceneType;
            public SceneReference Scene => scene;
        }

        [SerializeField]
        private SceneWithType[] scenes;

        public SceneWithType[] Scenes => scenes;
    }
}
