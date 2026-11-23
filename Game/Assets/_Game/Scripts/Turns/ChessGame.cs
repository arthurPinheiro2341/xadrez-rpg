using System;
using System.Collections.Generic;
using System.Linq;
using XadrezRpg.Board;
using XadrezRpg.Pieces;

namespace XadrezRpg.Turns
{
    public sealed class ChessGame
    {
        public const string InitialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private static readonly PieceType[] PromotionTypes =
        {
            PieceType.Queen,
            PieceType.Rook,
            PieceType.Bishop,
            PieceType.Knight
        };

        private static readonly (int file, int rank)[] KnightOffsets =
        {
            (-2, -1), (-2, 1), (-1, -2), (-1, 2),
            (1, -2), (1, 2), (2, -1), (2, 1)
        };

        private static readonly (int file, int rank)[] KingOffsets =
        {
            (-1, -1), (-1, 0), (-1, 1), (0, -1),
            (0, 1), (1, -1), (1, 0), (1, 1)
        };

        private static readonly (int file, int rank)[] OrthogonalDirections =
        {
            (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        private static readonly (int file, int rank)[] DiagonalDirections =
        {
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };

        private readonly ChessPiece[,] board = new ChessPiece[BoardCoordinate.BoardSize, BoardCoordinate.BoardSize];

        public PieceColor CurrentTurn { get; private set; }
        public ChessGameStatus Status { get; private set; }
        public PieceColor? Winner { get; private set; }
        public BoardCoordinate? EnPassantTarget { get; private set; }
        public int HalfmoveClock { get; private set; }
        public int FullmoveNumber { get; private set; }

        public ChessGame()
        {
            Reset();
        }

        public void Reset()
        {
            LoadFen(InitialFen);
        }

        public ChessPiece GetPiece(BoardCoordinate coordinate)
        {
            return board[coordinate.File, coordinate.Rank];
        }

        public IEnumerable<(BoardCoordinate Coordinate, ChessPiece Piece)> GetPieces()
        {
            for (int file = 0; file < BoardCoordinate.BoardSize; file++)
            {
                for (int rank = 0; rank < BoardCoordinate.BoardSize; rank++)
                {
                    ChessPiece piece = board[file, rank];
                    if (piece != null)
                    {
                        yield return (new BoardCoordinate(file, rank), piece);
                    }
                }
            }
        }

        public IReadOnlyList<ChessMove> GetLegalMovesFrom(BoardCoordinate from)
        {
            ChessPiece piece = GetPiece(from);
            if (piece == null || piece.Color != CurrentTurn || Status != ChessGameStatus.Active)
            {
                return Array.Empty<ChessMove>();
            }

            return GeneratePseudoLegalMoves(from)
                .Where(move => IsMoveKingSafe(move, piece.Color))
                .ToArray();
        }

        public IReadOnlyList<ChessMove> GetAllLegalMoves(PieceColor color)
        {
            List<ChessMove> result = new List<ChessMove>();
            foreach ((BoardCoordinate coordinate, ChessPiece piece) in GetPieces())
            {
                if (piece.Color != color)
                {
                    continue;
                }

                foreach (ChessMove move in GeneratePseudoLegalMoves(coordinate))
                {
                    if (IsMoveKingSafe(move, color))
                    {
                        result.Add(move);
                    }
                }
            }

            return result;
        }

        public bool TryMove(
            BoardCoordinate from,
            BoardCoordinate to,
            PieceType promotion,
            out ChessMove executedMove)
        {
            executedMove = default;
            ChessMove? matchingMove = null;
            foreach (ChessMove move in GetLegalMovesFrom(from))
            {
                if (move.To == to && (!move.Promotion.HasValue || move.Promotion == promotion))
                {
                    matchingMove = move;
                    break;
                }
            }

            if (!matchingMove.HasValue)
            {
                return false;
            }

            executedMove = matchingMove.Value;
            ApplyMoveUnchecked(executedMove);
            UpdateGameStatus();
            return true;
        }

        public bool IsInCheck(PieceColor color)
        {
            BoardCoordinate? king = FindKing(color);
            return king.HasValue && IsSquareAttacked(king.Value, Opponent(color));
        }

        public bool IsSquareAttacked(BoardCoordinate square, PieceColor byColor)
        {
            int pawnDirection = byColor == PieceColor.Light ? 1 : -1;
            int pawnRank = square.Rank - pawnDirection;
            for (int offset = -1; offset <= 1; offset += 2)
            {
                if (TryCoordinate(square.File + offset, pawnRank, out BoardCoordinate pawnCoordinate))
                {
                    ChessPiece pawn = GetPiece(pawnCoordinate);
                    if (pawn != null && pawn.Color == byColor && pawn.Type == PieceType.Pawn)
                    {
                        return true;
                    }
                }
            }

            if (IsAttackedByOffsets(square, byColor, PieceType.Knight, KnightOffsets)
                || IsAttackedByOffsets(square, byColor, PieceType.King, KingOffsets)
                || IsAttackedBySlidingPiece(square, byColor, OrthogonalDirections, PieceType.Rook)
                || IsAttackedBySlidingPiece(square, byColor, DiagonalDirections, PieceType.Bishop))
            {
                return true;
            }

            return false;
        }

        public void LoadFen(string fen, bool evaluateStatus = true)
        {
            if (string.IsNullOrWhiteSpace(fen))
            {
                throw new ArgumentException("FEN cannot be empty.", nameof(fen));
            }

            Array.Clear(board, 0, board.Length);
            string[] fields = fen.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 4)
            {
                throw new FormatException("FEN must include placement, active color, castling and en passant fields.");
            }

            string[] ranks = fields[0].Split('/');
            if (ranks.Length != BoardCoordinate.BoardSize)
            {
                throw new FormatException("FEN placement must contain eight ranks.");
            }

            for (int fenRank = 0; fenRank < ranks.Length; fenRank++)
            {
                int file = 0;
                int rank = BoardCoordinate.BoardSize - 1 - fenRank;
                foreach (char symbol in ranks[fenRank])
                {
                    if (char.IsDigit(symbol))
                    {
                        file += symbol - '0';
                        continue;
                    }

                    PieceColor color = char.IsUpper(symbol) ? PieceColor.Light : PieceColor.Dark;
                    PieceType type = PieceTypeFromFen(char.ToLowerInvariant(symbol));
                    if (file >= BoardCoordinate.BoardSize)
                    {
                        throw new FormatException("FEN rank exceeds eight files.");
                    }

                    bool hasMoved = type != PieceType.Pawn
                        || rank != (color == PieceColor.Light ? 1 : 6);
                    board[file, rank] = new ChessPiece(type, color, hasMoved);
                    file++;
                }

                if (file != BoardCoordinate.BoardSize)
                {
                    throw new FormatException("Every FEN rank must contain eight files.");
                }
            }

            CurrentTurn = fields[1] == "w" ? PieceColor.Light : PieceColor.Dark;
            ApplyCastlingRights(fields[2]);
            EnPassantTarget = fields[3] == "-"
                ? (BoardCoordinate?)null
                : ParseCoordinateOrThrow(fields[3]);
            HalfmoveClock = fields.Length > 4 && int.TryParse(fields[4], out int halfmove) ? halfmove : 0;
            FullmoveNumber = fields.Length > 5 && int.TryParse(fields[5], out int fullmove) ? fullmove : 1;
            Status = ChessGameStatus.Active;
            Winner = null;
            if (evaluateStatus) UpdateGameStatus();
        }

        // RPG combat shares classical geometry, but permits attacking the King and ignores check.
        public IEnumerable<ChessMove> GeneratePseudoLegalMoves(BoardCoordinate from)
        {
            ChessPiece piece = GetPiece(from);
            if (piece == null)
            {
                yield break;
            }

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    foreach (ChessMove move in GeneratePawnMoves(from, piece)) yield return move;
                    break;
                case PieceType.Knight:
                    foreach (ChessMove move in GenerateOffsetMoves(from, piece, KnightOffsets)) yield return move;
                    break;
                case PieceType.Bishop:
                    foreach (ChessMove move in GenerateSlidingMoves(from, piece, DiagonalDirections)) yield return move;
                    break;
                case PieceType.Rook:
                    foreach (ChessMove move in GenerateSlidingMoves(from, piece, OrthogonalDirections)) yield return move;
                    break;
                case PieceType.Queen:
                    foreach (ChessMove move in GenerateSlidingMoves(from, piece, OrthogonalDirections)) yield return move;
                    foreach (ChessMove move in GenerateSlidingMoves(from, piece, DiagonalDirections)) yield return move;
                    break;
                case PieceType.King:
                    foreach (ChessMove move in GenerateOffsetMoves(from, piece, KingOffsets)) yield return move;
                    foreach (ChessMove move in GenerateCastlingMoves(from, piece)) yield return move;
                    break;
            }
        }

