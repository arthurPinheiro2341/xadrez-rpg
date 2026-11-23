using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XadrezRpg.Board;
using XadrezRpg.Pieces;

namespace XadrezRpg.Turns
{
    public sealed class ChessGameController : MonoBehaviour
    {
        public static int RequestedMode;
        [SerializeField] private XadrezRpg.Combat.RpgBalance rpgBalance;
        [SerializeField] private BoardView boardView;
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject promotionPanel;
        [SerializeField] private Button queenPromotionButton;
        [SerializeField] private Button rookPromotionButton;
        [SerializeField] private Button bishopPromotionButton;
        [SerializeField] private Button knightPromotionButton;
        [SerializeField] private Button restartButton;

        private readonly ChessGame game = new ChessGame();
        private readonly Dictionary<BoardCoordinate, PieceView> pieceViews = new Dictionary<BoardCoordinate, PieceView>();
        private readonly Dictionary<(PieceColor Color, PieceType Type), Sprite> sprites =
            new Dictionary<(PieceColor Color, PieceType Type), Sprite>();

        private BoardCoordinate? selectedCoordinate;
        private IReadOnlyList<ChessMove> selectedMoves = Array.Empty<ChessMove>();
        private BoardCoordinate? pendingPromotionFrom;
        private BoardCoordinate? pendingPromotionTo;

        public ChessGame Game => game;

        public void Configure(
            BoardView board,
            Text status,
            GameObject promotion,
            Button queenButton,
            Button rookButton,
            Button bishopButton,
            Button knightButton,
            Button restart)
        {
            boardView = board;
            statusText = status;
            promotionPanel = promotion;
            queenPromotionButton = queenButton;
            rookPromotionButton = rookButton;
            bishopPromotionButton = bishopButton;
            knightPromotionButton = knightButton;
            restartButton = restart;
        }

        private void Start()
        {
            if (RequestedMode != 2)
            {
                promotionPanel.SetActive(false);
                restartButton.gameObject.SetActive(false);
                var rpg = gameObject.AddComponent<XadrezRpg.Combat.RpgController>();
                rpg.Initialize(boardView, statusText, rpgBalance, RequestedMode == 0);
                return;
            }
            CacheInitialPieceViews();
            boardView.SelectionChanged += OnBoardSelectionChanged;
            queenPromotionButton.onClick.AddListener(() => CompletePromotion(PieceType.Queen));
            rookPromotionButton.onClick.AddListener(() => CompletePromotion(PieceType.Rook));
            bishopPromotionButton.onClick.AddListener(() => CompletePromotion(PieceType.Bishop));
            knightPromotionButton.onClick.AddListener(() => CompletePromotion(PieceType.Knight));
            restartButton.onClick.AddListener(RestartGame);
            promotionPanel.SetActive(false);
            ClearSelection();
            UpdateStatus();
        }

