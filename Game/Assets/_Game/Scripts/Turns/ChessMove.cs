using System;
using XadrezRpg.Board;
using XadrezRpg.Pieces;

namespace XadrezRpg.Turns
{
    public readonly struct ChessMove : IEquatable<ChessMove>
    {
        public BoardCoordinate From { get; }
        public BoardCoordinate To { get; }
        public PieceType? Promotion { get; }
        public bool IsCapture { get; }
        public bool IsEnPassant { get; }
        public bool IsCastle { get; }

        public ChessMove(
            BoardCoordinate from,
            BoardCoordinate to,
            PieceType? promotion = null,
            bool isCapture = false,
            bool isEnPassant = false,
            bool isCastle = false)
        {
            From = from;
            To = to;
            Promotion = promotion;
            IsCapture = isCapture;
            IsEnPassant = isEnPassant;
            IsCastle = isCastle;
        }

        public bool Equals(ChessMove other)
        {
            return From == other.From && To == other.To && Promotion == other.Promotion;
        }

        public override bool Equals(object obj)
        {
            return obj is ChessMove other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From, To, Promotion);
        }

        public override string ToString()
        {
            string suffix = Promotion.HasValue ? $"={Promotion.Value}" : string.Empty;
            return $"{From}-{To}{suffix}";
        }
    }
}