        private IEnumerable<ChessMove> GeneratePawnMoves(BoardCoordinate from, ChessPiece pawn)
        {
            int direction = pawn.Color == PieceColor.Light ? 1 : -1;
            int startingRank = pawn.Color == PieceColor.Light ? 1 : 6;
            int promotionRank = pawn.Color == PieceColor.Light ? 7 : 0;

            if (TryCoordinate(from.File, from.Rank + direction, out BoardCoordinate oneStep)
                && GetPiece(oneStep) == null)
            {
                foreach (ChessMove move in CreatePawnDestinationMoves(from, oneStep, promotionRank, false, false))
                {
                    yield return move;
                }

                if (from.Rank == startingRank
                    && !pawn.HasMoved
                    && TryCoordinate(from.File, from.Rank + direction * 2, out BoardCoordinate twoSteps)
                    && GetPiece(twoSteps) == null)
                {
                    yield return new ChessMove(from, twoSteps);
                }
            }

            for (int fileOffset = -1; fileOffset <= 1; fileOffset += 2)
            {
                if (!TryCoordinate(from.File + fileOffset, from.Rank + direction, out BoardCoordinate target))
                {
                    continue;
                }

                ChessPiece targetPiece = GetPiece(target);
                if (targetPiece != null && targetPiece.Color != pawn.Color)
                {
                    foreach (ChessMove move in CreatePawnDestinationMoves(from, target, promotionRank, true, false))
                    {
                        yield return move;
                    }
                }
                else if (EnPassantTarget.HasValue && EnPassantTarget.Value == target)
                {
                    BoardCoordinate capturedPawnCoordinate = new BoardCoordinate(target.File, from.Rank);
                    ChessPiece capturedPawn = GetPiece(capturedPawnCoordinate);
                    if (capturedPawn != null
                        && capturedPawn.Color != pawn.Color
                        && capturedPawn.Type == PieceType.Pawn)
                    {
                        yield return new ChessMove(from, target, null, true, true);
                    }
                }
            }
        }

