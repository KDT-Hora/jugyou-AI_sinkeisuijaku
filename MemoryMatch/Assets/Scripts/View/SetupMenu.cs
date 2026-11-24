using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch.View
{
    // Setup screen: choose each player's kind, then start (FR-012, FR-026).
    // Rows: 0 = player 1, 1 = player 2, 2 = START. Keyboard and mouse both work.
    public class SetupMenu : MonoBehaviour
    {
        public const int StartRow = 2;

        public GameObject Panel;
        public Text Title;
        public Text PlayerOneRow;
        public Text PlayerTwoRow;
        public Text StartButton;
        public Text Hint;

        public void Show(bool visible)
        {
            if (Panel != null) Panel.SetActive(visible);
        }

        public void Render(PlayerKind kind0, PlayerKind kind1, int selectedRow)
        {
            Title.text = "MEMORY MATCH";
            PlayerOneRow.text = Marker(0, selectedRow) + "Player 1:  < " + kind0 + " >";
            PlayerTwoRow.text = Marker(1, selectedRow) + "Player 2:  < " + kind1 + " >";
            StartButton.text = Marker(StartRow, selectedRow) + "[ START ]";
            Hint.text = "Mouse: hover and click (left = next, right = previous).   START: click.\n" +
                        "Keys: Up/Down select   Left/Right change   Enter or Space start";
        }

        // Which row is under the screen point, or -1.
        public int RowAt(Vector2 screenPoint)
        {
            if (Contains(PlayerOneRow, screenPoint)) return 0;
            if (Contains(PlayerTwoRow, screenPoint)) return 1;
            if (Contains(StartButton, screenPoint)) return StartRow;
            return -1;
        }

        static bool Contains(Text text, Vector2 screenPoint)
        {
            return text != null && text.gameObject.activeInHierarchy &&
                   RectTransformUtility.RectangleContainsScreenPoint(text.rectTransform, screenPoint, null);
        }

        static string Marker(int row, int selected) => row == selected ? "> " : "  ";

        public static PlayerKind Cycle(PlayerKind kind, int step)
        {
            int count = System.Enum.GetValues(typeof(PlayerKind)).Length;
            return (PlayerKind)(((int)kind + step + count) % count);
        }
    }
}
