#if UNITY_EDITOR
using Aim.Clock;
using Aim.Time;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Aim.EditorTools
{
    public static class AimSceneHierarchyBuilder
    {
        private const string GameScenePath = "Assets/Aim/Scenes/Game.unity";
        private const string TimeSyncConfigPath = "Assets/Aim/Configs/TimeSyncConfig.asset";
        private const string ClockConfigPath = "Assets/Aim/Configs/ClockConfig.asset";

        [MenuItem("Aim/Setup/Create Or Repair Game Scene Hierarchy")]
        [MenuItem("Tools/Aim/Create Or Repair Game Scene Hierarchy")]
        public static void CreateOrRepairGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateRoot("Main Camera", out var cameraGo);
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f);
            camera.orthographic = true;
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();

            CreateRoot("[1. BOOTSTRAP]", out var bootstrapRoot);
            var bootstrap = bootstrapRoot.AddComponent<Bootstrap.Bootstrap>();

            CreateRoot("[2. SERVICES]", out _);

            CreateRoot("[3. UI]", out var uiRoot);
            CreateEventSystem();
            var canvas = CreateUiCanvas(uiRoot.transform);
            var clockView = CreateWatchUi(canvas.transform);

            CreateRoot("[4. GAME]", out _);

            var contextGo = new GameObject("SceneContext");
            var sceneContext = contextGo.AddComponent<SceneContext>();
            var installer = contextGo.AddComponent<Installers.GameInstaller>();

            var so = new SerializedObject(sceneContext);
            var installersProp = so.FindProperty("_monoInstallers");
            if (installersProp != null)
            {
                installersProp.arraySize = 1;
                installersProp.GetArrayElementAtIndex(0).objectReferenceValue = installer;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            WireInstaller(installer, clockView);

            EditorSceneManager.SaveScene(scene, GameScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[Aim] Game scene saved to {GameScenePath}. Canvas: {canvas.name}, Bootstrap: {bootstrap.name}");
        }

        private static void WireInstaller(Installers.GameInstaller installer, ClockView clockView)
        {
            var installerSo = new SerializedObject(installer);
            installerSo.FindProperty("_timeSyncConfig").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<TimeSyncConfig>(TimeSyncConfigPath);
            installerSo.FindProperty("_clockConfig").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<ClockConfig>(ClockConfigPath);
            installerSo.FindProperty("_clockView").objectReferenceValue = clockView;
            installerSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ClockView CreateWatchUi(Transform canvas)
        {
            var root = CreateUiObject("WatchRoot", canvas);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(900f, 1400f);

            CreateImage("Strap", root.transform, "Assets/Aim/Sprites/strap.png", new Vector2(420f, 1200f));
            CreateImage("Case", root.transform, "Assets/Aim/Sprites/watch_case.png", new Vector2(620f, 620f));
            CreateImage("Dial", root.transform, "Assets/Aim/Sprites/watch_dial.png", new Vector2(480f, 480f));

            var hour = CreateImage("HourHand", root.transform, "Assets/Aim/Sprites/hand_hour.png", new Vector2(36f, 160f));
            var minute = CreateImage("MinuteHand", root.transform, "Assets/Aim/Sprites/hand_minute.png", new Vector2(28f, 220f));
            var second = CreateImage("SecondHand", root.transform, "Assets/Aim/Sprites/hand_second.png", new Vector2(12f, 240f));
            CreateImage("Pivot", root.transform, "Assets/Aim/Sprites/hand_pivot.png", new Vector2(40f, 40f));

            // Wider hit targets for dragging hands in edit mode.
            ExpandHandHitArea(hour, new Vector2(80f, 200f));
            ExpandHandHitArea(minute, new Vector2(70f, 260f));
            second.GetComponent<Image>().raycastTarget = false;

            var digital = CreateUiObject("DigitalTime", root.transform);
            var digitalRect = digital.GetComponent<RectTransform>();
            digitalRect.anchoredPosition = new Vector2(0f, -420f);
            digitalRect.sizeDelta = new Vector2(500f, 80f);
            var digitalText = digital.AddComponent<Text>();
            digitalText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            digitalText.fontSize = 48;
            digitalText.alignment = TextAnchor.MiddleCenter;
            digitalText.color = Color.white;
            digitalText.text = "00:00:00";

            var editButton = CreateButton("EditButton", root.transform, "Редактировать", new Vector2(0f, -520f));
            var editPanel = CreateUiObject("EditPanel", root.transform);
            editPanel.SetActive(false);
            var editPanelRect = editPanel.GetComponent<RectTransform>();
            editPanelRect.anchoredPosition = new Vector2(0f, -620f);
            editPanelRect.sizeDelta = new Vector2(700f, 160f);

            var inputGo = CreateUiObject("TimeInput", editPanel.transform);
            var inputRect = inputGo.GetComponent<RectTransform>();
            inputRect.anchoredPosition = new Vector2(0f, 40f);
            inputRect.sizeDelta = new Vector2(360f, 56f);
            var inputImage = inputGo.AddComponent<Image>();
            inputImage.color = new Color(0.2f, 0.2f, 0.22f, 1f);
            var input = inputGo.AddComponent<InputField>();
            var inputTextGo = CreateUiObject("Text", inputGo.transform);
            var inputText = inputTextGo.AddComponent<Text>();
            inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            inputText.fontSize = 28;
            inputText.color = Color.white;
            inputText.supportRichText = false;
            input.textComponent = inputText;
            input.contentType = InputField.ContentType.Standard;
            input.placeholder = null;

            var saveButton = CreateButton("SaveButton", editPanel.transform, "Сохранить", new Vector2(-140f, -40f));
            var cancelButton = CreateButton("CancelButton", editPanel.transform, "Отмена", new Vector2(140f, -40f));

            var clockView = root.AddComponent<ClockView>();
            var viewSo = new SerializedObject(clockView);
            viewSo.FindProperty("_hourHand").objectReferenceValue = hour.GetComponent<RectTransform>();
            viewSo.FindProperty("_minuteHand").objectReferenceValue = minute.GetComponent<RectTransform>();
            viewSo.FindProperty("_secondHand").objectReferenceValue = second.GetComponent<RectTransform>();
            viewSo.FindProperty("_digitalTimeText").objectReferenceValue = digitalText;
            viewSo.FindProperty("_editButton").objectReferenceValue = editButton;
            viewSo.FindProperty("_saveButton").objectReferenceValue = saveButton;
            viewSo.FindProperty("_cancelButton").objectReferenceValue = cancelButton;
            viewSo.FindProperty("_timeInputField").objectReferenceValue = input;
            viewSo.FindProperty("_editPanel").objectReferenceValue = editPanel;
            viewSo.ApplyModifiedPropertiesWithoutUndo();

            AttachHandDrag(hour, clockView, ClockHandDragHandler.HandType.Hour, root.GetComponent<RectTransform>());
            AttachHandDrag(minute, clockView, ClockHandDragHandler.HandType.Minute, root.GetComponent<RectTransform>());

            return clockView;
        }

        private static void ExpandHandHitArea(GameObject hand, Vector2 size)
        {
            var rect = hand.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            hand.GetComponent<Image>().raycastTarget = true;
        }

        private static void AttachHandDrag(
            GameObject hand,
            ClockView clockView,
            ClockHandDragHandler.HandType handType,
            RectTransform dragArea)
        {
            var handler = hand.AddComponent<ClockHandDragHandler>();
            var so = new SerializedObject(handler);
            so.FindProperty("_clockView").objectReferenceValue = clockView;
            so.FindProperty("_handType").enumValueIndex = (int)handType;
            so.FindProperty("_handTransform").objectReferenceValue = hand.GetComponent<RectTransform>();
            so.FindProperty("_dragArea").objectReferenceValue = dragArea;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition)
        {
            var go = CreateUiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(240f, 64f);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.25f, 0.45f, 0.75f, 1f);
            var button = go.AddComponent<Button>();
            var textGo = CreateUiObject("Label", go.transform);
            var text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
        }

        private static GameObject CreateImage(string name, Transform parent, string spritePath, Vector2 size)
        {
            var go = CreateUiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            var image = go.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            image.preserveAspect = true;
            return go;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
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

        private static void CreateRoot(string name, out GameObject go)
        {
            go = new GameObject(name);
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateUiCanvas(Transform parent)
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(parent, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            return canvas;
        }
    }
}
#endif
