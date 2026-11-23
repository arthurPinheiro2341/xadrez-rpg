using System;
using System.Collections.Generic;
using System.Linq;
using XadrezRpg.Board;
using XadrezRpg.Pieces;

namespace XadrezRpg.Combat
{
    public enum RpgDifficulty { Easy, Medium, Hard }

    // Difficulty changes decisions only; all levels execute the same legal actions and prices.
    public static class RpgBot
    {
        private const int CandidateLimit = 6;
        private const int ReplyLimit = 6;
        private const float KingValue = 10000;

        public static BoardCoordinate? ChooseEvolution(RpgMatch match, RpgBalance balance,
            RpgDifficulty difficulty, Random random)
        {
            var available = match.Units.Keys.Where(match.CanEvolve).ToArray();
            if (available.Length == 0) return null;
            if (difficulty == RpgDifficulty.Easy)
                return random.NextDouble() < 0.5 ? available[random.Next(available.Length)] : (BoardCoordinate?)null;
            return available.OrderByDescending(at => EvolutionValue(match.Units[at], balance)).First();
        }

        private static float EvolutionValue(RpgUnit unit, RpgBalance balance)
        {
            // Prefer useful, healthy upgrades rather than saving indefinitely for the Queen.
            float value = unit.Type == PieceType.Rook ? 6 : unit.Type == PieceType.Knight ? 5
                : unit.Type == PieceType.Bishop ? 4 : unit.Type == PieceType.Queen ? 4 : 3;
            return value * unit.Health / balance.Health(unit.Type) / Math.Max(1, balance.Cost(unit.Type));
        }

        public static RpgAction? ChooseAction(RpgMatch match, RpgBalance balance,
            RpgDifficulty difficulty, Random random)
        {
            var actions = match.AllActions();
            if (actions.Count == 0) return null;
            if (difficulty == RpgDifficulty.Easy) return actions[random.Next(actions.Count)];
            var ranked = actions.Select(a => (action: a, score: ImmediateScore(match, balance, a)))
                .OrderByDescending(entry => entry.score).ToArray();
            if (difficulty == RpgDifficulty.Medium)
            {
                float best = ranked[0].score;
                var tied = ranked.Where(entry => Math.Abs(entry.score - best) < 0.001f).ToArray();
                return tied[random.Next(tied.Length)].action;
            }
            var side = match.Turn;
            float bestScore = float.NegativeInfinity;
            RpgAction bestAction = ranked[0].action;
            foreach (var entry in ranked.Take(CandidateLimit))
            {
                var next = match.Copy();
                next.Execute(entry.action);
                float score = Evaluate(next, side, balance);
                if (!next.Winner.HasValue)
                {
                    // Include the opponent's purchasable upgrade in the reply position.
                    if (next.Turn != side)
                    {
                        var upgrade = ChooseEvolution(next, balance, RpgDifficulty.Hard, random);
                        if (upgrade.HasValue) next.Evolve(upgrade.Value);
                    }
                    var replies = next.AllActions().OrderByDescending(a => ImmediateScore(next, balance, a)).Take(ReplyLimit).ToArray();
                    if (replies.Length > 0)
                    {
                        var values = new List<float>();
                        foreach (var reply in replies)
                        {
                            var response = next.Copy(); response.Execute(reply);
                            values.Add(Evaluate(response, side, balance));
                        }
                        score = next.Turn == side ? values.Max() : values.Min();
                    }
                }
                score += entry.score * 0.01f;
                if (score > bestScore) { bestScore = score; bestAction = entry.action; }
            }
            return bestAction;
        }

        private static float ImmediateScore(RpgMatch match, RpgBalance balance, RpgAction action)
        {
            var unit = match.Units[action.From];
            if (match.Units.TryGetValue(action.To, out var target))
            {
                if (target.Color == unit.Color)
                    return action.Skill ? Math.Min(balance.healAmount, balance.Health(target.Type) - target.Health) * 2 : 0;
                int damage = match.ActionDamage(action);
                return Math.Min(target.Health, damage) * 3
                    + (target.Health <= damage ? Value(target, balance) : 0)
                    + (target.Type == PieceType.King ? (target.Health <= damage ? KingValue : 8) : 0);
            }
            float advance = unit.Color == PieceColor.Light ? action.To.Rank - action.From.Rank : action.From.Rank - action.To.Rank;
            float center = 3.5f - Math.Abs(3.5f - action.To.File);
            return advance * 0.15f + center * 0.1f;
        }

        private static float Value(RpgUnit unit, RpgBalance balance)
        {
            return unit.Type == PieceType.King ? KingValue : balance.Health(unit.Type) * 2
                + balance.Damage(unit.Type) * 3 + (unit.Evolved ? balance.Cost(unit.Type) : 0);
        }

        private static float Evaluate(RpgMatch match, PieceColor side, RpgBalance balance)
        {
            if (match.Winner.HasValue) return match.Winner == side ? KingValue * 10 : -KingValue * 10;
            return match.Units.Values.Sum(unit => (unit.Color == side ? 1 : -1)
                * (Value(unit, balance) + unit.Health * 3));
        }
    }
}
