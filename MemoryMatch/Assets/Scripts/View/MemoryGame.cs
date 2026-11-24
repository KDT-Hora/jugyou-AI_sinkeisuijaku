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

        // ---- Keyboard input --------------------------------------------------------------
        // A key counts as pressed if ANY source reports it:
        //   - every keyboard device of the Input System (the physical one, and the virtual one that
        //     unity-cli adds), and
        //   - the legacy Input Manager (OS keyboard), when it is enabled ("Both" in Player Settings).
        // Edge detection is done here by polling, because some devices (unity-cli's virtual keyboard)
        // do not report wasPressedThisFrame.
        enum GameKey { Left, Right, Up, Down, Space, Enter, R }

        const int KeyCount = 7;
        readonly bool[] _keyNow = new bool[KeyCount];
        readonly bool[] _keyWas = new bool[KeyCount];

        bool Down(GameKey key) => _keyNow[(int)key] && !_keyWas[(int)key];

        void PollKeys()
        {
            for (int i = 0; i < KeyCount; i++) _keyNow[i] = IsHeld((GameKey)i);
        }

        void RememberKeys()
        {
            for (int i = 0; i < KeyCount; i++) _keyWas[i] = _keyNow[i];
        }

        static bool IsHeld(GameKey key)
        {
            foreach (InputDevice device in InputSystem.devices)
            {
                if (device is Keyboard kb && IsHeld(kb, key)) return true;
            }
#if ENABLE_LEGACY_INPUT_MANAGER
            switch (key)
            {
                case GameKey.Left: return Input.GetKey(KeyCode.LeftArrow);
                case GameKey.Right: return Input.GetKey(KeyCode.RightArrow);
                case GameKey.Up: return Input.GetKey(KeyCode.UpArrow);
                case GameKey.Down: return Input.GetKey(KeyCode.DownArrow);
                case GameKey.Space: return Input.GetKey(KeyCode.Space);
                case GameKey.Enter: return Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter);
                case GameKey.R: return Input.GetKey(KeyCode.R);
            }
#endif
            return false;
        }

        static bool IsHeld(Keyboard kb, GameKey key)
        {
            switch (key)
            {
                case GameKey.Left: return kb.leftArrowKey.isPressed;
                case GameKey.Right: return kb.rightArrowKey.isPressed;
                case GameKey.Up: return kb.upArrowKey.isPressed;
                case GameKey.Down: return kb.downArrowKey.isPressed;
                case GameKey.Space: return kb.spaceKey.isPressed;
                case GameKey.Enter: return kb.enterKey.isPressed || kb.numpadEnterKey.isPressed;
                case GameKey.R: return kb.rKey.isPressed;
            }
            return false;
        }

        void Update()
        {
            PollKeys();
            HandleInput();
            RememberKeys();
            if (_engine != null) _engine.Tick(Time.deltaTime);
            Refresh();
        }

        void HandleInput()
        {
            if (_engine == null) { HandleSetupInput(); return; }

            if (Down(GameKey.R))
            {
                _engine.Restart();
                CursorIndex = 0;
                return;
            }

            if (_engine.Phase == GamePhase.Finished) return;

            if (Down(GameKey.Left)) CursorIndex = CursorGrid.Move(CursorIndex, Direction.Left);
            if (Down(GameKey.Right)) CursorIndex = CursorGrid.Move(CursorIndex, Direction.Right);
            if (Down(GameKey.Up)) CursorIndex = CursorGrid.Move(CursorIndex, Direction.Up);
            if (Down(GameKey.Down)) CursorIndex = CursorGrid.Move(CursorIndex, Direction.Down);
            if (Down(GameKey.Space)) _engine.Pick(CursorIndex);
        }

        void HandleSetupInput()
        {
            if (Down(GameKey.Up)) _menuRow = 0;
            if (Down(GameKey.Down)) _menuRow = 1;

            int step = 0;
            if (Down(GameKey.Right)) step = 1;
            if (Down(GameKey.Left)) step = -1;
            if (step != 0)
            {
                if (_menuRow == 0) PlayerKind0 = SetupMenu.Cycle(PlayerKind0, step);
                else PlayerKind1 = SetupMenu.Cycle(PlayerKind1, step);
            }

            if (Down(GameKey.Enter) || Down(GameKey.Space)) Begin();
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
