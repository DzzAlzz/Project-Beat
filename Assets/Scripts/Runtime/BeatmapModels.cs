using System;
using UnityEngine;

namespace ProjectBeat.Runtime
{
    [Serializable]
    public class BeatmapNoteData
    {
        public int lane;
        public float time;

        // Avance 28: duración opcional para Hold Notes.
        // 0 = nota normal; >0 = nota mantenida.
        public float duration = 0f;
    }

    [Serializable]
    public class BeatmapData
    {
        public string songName;
        public string artist;
        public float bpm = 120f;
        public float offset = 0f;
        public string audio;
        public float leadTime = 2f;
        public float noteSpeed = 6.5f;
        public BeatmapNoteData[] notes;
    }
}
