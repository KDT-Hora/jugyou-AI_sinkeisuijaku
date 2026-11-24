using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch.View
{
    // Setup screen: choose each player's kind, then start (FR-012).
    public class SetupMenu : MonoBehaviour
    {
        public GameObject Panel;
        public Text Body;

        public void Show(bool visible)
        {
            if (Panel != null) Panel.SetActive(visible);
        }

        public void Render(PlayerKind kind0, PlayerKind kind1, int selectedRow)
        {
            Body.text =
                "MEMORY MATCH\n\n" +
                Row(0, selectedRow) + "Player 1:  < " + kind0 + " >\n" +
                Row(1, selectedRow) + "Player 2:  < " + kind1 + " >\n\n" +
                "Up/Down: select   Left/Right: change   Enter or Space: start";
        }

        static string Row(int row, int selected) => row == selected ? "> " : "  ";

        public static PlayerKind Cycle(PlayerKind kind, int step)
        {
            int count = System.Enum.GetValues(typeof(PlayerKind)).Length;
            return (PlayerKind)(((int)kind + step + count) % count);
        }
    }
}