        private void OnGUI()
        {
            if (RequestedMode == 2 && GUI.Button(new Rect(10, 10, 185, 32), "Jogar RPG contra bot"))
            {
                RequestedMode = 0;
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void OnDestroy()
        {
            if (boardView != null)
            {
                boardView.SelectionChanged -= OnBoardSelectionChanged;
            }
        }

        private void OnBoardSelectionChanged(BoardCoordinate coordinate)
        {
            if (pendingPromotionFrom.HasValue || game.Status != ChessGameStatus.Active)
            {
                RestoreMoveHints();
                return;
            }

            ChessPiece clickedPiece = game.GetPiece(coordinate);
            if (!selectedCoordinate.HasValue)
            {
                if (clickedPiece != null && clickedPiece.Color == game.CurrentTurn)
                {
                    SelectPiece(coordinate);
                }
                else
                {
                    ClearSelection();
                    UpdateStatus("Selecione uma peça do jogador da vez.");
                }

                return;
            }

            if (coordinate == selectedCoordinate.Value)
            {
                ClearSelection();
                UpdateStatus();
                return;
            }

            if (clickedPiece != null && clickedPiece.Color == game.CurrentTurn)
            {
                SelectPiece(coordinate);
                return;
            }

            ChessMove[] destinationMoves = selectedMoves.Where(move => move.To == coordinate).ToArray();
            if (destinationMoves.Length == 0)
            {
                RestoreMoveHints();
                UpdateStatus("Movimento inválido.");
                return;
            }

            if (destinationMoves.Any(move => move.Promotion.HasValue))
            {
                pendingPromotionFrom = selectedCoordinate;
                pendingPromotionTo = coordinate;
                promotionPanel.SetActive(true);
                UpdateStatus("Escolha a peça da promoção.");
                return;
            }

            ExecuteMove(selectedCoordinate.Value, coordinate, PieceType.Queen);
        }

        private void SelectPiece(BoardCoordinate coordinate)
        {
            selectedCoordinate = coordinate;
            selectedMoves = game.GetLegalMovesFrom(coordinate);
            RestoreMoveHints();
            UpdateStatus(selectedMoves.Count > 0
                ? $"{PieceName(game.GetPiece(coordinate).Type)} selecionado."
                : "Esta peça não possui movimentos legais.");
        }

        private void RestoreMoveHints()
        {
            boardView.ShowMoveHints(
                selectedCoordinate,
                selectedMoves.Select(move => move.To),
                selectedMoves.Where(move => move.IsCapture).Select(move => move.To));
        }

        private void ClearSelection()
        {
            selectedCoordinate = null;
            selectedMoves = Array.Empty<ChessMove>();
            boardView.ClearMoveHints();
        }

        private void CompletePromotion(PieceType pieceType)
        {
            if (!pendingPromotionFrom.HasValue || !pendingPromotionTo.HasValue)
            {
                return;
            }

            BoardCoordinate from = pendingPromotionFrom.Value;
            BoardCoordinate to = pendingPromotionTo.Value;
            pendingPromotionFrom = null;
            pendingPromotionTo = null;
            promotionPanel.SetActive(false);
            ExecuteMove(from, to, pieceType);
        }

        private void ExecuteMove(BoardCoordinate from, BoardCoordinate to, PieceType promotion)
        {
            if (!game.TryMove(from, to, promotion, out ChessMove executedMove))
            {
                RestoreMoveHints();
                UpdateStatus("Movimento inválido.");
                return;
            }

            PieceView movingView = pieceViews[from];
            BoardCoordinate capturedCoordinate = executedMove.IsEnPassant
                ? new BoardCoordinate(to.File, from.Rank)
                : to;
            if (pieceViews.TryGetValue(capturedCoordinate, out PieceView capturedView))
            {
                pieceViews.Remove(capturedCoordinate);
                capturedView.gameObject.SetActive(false);
                Destroy(capturedView.gameObject);
            }

            pieceViews.Remove(from);
            MoveView(movingView, to, game.GetPiece(to).Type);
            pieceViews[to] = movingView;

            if (executedMove.IsCastle)
            {
                int rookFromFile = to.File > from.File ? 7 : 0;
                int rookToFile = to.File > from.File ? 5 : 3;
                BoardCoordinate rookFrom = new BoardCoordinate(rookFromFile, from.Rank);
                BoardCoordinate rookTo = new BoardCoordinate(rookToFile, from.Rank);
                PieceView rookView = pieceViews[rookFrom];
                pieceViews.Remove(rookFrom);
                MoveView(rookView, rookTo, PieceType.Rook);
                pieceViews[rookTo] = rookView;
            }

            ClearSelection();
            UpdateStatus();
        }

        private void MoveView(PieceView view, BoardCoordinate destination, PieceType type)
        {
            if (!boardView.TryGetTile(destination, out BoardTileView tile))
            {
                throw new InvalidOperationException($"Missing board tile {destination}.");
            }

            PieceColor color = view.Color;
            view.Initialize(type, color, destination, sprites[(color, type)]);
            view.PlaceOn(tile);
        }

        private void RestartGame()
        {
            pendingPromotionFrom = null;
            pendingPromotionTo = null;
            promotionPanel.SetActive(false);
            game.Reset();

            foreach (PieceView view in pieceViews.Values)
            {
                if (view != null)
                {
                    view.gameObject.SetActive(false);
                    Destroy(view.gameObject);
                }
            }

            pieceViews.Clear();
            foreach ((BoardCoordinate coordinate, ChessPiece piece) in game.GetPieces())
            {
                if (!boardView.TryGetTile(coordinate, out BoardTileView tile))
                {
                    continue;
                }

                PieceView view = PieceView.Create(tile, piece.Type, piece.Color, coordinate, sprites[(piece.Color, piece.Type)]);
                pieceViews.Add(coordinate, view);
            }

            ClearSelection();
            UpdateStatus();
        }

        private void CacheInitialPieceViews()
        {
            pieceViews.Clear();
            PieceView[] views = boardView.GetComponentsInChildren<PieceView>(true);
            foreach (PieceView view in views)
            {
                pieceViews[view.Coordinate] = view;
                sprites[(view.Color, view.Type)] = view.Sprite;
            }

            if (sprites.Count != 12)
            {
                Debug.LogError($"Expected 12 standard piece sprites, found {sprites.Count}.");
            }
        }

        private void UpdateStatus(string temporaryMessage = null)
        {
            if (statusText == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(temporaryMessage))
            {
                statusText.text = temporaryMessage;
                return;
            }

            switch (game.Status)
            {
                case ChessGameStatus.Checkmate:
                    statusText.text = $"Xeque-mate! {ColorName(game.Winner.Value)} venceram.";
                    break;
                case ChessGameStatus.Stalemate:
                    statusText.text = "Empate por afogamento.";
                    break;
                default:
                    string check = game.IsInCheck(game.CurrentTurn) ? " — xeque!" : string.Empty;
                    statusText.text = $"Vez das {ColorName(game.CurrentTurn)}{check}";
                    break;
            }
        }

        private static string ColorName(PieceColor color)
        {
            return color == PieceColor.Light ? "peças claras" : "peças escuras";
        }

        private static string PieceName(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return "Peão";
                case PieceType.Rook: return "Torre";
                case PieceType.Knight: return "Cavalo";
                case PieceType.Bishop: return "Bispo";
                case PieceType.Queen: return "Rainha";
                case PieceType.King: return "Rei";
                default: return type.ToString();
            }
        }
    }
}
