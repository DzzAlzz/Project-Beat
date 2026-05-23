using UnityEngine;

namespace ProjectBeat.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class Conductor : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float startDelay = 0.75f;

        private double songDspStartTime;
        private bool isPlayingScheduled;

        // Pausa: guardamos la posición de la canción en el momento de pausar
        // para poder retomarla correctamente sin depender del dspTime continuo.
        private bool isPaused;
        private float pausedSongPosition;

        public float Bpm { get; private set; } = 120f;
        public float SongOffset { get; private set; } = 0f;
        public float SecondsPerBeat => 60f / Bpm;
        public float SongPosition { get; private set; }
        public float SongPositionInBeats => SongPosition / SecondsPerBeat;
        public bool IsSongStarted { get; private set; }
        public bool IsSongFinished => IsSongStarted && audioSource.clip != null && SongPosition >= audioSource.clip.length;

        public AudioSource AudioSource => audioSource;

        private void Reset()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Initialize(float bpm, float offset, AudioClip clip)
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            Bpm        = Mathf.Max(1f, bpm);
            SongOffset = offset;

            if (clip == null)
            {
                Debug.LogError("[Conductor] AudioClip es null. No habrá música.");
                return;
            }

            audioSource.clip         = clip;
            audioSource.playOnAwake  = false;
            audioSource.mute         = false;
            audioSource.volume       = 1f;
            audioSource.spatialBlend = 0f; // 2D — sin atenuación por distancia
            audioSource.priority     = 0;  // máxima prioridad
        }

        public void StartSong()
        {
            if (audioSource == null || audioSource.clip == null)
            {
                Debug.LogError("Conductor: falta AudioSource o AudioClip.");
                return;
            }

            SongPosition = 0f;
            isPaused = false;
            pausedSongPosition = 0f;
            IsSongStarted = false;
            isPlayingScheduled = true;
            songDspStartTime = AudioSettings.dspTime + startDelay;
            audioSource.PlayScheduled(songDspStartTime);
        }

        /// <summary>
        /// Pausa o reanuda el audio del juego.
        /// Al pausar, guarda la posición exacta de la canción.
        /// Al reanudar, recalcula el songDspStartTime para que SongPosition
        /// continúe desde donde se dejó sin desincronizarse.
        /// </summary>
        public void SetPaused(bool pause)
        {
            if (!isPlayingScheduled) return;

            if (pause && !isPaused)
            {
                isPaused = true;
                pausedSongPosition = SongPosition;
                audioSource.Pause();
            }
            else if (!pause && isPaused)
            {
                isPaused = false;
                // Recalculamos el tiempo de inicio virtual para que SongPosition
                // retome exactamente desde pausedSongPosition.
                songDspStartTime = AudioSettings.dspTime - pausedSongPosition - SongOffset;
                audioSource.UnPause();
            }
        }

        private void Update()
        {
            if (!isPlayingScheduled) return;
            if (isPaused) return;

            double dsp = AudioSettings.dspTime;
            SongPosition = (float)(dsp - songDspStartTime) - SongOffset;

            if (SongPosition >= 0f)
            {
                IsSongStarted = true;
            }
        }
    }
}
