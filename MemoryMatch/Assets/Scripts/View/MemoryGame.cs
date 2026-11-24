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

        // ---- Mouse input (FR-024..027) -------------------------------------------------------
        // Position and buttons are read from every Input System mouse (the physical one, and the
        // virtual one unity-cli adds) and from the legacy Input Manager. Hover only moves the
        // cursor when the mouse actually moved, so the keyboard keeps working after the mouse rests.
        readonly Dictionary<int, Vector2> _mouseSeen = new Dictionary<int, Vector2>();
        Vector2 _mousePos;
        bool _mouseMoved;
        bool _leftNow, _leftWas, _rightNow, _rightWas;
        bool _leftPressedEvent, _rightPressedEvent;   // press seen as an event (catches very short clicks)

        void PollMouse()
        {
            _mouseMoved = false;
            _leftNow = false;
            _rightNow = false;
            _leftPressedEvent = false;
            _rightPressedEvent = false;
            foreach (InputDevice device in InputSystem.devices)
            {
                if (device is Mouse mouse)
                {
                    ObserveMouse(mouse.deviceId, mouse.position.ReadValue());
                    _leftNow |= mouse.leftButton.isPressed;
                    _rightNow |= mouse.rightButton.isPressed;
                    _leftPressedEvent |= mouse.leftButton.wasPressedThisFrame;
                    _rightPressedEvent |= mouse.rightButton.wasPressedThisFrame;
                }
            }
#if ENABLE_LEGACY_INPUT_MANAGER
            ObserveMouse(-1, Input.mousePosition);
            _leftNow |= Input.GetMouseButton(0);
            _rightNow |= Input.GetMouseButton(1);
            _leftPressedEvent |= Input.GetMouseButtonDown(0);
            _rightPressedEvent |= Input.GetMouseButtonDown(1);
#endif
        }

        void ObserveMouse(int sourceId, Vector2 position)
        {
            if (!_mouseSeen.TryGetValue(sourceId, out Vector2 previous))
            {
                _mouseSeen[sourceId] = position;
                _mousePos = position;
                return;
            }
            if ((position - previous).sqrMagnitude > 0.25f)
            {
                _mousePos = position;
                _mouseMoved = true;
            }
            _mouseSeen[sourceId] = position;
        }

        int CardAt(Vector2 screenPoint)
        {
            if (Cards == null) return -1;
            for (int i = 0; i < Cards.Length; i++)
            {
                var rect = Cards[i].transform as RectTransform;
                if (rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, null)) return i;
            }
            return -1;
        }

        void HandleMouse()
        {
            bool leftDown = (_leftNow && !_leftWas) || _leftPressedEvent;
            bool rightDown = (_rightNow && !_rightWas) || _rightPressedEvent;

            if (_engine == null)
            {
                HandleSetupMouse(leftDown, rightDown);
                return;
            }
            if (_engine.Phase == GamePhase.Finished) return;

            int hit = CardAt(_mousePos);
            if (hit < 0) return;
            if (_mouseMoved || leftDown) CursorIndex = hit;
            if (leftDown) _engine.Pick(hit);
        }

        void HandleSetupMouse(bool leftDown, bool rightDown)
        {
            if (Menu == null) return;
            int row = Menu.RowAt(_mousePos);
            if (row < 0) return;
            if (_mouseMoved) _menuRow = row;

            if (row == SetupMenu.StartRow)
            {
                if (leftDown) Begin();
                return;
            }
            int step = leftDown ? 1 : (rightDown ? -1 : 0);
            if (step == 0) return;
            _menuRow = row;
            if (row == 0) PlayerKind0 = SetupMenu.Cycle(PlayerKind0, step);
            else PlayerKind1 = SetupMenu.Cycle(PlayerKind1, step);
        }

        void Update()
        {
            PollKeys();
            PollMouse();
            HandleInput();
            HandleMouse();
            RememberKeys();
            _leftWas = _leftNow;
            _rightWas = _rightNow;
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
            if (step != 0 && _menuRow != SetupMenu.StartRow)
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
