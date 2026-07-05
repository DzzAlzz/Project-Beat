using System.Collections.Generic;
using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Handles input for one lane. On keydown: glows brightly and hits nearest note.
    /// On perfect hit: triggers a colour flash for extra feedback.
    /// </summary>
    public class LaneInput : MonoBehaviour
    {
        [SerializeField] private KeyCode       key = KeyCode.D;
        [SerializeField] private int           laneIndex;
        [SerializeField] private SpriteRenderer glowRenderer;

        private readonly List<NoteObject> activeNotes = new List<NoteObject>();
        private GameController controller;
        private PauseMenu cachedPauseMenu;

        // Avance 95: entrada tactil movil. No reemplaza el teclado/gamepad,
        // solo suma un estado virtual para que los botones en pantalla usen
        // exactamente la misma logica de hit y hold notes.
        private bool virtualTouchHeld;
        private bool virtualTouchReleasedThisFrame;

        // Colour state
        private Color baseColor;
        private Color pressColor;
        private Color idleColor;

        // Glow flash timer (brief extra pulse on hit)
        private float flashTimer;
        private Color flashColor;
        private const float FlashDuration = 0.08f;

        public int LaneIndex => laneIndex;
        public bool IsKeyHeld => Input.GetKey(key) || virtualTouchHeld;
        public bool WasKeyReleasedThisFrame => Input.GetKeyUp(key) || virtualTouchReleasedThisFrame;

        // ── Init ──────────────────────────────────────────────────────────
        public void Initialize(GameController gc, int index, KeyCode assignedKey, Color laneColor)
        {
            controller  = gc;
            laneIndex   = index;
            key         = assignedKey;
            baseColor   = laneColor;
            idleColor   = new Color(laneColor.r, laneColor.g, laneColor.b, 0.09f);
            pressColor  = new Color(
                Mathf.Min(laneColor.r * 1.3f, 1f),
                Mathf.Min(laneColor.g * 1.3f, 1f),
                Mathf.Min(laneColor.b * 1.3f, 1f), 0.65f);
            flashColor  = new Color(1f, 1f, 1f, 0.90f); // white flash on perfect
            cachedPauseMenu = FindObjectOfType<PauseMenu>();
            virtualTouchHeld = false;
            virtualTouchReleasedThisFrame = false;
            SetGlow(idleColor);
        }

        public void Initialize(GameController gc, int index, KeyCode assignedKey)
            => Initialize(gc, index, assignedKey, new Color(0.4f, 0.8f, 1f));

        // ── Update ────────────────────────────────────────────────────────
        private void Update()
        {
            if (controller == null || !controller.IsGameplayRunning)
            {
                virtualTouchHeld = false;
                SetGlow(Color.clear);
                return;
            }

            if (cachedPauseMenu == null) cachedPauseMenu = FindObjectOfType<PauseMenu>();
            if (cachedPauseMenu != null && cachedPauseMenu.IsPaused) { virtualTouchHeld = false; SetGlow(Color.clear); return; }

            // Avance 31: el hit se evalua al inicio del frame de Update para que
            // el input se sienta mas inmediato antes de ejecutar respiracion/glow.
            if (Input.GetKeyDown(key)) TryHit();

            // Flash timer overrides normal glow
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                float a = flashTimer / FlashDuration;
                SetGlow(Color.Lerp(pressColor, flashColor, a));
            }
            else if (Input.GetKey(key) || virtualTouchHeld)
            {
                SetGlow(pressColor);
            }
            else
            {
                // Subtle idle breathing
                float breath = 0.5f + 0.5f * Mathf.Sin(Time.time * 1.8f + laneIndex * 1.1f);
                Color c = idleColor;
                c.a = Mathf.Lerp(0.06f, 0.13f, breath);
                SetGlow(c);
            }

        }

        private void LateUpdate()
        {
            // Se limpia al final del frame para que NoteObject pueda detectar
            // el release tactil durante el Update de una hold note.
            virtualTouchReleasedThisFrame = false;
        }

        // ── Avance 95: API para botones tactiles moviles ─────────────────
        public void HandleVirtualPress()
        {
            if (controller == null || !controller.IsGameplayRunning)
                return;

            if (cachedPauseMenu == null) cachedPauseMenu = FindObjectOfType<PauseMenu>();
            if (cachedPauseMenu != null && cachedPauseMenu.IsPaused)
                return;

            if (virtualTouchHeld)
                return;

            virtualTouchHeld = true;
            TryHit();
        }

        public void HandleVirtualRelease()
        {
            if (!virtualTouchHeld)
                return;

            virtualTouchHeld = false;
            virtualTouchReleasedThisFrame = true;
        }

        public void ForceReleaseVirtualInput()
        {
            virtualTouchHeld = false;
            virtualTouchReleasedThisFrame = false;
        }

        // ── Called by GameController after a perfect ─────────────────────
        public void TriggerPerfectFlash()
        {
            flashTimer = FlashDuration;
        }

        // ── Hit logic ─────────────────────────────────────────────────────
        private void TryHit()
        {
            CleanupList();
            if (activeNotes.Count == 0) { controller.RegisterEmptyTap(); return; }

            NoteObject closest      = null;
            float      closestDelta = float.MaxValue;
            foreach (NoteObject note in activeNotes)
            {
                if (note == null || note.IsJudged) continue;
                float delta = Mathf.Abs(note.HitTime - controller.CalibratedSongPosition);
                if (delta < closestDelta) { closestDelta = delta; closest = note; }
            }

            if (closest == null) { controller.RegisterEmptyTap(); return; }
            closest.TryHit();
        }

        public void RegisterNote(NoteObject note)   { if (!activeNotes.Contains(note)) activeNotes.Add(note); }
        public void UnregisterNote(NoteObject note) { activeNotes.Remove(note); }
        private void CleanupList()                  { activeNotes.RemoveAll(n => n == null || n.IsJudged); }

        private void SetGlow(Color c)
        {
            if (glowRenderer != null) glowRenderer.color = c;
        }
    }
}
