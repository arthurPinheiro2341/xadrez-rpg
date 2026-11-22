using System.Linq;
using NUnit.Framework;
using XadrezRpg.Board;
using XadrezRpg.Pieces;
using XadrezRpg.Turns;

namespace XadrezRpg.Tests.EditMode
{
    public sealed class ChessGameTests
    {
        [Test]
        public void InitialPosition_HasThirtyTwoPiecesAndLightToMove()
        {
            ChessGame game = new ChessGame();

            Assert.That(game.GetPieces().Count(), Is.EqualTo(32));
            Assert.That(game.CurrentTurn, Is.EqualTo(PieceColor.Light));
            Assert.That(game.Status, Is.EqualTo(ChessGameStatus.Active));
        }

        [Test]
        public void Pawn_CanAdvanceOneOrTwoSquaresFromInitialPosition()
        {
            ChessGame game = new ChessGame();

            string[] destinations = game.GetLegalMovesFrom(Coordinate("e2"))
                .Select(move => move.To.ToString())
                .ToArray();

            Assert.That(destinations, Is.EquivalentTo(new[] { "e3", "e4" }));
        }

        [Test]
        public void Move_ChangesTurnAndBoardOccupation()
        {
            ChessGame game = new ChessGame();

            bool moved = game.TryMove(Coordinate("e2"), Coordinate("e4"), PieceType.Queen, out ChessMove move);

            Assert.That(moved, Is.True);
            Assert.That(move.To, Is.EqualTo(Coordinate("e4")));
            Assert.That(game.GetPiece(Coordinate("e2")), Is.Null);
            Assert.That(game.GetPiece(Coordinate("e4")).Type, Is.EqualTo(PieceType.Pawn));
            Assert.That(game.CurrentTurn, Is.EqualTo(PieceColor.Dark));
        }

        [Test]
        public void FoolMate_EndsInCheckmateForDark()
        {
            ChessGame game = new ChessGame();

            Move(game, "f2", "f3");
            Move(game, "e7", "e5");
            Move(game, "g2", "g4");
            Move(game, "d8", "h4");

            Assert.That(game.Status, Is.EqualTo(ChessGameStatus.Checkmate));
            Assert.That(game.Winner, Is.EqualTo(PieceColor.Dark));
            Assert.That(game.IsInCheck(PieceColor.Light), Is.True);
        }

        [Test]
        public void Castling_MovesKingAndRook()
        {
            ChessGame game = new ChessGame();
            game.LoadFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

            Move(game, "e1", "g1");

            Assert.That(game.GetPiece(Coordinate("g1")).Type, Is.EqualTo(PieceType.King));
            Assert.That(game.GetPiece(Coordinate("f1")).Type, Is.EqualTo(PieceType.Rook));
            Assert.That(game.GetPiece(Coordinate("e1")), Is.Null);
            Assert.That(game.GetPiece(Coordinate("h1")), Is.Null);
        }

        [Test]
        public void EnPassant_RemovesPassedPawn()
        {
            ChessGame game = new ChessGame();
            game.LoadFen("4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1");

            Move(game, "e5", "d6");

            Assert.That(game.GetPiece(Coordinate("d6")).Color, Is.EqualTo(PieceColor.Light));
            Assert.That(game.GetPiece(Coordinate("d5")), Is.Null);
        }

        [Test]
        public void Promotion_UsesSelectedStandardPiece()
        {
            ChessGame game = new ChessGame();
            game.LoadFen("4k3/P7/8/8/8/8/8/4K3 w - - 0 1");

            bool moved = game.TryMove(Coordinate("a7"), Coordinate("a8"), PieceType.Knight, out _);

            Assert.That(moved, Is.True);
            Assert.That(game.GetPiece(Coordinate("a8")).Type, Is.EqualTo(PieceType.Knight));
        }

        [Test]
        public void PinnedPiece_CannotExposeOwnKing()
        {
            ChessGame game = new ChessGame();
            game.LoadFen("4r1k1/8/8/8/8/8/4R3/4K3 w - - 0 1");

            string[] destinations = game.GetLegalMovesFrom(Coordinate("e2"))
                .Select(move => move.To.ToString())
                .ToArray();

            Assert.That(destinations, Does.Not.Contain("d2"));
            Assert.That(destinations, Does.Not.Contain("f2"));
        }

        [Test]
        public void PositionWithoutLegalMoveAndWithoutCheck_IsStalemate()
        {
            ChessGame game = new ChessGame();

            game.LoadFen("7k/5Q2/6K1/8/8/8/8/8 b - - 0 1");

            Assert.That(game.Status, Is.EqualTo(ChessGameStatus.Stalemate));
            Assert.That(game.Winner, Is.Null);
        }

        private static void Move(ChessGame game, string from, string to)
        {
            Assert.That(
                game.TryMove(Coordinate(from), Coordinate(to), PieceType.Queen, out _),
                Is.True,
                $"Expected {from}-{to} to be legal.");
        }

        private static BoardCoordinate Coordinate(string notation)
        {
            Assert.That(BoardCoordinate.TryParse(notation, out BoardCoordinate coordinate), Is.True);
            return coordinate;
        }
    }
}
