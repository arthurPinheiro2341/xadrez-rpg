using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XadrezRpg.Board
{
    [RequireComponent(typeof(RectTransform), typeof(GridLayoutGroup))]
    public sealed class BoardView : MonoBehaviour
    {
        private const int TileCountExpected = BoardCoordinate.BoardSize * BoardCoordinate.BoardSize;

        [Header("Layout")]
        [SerializeField] private Vector2 tileSize = new Vector2(88f, 88f);
        [SerializeField] private Vector2 spacing = new Vector2(2f, 2f);

        [Header("Colors")]
        [SerializeField] private Color lightTileColor = new Color(0.91f, 0.86f, 0.73f, 1f);
        [SerializeField] private Color darkTileColor = new Color(0.24f, 0.36f, 0.31f, 1f);
        [SerializeField] private Color selectedTileColor = new Color(1f, 0.66f, 0.12f, 1f);

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
            ConfigureGrid();
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

                Text label = CreateCoordinateLabel(tileObject.transform, labelFont, tileColor);
                BoardTileView tile = tileObject.GetComponent<BoardTileView>();
                tile.Initialize(this, coordinate, background, label, tileColor, selectedTileColor);
                tiles.Add(tile);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
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

        private void ConfigureGrid()
        {
            grid = GetComponent<GridLayoutGroup>();
            grid.cellSize = tileSize;
            grid.spacing = spacing;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = BoardCoordinate.BoardSize;
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
            return (coordinate.File + coordinate.Rank) % 2 == 0 ? darkTileColor : lightTileColor;
        }

        private static Text CreateCoordinateLabel(Transform tile, Font font, Color tileColor)
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

            float luminance = tileColor.r * 0.299f + tileColor.g * 0.587f + tileColor.b * 0.114f;
            label.color = luminance > 0.55f
                ? new Color(0.08f, 0.12f, 0.10f, 0.72f)
                : new Color(0.95f, 0.92f, 0.82f, 0.78f);
            return label;
        }
    }
}
