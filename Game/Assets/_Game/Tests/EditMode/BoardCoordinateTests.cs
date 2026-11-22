using NUnit.Framework;
using UnityEngine;
using XadrezRpg.Board;

namespace XadrezRpg.Tests.EditMode
{
    public sealed class BoardCoordinateTests
    {
        [TestCase(0, 0, "a1")]
        [TestCase(7, 0, "h1")]
        [TestCase(0, 7, "a8")]
        [TestCase(7, 7, "h8")]
        public void ToString_ReturnsAlgebraicNotation(int file, int rank, string expected)
        {
            Assert.That(new BoardCoordinate(file, rank).ToString(), Is.EqualTo(expected));
        }

        [TestCase("a1", 0, 0)]
        [TestCase("H8", 7, 7)]
        [TestCase(" d5 ", 3, 4)]
        public void TryParse_AcceptsValidNotation(string notation, int expectedFile, int expectedRank)
        {
            bool parsed = BoardCoordinate.TryParse(notation, out BoardCoordinate coordinate);

            Assert.That(parsed, Is.True);
            Assert.That(coordinate, Is.EqualTo(new BoardCoordinate(expectedFile, expectedRank)));
        }

        [TestCase("")]
        [TestCase("a0")]
        [TestCase("a9")]
        [TestCase("i1")]
        [TestCase("aa1")]
        public void TryParse_RejectsInvalidNotation(string notation)
        {
            Assert.That(BoardCoordinate.TryParse(notation, out _), Is.False);
        }

        [Test]
        public void DisplayIndex_UsesRankEightAtTheTop()
        {
            Assert.That(new BoardCoordinate(0, 7).ToDisplayIndex(), Is.EqualTo(0));
            Assert.That(new BoardCoordinate(7, 7).ToDisplayIndex(), Is.EqualTo(7));
            Assert.That(new BoardCoordinate(0, 0).ToDisplayIndex(), Is.EqualTo(56));
            Assert.That(new BoardCoordinate(7, 0).ToDisplayIndex(), Is.EqualTo(63));
        }

        [Test]
        public void GenerateBoard_CreatesAndSelectsAllCoordinates()
        {
            GameObject boardObject = new GameObject("Board Test", typeof(RectTransform), typeof(BoardView));
            try
            {
                BoardView board = boardObject.GetComponent<BoardView>();
                board.GenerateBoard();

                Assert.That(board.TileCount, Is.EqualTo(64));
                Assert.That(board.TryGetTile(new BoardCoordinate(0, 0), out BoardTileView a1), Is.True);
                Assert.That(a1.Coordinate.ToString(), Is.EqualTo("a1"));
                Assert.That(board.TrySelect(new BoardCoordinate(4, 3)), Is.True);
                Assert.That(board.SelectedCoordinate, Is.EqualTo(new BoardCoordinate(4, 3)));
            }
            finally
            {
                Object.DestroyImmediate(boardObject);
            }
        }
    }
}
