using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XadrezRpg.Board;
using XadrezRpg.Pieces;
using XadrezRpg.Turns;

namespace XadrezRpg.Combat
{
    public sealed class RpgUnit
    {
        public PieceType Type;
        public PieceColor Color;
        public int Health;
        public bool Evolved;
    }

    public readonly struct RpgAction
    {
        public readonly BoardCoordinate From, To;
        public readonly bool Skill;
        public RpgAction(BoardCoordinate from, BoardCoordinate to, bool skill = false)
        { From = from; To = to; Skill = skill; }
    }

    public sealed class RpgMatch
    {
        public readonly Dictionary<BoardCoordinate, RpgUnit> Units = new Dictionary<BoardCoordinate, RpgUnit>();
        public readonly int[] Coins = new int[2];
        public PieceColor Turn { get; private set; }
        public PieceColor? Winner { get; private set; }
        public int TurnNumber { get; private set; }
        public bool Purchased { get; private set; }
        public string LastAction { get; private set; } = "";
        private int bonusMoves;
        private BoardCoordinate? bonusPiece;
        private readonly RpgBalance balance;
        public RpgMatch Copy()
        {
            var copy = new RpgMatch(balance, false)
            {
                Turn = Turn, Winner = Winner, TurnNumber = TurnNumber,
                Purchased = Purchased, LastAction = LastAction,
                bonusMoves = bonusMoves, bonusPiece = bonusPiece
            };
            Array.Copy(Coins, copy.Coins, Coins.Length);
            foreach (var entry in Units)
                copy.Units.Add(entry.Key, new RpgUnit { Type = entry.Value.Type, Color = entry.Value.Color,
                    Health = entry.Value.Health, Evolved = entry.Value.Evolved });
            return copy;
        }

        private RpgMatch(RpgBalance settings, bool initialize)
        { balance = settings; }

        public int UnitDamage(RpgUnit unit) => unit.Evolved && unit.Type == PieceType.Queen
            ? balance.valkyrieDamage : balance.Damage(unit.Type);

        public static string UnitName(RpgUnit unit)
        {
            string[] normal = { "Peão", "Torre", "Cavalo", "Bispo", "Rainha", "Rei" };
            string[] evolved = { "Cavaleiro", "Catapulta", "Cavalo de Troia", "Curandeiro", "Valkíria", "Rei" };
            return (unit.Evolved ? evolved : normal)[(int)unit.Type];
        }
        public RpgMatch(RpgBalance settings)
        {
            balance = settings;
            foreach (var entry in new ChessGame().GetPieces())
                Units.Add(entry.Coordinate, new RpgUnit { Type = entry.Piece.Type, Color = entry.Piece.Color, Health = settings.Health(entry.Piece.Type) });
            Turn = PieceColor.Light;
            BeginTurn();
        }

        private void BeginTurn()
        {
            TurnNumber++;
            Coins[(int)Turn] += balance.income;
            Purchased = false;
            bonusMoves = 0;
            bonusPiece = null;
        }

        public bool CanEvolve(BoardCoordinate at)
        {
            return !Winner.HasValue && !bonusPiece.HasValue && !Purchased && Units.TryGetValue(at, out var unit)
                && unit.Color == Turn && !unit.Evolved && unit.Type != PieceType.King
                && Coins[(int)Turn] >= balance.Cost(unit.Type);
        }

        public bool Evolve(BoardCoordinate at)
        {
            if (!CanEvolve(at)) return false;
            Coins[(int)Turn] -= balance.Cost(Units[at].Type);
            Units[at].Evolved = true;
            Purchased = true;
            return true;
        }

        public List<RpgAction> Actions(BoardCoordinate from)
        {
            var result = new List<RpgAction>();
            if (Winner.HasValue || !Units.TryGetValue(from, out var unit) || unit.Color != Turn) return result;
            if (bonusPiece.HasValue && from != bonusPiece.Value) return result;
            // Reuse the existing chess movement generator with the current RPG occupancy.
            var geometry = new ChessGame();
            geometry.LoadFen(Fen(), false);
            foreach (var move in geometry.GeneratePseudoLegalMoves(from))
                if (!move.IsCastle && !move.IsEnPassant && (!move.Promotion.HasValue || move.Promotion == PieceType.Queen))
                    result.Add(new RpgAction(from, move.To));
            if (!unit.Evolved) return result;
            int direction = unit.Color == PieceColor.Light ? 1 : -1;
            if (unit.Type == PieceType.Pawn)
            {
                for (int dx = -1; dx <= 1; dx++)
                    if (TrySquare(from.File + dx, from.Rank + direction, out var target)
                        && Units.TryGetValue(target, out var enemy) && enemy.Color != Turn)
                        result.Add(new RpgAction(from, target, true));
            }
            if (unit.Type == PieceType.Bishop)
                foreach (var entry in Units)
                    if (entry.Value.Color == Turn && entry.Value.Health < balance.Health(entry.Value.Type)
                        && Distance(from, entry.Key) <= balance.healRange)
                        result.Add(new RpgAction(from, entry.Key, true));
            if (unit.Type == PieceType.Rook)
                foreach (var entry in Units)
                    if (entry.Value.Color != Turn && Distance(from, entry.Key) <= balance.catapultRange)
                        result.Add(new RpgAction(from, entry.Key, true));
            if (unit.Type == PieceType.Queen)
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        for (int step = 1; step < 8; step++)
                            if (TrySquare(from.File + dx * step, from.Rank + dy * step, out var target)
                                && (!Units.TryGetValue(target, out var occupant) || occupant.Color != Turn))
                                result.Add(new RpgAction(from, target));
                    }
            return result.GroupBy(a => (a.To, a.Skill)).Select(g => g.First()).ToList();
        }

