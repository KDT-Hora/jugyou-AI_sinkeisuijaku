using System.Collections.Generic;
using System.Text;
using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MemoryMatch.View
{
    // GameObject "Game". Connects Core to the screen and the keyboard. No game rules here.
    public class MemoryGame : MonoBehaviour
    {
        [Header("Setup (set before Play, or use the setup screen)")]
        public bool UseSeed;
        public int Seed = 1;
        public float TimeLimitSeconds = MatchEngine.DefaultTimeLimitSeconds;
        public PlayerKind PlayerKind0 = PlayerKind.Human;
        public PlayerKind PlayerKind1 = PlayerKind.CpuNormal;

        [Header("State (read-only, for unity-cli)")]
        public string Phase = "Setup";
        public int CurrentPlayer;
        public int Pairs0;
        public int Pairs1;
        public int RemainingPairs = MatchEngine.PairCount;
        public string Winner = "None";
        public float RemainingTime = MatchEngine.DefaultTimeLimitSeconds;
        public string EndReason = "None";
        public int CursorIndex;
        public string CardStates = "HHHHHHHHHHHHHHHHHHHHHHHH";
        public string FaceSymbols = "????????????????????????";

        [Header("Scene refs")]
        public CardView[] Cards;
        public Hud Hud;
        public SetupMenu Menu;

        MatchEngine _engine;
        int _menuRow;

        void Awake()
        {
            // Keep the game loop running when the Editor window is not focused,
            // so unity-cli can drive and observe the game from the outside.
            Application.runInBackground = true;
            // Accept keyboard input even when the window is not focused (runtime setting only, not saved).
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
        }

        void Start()
        {
            Refresh();
        }

        // Edge detection by polling isPressed. This also works for devices that do not
        // report wasPressedThisFrame (for example the virtual keyboard used by unity-cli).
        readonly Dictionary<UnityEngine.InputSystem.Controls.KeyControl, bool> _previous =
            new Dictionary<UnityEngine.InputSystem.Controls.KeyControl, bool>();

        bool Down(UnityEngine.InputSystem.Controls.KeyControl key)
        {
            _previous.TryGetValue(key, out bool was);
            return key.isPressed && !was;
        }

        void RememberKeys(Keyboard kb)
        {
            var keys = new[] { kb.leftArrowKey, kb.rightArrowKey, kb.upArrowKey, kb.downArrowKey,
                               kb.spaceKey, kb.enterKey, kb.rKey };
            foreach (var k in keys) _previous[k] = k.isPressed;
        }

        void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                HandleInput(kb);
                RememberKeys(kb);
            }
            if (_engine != null) _engine.Tick(Time.deltaTime);
            Refresh();
        }

        void HandleInput(Keyboard kb)
        {
            if (_engine == null) { HandleSetupInput(kb); return; }

            if (Down(kb.rKey))
            {
                _engine.Restart();
                CursorIndex = 0;
                return;
            }

            if (_engine.Phase == GamePhase.Finished) return;

            if (Down(kb.leftArrowKey)) CursorIndex = CursorGrid.Move(CursorIndex, Direction.Left);
            if (Down(kb.rightArrowKey)) CursorIndex = CursorGrid.Move(CursorIndex, Direction.Right);
            if (Down(kb.upArrowKey)) CursorIndex = CursorGrid.Move(CursorIndex, Direction.Up);
            if (Down(kb.downArrowKey)) CursorIndex = CursorGrid.Move(CursorIndex, Direction.Down);
            if (Down(kb.spaceKey)) _engine.Pick(CursorIndex);
        }

        void HandleSetupInput(Keyboard kb)
        {
            if (Down(kb.upArrowKey)) _menuRow = 0;
            if (Down(kb.downArrowKey)) _menuRow = 1;

            int step = 0;
            if (Down(kb.rightArrowKey)) step = 1;
            if (Down(kb.leftArrowKey)) step = -1;
            if (step != 0)
            {
                if (_menuRow == 0) PlayerKind0 = SetupMenu.Cycle(PlayerKind0, step);
                else PlayerKind1 = SetupMenu.Cycle(PlayerKind1, step);
            }

            if (Down(kb.enterKey) || Down(kb.spaceKey)) Begin();
        }

        // Starts the game with the current setup values.
        public void Begin()
        {
            _engine = new MatchEngine(UseSeed ? Seed : (int?)null, TimeLimitSeconds);
            _engine.SetKind(0, PlayerKind0);
            _engine.SetKind(1, PlayerKind1);
            _engine.Start();
            CursorIndex = 0;
        }

        void Refresh()
        {
            if (Menu != null) Menu.Show(_engine == null);

            if (_engine == null)
            {
                Phase = "Setup";
                RemainingTime = TimeLimitSeconds;
                EndReason = "None";
                if (Menu != null) Menu.Render(PlayerKind0, PlayerKind1, _menuRow);
                if (Hud != null) Hud.RenderSetup();
                return;
            }

            Phase = _engine.Phase.ToString();
            CurrentPlayer = _engine.CurrentPlayer;
            Pairs0 = _engine.Pairs[0];
            Pairs1 = _engine.Pairs[1];
            RemainingPairs = _engine.RemainingPairs;
            Winner = _engine.Winner.ToString();
            RemainingTime = _engine.RemainingTime;
            EndReason = _engine.EndReason.ToString();

            var states = new StringBuilder(CursorGrid.CardCount);
            var faces = new StringBuilder(CursorGrid.CardCount);
            bool showCursor = _engine.Phase != GamePhase.Finished && !_engine.Kinds[_engine.CurrentPlayer].IsCpu();
            for (int i = 0; i < _engine.Cards.Length; i++)
            {
                Card card = _engine.Cards[i];
                states.Append(card.State == CardState.Hidden ? 'H' : card.State == CardState.Revealed ? 'R' : (char)('0' + card.Owner));
                faces.Append(card.State == CardState.Hidden ? '?' : (char)('A' + card.Symbol));
                if (Cards != null && i < Cards.Length) Cards[i].Apply(card, showCursor && i == CursorIndex);
            }
            CardStates = states.ToString();
            FaceSymbols = faces.ToString();
            if (Hud != null) Hud.Render(_engine);
        }
    }
}
