#if UNITY_EDITOR
using Aim.Loading;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aim.EditorTools
{
    public static class AimAddressablesBootSetup
    {
        private const string BootScenePath = "Assets/Aim/Scenes/Boot.unity";
        private const string GameScenePath = "Assets/Aim/Scenes/Game.unity";
        private const string LoadingConfigPath = "Assets/Aim/Configs/LoadingConfig.asset";
        private const string GameSceneAddress = "Game";
        private const string LocalGroupName = "Local Game";

        [MenuItem("Aim/Setup/Configure Addressables For Game Scene")]
        [MenuItem("Tools/Aim/Configure Addressables For Game Scene")]
        public static void ConfigureAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            var group = settings.FindGroup(LocalGroupName);
            if (group == null)
            {
                group = settings.CreateGroup(
                    LocalGroupName,
                    false,
                    false,
                    true,
                    null,
                    typeof(ContentUpdateGroupSchema),
                    typeof(BundledAssetGroupSchema));
            }

            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema != null)
            {
                schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalBuildPath);
                schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kLocalLoadPath);
            }

            var guid = AssetDatabase.AssetPathToGUID(GameScenePath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"[Aim] Game scene not found at {GameScenePath}");
                return;
            }

            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.SetAddress(GameSceneAddress);
            settings.AddLabel("Local");
            entry.SetLabel("Local", true, true);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Aim] Addressables configured. Scene address: '{GameSceneAddress}' in group '{LocalGroupName}'.");
        }

        [MenuItem("Aim/Setup/Create Boot Scene")]
        [MenuItem("Tools/Aim/Create Boot Scene")]
        public static void CreateBootScene()
        {
            EnsureLoadingConfig();
            ConfigureAddressables();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            var loadingRoot = CreateUi("LoadingRoot", canvasGo.transform);
            var loadingRect = loadingRoot.GetComponent<RectTransform>();
            loadingRect.anchorMin = Vector2.zero;
            loadingRect.anchorMax = Vector2.one;
            loadingRect.offsetMin = Vector2.zero;
            loadingRect.offsetMax = Vector2.zero;
            var bg = loadingRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.1f, 1f);

            var title = CreateUi("Title", loadingRoot.transform);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0f, 120f);
            titleRect.sizeDelta = new Vector2(800f, 80f);
            var titleText = title.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 48;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            titleText.text = "Watch";

            var status = CreateUi("Status", loadingRoot.transform);
            var statusRect = status.GetComponent<RectTransform>();
            statusRect.anchoredPosition = new Vector2(0f, 20f);
            statusRect.sizeDelta = new Vector2(800f, 50f);
            var statusText = status.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statusText.fontSize = 28;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = Color.white;
            statusText.text = "Loading...";

            var sliderGo = CreateUi("Progress", loadingRoot.transform);
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.anchoredPosition = new Vector2(0f, -60f);
            sliderRect.sizeDelta = new Vector2(600f, 24f);
            var sliderBg = sliderGo.AddComponent<Image>();
            sliderBg.color = new Color(0.2f, 0.2f, 0.22f, 1f);
            var slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            var fillArea = CreateUi("Fill Area", sliderGo.transform);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            var fill = CreateUi("Fill", fillArea.transform);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.55f, 0.9f, 1f);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            slider.fillRect = fillRect;

            var loadingView = loadingRoot.AddComponent<LoadingView>();
            var viewSo = new SerializedObject(loadingView);
            viewSo.FindProperty("_root").objectReferenceValue = loadingRoot;
            viewSo.FindProperty("_statusText").objectReferenceValue = statusText;
            viewSo.FindProperty("_progressSlider").objectReferenceValue = slider;
            viewSo.ApplyModifiedPropertiesWithoutUndo();

            var loaderGo = new GameObject("AddressablesSceneLoader");
            var loader = loaderGo.AddComponent<AddressablesSceneLoader>();
            var loaderSo = new SerializedObject(loader);
            loaderSo.FindProperty("_config").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<LoadingConfig>(LoadingConfigPath);
            loaderSo.FindProperty("_loadingView").objectReferenceValue = loadingView;
            loaderSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, BootScenePath);
            SetBootAsFirstScene();
            AssetDatabase.Refresh();
            Debug.Log($"[Aim] Boot scene saved to {BootScenePath} and set as first Build Settings scene.");
        }

        private static void EnsureLoadingConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<LoadingConfig>(LoadingConfigPath);
            if (existing != null)
            {
                return;
            }

            var config = ScriptableObject.CreateInstance<LoadingConfig>();
            AssetDatabase.CreateAsset(config, LoadingConfigPath);
            AssetDatabase.SaveAssets();
        }

        private static void SetBootAsFirstScene()
        {
            var bootGuid = AssetDatabase.AssetPathToGUID(BootScenePath);
            var scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log($"[Aim] EditorBuildSettings updated. Boot guid={bootGuid}");
        }

        private static GameObject CreateUi(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            return go;
        }
    }
}
#endif