        public int ActionDamage(RpgAction action)
        {
            if (!Units.TryGetValue(action.From, out var unit)) return 0;
            return action.Skill && unit.Type == PieceType.Rook
                ? balance.catapultDamage
                : UnitDamage(unit);
        }

        public bool Execute(RpgAction action)
        {
            if (!Actions(action.From).Any(a => a.To == action.To && a.Skill == action.Skill)) return false;
            var unit = Units[action.From];
            int damage = ActionDamage(action);
            string side = Turn == PieceColor.Light ? "Claras" : "Escuras";
            bool rangedShot = action.Skill && unit.Type == PieceType.Rook;
            bool attack = Units.TryGetValue(action.To, out var defender) && defender.Color != Turn;
            LastAction = rangedShot
                ? $"{side}: catapulta {action.From} → {action.To}, {damage} de dano."
                : attack
                    ? $"{side}: {UnitName(unit)} atacou {action.From} → {action.To}."
                    : $"{side}: {UnitName(unit)} moveu {action.From} → {action.To}.";
            bool killed = false;
            var deadTrojan = new List<(BoardCoordinate at, PieceColor color)>();
            if (action.Skill && unit.Type == PieceType.Bishop)
            {
                var ally = Units[action.To];
                int before = ally.Health;
                ally.Health = Math.Min(balance.Health(ally.Type), ally.Health + balance.healAmount);
                LastAction = $"{side}: Curandeiro em {action.From} curou {UnitName(ally)} em {action.To}: +{ally.Health - before} de vida ({ally.Health}).";
            }
            else
            {
                var targets = new List<BoardCoordinate> { action.To };
                if (unit.Evolved && unit.Type == PieceType.Pawn && Units.ContainsKey(action.To))
                {
                    targets.Clear();
                    int forward = Turn == PieceColor.Light ? 1 : -1;
                    for (int dx = -1; dx <= 1; dx++)
                        if (TrySquare(action.From.File + dx, action.From.Rank + forward, out var at)) targets.Add(at);
                }
                foreach (var target in targets)
                    if (Units.TryGetValue(target, out var enemy) && enemy.Color != Turn)
                    {
                        enemy.Health -= damage;
                        LastAction += $" {UnitName(enemy)} em {target}: −{damage} de vida ({Math.Max(0, enemy.Health)} restante).";
                        if (enemy.Health > 0) continue;
                        LastAction += $" {UnitName(enemy)} eliminado.";
                        Units.Remove(target);
                        if (target == action.To) killed = true;
                        if (enemy.Type == PieceType.King) Winner = Turn;
                        if (enemy.Type == PieceType.Knight && enemy.Evolved) deadTrojan.Add((target, enemy.Color));
                    }
                bool stationary = action.Skill && unit.Type == PieceType.Rook;
                if (!stationary && !Units.ContainsKey(action.To))
                {
                    Units.Remove(action.From);
                    Units[action.To] = unit;
                    if (unit.Type == PieceType.Pawn && (action.To.Rank == 0 || action.To.Rank == 7))
                    {
                        int lost = balance.Health(unit.Type) - unit.Health;
                        unit.Type = PieceType.Queen;
                        unit.Evolved = false;
                        unit.Health = Math.Max(1, balance.Health(unit.Type) - lost);
                    }
                }
                if (!Winner.HasValue)
                    foreach (var dead in deadTrojan)
                    {
                        bool spawned = false;
                        for (int dy = -1; dy <= 1 && !spawned; dy++)
                            for (int dx = -1; dx <= 1 && !spawned; dx++)
                                if (TrySquare(dead.at.File + dx, dead.at.Rank + dy, out var spawn)
                                    && spawn != dead.at && !Units.ContainsKey(spawn))
                                {
                                    Units[spawn] = new RpgUnit { Type = PieceType.Pawn, Color = dead.color, Health = balance.Health(PieceType.Pawn), Evolved = true };
                                    spawned = true;
                                    LastAction += $" Troia invocou Cavaleiro em {spawn}.";
                                }
                    }
            }
            if (Winner.HasValue) return true;
            if (unit.Type == PieceType.Queen && unit.Evolved && killed && bonusMoves < balance.valkyrieBonusMoves)
            { bonusMoves++; bonusPiece = action.To; LastAction += " Valkíria ganhou uma ação extra."; return true; }
            Turn = Turn == PieceColor.Light ? PieceColor.Dark : PieceColor.Light;
            BeginTurn();
            return true;
        }

        public List<RpgAction> AllActions() => Units.Where(e => e.Value.Color == Turn).SelectMany(e => Actions(e.Key)).ToList();
        private string Fen()
        {
            var text = new StringBuilder();
            const string symbols = "prnbqk";
            for (int rank = 7; rank >= 0; rank--)
            {
                int empty = 0;
                for (int file = 0; file < 8; file++)
                {
                    if (!Units.TryGetValue(new BoardCoordinate(file, rank), out var unit)) { empty++; continue; }
                    if (empty > 0) { text.Append(empty); empty = 0; }
                    char symbol = symbols[(int)unit.Type];
                    text.Append(unit.Color == PieceColor.Light ? char.ToUpperInvariant(symbol) : symbol);
                }
                if (empty > 0) text.Append(empty);
                if (rank > 0) text.Append('/');
            }
            return text + (Turn == PieceColor.Light ? " w - - 0 1" : " b - - 0 1");
        }
        private static int Distance(BoardCoordinate a, BoardCoordinate b) => Math.Max(Math.Abs(a.File - b.File), Math.Abs(a.Rank - b.Rank));
        private static bool TrySquare(int x, int y, out BoardCoordinate at)
        {
            at = default;
            if (x < 0 || x > 7 || y < 0 || y > 7) return false;
            at = new BoardCoordinate(x, y); return true;
        }
    }
}
