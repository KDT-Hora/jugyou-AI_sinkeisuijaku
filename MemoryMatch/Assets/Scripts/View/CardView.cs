using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch.View
{
    // One card on screen (GameObject Card_00..Card_23). Display only: no rules here.
    public class CardView : MonoBehaviour
    {
        public static readonly Color[] SymbolColors =
        {
            new Color(0.90f, 0.25f, 0.25f), new Color(0.95f, 0.55f, 0.15f), new Color(0.95f, 0.85f, 0.20f),
            new Color(0.55f, 0.80f, 0.25f), new Color(0.20f, 0.70f, 0.35f), new Color(0.15f, 0.75f, 0.75f),
            new Color(0.25f, 0.55f, 0.95f), new Color(0.35f, 0.35f, 0.85f), new Color(0.65f, 0.35f, 0.85f),
            new Color(0.90f, 0.40f, 0.75f), new Color(0.60f, 0.45f, 0.30f), new Color(0.70f, 0.70f, 0.70f),
        };

        static readonly Color BackColor = new Color(0.16f, 0.20f, 0.30f);
        static readonly Color CursorColor = new Color(1f, 1f, 1f, 1f);
        static readonly Color[] OwnerColors = { new Color(0.25f, 0.55f, 1f), new Color(1f, 0.40f, 0.35f) };

        [Header("State (read-only, for unity-cli)")]
        public string State = "Hidden";
        public string Owner = "None";

        [Header("Scene refs")]
        public Image Face;
        public Image Border;
        public Text Label;

        public void Apply(Card card, bool showCursor)
        {
            State = card.State.ToString();
            Owner = card.Owner < 0 ? "None" : (card.Owner == 0 ? "P1" : "P2");

            if (card.State == CardState.Hidden)
            {
                Face.color = BackColor;
                Label.text = "";
            }
            else
            {
                Color c = SymbolColors[card.Symbol % SymbolColors.Length];
                Face.color = card.State == CardState.Captured ? Color.Lerp(c, Color.black, 0.35f) : c;
                Label.text = ((char)('A' + card.Symbol)).ToString();
            }

            if (showCursor) Border.color = CursorColor;
            else if (card.State == CardState.Captured && card.Owner >= 0) Border.color = OwnerColors[card.Owner];
            else Border.color = new Color(0f, 0f, 0f, 0f);
        }
    }
}
