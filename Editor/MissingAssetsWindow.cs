using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Telegraphist.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Telegraphist.Editor
{
    public class SearchResult
    {
        public UnityEngine.Object ChildObject;
        public string AssetPath;

        public SearchResult(UnityEngine.Object childObject, string assetPath)
        {
            ChildObject = childObject;
            AssetPath = assetPath;
        }

        public string GetButtonText()
        {
            if (ChildObject == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(AssetPath))
            {
                return $"{AssetPath.Split('/').Last()}: {ChildObject.name}\n({AssetPath})";
            }
            else
            {
                var scene = (ChildObject as GameObject).scene;
                return $"{scene.name}: {ChildObject.name}\n({scene.path})";
            }
        }
    }

    public class MissingAssetsWindow : EditorWindow
    {
        private const string WINDOW_TITLE = "Find Missing Assets";
        private const string MENU_TITLE = "Tools/Cubepotato/Find Missing Assets";

        private const int FILTERMODE_ALL = 0;
        private const int FILTERMODE_NAME = 1;
        private const int FILTERMODE_TYPE = 2;

        [SerializeField]
        private List<SceneAsset> scenes = new();

        private GUIStyle root;
        private Vector2 scrollPos;
        private bool searchOnlyScenes = false;
        private SerializedObject serializedObject;
        private SerializedProperty scenesProperty;
        private SearchableEditorWindow hierarchy;
        private List<Scene> previewLoadedScenes = new();
        private string loadedScenePath;
        private readonly List<SearchResult> missingAssets = new();

        private string TargetName => searchOnlyScenes ? "Scenes" : "Assets";

        [MenuItem(MENU_TITLE)]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(MissingAssetsWindow));
            window.titleContent = new(WINDOW_TITLE);
        }

        private void OnEnable()
        {
            serializedObject = new(this);
            scenesProperty = serializedObject.FindProperty(nameof(scenes));
        }

        private void OnDisable()
        {
            if (searchOnlyScenes)
            {
                DiscardLoadedScenes();
                Debug.Log("Closed loaded scenes, just for your safety :)");
            }
        }

        private void OnGUI()
        {
            GUILayout.Label(WINDOW_TITLE, EditorStyles.boldLabel);
            GUILayout.Space(10);

            root = new GUIStyle(GUI.skin.box);
            root.normal.background = MakeTexture(2, 2, new Color(0.6f, 0.6f, 0.6f, 0.5f));

            searchOnlyScenes = EditorGUILayout.Toggle("Search only scenes", searchOnlyScenes);
            if (searchOnlyScenes)
            {
                MakeScenesLayout();
                GUILayout.Space(20);
            }

            MakeSearchButtons();
        }

        private Texture2D MakeTexture(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void MakeScenesLayout()
        {
            GUILayout.BeginVertical(root);

            CreateScenesProperty();

            if (GUILayout.Button("Import open scenes"))
            {
                ImportOpenScenes();
            }
            if (GUILayout.Button("Import build settings scenes"))
            {
                ImportBuildScenes();
            }

            GUILayout.EndVertical();
        }

        private void CreateScenesProperty()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(scenesProperty, new GUIContent("Scenes"), true);
            serializedObject.ApplyModifiedProperties();
        }

        private void ImportOpenScenes()
        {
            scenes.Clear();
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                AddSceneAssetByPath(scene.path);
            }
        }

        private void ImportBuildScenes()
        {
            scenes.Clear();
            var buildSettingsScenes = EditorBuildSettings.scenes;
            foreach (var buildScene in buildSettingsScenes)
            {
                AddSceneAssetByPath(buildScene.path);
            }
        }

        private void AddSceneAssetByPath(string path)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset));
            scenes.Add(sceneAsset as SceneAsset);
        }

        private void DiscardLoadedScenes()
        {
            foreach (var previewScene in previewLoadedScenes)
            {
                EditorSceneManager.CloseScene(previewScene, true);
            }
            previewLoadedScenes.Clear();
            if (loadedScenePath != null)
            {
                EditorSceneManager.OpenScene(loadedScenePath, OpenSceneMode.Single);
            }
        }

        private void SearchInScenes(params Action<GameObject, Component, string>[] actions)
        {
            previewLoadedScenes.Clear();
            var loadedScene = EditorSceneManager.GetActiveScene();
            if (loadedScene.path != null)
            {
                loadedScenePath = loadedScene.path;
            }

            if (scenes.Count == 0)
            {
                Debug.LogError("Add scenes to list first before searching!", this);
                return;
            }

            foreach (var sceneAsset in scenes)
            {
                string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                previewLoadedScenes.Add(scene);
            }

            foreach (var scene in previewLoadedScenes)
            {
                GameObject[] objects = scene.GetRootGameObjects();

                foreach (var obj in objects)
                {
                    if (obj == null)
                    {
                        return;
                    }

                    SearchRecursiveByRoot(obj, assetPath: null, actions);
                }
            }
        }

        private void MakeSearchButtons()
        {
            GUILayout.BeginVertical(root);

            GUILayout.Label($"Target: {TargetName}", EditorStyles.boldLabel);

            if (GUILayout.Button("Find missing sprites"))
            {
                if (searchOnlyScenes)
                {
                    SearchInScenes(FindMissingSpriteInImage, FindMissingSpriteInRawImage);
                }
                else
                {
                    SearchInAssets(FindMissingSpriteInImage, FindMissingSpriteInRawImage);
                }
            }
            if (GUILayout.Button("Find missing scripts"))
            {
                if (searchOnlyScenes)
                {
                    SearchInScenes(FindMissingScripts);
                }
                else
                {
                    SearchInAssets(FindMissingScripts);
                }
            }

            if (searchOnlyScenes && GUILayout.Button("Discard loaded scenes"))
            {
                DiscardLoadedScenes();
                missingAssets.Clear();
            }

            if (missingAssets.Count > 0)
            {
                CreateResultList();
            }

            GUILayout.EndVertical();
        }

        private void SearchInAssets(params Action<GameObject, Component, string>[] actions)
        {
            missingAssets.Clear();
            var allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in allAssets)
            {
                if (assetPath.StartsWith("Packages/"))
                {
                    continue;
                }

                if (Path.GetExtension(assetPath) == ".prefab")
                {
                    var assetRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    SearchRecursiveByRoot(assetRoot, assetPath, actions);
                }
            }
        }

        private void SearchRecursiveByRoot(GameObject root, string assetPath = null, params Action<GameObject, Component, string>[] actions)
        {
            List<Transform> children = new()
            {
                root.transform
            };
            root.transform.GetAllChildrenRecursive(ref children);

            foreach (var child in children)
            {
                var childObject = child.gameObject;
                var components = childObject.GetComponents<Component>();
                foreach (var component in components)
                {
                    foreach (var action in actions)
                    {
                        action.Invoke(childObject, component, assetPath);
                    }
                }
            }
        }

        private void CreateResultList()
        {
            GUILayout.Label($"Results:", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var searchResult in missingAssets)
            {
                CreateResultButton(searchResult);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void CreateResultButton(SearchResult searchResult)
        {
            if (GUILayout.Button(searchResult.GetButtonText()))
            {
                AssetDatabase.OpenAsset(searchResult.ChildObject);
                EditorGUIUtility.PingObject(searchResult.ChildObject);

                SetSearchFilter(searchResult.ChildObject.name, FILTERMODE_NAME);
            }
        }

        private void FindMissingScripts(GameObject childObject, Component component, string assetPath)
        {
            if (component == null)
            {
                SearchResult searchResult = new(childObject, assetPath);
                missingAssets.Add(searchResult);
            }
        }

        private void FindMissingSpriteInImage(GameObject childObject, Component component, string assetPath)
        {
            if (component is Image image)
            {
                // TODO: maybe find a better way of handling it
                try
                {
                    var test = image.sprite.name;
                }
                catch (MissingReferenceException)
                {
                    SearchResult searchResult = new(childObject, assetPath);
                    missingAssets.Add(searchResult);
                }
                catch
                {
                }
            }
        }

        private void FindMissingSpriteInRawImage(GameObject childObject, Component component, string assetPath)
        {
            if (component is RawImage image)
            {
                // TODO: maybe find a better way of handling it
                try
                {
                    var test = image.texture.name;
                }
                catch (MissingReferenceException)
                {
                    SearchResult searchResult = new(childObject, assetPath);
                    missingAssets.Add(searchResult);
                }
                catch
                {
                }
            }
        }

        private void SetSearchFilter(string filter, int filterMode)
        {
            SearchableEditorWindow[] windows = (SearchableEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(SearchableEditorWindow));

            foreach (SearchableEditorWindow window in windows)
            {
                if (window.GetType().ToString() == "UnityEditor.SceneHierarchyWindow")
                {
                    hierarchy = window;
                    break;
                }
            }

            if (hierarchy == null)
            {
                return;
            }

            MethodInfo setSearchType = typeof(SearchableEditorWindow).GetMethod("SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = new object[] { filter, filterMode, false, false };

            setSearchType.Invoke(hierarchy, parameters);
        }
    }
}
