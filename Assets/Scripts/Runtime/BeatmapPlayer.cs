using System.Collections.Generic;
using UnityEngine;

namespace ProjectBeat.Runtime
{
    public class BeatmapPlayer : MonoBehaviour
    {
        [SerializeField] private GameController controller;
        [SerializeField] private TextAsset beatmapJson;
        [SerializeField] private AudioClip songOverride;

        private readonly List<BeatmapNoteData> notes = new List<BeatmapNoteData>();
        private int spawnIndex;
        private bool setupComplete;
        private PauseMenu cachedPauseMenu;

        public void Initialize(GameController gameController, TextAsset json, AudioClip audioOverride = null)
        {
            controller   = gameController;
            beatmapJson  = json;
            songOverride = audioOverride;

            BeatmapData beatmap = BeatmapLoader.LoadFromJson(beatmapJson);
            if (beatmap == null) return;

            controller.SetBeatmap(beatmap, songOverride);
            notes.Clear();
            notes.AddRange(beatmap.notes);
            spawnIndex    = 0;
            cachedPauseMenu = FindObjectOfType<PauseMenu>();
            setupComplete = true;
        }

        private void Update()
        {
            if (!setupComplete || controller == null || !controller.IsGameplayRunning) return;

            if (cachedPauseMenu == null) cachedPauseMenu = FindObjectOfType<PauseMenu>();
            if (cachedPauseMenu != null && cachedPauseMenu.IsPaused) return;

            float songTime = controller.CalibratedSongPosition;
            while (spawnIndex < notes.Count)
            {
                BeatmapNoteData note = notes[spawnIndex];
                if (songTime + controller.Beatmap.leadTime < note.time) break;
                controller.SpawnNote(note.lane, note.time, Mathf.Max(0f, note.duration));
                spawnIndex++;
            }

            if (spawnIndex >= notes.Count && controller.Conductor.IsSongFinished)
            {
                controller.FinishSong();
                setupComplete = false;
            }
        }
    }
}
