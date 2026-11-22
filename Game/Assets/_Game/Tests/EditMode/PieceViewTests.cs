using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using XadrezRpg.Board;
using XadrezRpg.Pieces;

namespace XadrezRpg.Tests.EditMode
{
    public sealed class PieceViewTests
    {
        [Test]
        public void Initialize_StoresStandardPieceVisualData()
        {
            GameObject pieceObject = new GameObject(
                "Piece Test",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(PieceView));
            Texture2D texture = new Texture2D(1, 1);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);

            try
            {
                PieceView piece = pieceObject.GetComponent<PieceView>();
                BoardCoordinate coordinate = new BoardCoordinate(4, 0);
                piece.Initialize(PieceType.King, PieceColor.Light, coordinate, sprite);

                Assert.That(piece.Type, Is.EqualTo(PieceType.King));
                Assert.That(piece.Color, Is.EqualTo(PieceColor.Light));
                Assert.That(piece.Coordinate, Is.EqualTo(coordinate));
                Assert.That(piece.Sprite, Is.SameAs(sprite));
                Assert.That(pieceObject.GetComponent<Image>().raycastTarget, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(pieceObject);
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }
    }
}
