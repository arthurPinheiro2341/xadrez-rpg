using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XadrezRpg.Board
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class BoardView : MonoBehaviour
    {
        private const int TileCountExpected = BoardCoordinate.BoardSize * BoardCoordinate.BoardSize;

        [Header("Layout")]
        [SerializeField] private Vector2 tileSize = new Vector2(88f, 88f);
        [SerializeField] private Vector2 spacing = new Vector2(2f, 2f);

        [Header("Artwork")]
        [SerializeField] private Sprite artworkSprite;
        [SerializeField] private bool showCoordinateLabels = true;
        [SerializeField] private Vector2 artworkPixelSize;
        [SerializeField] private float artworkDisplaySize;
        [SerializeField] private float[] artworkColumnEdges;
        [SerializeField] private float[] artworkRowEdges;

        [Header("Colors")]
        [SerializeField] private Color lightTileColor = new Color(0.91f, 0.86f, 0.73f, 1f);
        [SerializeField] private Color darkTileColor = new Color(0.24f, 0.36f, 0.31f, 1f);
        [SerializeField] private Color selectedTileColor = new Color(1f, 0.66f, 0.12f, 0.48f);

        [SerializeField] private List<BoardTileView> tiles = new List<BoardTileView>(TileCountExpected);

        private GridLayoutGroup grid;
        private BoardTileView selectedTile;

        public int TileCount => tiles.Count;
        public BoardCoordinate? SelectedCoordinate => selectedTile != null ? selectedTile.Coordinate : null;

        public event Action<BoardCoordinate> SelectionChanged;

        private void Awake()
        {
            if (!HasCompleteBoard())
            {
                GenerateBoard();
            }
        }

        public void GenerateBoard()
        {
            bool useMeasuredArtworkLayout = HasMeasuredArtworkLayout();
            ConfigureGrid(!useMeasuredArtworkLayout);
            ClearBoard();

            Font labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            for (int displayIndex = 0; displayIndex < TileCountExpected; displayIndex++)
            {
                BoardCoordinate coordinate = BoardCoordinate.FromDisplayIndex(displayIndex);
                Color tileColor = GetTileColor(coordinate);

                GameObject tileObject = new GameObject(
                    coordinate.ToString(),
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(BoardTileView));
                tileObject.transform.SetParent(transform, false);

                Image background = tileObject.GetComponent<Image>();
                background.color = tileColor;
                background.raycastTarget = true;

                Text label = showCoordinateLabels
                    ? CreateCoordinateLabel(tileObject.transform, labelFont, coordinate)
                    : null;
                BoardTileView tile = tileObject.GetComponent<BoardTileView>();
                tile.Initialize(this, coordinate, background, label, tileColor, selectedTileColor);

                if (useMeasuredArtworkLayout)
                {
                    ConfigureArtworkTileRect((RectTransform)tileObject.transform, displayIndex);
                }

                tiles.Add(tile);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }

        public void SetArtwork(Sprite sprite, bool hideCoordinateLabels = true)
        {
            artworkSprite = sprite;
            showCoordinateLabels = !hideCoordinateLabels;
        }

        public void SetTileLayout(Vector2 size, Vector2 tileSpacing)
        {
            tileSize = size;
            spacing = tileSpacing;
        }

        public void SetArtworkLayout(
            Vector2 sourcePixelSize,
            float displaySize,
            float[] columnEdges,
            float[] rowEdges)
        {
            artworkPixelSize = sourcePixelSize;
            artworkDisplaySize = displaySize;
            artworkColumnEdges = columnEdges != null ? (float[])columnEdges.Clone() : null;
            artworkRowEdges = rowEdges != null ? (float[])rowEdges.Clone() : null;
        }

        public bool TryGetTile(BoardCoordinate coordinate, out BoardTileView tile)
        {
            tile = null;
            if (!coordinate.IsValid)
            {
                return false;
            }

            int index = coordinate.ToDisplayIndex();
            if (index < 0 || index >= tiles.Count)
            {
                return false;
            }

            tile = tiles[index];
            return tile != null && tile.Coordinate == coordinate;
        }

        public bool TrySelect(BoardCoordinate coordinate)
        {
            if (!TryGetTile(coordinate, out BoardTileView tile))
            {
                return false;
            }

            SelectTile(tile);
            return true;
        }

        public void SelectTile(BoardTileView tile)
        {
            if (tile == null || !tiles.Contains(tile))
            {
                return;
            }

            if (selectedTile != null)
            {
                selectedTile.SetSelected(false);
            }

            selectedTile = tile;
            selectedTile.SetSelected(true);
            SelectionChanged?.Invoke(selectedTile.Coordinate);
        }

        public void ShowMoveHints(
            BoardCoordinate? selected,
            IEnumerable<BoardCoordinate> legalMoves,
            IEnumerable<BoardCoordinate> captures)
        {
            HashSet<BoardCoordinate> legalSet = legalMoves != null
                ? new HashSet<BoardCoordinate>(legalMoves)
                : new HashSet<BoardCoordinate>();
            HashSet<BoardCoordinate> captureSet = captures != null
                ? new HashSet<BoardCoordinate>(captures)
                : new HashSet<BoardCoordinate>();

            selectedTile = null;
            foreach (BoardTileView tile in tiles)
            {
                bool isSelected = selected.HasValue && tile.Coordinate == selected.Value;
                tile.SetMoveHint(isSelected, legalSet.Contains(tile.Coordinate), captureSet.Contains(tile.Coordinate));
                if (isSelected)
                {
                    selectedTile = tile;
                }
            }
        }

        public void ClearMoveHints()
        {
            ShowMoveHints(null, null, null);
        }

        public Vector3 GetTileCenterWorldPosition(BoardCoordinate coordinate)
        {
            if (!TryGetTile(coordinate, out BoardTileView tile))
            {
                throw new ArgumentOutOfRangeException(nameof(coordinate), coordinate, "Coordinate is not present on this board.");
            }

            RectTransform tileRect = (RectTransform)tile.transform;
            return tileRect.TransformPoint(tileRect.rect.center);
        }

        private bool HasCompleteBoard()
        {
            if (tiles.Count != TileCountExpected)
            {
                return false;
            }

            for (int index = 0; index < tiles.Count; index++)
            {
                if (tiles[index] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void ConfigureGrid(bool enabled)
        {
            grid = GetComponent<GridLayoutGroup>();
            if (grid == null && enabled)
            {
                grid = gameObject.AddComponent<GridLayoutGroup>();
            }

            if (grid == null)
            {
                return;
            }

            grid.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            grid.cellSize = tileSize;
            grid.spacing = spacing;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = BoardCoordinate.BoardSize;
        }

        private bool HasMeasuredArtworkLayout()
        {
            int expectedEdgeCount = BoardCoordinate.BoardSize + 1;
            return artworkSprite != null
                && artworkPixelSize.x > 0f
                && artworkPixelSize.y > 0f
                && artworkDisplaySize > 0f
                && artworkColumnEdges != null
                && artworkColumnEdges.Length == expectedEdgeCount
                && artworkRowEdges != null
                && artworkRowEdges.Length == expectedEdgeCount;
        }

        private void ConfigureArtworkTileRect(RectTransform tileRect, int displayIndex)
        {
            int column = displayIndex % BoardCoordinate.BoardSize;
            int row = displayIndex / BoardCoordinate.BoardSize;
            float scaleX = artworkDisplaySize / artworkPixelSize.x;
            float scaleY = artworkDisplaySize / artworkPixelSize.y;

            float left = artworkColumnEdges[column] * scaleX;
            float right = artworkColumnEdges[column + 1] * scaleX;
            float top = artworkRowEdges[row] * scaleY;
            float bottom = artworkRowEdges[row + 1] * scaleY;

            tileRect.anchorMin = new Vector2(0.5f, 0.5f);
            tileRect.anchorMax = new Vector2(0.5f, 0.5f);
            tileRect.pivot = new Vector2(0.5f, 0.5f);
            tileRect.sizeDelta = new Vector2(right - left, bottom - top);
            tileRect.anchoredPosition = new Vector2(
                -artworkDisplaySize * 0.5f + (left + right) * 0.5f,
                artworkDisplaySize * 0.5f - (top + bottom) * 0.5f);
        }

        private void ClearBoard()
        {
            selectedTile = null;
            tiles.Clear();

            for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
            {
                GameObject child = transform.GetChild(childIndex).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private Color GetTileColor(BoardCoordinate coordinate)
        {
            if (artworkSprite != null)
            {
                return Color.clear;
            }

            return (coordinate.File + coordinate.Rank) % 2 == 0 ? darkTileColor : lightTileColor;
        }

        private static Text CreateCoordinateLabel(Transform tile, Font font, BoardCoordinate coordinate)
        {
            GameObject labelObject = new GameObject(
                "Coordinate",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            labelObject.transform.SetParent(tile, false);

            RectTransform labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(6f, 4f);
            labelRect.offsetMax = new Vector2(-4f, -4f);

            Text label = labelObject.GetComponent<Text>();
            label.font = font;
            label.fontSize = 17;
            label.alignment = TextAnchor.LowerLeft;
            label.raycastTarget = false;
            bool lightSquare = (coordinate.File + coordinate.Rank) % 2 != 0;
            label.color = lightSquare
                ? new Color(0.08f, 0.12f, 0.10f, 0.72f)
                : new Color(0.95f, 0.92f, 0.82f, 0.78f);
            return label;
        }
    }
}
