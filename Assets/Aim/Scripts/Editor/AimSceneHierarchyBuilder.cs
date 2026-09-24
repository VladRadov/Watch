#if UNITY_EDITOR
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

        [MenuItem("Aim/Setup/Create Or Repair Game Scene Hierarchy")]
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

            EditorSceneManager.SaveScene(scene, GameScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[Aim] Game scene saved to {GameScenePath}. Canvas: {canvas.name}, Bootstrap: {bootstrap.name}");
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
