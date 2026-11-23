using NUnit.Framework;
using UnityEngine;
using XadrezRpg.Board;
using XadrezRpg.Pieces;
using XadrezRpg.Combat;
namespace XadrezRpg.Tests.EditMode
{
    public sealed class RpgMatchTests
    {
        private RpgBalance config;
        private RpgMatch match;
        private static BoardCoordinate At(int x, int y) => new BoardCoordinate(x, y);
        [SetUp] public void Setup() { config = ScriptableObject.CreateInstance<RpgBalance>(); match = new RpgMatch(config); }
        [TearDown] public void Cleanup() { Object.DestroyImmediate(config); }
        private void Empty() { match.Units.Clear(); Add(0,0,PieceType.King,PieceColor.Light); Add(7,7,PieceType.King,PieceColor.Dark); }
        private RpgUnit Add(int x,int y,PieceType type,PieceColor color,bool evolved=false)
        {
            var unit=new RpgUnit {Type=type,Color=color,Evolved=evolved,Health=config.Health(type)};
            match.Units[At(x,y)]=unit; return unit;
        }
        [Test] public void IncomeOnlyOnTurnChange()
        {
            match.Actions(At(4,1)); match.AllActions(); Assert.That(match.Coins[0],Is.EqualTo(2));
            Assert.That(match.Execute(new RpgAction(At(4,1),At(4,2))),Is.True);
            Assert.That(match.Coins[1],Is.EqualTo(2));
            Assert.That(match.Execute(new RpgAction(At(4,1),At(4,2))),Is.False);
            Assert.That(match.Coins[1],Is.EqualTo(2));
        }
        [Test] public void SurvivingTargetKeepsAttackerAtOrigin()
        {
            Empty(); var attacker=Add(3,3,PieceType.Rook,PieceColor.Light); var target=Add(3,5,PieceType.Rook,PieceColor.Dark);
            match.Execute(new RpgAction(At(3,3),At(3,5)));
            Assert.That(target.Health,Is.EqualTo(3)); Assert.That(match.Units[At(3,3)],Is.SameAs(attacker));
        }
        [Test] public void KingDeathEndsMatch()
        {
            Empty(); Add(7,5,PieceType.Queen,PieceColor.Light); match.Units[At(7,7)].Health=1;
            match.Execute(new RpgAction(At(7,5),At(7,7)));
            Assert.That(match.Winner,Is.EqualTo(PieceColor.Light)); Assert.That(match.AllActions(),Is.Empty);
        }
        [Test] public void EvolutionChargesOnceAndRejectsKing()
        {
            match.Coins[0]=20; Assert.That(match.Evolve(At(4,0)),Is.False);
            Assert.That(match.Evolve(At(0,1)),Is.True); Assert.That(match.Coins[0],Is.EqualTo(17));
            Assert.That(match.Evolve(At(1,1)),Is.False);
        }
        [Test] public void CavaleiroHitsThreeEnemies()
        {
            Empty(); Add(3,3,PieceType.Pawn,PieceColor.Light,true);
            for(int x=2;x<=4;x++) Add(x,4,PieceType.Pawn,PieceColor.Dark);
            match.Execute(new RpgAction(At(3,3),At(3,4),true));
            for(int x=2;x<=4;x++) Assert.That(match.Units[At(x,4)].Health,Is.EqualTo(2));
        }
        [Test] public void HealerRestoresLifeWithoutMoving()
        {
            Empty(); var healer=Add(3,3,PieceType.Bishop,PieceColor.Light,true);
            var target=Add(4,3,PieceType.Rook,PieceColor.Light); target.Health=1;
            match.Execute(new RpgAction(At(3,3),At(4,3),true));
            Assert.That(target.Health,Is.EqualTo(4)); Assert.That(match.Units[At(3,3)],Is.SameAs(healer));
        }
        [Test] public void CatapultStaysAfterKill()
        {
            Empty(); var catapult=Add(3,3,PieceType.Rook,PieceColor.Light,true); Add(4,5,PieceType.Pawn,PieceColor.Dark).Health=1;
            match.Execute(new RpgAction(At(3,3),At(4,5),true));
            Assert.That(match.Units.ContainsKey(At(4,5)),Is.False); Assert.That(match.Units[At(3,3)],Is.SameAs(catapult));
        }
        [Test] public void CatapultShotDealsOneAndDoesNotKillHealthyPawn()
        {
            Empty(); Add(3,3,PieceType.Rook,PieceColor.Light,true);
            var target=Add(4,5,PieceType.Pawn,PieceColor.Dark);
            Assert.That(match.Execute(new RpgAction(At(3,3),At(4,5),true)),Is.True);
            Assert.That(target.Health,Is.EqualTo(2));
            Assert.That(match.LastAction,Does.Contain("1 de dano"));
        }
        [Test] public void CatapultShotCannotReachBeyondTwoSquares()
        {
            Empty(); Add(3,3,PieceType.Rook,PieceColor.Light,true);
            var target=Add(4,6,PieceType.Pawn,PieceColor.Dark);
            Assert.That(match.Execute(new RpgAction(At(3,3),At(4,6),true)),Is.False);
            Assert.That(target.Health,Is.EqualTo(3));
            Assert.That(match.Turn,Is.EqualTo(PieceColor.Light));
        }
        [Test] public void CatapultNormalAttackStillDealsFour()
        {
            Empty(); Add(3,3,PieceType.Rook,PieceColor.Light,true);
            var target=Add(3,5,PieceType.Rook,PieceColor.Dark);
            Assert.That(match.Execute(new RpgAction(At(3,3),At(3,5))),Is.True);
            Assert.That(target.Health,Is.EqualTo(3));
        }
        [Test] public void TrojanSpawnsEvolvedPawnAfterOccupation()
        {
            Empty(); Add(3,3,PieceType.Rook,PieceColor.Light);
            Add(3,5,PieceType.Knight,PieceColor.Dark,true).Health=1;
            match.Execute(new RpgAction(At(3,3),At(3,5)));
            Assert.That(match.Units[At(3,5)].Color,Is.EqualTo(PieceColor.Light));
            Assert.That(match.Units[At(2,4)].Evolved,Is.True);
        }
        [Test] public void ValkyrieBonusIsRestrictedAndDoesNotCreditIncome()
        {
            Empty(); Add(3,3,PieceType.Queen,PieceColor.Light,true); Add(3,4,PieceType.Pawn,PieceColor.Light); Add(3,5,PieceType.Pawn,PieceColor.Dark);
            Assert.That(match.Execute(new RpgAction(At(3,3),At(3,5))),Is.True);
            Assert.That(match.Turn,Is.EqualTo(PieceColor.Light)); Assert.That(match.Coins[0],Is.EqualTo(2));
            Assert.That(match.Actions(At(0,0)),Is.Empty);
            Assert.That(match.Execute(new RpgAction(At(3,5),At(4,5))),Is.True);
            Assert.That(match.Turn,Is.EqualTo(PieceColor.Dark));
        }
        [Test] public void ValkyrieDealsThreeWhileNormalQueenDealsFive()
        {
            Empty(); var queen=Add(3,3,PieceType.Queen,PieceColor.Light,true);
            var target=Add(3,5,PieceType.Rook,PieceColor.Dark);
            Assert.That(match.ActionDamage(new RpgAction(At(3,3),At(3,5))),Is.EqualTo(3));
            queen.Evolved=false;
            Assert.That(match.ActionDamage(new RpgAction(At(3,3),At(3,5))),Is.EqualTo(5));
            queen.Evolved=true;
            match.Execute(new RpgAction(At(3,3),At(3,5)));
            Assert.That(target.Health,Is.EqualTo(4));
            Assert.That(match.LastAction,Does.Contain("Valkíria"));
            Assert.That(match.LastAction,Does.Contain("4 restante"));
        }
        [TestCase(RpgDifficulty.Easy)]
        [TestCase(RpgDifficulty.Medium)]
        [TestCase(RpgDifficulty.Hard)]
        public void EveryDifficultyReturnsLegalActionsWithoutChangingMatch(RpgDifficulty difficulty)
        {
            Empty(); Add(3,3,PieceType.Rook,PieceColor.Light);
            Add(3,5,PieceType.Pawn,PieceColor.Dark);
            var choice=RpgBot.ChooseAction(match,config,difficulty,new System.Random(42));
            Assert.That(choice.HasValue,Is.True);
            Assert.That(match.Units[At(3,5)].Health,Is.EqualTo(3));
            Assert.That(match.Coins[0],Is.EqualTo(2));
            Assert.That(match.Execute(choice.Value),Is.True);
        }
        [TestCase(RpgDifficulty.Medium)]
        [TestCase(RpgDifficulty.Hard)]
        public void TacticalBotTakesWinningAttack(RpgDifficulty difficulty)
        {
            Empty(); Add(7,5,PieceType.Rook,PieceColor.Light);
            match.Units[At(7,7)].Health=1;
            var choice=RpgBot.ChooseAction(match,config,difficulty,new System.Random(1));
            Assert.That(choice.HasValue,Is.True);
            match.Execute(choice.Value);
            Assert.That(match.Winner,Is.EqualTo(PieceColor.Light));
        }
        [Test] public void HardBotAvoidsCaptureThatLosesRookOnReply()
        {
            Empty(); Add(3,3,PieceType.Rook,PieceColor.Light);
            Add(4,3,PieceType.Pawn,PieceColor.Dark);
            Add(3,4,PieceType.Pawn,PieceColor.Dark);
            Add(7,3,PieceType.Queen,PieceColor.Dark);
            var choice=RpgBot.ChooseAction(match,config,RpgDifficulty.Hard,new System.Random(1));
            Assert.That(choice.HasValue,Is.True);
            Assert.That(choice.Value.To,Is.EqualTo(At(3,4)));
        }
    }
}
