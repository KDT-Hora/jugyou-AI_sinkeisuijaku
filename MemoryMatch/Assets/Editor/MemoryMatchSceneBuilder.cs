using MemoryMatch.Core;
using MemoryMatch.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch.EditorTools
{
    // Builds Assets/Scenes/MemoryMatch.unity (Edit Mode). Run from unity-cli via execute_menu_item.
    public static class MemoryMatchSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/MemoryMatch.unity";
        const float CardSize = 140f;
        const float Gap = 10f;

        [MenuItem("MemoryMatch/Build Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.07f, 0.08f, 0.12f);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            // Board: 6 columns x 4 rows, left side.
            var board = NewRect("Board", canvasGo.transform);
            float boardW = CursorGrid.Columns * CardSize + (CursorGrid.Columns - 1) * Gap;
            float boardH = CursorGrid.Rows * CardSize + (CursorGrid.Rows - 1) * Gap;
            board.anchorMin = board.anchorMax = new Vector2(0f, 0.5f);
            board.pivot = new Vector2(0f, 0.5f);
            board.anchoredPosition = new Vector2(30f, 0f);
            board.sizeDelta = new Vector2(boardW, boardH);

            var cards = new CardView[CursorGrid.CardCount];
            for (int i = 0; i < cards.Length; i++)
            {
                int row = i / CursorGrid.Columns;
                int col = i % CursorGrid.Columns;
                cards[i] = NewCard(i, board, font, col * (CardSize + Gap), -row * (CardSize + Gap));
            }

            // HUD: right side.
            var hudGo = NewRect("Hud", canvasGo.transform);
            hudGo.anchorMin = hudGo.anchorMax = new Vector2(1f, 0.5f);
            hudGo.pivot = new Vector2(1f, 0.5f);
            hudGo.anchoredPosition = new Vector2(-20f, 0f);
            hudGo.sizeDelta = new Vector2(300f, 590f);
            var hud = hudGo.gameObject.AddComponent<Hud>();
            hud.PlayerOne = NewText("PlayerOne", hudGo, font, 26, TextAnchor.UpperLeft, new Vector2(0, -20), new Vector2(300, 44));
            hud.PlayerTwo = NewText("PlayerTwo", hudGo, font, 26, TextAnchor.UpperLeft, new Vector2(0, -74), new Vector2(300, 44));
            hud.Info = NewText("Info", hudGo, font, 20, TextAnchor.UpperLeft, new Vector2(0, -150), new Vector2(300, 120));
            hud.Result = NewText("Result", hudGo, font, 44, TextAnchor.MiddleCenter, new Vector2(0, -330), new Vector2(300, 120));
            hud.Result.color = new Color(1f, 0.9f, 0.3f);

            // Setup screen: covers the screen until the game starts.
            var setupGo = NewRect("SetupMenu", canvasGo.transform);
            Stretch(setupGo);
            var bg = setupGo.gameObject.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.06f, 0.10f, 0.96f);
            var menu = setupGo.gameObject.AddComponent<SetupMenu>();
            menu.Panel = setupGo.gameObject;
            menu.Title = NewMenuText("Title", setupGo, font, 56, 0f, 200f, 800f, 80f);
            menu.PlayerOneRow = NewMenuText("PlayerOneRow", setupGo, font, 36, 0f, 70f, 700f, 60f);
            menu.PlayerTwoRow = NewMenuText("PlayerTwoRow", setupGo, font, 36, 0f, -10f, 700f, 60f);
            menu.StartButton = NewMenuText("StartButton", setupGo, font, 40, 0f, -110f, 400f, 70f);
            menu.Hint = NewMenuText("Hint", setupGo, font, 22, 0f, -230f, 1100f, 80f);

            var game = new GameObject("Game", typeof(MemoryGame));
            var mg = game.GetComponent<MemoryGame>();
            mg.Cards = cards;
            mg.Hud = hud;
            mg.Menu = menu;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[MemoryMatch] Scene built: " + ScenePath);
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static CardView NewCard(int index, RectTransform board, Font font, float x, float y)
        {
            var root = NewRect("Card_" + index.ToString("00"), board);
            root.anchorMin = root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);
            root.anchoredPosition = new Vector2(x, y);
            root.sizeDelta = new Vector2(CardSize, CardSize);
            var border = root.gameObject.AddComponent<Image>();
            border.color = Color.clear;

            var faceRt = NewRect("Face", root);
            Stretch(faceRt);
            faceRt.offsetMin = new Vector2(6f, 6f);
            faceRt.offsetMax = new Vector2(-6f, -6f);
            var face = faceRt.gameObject.AddComponent<Image>();

            var label = NewText("Label", faceRt, font, 72, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(label.rectTransform);
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;

            var view = root.gameObject.AddComponent<CardView>();
            view.Border = border;
            view.Face = face;
            view.Label = label;
            return view;
        }

        // Centered text for the setup screen (anchor = screen centre).
        static Text NewMenuText(string name, RectTransform parent, Font font, int size, float x, float y, float width, float height)
        {
            var rt = NewRect(name, parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        static Text NewText(string name, RectTransform parent, Font font, int size, TextAnchor anchor, Vector2 pos, Vector2 sizeDelta)
        {
            var rt = NewRect(name, parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var t = rt.gameObject.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }
    }
}
