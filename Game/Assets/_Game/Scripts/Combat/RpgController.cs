using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using XadrezRpg.Board;
using XadrezRpg.Pieces;
using XadrezRpg.Turns;

namespace XadrezRpg.Combat
{
    public sealed class RpgController : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float jumpDuration = 0.35f;
        [SerializeField, Min(0)] private float jumpHeight = 45f;
        private BoardView board;
        private Text status;
        private RpgBalance balance;
        private RpgMatch match;
        private bool bot, busy, skill, confirmEvolution;
        private BoardCoordinate? selected;
        private BoardCoordinate? lastFrom, lastTo;
        private bool lastWasAttack;
        private readonly Queue<string> history = new Queue<string>();
        private string lastBotAction = "O bot ainda não jogou.";
        private Vector2 panelScroll;
        private static RpgDifficulty difficulty = RpgDifficulty.Medium;
        private readonly System.Random botRandom = new System.Random();
        private readonly Dictionary<(PieceColor, PieceType), Sprite> standard = new Dictionary<(PieceColor, PieceType), Sprite>();
        private readonly Dictionary<BoardCoordinate, PieceView> views = new Dictionary<BoardCoordinate, PieceView>();
        private readonly Dictionary<(PieceColor, PieceType), Sprite> classes = new Dictionary<(PieceColor, PieceType), Sprite>();
        private static readonly string[] ClassFiles = { "Cavaleiro", "Muralha_Catapulta", "Cavalo_de_Troia", "Curandeiro", "Valkiria", "" };
        private static readonly string[] ClassNames = { "Cavaleiro", "Muralha com Catapulta", "Cavalo de Troia", "Curandeiro", "Valkíria", "Rei" };

        public void Initialize(BoardView boardView, Text statusText, RpgBalance settings, bool againstBot)
        {
            board = boardView; status = statusText; bot = againstBot;
            balance = settings != null ? settings : Resources.Load<RpgBalance>("RpgBalance");
            if (balance == null) balance = ScriptableObject.CreateInstance<RpgBalance>();
            jumpDuration = balance.jumpDuration; jumpHeight = balance.jumpHeight;
            foreach (var view in board.GetComponentsInChildren<PieceView>())
                standard[(view.Color, view.Type)] = view.Sprite;
            foreach (PieceColor color in Enum.GetValues(typeof(PieceColor)))
                for (int type = 0; type < 5; type++)
                    classes[(color, (PieceType)type)] = Resources.Load<Sprite>("Classes/" + (color == PieceColor.Light ? "Brancas_" : "Pretas_") + ClassFiles[type]);
            match = new RpgMatch(balance);
            board.SelectionChanged += Click;
            Render();
        }

        private void OnDestroy() { if (board != null) board.SelectionChanged -= Click; }
        private bool HumanTurn => !busy && !match.Winner.HasValue && (!bot || match.Turn == PieceColor.Light);
        private void Click(BoardCoordinate at)
        {
            if (!HumanTurn || PointerOnPanel()) { Hints(); return; }
            confirmEvolution = false;
            if (selected.HasValue)
            {
                var candidate = match.Actions(selected.Value).Where(a => a.To == at && a.Skill == skill).ToArray();
                if (candidate.Length > 0) { StartCoroutine(Resolve(candidate[0])); return; }
            }
            selected = match.Units.TryGetValue(at, out var unit) && unit.Color == match.Turn ? at : (BoardCoordinate?)null;
            Hints();
        }

        private Vector2 guiPointer;
        private bool PointerOnPanel() => panelRect.Contains(guiPointer);
        private void Hints()
        {
            if (!selected.HasValue && lastFrom.HasValue && lastTo.HasValue)
            {
                board.ShowMoveHints(lastFrom, new[] { lastTo.Value },
                    lastWasAttack ? new[] { lastTo.Value } : Array.Empty<BoardCoordinate>());
                return;
            }
            var actions = selected.HasValue ? match.Actions(selected.Value).Where(a => a.Skill == skill).ToArray() : Array.Empty<RpgAction>();
            board.ShowMoveHints(selected, actions.Select(a => a.To), actions.Where(a => match.Units.TryGetValue(a.To, out var unit) && unit.Color != match.Turn).Select(a => a.To));
        }

