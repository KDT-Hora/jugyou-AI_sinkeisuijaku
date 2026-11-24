using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch.View
{
    // Shows turn, pairs, remaining pairs, and the result (FR-010, FR-011).
    public class Hud : MonoBehaviour
    {
        public Text PlayerOne;
        public Text PlayerTwo;
        public Text Info;
        public Text Result;

        public void Render(MatchEngine e)
        {
            bool playing = e.Phase == GamePhase.Playing || e.Phase == GamePhase.Resolving;
            PlayerOne.text = Line(e, 0, playing);
            PlayerTwo.text = Line(e, 1, playing);
            Info.text = "Time: " + FormatTime(e.RemainingTime) + "\nRemaining pairs: " + e.RemainingPairs + "\nArrows: move   Space: flip\nR: restart";

            string reason = e.EndReason == EndReason.TimeUp ? "TIME UP\n" : "";
            switch (e.Winner)
            {
                case Winner.P1: Result.text = reason + "P1 WINS!"; break;
                case Winner.P2: Result.text = reason + "P2 WINS!"; break;
                case Winner.Draw: Result.text = reason + "DRAW"; break;
                default: Result.text = ""; break;
            }
        }

        // m:ss, rounded up so the display shows 3:00 at the start and 0:00 only when time is out.
        public static string FormatTime(float seconds)
        {
            int total = Mathf.CeilToInt(Mathf.Max(0f, seconds) - 1e-4f);
            return (total / 60) + ":" + (total % 60).ToString("00");
        }

        static string Line(MatchEngine e, int player, bool markTurn)
        {
            string marker = markTurn && e.CurrentPlayer == player ? "> " : "  ";
            return marker + "P" + (player + 1) + " (" + e.Kinds[player] + ")   Pairs: " + e.Pairs[player];
        }

        public void RenderSetup()
        {
            PlayerOne.text = "";
            PlayerTwo.text = "";
            Result.text = "";
            Info.text = "";
        }
    }
}
