using UnityEngine;
using XadrezRpg.Pieces;

namespace XadrezRpg.Combat
{
    [CreateAssetMenu(menuName = "Xadrez RPG/Balanceamento")]
    public sealed class RpgBalance : ScriptableObject
    {
        public int income = 2;
        public int[] health = { 3, 7, 6, 5, 8, 15 };
        public int[] damage = { 1, 4, 3, 3, 5, 2 };
        public int[] evolutionCost = { 3, 3, 4, 3, 5, 0 };
        public int healAmount = 3;
        public int healRange = 2;
        public int catapultRange = 2;
        public int catapultDamage = 1;
        public int valkyrieBonusMoves = 1;
        public int valkyrieDamage = 3;
        public float actionPreviewDuration = 0.7f;
        public float actionResultDuration = 0.8f;
        public float botDelay = 0.65f;
        public float jumpDuration = 0.35f;
        public float jumpHeight = 45f;
        public int Health(PieceType type) => health[(int)type];
        public int Damage(PieceType type) => damage[(int)type];
        public int Cost(PieceType type) => evolutionCost[(int)type];
    }
}
