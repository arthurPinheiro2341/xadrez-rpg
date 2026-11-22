using System;

namespace XadrezRpg.Pieces
{
    [Serializable]
    public sealed class ChessPiece
    {
        public PieceType Type { get; internal set; }
        public PieceColor Color { get; }
        public bool HasMoved { get; internal set; }

        public ChessPiece(PieceType type, PieceColor color, bool hasMoved = false)
        {
            Type = type;
            Color = color;
            HasMoved = hasMoved;
        }

        public ChessPiece Copy()
        {
            return new ChessPiece(Type, Color, HasMoved);
        }
    }
}
