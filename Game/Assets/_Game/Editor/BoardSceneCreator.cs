using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XadrezRpg.Board;

namespace XadrezRpg.Editor
{
    public static class BoardSceneCreator
    {
        public const string ScenePath = "Assets/_Game/Scenes/Board.unity";
        public const string ArtworkPath = "Assets/_Game/Sprites/Board/tabuleiro_medieval.aseprite";

        private const float ArtworkPixelSize = 1254f;
        private const float ArtworkDisplaySize = 718f;

        private static readonly float[] ArtworkColumnEdges =
        {
            133f, 256f, 381f, 502f, 627f, 750f, 874f, 997f, 1128f
        };

        private static readonly float[] ArtworkRowEdges =
        {
            129f, 258f, 385f, 512f, 636f, 764f, 890f, 1010f, 1132f
        };

        [MenuItem("Xadrez RPG/Create Board Scene")]
        public static void CreateBoardScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            Canvas canvas = CreateCanvas();
            CreateBoard(canvas.transform);
            CreateInterfaceText(canvas.transform);
            CreateEventSystem();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            SetAsFirstBuildScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Board scene created at {ScenePath}.");
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.065f, 0.075f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject(
                "Game Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateBoard(Transform canvas)
        {
            Sprite artwork = AssetDatabase.LoadAllAssetsAtPath(ArtworkPath)
                .OfType<Sprite>()
                .OrderByDescending(sprite => sprite.rect.width * sprite.rect.height)
                .FirstOrDefault();

            GameObject frameObject = new GameObject(
                "Board Frame",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            frameObject.transform.SetParent(canvas, false);

            RectTransform frameRect = (RectTransform)frameObject.transform;
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(ArtworkDisplaySize, ArtworkDisplaySize);
            frameRect.anchoredPosition = Vector2.zero;

            Image frameImage = frameObject.GetComponent<Image>();
            frameImage.sprite = artwork;
            frameImage.color = Color.white;
            frameImage.preserveAspect = true;
            frameImage.raycastTarget = false;

            GameObject boardObject = new GameObject(
                "Board",
                typeof(RectTransform),
                typeof(GridLayoutGroup),
                typeof(BoardView));
            boardObject.transform.SetParent(frameObject.transform, false);

            RectTransform boardRect = (RectTransform)boardObject.transform;
            boardRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardRect.pivot = new Vector2(0.5f, 0.5f);
            boardRect.sizeDelta = new Vector2(ArtworkDisplaySize, ArtworkDisplaySize);
            boardRect.anchoredPosition = Vector2.zero;

            BoardView board = boardObject.GetComponent<BoardView>();
            board.SetArtwork(artwork);
            board.SetArtworkLayout(
                new Vector2(ArtworkPixelSize, ArtworkPixelSize),
                ArtworkDisplaySize,
                ArtworkColumnEdges,
                ArtworkRowEdges);
            board.GenerateBoard();
            Selection.activeGameObject = boardObject;
        }

        private static void CreateInterfaceText(Transform canvas)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateText(canvas, "Title", "Xadrez RPG", font, 42, new Vector2(0f, 425f));
            CreateText(
                canvas,
                "Instructions",
                "Clique em uma casa para selecionar",
                font,
                24,
                new Vector2(0f, -420f));
        }

        private static void CreateText(
            Transform parent,
            string objectName,
            string content,
            Font font,
            int fontSize,
            Vector2 position)
        {
            GameObject textObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = (RectTransform)textObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(900f, 70f);
            rect.anchoredPosition = position;

            Text text = textObject.GetComponent<Text>();
            text.text = content;
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.94f, 0.94f, 0.94f, 1f);
            text.raycastTarget = false;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("Event System", typeof(EventSystem));
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private static void SetAsFirstBuildScene()
        {
            EditorBuildSettingsScene[] remainingScenes = EditorBuildSettings.scenes
                .Where(scene => scene.path != ScenePath)
                .ToArray();

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }
                .Concat(remainingScenes)
                .ToArray();
        }
    }
}