        private static IEnumerable<ChessMove> CreatePawnDestinationMoves(
            BoardCoordinate from,
            BoardCoordinate to,
            int promotionRank,
            bool capture,
            bool enPassant)
        {
            if (to.Rank != promotionRank)
            {
                yield return new ChessMove(from, to, null, capture, enPassant);
                yield break;
            }

            foreach (PieceType promotion in PromotionTypes)
            {
                yield return new ChessMove(from, to, promotion, capture, enPassant);
            }
        }

        private IEnumerable<ChessMove> GenerateOffsetMoves(
            BoardCoordinate from,
            ChessPiece piece,
            IEnumerable<(int file, int rank)> offsets)
        {
            foreach ((int file, int rank) offset in offsets)
            {
                if (!TryCoordinate(from.File + offset.file, from.Rank + offset.rank, out BoardCoordinate to))
                {
                    continue;
                }

                ChessPiece occupant = GetPiece(to);
                if (occupant == null || occupant.Color != piece.Color)
                {
                    yield return new ChessMove(from, to, null, occupant != null);
                }
            }
        }

        private IEnumerable<ChessMove> GenerateSlidingMoves(
            BoardCoordinate from,
            ChessPiece piece,
            IEnumerable<(int file, int rank)> directions)
        {
            foreach ((int file, int rank) direction in directions)
            {
                int file = from.File + direction.file;
                int rank = from.Rank + direction.rank;
                while (TryCoordinate(file, rank, out BoardCoordinate to))
                {
                    ChessPiece occupant = GetPiece(to);
                    if (occupant == null)
                    {
                        yield return new ChessMove(from, to);
                    }
                    else
                    {
                        if (occupant.Color != piece.Color)
                        {
                            yield return new ChessMove(from, to, null, true);
                        }

                        break;
                    }

                    file += direction.file;
                    rank += direction.rank;
                }
            }
        }

