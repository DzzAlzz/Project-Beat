using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Notificacion lateral tipo logro de videojuego. Se crea en runtime para
    /// funcionar tanto en menu como en gameplay/resultados sin depender de escenas.
    /// </summary>
    public class AchievementNotification : MonoBehaviour
    {
        private static AchievementNotification instance;
        private CanvasGroup group;
        private RectTransform panel;
        private TMP_Text titleText;
        private TMP_Text nameText;
        private TMP_Text descText;
        private Coroutine routine;
        private readonly Queue<ProfileAchievementStorage.AchievementDefinition> pending = new Queue<ProfileAchievementStorage.AchievementDefinition>();

        private static readonly Color NeonCyan = new Color(0.0f, 0.94f, 1f, 1f);
        private static readonly Color NeonPink = new Color(1.0f, 0.22f, 0.72f, 1f);
        private static readonly Color NeonGold = new Color(1.0f, 0.82f, 0.18f, 1f);

        public static void Show(ProfileAchievementStorage.AchievementDefinition achievement)
        {
            if (achievement == null) return;
            EnsureInstance();
            instance.ShowInternal(achievement);
        }

        private static void EnsureInstance()
        {
            if (instance != null) return;
            GameObject go = new GameObject("AchievementNotification");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<AchievementNotification>();
            instance.BuildUI();
        }

        private void BuildUI()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9000;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            GameObject p = new GameObject("Panel");
            p.transform.SetParent(transform, false);
            panel = p.AddComponent<RectTransform>();
            panel.anchorMin = new Vector2(1f, 0.5f);
            panel.anchorMax = new Vector2(1f, 0.5f);
            panel.pivot = new Vector2(1f, 0.5f);
            panel.sizeDelta = new Vector2(430f, 118f);
            panel.anchoredPosition = new Vector2(500f, 145f);

            Image bg = p.AddComponent<Image>();
            bg.sprite = MakeBoxSprite(new Color(1f, 1f, 1f, 1f));
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.075f, 0.018f, 0.130f, 0.94f);
            group = p.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            Image glow = CreateImage("Glow", panel, new Color(1f, 0.22f, 0.72f, 0.18f));
            glow.sprite = MakeRadialGlowSprite(128);
            glow.rectTransform.sizeDelta = new Vector2(520f, 180f);
            glow.rectTransform.anchoredPosition = Vector2.zero;
            glow.raycastTarget = false;

            Image topLine = CreateImage("TopLine", panel, NeonCyan);
            topLine.rectTransform.sizeDelta = new Vector2(355f, 3f);
            topLine.rectTransform.anchoredPosition = new Vector2(24f, 51f);
            topLine.raycastTarget = false;

            Image bottomLine = CreateImage("BottomLine", panel, NeonPink);
            bottomLine.rectTransform.sizeDelta = new Vector2(355f, 3f);
            bottomLine.rectTransform.anchoredPosition = new Vector2(24f, -51f);
            bottomLine.raycastTarget = false;

            Image trophy = CreateImage("Trophy", panel, NeonGold);
            trophy.sprite = MakeTrophySprite(96);
            trophy.rectTransform.sizeDelta = new Vector2(72f, 72f);
            trophy.rectTransform.anchoredPosition = new Vector2(-168f, 0f);
            trophy.raycastTarget = false;

            titleText = CreateText("Title", panel, "LOGRO DESBLOQUEADO", 15f, NeonCyan, FontStyles.Bold);
            titleText.characterSpacing = 2f;
            titleText.rectTransform.sizeDelta = new Vector2(310f, 24f);
            titleText.rectTransform.anchoredPosition = new Vector2(58f, 30f);

            nameText = CreateText("Name", panel, "LOGRO", 24f, Color.white, FontStyles.Bold);
            nameText.rectTransform.sizeDelta = new Vector2(310f, 34f);
            nameText.rectTransform.anchoredPosition = new Vector2(58f, 2f);

            descText = CreateText("Description", panel, "Descripcion", 14f, new Color(0.84f, 0.78f, 1f, 1f), FontStyles.Bold);
            descText.rectTransform.sizeDelta = new Vector2(310f, 34f);
            descText.rectTransform.anchoredPosition = new Vector2(58f, -30f);
        }

        private void ShowInternal(ProfileAchievementStorage.AchievementDefinition achievement)
        {
            pending.Enqueue(achievement);
            if (routine == null)
                routine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            float enterDuration = 0.32f;
            float holdDuration = 5.0f;
            float exitDuration = 0.35f;
            Vector2 hidden = new Vector2(500f, 145f);
            Vector2 shown = new Vector2(-30f, 145f);

            while (pending.Count > 0)
            {
                ProfileAchievementStorage.AchievementDefinition achievement = pending.Dequeue();
                if (nameText != null) nameText.text = achievement.title;
                if (descText != null) descText.text = achievement.description;

                for (float t = 0f; t < enterDuration; t += Time.unscaledDeltaTime)
                {
                    float k = Mathf.SmoothStep(0f, 1f, t / enterDuration);
                    group.alpha = k;
                    panel.anchoredPosition = Vector2.Lerp(hidden, shown, k);
                    panel.localScale = Vector3.one * Mathf.Lerp(0.96f, 1.02f, k);
                    yield return null;
                }

                group.alpha = 1f;
                panel.anchoredPosition = shown;
                panel.localScale = Vector3.one;
                yield return new WaitForSecondsRealtime(holdDuration);

                for (float t = 0f; t < exitDuration; t += Time.unscaledDeltaTime)
                {
                    float k = Mathf.SmoothStep(0f, 1f, t / exitDuration);
                    group.alpha = 1f - k;
                    panel.anchoredPosition = Vector2.Lerp(shown, hidden, k);
                    yield return null;
                }

                group.alpha = 0f;
                panel.anchoredPosition = hidden;
                yield return new WaitForSecondsRealtime(0.15f);
            }

            routine = null;
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.rectTransform.anchorMin = img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            return img;
        }

        private TMP_Text CreateText(string name, Transform parent, string text, float size, Color color, FontStyles style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static Sprite MakeBoxSprite(Color color)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16, 4, SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
        }

        private static Sprite MakeRadialGlowSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 c = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c) / (size * 0.5f);
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a * 0.9f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite MakeTrophySprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(1f, 1f, 1f, 0f);
            Color solid = Color.white;
            Vector2 c = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - c;
                    bool cup = p.y > -size * 0.10f && p.y < size * 0.32f && Mathf.Abs(p.x) < Mathf.Lerp(size * 0.28f, size * 0.20f, (p.y + size * 0.10f) / (size * 0.42f));
                    bool leftHandle = Mathf.Abs((p.x + size * 0.34f) * (p.x + size * 0.34f) / (size * 0.16f * size * 0.16f) + (p.y - size * 0.13f) * (p.y - size * 0.13f) / (size * 0.18f * size * 0.18f) - 1f) < 0.26f && p.x < -size * 0.20f;
                    bool rightHandle = Mathf.Abs((p.x - size * 0.34f) * (p.x - size * 0.34f) / (size * 0.16f * size * 0.16f) + (p.y - size * 0.13f) * (p.y - size * 0.13f) / (size * 0.18f * size * 0.18f) - 1f) < 0.26f && p.x > size * 0.20f;
                    bool stem = Mathf.Abs(p.x) < size * 0.07f && p.y >= -size * 0.32f && p.y <= -size * 0.08f;
                    bool base1 = Mathf.Abs(p.y + size * 0.35f) < size * 0.045f && Mathf.Abs(p.x) < size * 0.25f;
                    bool base2 = Mathf.Abs(p.y + size * 0.43f) < size * 0.045f && Mathf.Abs(p.x) < size * 0.36f;
                    Vector2 sp = p - new Vector2(0f, size * 0.12f);
                    float ang = Mathf.Atan2(sp.y, sp.x);
                    float rr = sp.magnitude;
                    float starR = size * (0.09f + 0.035f * Mathf.Cos(5f * ang));
                    bool star = rr < starR;
                    bool draw = cup || leftHandle || rightHandle || stem || base1 || base2;
                    tex.SetPixel(x, y, draw && !star ? solid : clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
