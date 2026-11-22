using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XadrezRpg.Board;
using XadrezRpg.Pieces;
using XadrezRpg.Turns;

namespace XadrezRpg.Editor
{
    public static class BoardSceneCreator
    {
        public const string ScenePath = "Assets/_Game/Scenes/Board.unity";
        public const string ArtworkPath = "Assets/_Game/Sprites/Board/tabuleiro_medieval.aseprite";

        private const float ArtworkPixelSize = 1254f;
        private const float ArtworkDisplaySize = 718f;
        private static readonly PieceType[] BackRankPieces =
        {
            PieceType.Rook,
            PieceType.Knight,
            PieceType.Bishop,
            PieceType.Queen,
            PieceType.King,
            PieceType.Bishop,
            PieceType.Knight,
            PieceType.Rook
        };

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
            PieceSpriteImportSettings.ConfigureAll();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            Canvas canvas = CreateCanvas();
            BoardView board = CreateBoard(canvas.transform);
            Text status = CreateInterfaceText(canvas.transform);
            PromotionControls promotion = CreatePromotionPanel(canvas.transform);
            Button restart = CreateButton(canvas.transform, "Restart", "Reiniciar partida", new Vector2(540f, -280f), new Vector2(270f, 58f));
            CreateGameController(canvas.transform, board, status, promotion, restart);
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

        private static BoardView CreateBoard(Transform canvas)
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
            CreateStandardPieces(board);
            Selection.activeGameObject = boardObject;
            return board;
        }

        private static void CreateStandardPieces(BoardView board)
        {
            for (int file = 0; file < BoardCoordinate.BoardSize; file++)
            {
                CreatePiece(board, new BoardCoordinate(file, 0), BackRankPieces[file], PieceColor.Light);
                CreatePiece(board, new BoardCoordinate(file, 1), PieceType.Pawn, PieceColor.Light);
                CreatePiece(board, new BoardCoordinate(file, 6), PieceType.Pawn, PieceColor.Dark);
                CreatePiece(board, new BoardCoordinate(file, 7), BackRankPieces[file], PieceColor.Dark);
            }
        }

        private static void CreatePiece(
            BoardView board,
            BoardCoordinate coordinate,
            PieceType type,
            PieceColor color)
        {
            if (!board.TryGetTile(coordinate, out BoardTileView tile))
            {
                Debug.LogError($"Could not place {color} {type} at {coordinate}.");
                return;
            }

            Sprite sprite = LoadPieceSprite(type, color);
            if (sprite == null)
            {
                Debug.LogError($"Missing sprite for {color} {type}.");
                return;
            }

            PieceView.Create(tile, type, color, coordinate, sprite);
        }

        private static Sprite LoadPieceSprite(PieceType type, PieceColor color)
        {
            string colorName = color == PieceColor.Light ? "Brancas" : "Pretas";
            string folder;
            string typeName;

            switch (type)
            {
                case PieceType.Pawn:
                    folder = "Pawn";
                    typeName = "Peao";
                    break;
                case PieceType.Rook:
                    folder = "Rook";
                    typeName = "Torre";
                    break;
                case PieceType.Knight:
                    folder = "Horse";
                    typeName = "Cavalo";
                    break;
                case PieceType.Bishop:
                    folder = "Bishop";
                    typeName = "Bispo";
                    break;
                case PieceType.Queen:
                    folder = "Queen";
                    typeName = "Rainha";
                    break;
                case PieceType.King:
                    folder = "King";
                    typeName = "Rei";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            string path = $"Assets/_Game/Sprites/Pieces/{folder}/{colorName}_{typeName}.png";
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Text CreateInterfaceText(Transform canvas)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateText(canvas, "Title", "Xadrez RPG", font, 42, new Vector2(0f, 425f));
            return CreateText(
                canvas,
                "Game Status",
                "Vez das peças claras",
                font,
                24,
                new Vector2(0f, -420f));
        }

        private static Text CreateText(
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
            return text;
        }

        private static PromotionControls CreatePromotionPanel(Transform canvas)
        {
            GameObject panelObject = new GameObject(
                "Promotion Panel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            panelObject.transform.SetParent(canvas, false);

            RectTransform rect = (RectTransform)panelObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(540f, 30f);
            rect.sizeDelta = new Vector2(300f, 270f);

            Image background = panelObject.GetComponent<Image>();
            background.color = new Color(0.08f, 0.09f, 0.1f, 0.96f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Text title = CreateText(panelObject.transform, "Promotion Title", "Promover peão para:", font, 24, new Vector2(0f, 88f));
            ((RectTransform)title.transform).sizeDelta = new Vector2(280f, 48f);

            PromotionControls controls = new PromotionControls
            {
                Panel = panelObject,
                Queen = CreateButton(panelObject.transform, "Queen", "Rainha", new Vector2(-72f, 25f), new Vector2(130f, 52f)),
                Rook = CreateButton(panelObject.transform, "Rook", "Torre", new Vector2(72f, 25f), new Vector2(130f, 52f)),
                Bishop = CreateButton(panelObject.transform, "Bishop", "Bispo", new Vector2(-72f, -45f), new Vector2(130f, 52f)),
                Knight = CreateButton(panelObject.transform, "Knight", "Cavalo", new Vector2(72f, -45f), new Vector2(130f, 52f))
            };
            panelObject.SetActive(false);
            return controls;
        }

        private static Button CreateButton(
            Transform parent,
            string objectName,
            string label,
            Vector2 position,
            Vector2 size)
        {
            GameObject buttonObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.36f, 0.24f, 0.13f, 1f);
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.58f, 0.4f, 0.2f, 1f);
            colors.pressedColor = new Color(0.24f, 0.15f, 0.08f, 1f);
            button.colors = colors;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Text text = CreateText(buttonObject.transform, "Label", label, font, 21, Vector2.zero);
            RectTransform textRect = (RectTransform)text.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            text.raycastTarget = false;
            return button;
        }

        private static void CreateGameController(
            Transform canvas,
            BoardView board,
            Text status,
            PromotionControls promotion,
            Button restart)
        {
            GameObject controllerObject = new GameObject("Chess Game Controller", typeof(ChessGameController));
            controllerObject.transform.SetParent(canvas, false);
            controllerObject.GetComponent<ChessGameController>().Configure(
                board,
                status,
                promotion.Panel,
                promotion.Queen,
                promotion.Rook,
                promotion.Bishop,
                promotion.Knight,
                restart);
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

        private sealed class PromotionControls
        {
            public GameObject Panel;
            public Button Queen;
            public Button Rook;
            public Button Bishop;
            public Button Knight;
        }
    }
}
