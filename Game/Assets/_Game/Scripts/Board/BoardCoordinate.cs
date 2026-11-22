using System;
using UnityEngine;

namespace XadrezRpg.Board
{
    [Serializable]
    public struct BoardCoordinate : IEquatable<BoardCoordinate>
    {
        public const int BoardSize = 8;

        [SerializeField] private int file;
        [SerializeField] private int rank;

        public int File => file;
        public int Rank => rank;
        public bool IsValid => file >= 0 && file < BoardSize && rank >= 0 && rank < BoardSize;

        public BoardCoordinate(int file, int rank)
        {
            if (file < 0 || file >= BoardSize)
            {
                throw new ArgumentOutOfRangeException(nameof(file), file, "File must be between 0 and 7.");
            }

            if (rank < 0 || rank >= BoardSize)
            {
                throw new ArgumentOutOfRangeException(nameof(rank), rank, "Rank must be between 0 and 7.");
            }

            this.file = file;
            this.rank = rank;
        }

        public int ToDisplayIndex()
        {
            EnsureValid();
            return (BoardSize - 1 - rank) * BoardSize + file;
        }

        public static BoardCoordinate FromDisplayIndex(int displayIndex)
        {
            if (displayIndex < 0 || displayIndex >= BoardSize * BoardSize)
            {
                throw new ArgumentOutOfRangeException(nameof(displayIndex), displayIndex, "Display index must be between 0 and 63.");
            }

            int rowFromTop = displayIndex / BoardSize;
            int file = displayIndex % BoardSize;
            int rank = BoardSize - 1 - rowFromTop;
            return new BoardCoordinate(file, rank);
        }

        public static bool TryParse(string notation, out BoardCoordinate coordinate)
        {
            coordinate = default;
            if (string.IsNullOrWhiteSpace(notation))
            {
                return false;
            }

            string trimmed = notation.Trim().ToLowerInvariant();
            if (trimmed.Length != 2)
            {
                return false;
            }

            int parsedFile = trimmed[0] - 'a';
            int parsedRank = trimmed[1] - '1';
            if (parsedFile < 0 || parsedFile >= BoardSize || parsedRank < 0 || parsedRank >= BoardSize)
            {
                return false;
            }

            coordinate = new BoardCoordinate(parsedFile, parsedRank);
            return true;
        }

        public bool Equals(BoardCoordinate other)
        {
            return file == other.file && rank == other.rank;
        }

        public override bool Equals(object obj)
        {
            return obj is BoardCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(file, rank);
        }

        public override string ToString()
        {
            EnsureValid();
            return $"{(char)('a' + file)}{rank + 1}";
        }

        public static bool operator ==(BoardCoordinate left, BoardCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoardCoordinate left, BoardCoordinate right)
        {
            return !left.Equals(right);
        }

        private void EnsureValid()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("The board coordinate is not valid.");
            }
        }
    }
}
