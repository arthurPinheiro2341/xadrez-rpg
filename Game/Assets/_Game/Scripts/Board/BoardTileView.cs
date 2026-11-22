using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XadrezRpg.Board
{
    [RequireComponent(typeof(Image))]
    public sealed class BoardTileView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private BoardView board;
        [SerializeField] private BoardCoordinate coordinate;
        [SerializeField] private Image background;
        [SerializeField] private Text coordinateLabel;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color selectedColor;

        public BoardCoordinate Coordinate => coordinate;
        public bool IsSelected { get; private set; }

        private void Awake()
        {
            if (board == null)
            {
                board = GetComponentInParent<BoardView>();
            }

            if (background == null)
            {
                background = GetComponent<Image>();
            }
        }

        public void Initialize(
            BoardView owner,
            BoardCoordinate boardCoordinate,
            Image tileBackground,
            Text label,
            Color tileColor,
            Color highlightColor)
        {
            board = owner;
            coordinate = boardCoordinate;
            background = tileBackground;
            coordinateLabel = label;
            normalColor = tileColor;
            selectedColor = highlightColor;

            if (coordinateLabel != null)
            {
                coordinateLabel.text = coordinate.ToString();
            }

            SetSelected(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            board?.SelectTile(this);
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            if (background != null)
            {
                background.color = selected ? selectedColor : normalColor;
            }
        }
    }
}
