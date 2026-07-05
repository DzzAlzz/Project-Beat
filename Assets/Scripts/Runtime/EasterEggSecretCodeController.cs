using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 87: detector seguro de palabras secretas para logros easter egg.
    /// No muestra input en pantalla y se desactiva durante gameplay para no interferir con D/F/J/K.
    /// </summary>
    public class EasterEggSecretCodeController : MonoBehaviour
    {
        private const int MaxBufferLength = 32;
        private string buffer = string.Empty;
        private float welcomeCheckTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Ensure();
        }

        public static void Ensure()
        {
            if (FindObjectOfType<EasterEggSecretCodeController>() != null) return;
            GameObject go = new GameObject("EasterEggSecretCodeController");
            DontDestroyOnLoad(go);
            go.AddComponent<EasterEggSecretCodeController>();
        }

        private void Update()
        {
            TryUnlockWelcomeIfReady();

            if (!CanCaptureSecretInput()) return;

            string input = Input.inputString;
            if (string.IsNullOrEmpty(input)) return;

            for (int i = 0; i < input.Length; i++)
            {
                char c = char.ToUpperInvariant(input[i]);
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    buffer += c;
                    if (buffer.Length > MaxBufferLength)
                        buffer = buffer.Substring(buffer.Length - MaxBufferLength);
                }
            }

            CheckCode("PROJECTBEAT");
            CheckCode("FEELTHERHYTHM");
            CheckCode("RITMO");
        }

        private void TryUnlockWelcomeIfReady()
        {
            welcomeCheckTimer += Time.unscaledDeltaTime;
            if (welcomeCheckTimer < 1.0f) return;
            welcomeCheckTimer = 0f;

            if (!StartupFlowController.IsMainMenuVisible) return;
            if (!ProfileStatsStorage.HasLoadedProfileGame()) return;

            if (ProfileAchievementStorage.TryUnlockById("WELCOME_RHYTHM", out _))
            {
                // Avance 91: este logro se guarda de forma silenciosa al cargar un perfil.
                // No debe sonar ni mostrar notificación solo por cambiar/cargar partida.
            }
        }

        private bool CanCaptureSecretInput()
        {
            GameController gc = FindObjectOfType<GameController>();
            if (gc != null && gc.IsGameplayRunning) return false;
            if (IsTypingInInputField()) return false;
            return StartupFlowController.IsMainMenuVisible;
        }

        private bool IsTypingInInputField()
        {
            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
                return false;

            TMP_InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
            return inputField != null && inputField.isFocused;
        }

        private void CheckCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return;
            if (!buffer.Contains(code)) return;

            if (ProfileAchievementStorage.TryUnlockSecretCode(code, out var achievement))
                AchievementNotification.Show(achievement);

            buffer = string.Empty;
        }
    }
}