        private IEnumerator Resolve(RpgAction action)
        {
            busy = true;
            confirmEvolution = false;
            board.ClearMoveHints();
            var mover = match.Units[action.From];
            bool botAction = bot && mover.Color == PieceColor.Dark;
            lastFrom = action.From; lastTo = action.To;
            lastWasAttack = match.Units.TryGetValue(action.To, out var target) && target.Color != mover.Color;
            board.ShowMoveHints(action.From, new[] { action.To }, lastWasAttack ? new[] { action.To } : Array.Empty<BoardCoordinate>());
            status.text = (botAction ? "Bot: " : "") + RpgMatch.UnitName(mover) + " " + action.From + " → " + action.To;
            if (botAction || action.Skill) yield return new WaitForSeconds(balance.actionPreviewDuration);
            bool stationary = action.Skill && (mover.Type == PieceType.Rook || mover.Type == PieceType.Bishop);
            var view = views[action.From];
            var start = view.transform.position;
            board.TryGetTile(action.To, out var destination);
            Vector3 end = Landing(destination, view);
            if (!stationary) yield return Jump(view.transform, start, end);
            if (!match.Execute(action)) { busy = false; Hints(); yield break; }
            Record(match.LastAction);
            if (botAction) lastBotAction = match.LastAction;
            // A surviving defender leaves the attacker at its original square.
            if (!stationary && match.Units.TryGetValue(action.From, out var stillHere) && ReferenceEquals(stillHere, mover))
                yield return Jump(view.transform, end, start);
            selected = null; skill = false;
            Render();
            if (botAction || action.Skill) yield return new WaitForSeconds(balance.actionResultDuration);
            busy = false;
            if (bot && match.Turn == PieceColor.Dark && !match.Winner.HasValue) StartCoroutine(BotTurn());
        }