        private IEnumerable<ChessMove> GenerateCastlingMoves(BoardCoordinate from, ChessPiece king)
        {
            if (king.HasMoved || IsInCheck(king.Color))
            {
                yield break;
            }

            int homeRank = king.Color == PieceColor.Light ? 0 : 7;
            if (from.File != 4 || from.Rank != homeRank)
            {
                yield break;
            }

            foreach (int rookFile in new[] { 7, 0 })
            {
                BoardCoordinate rookCoordinate = new BoardCoordinate(rookFile, homeRank);
                ChessPiece rook = GetPiece(rookCoordinate);
                if (rook == null || rook.Color != king.Color || rook.Type != PieceType.Rook || rook.HasMoved)
                {
                    continue;
                }

                int step = rookFile == 7 ? 1 : -1;
                bool pathClear = true;
                for (int file = from.File + step; file != rookFile; file += step)
                {
                    if (GetPiece(new BoardCoordinate(file, homeRank)) != null)
                    {
                        pathClear = false;
                        break;
                    }
                }

                if (!pathClear)
                {
                    continue;
                }

                BoardCoordinate crossing = new BoardCoordinate(from.File + step, homeRank);
                BoardCoordinate destination = new BoardCoordinate(from.File + step * 2, homeRank);
                if (!IsSquareAttacked(crossing, Opponent(king.Color))
                    && !IsSquareAttacked(destination, Opponent(king.Color)))
                {
                    yield return new ChessMove(from, destination, null, false, false, true);
                }
            }
        }

        private bool IsMoveKingSafe(ChessMove move, PieceColor movingColor)
        {
            ChessGame copy = Copy();
            copy.ApplyMoveUnchecked(move, false);
            return !copy.IsInCheck(movingColor);
        }

        private void ApplyMoveUnchecked(ChessMove move, bool switchTurn = true)
        {
            ChessPiece movingPiece = GetPiece(move.From);
            ChessPiece capturedPiece = GetPiece(move.To);
            board[move.From.File, move.From.Rank] = null;

            if (move.IsEnPassant)
            {
                BoardCoordinate capturedPawn = new BoardCoordinate(move.To.File, move.From.Rank);
                capturedPiece = GetPiece(capturedPawn);
                board[capturedPawn.File, capturedPawn.Rank] = null;
            }

            if (move.IsCastle)
            {
                int rookFromFile = move.To.File > move.From.File ? 7 : 0;
                int rookToFile = move.To.File > move.From.File ? 5 : 3;
                ChessPiece rook = board[rookFromFile, move.From.Rank];
                board[rookFromFile, move.From.Rank] = null;
                board[rookToFile, move.From.Rank] = rook;
                rook.HasMoved = true;
            }

            board[move.To.File, move.To.Rank] = movingPiece;
            movingPiece.HasMoved = true;
            if (move.Promotion.HasValue)
            {
                movingPiece.Type = move.Promotion.Value;
            }

            EnPassantTarget = null;
            if (movingPiece.Type == PieceType.Pawn && Math.Abs(move.To.Rank - move.From.Rank) == 2)
            {
                EnPassantTarget = new BoardCoordinate(move.From.File, (move.From.Rank + move.To.Rank) / 2);
            }

            HalfmoveClock = movingPiece.Type == PieceType.Pawn || capturedPiece != null ? 0 : HalfmoveClock + 1;
            if (!switchTurn)
            {
                return;
            }

            if (CurrentTurn == PieceColor.Dark)
            {
                FullmoveNumber++;
            }

            CurrentTurn = Opponent(CurrentTurn);
        }

        private void UpdateGameStatus()
        {
            bool anyLegalMove = GetAllLegalMoves(CurrentTurn).Count > 0;
            if (anyLegalMove)
            {
                Status = ChessGameStatus.Active;
                Winner = null;
                return;
            }

            if (IsInCheck(CurrentTurn))
            {
                Status = ChessGameStatus.Checkmate;
                Winner = Opponent(CurrentTurn);
            }
            else
            {
                Status = ChessGameStatus.Stalemate;
                Winner = null;
            }
        }

        private BoardCoordinate? FindKing(PieceColor color)
        {
            foreach ((BoardCoordinate coordinate, ChessPiece piece) in GetPieces())
            {
                if (piece.Color == color && piece.Type == PieceType.King)
                {
                    return coordinate;
                }
            }

            return null;
        }

