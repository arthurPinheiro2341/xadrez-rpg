using UnityEngine;
using UnityEngine.UI;
using XadrezRpg.Board;

namespace XadrezRpg.Pieces
{
    [RequireComponent(typeof(Image))]
    public sealed class PieceView : MonoBehaviour
    {
        private const float MaximumSourceHeight = 514f;
        private const float MaximumTileHeight = 0.92f;
        private const float BaseOffset = 0.035f;

        [SerializeField] private PieceType type;
        [SerializeField] private PieceColor color;
        [SerializeField] private BoardCoordinate coordinate;
        [SerializeField] private Image artwork;

        public PieceType Type => type;
        public PieceColor Color => color;
        public BoardCoordinate Coordinate => coordinate;
        public Sprite Sprite => artwork != null ? artwork.sprite : null;

        public static PieceView Create(
            BoardTileView tile,
            PieceType pieceType,
            PieceColor pieceColor,
            BoardCoordinate boardCoordinate,
            Sprite sprite)
        {
            GameObject pieceObject = new GameObject(
                $"{pieceColor} {pieceType} {boardCoordinate}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(PieceView));
            PieceView piece = pieceObject.GetComponent<PieceView>();
            piece.Initialize(pieceType, pieceColor, boardCoordinate, sprite);
            piece.PlaceOn(tile);
            return piece;
        }

        private void Awake()
        {
            ConfigureArtwork();
        }

        public void Initialize(
            PieceType pieceType,
            PieceColor pieceColor,
            BoardCoordinate boardCoordinate,
            Sprite sprite)
        {
            type = pieceType;
            color = pieceColor;
            coordinate = boardCoordinate;
            ConfigureArtwork();
            artwork.sprite = sprite;
            gameObject.name = $"{pieceColor} {pieceType} {boardCoordinate}";
        }

        public void PlaceOn(BoardTileView tile)
        {
            transform.SetParent(tile.transform, false);
            coordinate = tile.Coordinate;
            gameObject.name = $"{color} {type} {coordinate}";

            RectTransform tileRect = (RectTransform)tile.transform;
            RectTransform pieceRect = (RectTransform)transform;
            float spriteHeight = artwork.sprite != null ? artwork.sprite.rect.height : MaximumSourceHeight;
            float spriteWidth = artwork.sprite != null ? artwork.sprite.rect.width : spriteHeight;
            float displayHeight = tileRect.rect.height * MaximumTileHeight * spriteHeight / MaximumSourceHeight;
            float displayWidth = displayHeight * spriteWidth / spriteHeight;

            pieceRect.anchorMin = new Vector2(0.5f, 0f);
            pieceRect.anchorMax = new Vector2(0.5f, 0f);
            pieceRect.pivot = new Vector2(0.5f, 0f);
            pieceRect.anchoredPosition = new Vector2(0f, tileRect.rect.height * BaseOffset);
            pieceRect.sizeDelta = new Vector2(displayWidth, displayHeight);
        }

        private void ConfigureArtwork()
        {
            if (artwork == null)
            {
                artwork = GetComponent<Image>();
            }

            artwork.color = UnityEngine.Color.white;
            artwork.preserveAspect = true;
            artwork.raycastTarget = false;
        }
    }
}