        private Vector3 Landing(BoardTileView tile, PieceView view)
        {
            // Match PieceView.PlaceOn's bottom-centred base, using the destination tile's size.
            var rect = (RectTransform)tile.transform;
            return rect.TransformPoint(new Vector3(0, -rect.rect.height * 0.5f + rect.rect.height * 0.035f, 0));
        }
        private IEnumerator Jump(Transform piece, Vector3 from, Vector3 to)
        {
            float elapsed = 0;
            float height = jumpHeight * board.GetComponentInParent<Canvas>().scaleFactor;
            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.05f, jumpDuration));
                piece.position = Vector3.Lerp(from, to, t) + Vector3.up * (4 * t * (1 - t) * height);
                yield return null;
            }
            piece.position = to;
        }

        private IEnumerator BotTurn()
        {
            busy = true;
            status.text = "Bot pensando…";
            yield return new WaitForSeconds(balance.botDelay);
            var purchase = RpgBot.ChooseEvolution(match, balance, difficulty, botRandom);
            if (purchase.HasValue && match.Evolve(purchase.Value))
            {
                lastBotAction = "Bot evoluiu " + purchase.Value + " para " + RpgMatch.UnitName(match.Units[purchase.Value]) + ".";
                Record(lastBotAction);
            }
            Render();
            var action = RpgBot.ChooseAction(match, balance, difficulty, botRandom);
            if (!action.HasValue)
            {
                status.text = "Sem ações disponíveis. Reinicie a partida.";
                busy = false; yield break;
            }
            yield return Resolve(action.Value);
        }

        private void Record(string message)
        {
            history.Enqueue(message);
            while (history.Count > 8) history.Dequeue();
        }

        private void Render()
        {
            foreach (var old in board.GetComponentsInChildren<PieceView>()) { old.gameObject.SetActive(false); Destroy(old.gameObject); }
            views.Clear();
            foreach (var entry in match.Units)
            {
                var unit = entry.Value;
                var sprite = standard[(unit.Color, unit.Type)];
                if (unit.Evolved && classes.TryGetValue((unit.Color, unit.Type), out var evolvedSprite) && evolvedSprite != null) sprite = evolvedSprite;
                board.TryGetTile(entry.Key, out var tile);
                var view = PieceView.Create(tile, unit.Type, unit.Color, entry.Key, sprite);
                // Normalize class art to its tile rather than assuming the source canvas dimensions.
                if (unit.Evolved)
                {
                    var rect = (RectTransform)view.transform;
                    var tileRect = (RectTransform)tile.transform;
                    float h = tileRect.rect.height * 0.92f;
                    rect.sizeDelta = new Vector2(h * sprite.rect.width / sprite.rect.height, h);
                }
                views[entry.Key] = view;
            }
            status.text = match.Winner.HasValue
                ? (bot ? (match.Winner == PieceColor.Light ? "Vitória!" : "Derrota!") : "Vitória das " + ColorName(match.Winner.Value)) + " Rei derrotado."
                : "Vez das " + ColorName(match.Turn) + " • turno " + match.TurnNumber;
            Hints();
        }

        private Rect panelRect;
        private void OnGUI()
        {
            if (match == null) return;
            guiPointer = Event.current.mousePosition;
            float width = Mathf.Min(245, Screen.width * 0.25f);
            panelRect = new Rect(Screen.width - width - 12, 25, width, Screen.height - 50);
            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.Label(bot ? "RPG contra bot" : "RPG local");
            panelScroll = GUILayout.BeginScrollView(panelScroll);
            if (bot)
            {
                GUILayout.Label("Dificuldade");
                GUI.enabled = !busy;
                difficulty = (RpgDifficulty)GUILayout.Toolbar((int)difficulty, new[] { "Fácil", "Médio", "Difícil" });
                GUI.enabled = true;
            }
            GUILayout.Label("Claras: " + match.Coins[0] + " moedas\nEscuras: " + match.Coins[1] + " moedas");
            GUILayout.Label(status.text);
            if (bot) { GUILayout.Label("Última ação do bot:"); GUILayout.Label(lastBotAction); }
            if (selected.HasValue && match.Units.TryGetValue(selected.Value, out var unit))
            {
                GUILayout.Space(12);
                GUILayout.Label((unit.Evolved ? ClassNames[(int)unit.Type] : unit.Type.ToString()) + " • " + selected.Value);
                GUILayout.Label("Vida: " + unit.Health + "/" + balance.Health(unit.Type) + " | Dano: " + match.UnitDamage(unit));
                if (unit.Evolved && unit.Type == PieceType.Rook)
                    GUILayout.Label("Tiro: " + balance.catapultDamage + " de dano | Alcance: " + balance.catapultRange + " casas");
                GUI.enabled = HumanTurn && match.CanEvolve(selected.Value);
                if (GUILayout.Button("Evoluir: " + balance.Cost(unit.Type) + " moedas")) confirmEvolution = true;
                if (confirmEvolution && GUILayout.Button("Confirmar " + ClassNames[(int)unit.Type]))
                { match.Evolve(selected.Value); confirmEvolution = false; Render(); }
                GUI.enabled = HumanTurn && unit.Evolved;
                bool nextSkill = GUILayout.Toggle(skill, "Usar habilidade");
                if (nextSkill != skill) { skill = nextSkill; Hints(); }
                GUI.enabled = true;
                if (unit.Evolved) GUILayout.Label(Description(unit.Type));
            }
            GUILayout.Space(12);
            GUILayout.Label("Histórico (mais recente primeiro)");
            foreach (string message in history.Reverse()) GUILayout.Label("• " + message);
            GUILayout.Space(12);
            GUI.enabled = !busy;
            if (GUILayout.Button("Reiniciar partida")) Reload(bot ? 0 : 1);
            if (GUILayout.Button("RPG contra bot")) Reload(0);
            if (GUILayout.Button("RPG local")) Reload(1);
            if (GUILayout.Button("Xadrez clássico local")) Reload(2);
            GUI.enabled = true;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        private static void Reload(int mode)
        { ChessGameController.RequestedMode = mode; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        private static string ColorName(PieceColor color) => color == PieceColor.Light ? "claras" : "escuras";
        private static string Description(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return "Ataque às três casas frontais. Clique num inimigo frontal.";
                case PieceType.Bishop: return "Habilidade: clique num aliado ferido para curar.";
                case PieceType.Rook: return "Habilidade: tiro à distância sem sair da casa.";
                case PieceType.Knight: return "Ao morrer, invoca um Peão Cavaleiro vizinho.";
                case PieceType.Queen: return "Salta bloqueios. Uma captura letal concede uma ação extra.";
                default: return "";
            }
        }
    }
}