        private bool IsAttackedByOffsets(
            BoardCoordinate square,
            PieceColor byColor,
            PieceType type,
            IEnumerable<(int file, int rank)> offsets)
        {
            foreach ((int file, int rank) offset in offsets)
            {
                if (!TryCoordinate(square.File + offset.file, square.Rank + offset.rank, out BoardCoordinate coordinate))
                {
                    continue;
                }

                ChessPiece piece = GetPiece(coordinate);
                if (piece != null && piece.Color == byColor && piece.Type == type)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsAttackedBySlidingPiece(
            BoardCoordinate square,
            PieceColor byColor,
            IEnumerable<(int file, int rank)> directions,
            PieceType primaryType)
        {
            foreach ((int file, int rank) direction in directions)
            {
                int file = square.File + direction.file;
                int rank = square.Rank + direction.rank;
                while (TryCoordinate(file, rank, out BoardCoordinate coordinate))
                {
                    ChessPiece piece = GetPiece(coordinate);
                    if (piece == null)
                    {
                        file += direction.file;
                        rank += direction.rank;
                        continue;
                    }

                    if (piece.Color == byColor && (piece.Type == primaryType || piece.Type == PieceType.Queen))
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        private void ApplyCastlingRights(string rights)
        {
            SetCastlingPiecesMoved(PieceColor.Light, !rights.Contains('K'), !rights.Contains('Q'));
            SetCastlingPiecesMoved(PieceColor.Dark, !rights.Contains('k'), !rights.Contains('q'));
        }

        private void SetCastlingPiecesMoved(PieceColor color, bool kingSideMoved, bool queenSideMoved)
        {
            int rank = color == PieceColor.Light ? 0 : 7;
            ChessPiece king = board[4, rank];
            if (king != null && king.Type == PieceType.King && king.Color == color)
            {
                king.HasMoved = kingSideMoved && queenSideMoved;
            }

            ChessPiece kingSideRook = board[7, rank];
            if (kingSideRook != null && kingSideRook.Type == PieceType.Rook && kingSideRook.Color == color)
            {
                kingSideRook.HasMoved = kingSideMoved;
            }

            ChessPiece queenSideRook = board[0, rank];
            if (queenSideRook != null && queenSideRook.Type == PieceType.Rook && queenSideRook.Color == color)
            {
                queenSideRook.HasMoved = queenSideMoved;
            }
        }

        private ChessGame Copy()
        {
            ChessGame copy = new ChessGame(false)
            {
                CurrentTurn = CurrentTurn,
                Status = Status,
                Winner = Winner,
                EnPassantTarget = EnPassantTarget,
                HalfmoveClock = HalfmoveClock,
                FullmoveNumber = FullmoveNumber
            };

            for (int file = 0; file < BoardCoordinate.BoardSize; file++)
            {
                for (int rank = 0; rank < BoardCoordinate.BoardSize; rank++)
                {
                    copy.board[file, rank] = board[file, rank]?.Copy();
                }
            }

            return copy;
        }

        private ChessGame(bool initialize)
        {
            if (initialize)
            {
                Reset();
            }
        }

        private static bool TryCoordinate(int file, int rank, out BoardCoordinate coordinate)
        {
            if (file < 0 || file >= BoardCoordinate.BoardSize || rank < 0 || rank >= BoardCoordinate.BoardSize)
            {
                coordinate = default;
                return false;
            }

            coordinate = new BoardCoordinate(file, rank);
            return true;
        }

        private static PieceType PieceTypeFromFen(char symbol)
        {
            switch (symbol)
            {
                case 'p': return PieceType.Pawn;
                case 'r': return PieceType.Rook;
                case 'n': return PieceType.Knight;
                case 'b': return PieceType.Bishop;
                case 'q': return PieceType.Queen;
                case 'k': return PieceType.King;
                default: throw new FormatException($"Unknown FEN piece '{symbol}'.");
            }
        }

        private static BoardCoordinate ParseCoordinateOrThrow(string notation)
        {
            if (!BoardCoordinate.TryParse(notation, out BoardCoordinate coordinate))
            {
                throw new FormatException($"Invalid FEN coordinate '{notation}'.");
            }

            return coordinate;
        }

        private static PieceColor Opponent(PieceColor color)
        {
            return color == PieceColor.Light ? PieceColor.Dark : PieceColor.Light;
        }
    }
}
