using UnityEngine;

namespace ProjectBeat.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBeat/Level Data", fileName = "LevelData")]
    public class LevelData : ScriptableObject
    {
        public string levelName;
        public string artistName;
        public TextAsset beatmapJson;
        public AudioClip audioClip;
        public BackgroundTheme backgroundTheme = BackgroundTheme.NeonPurple;
        public Color[] laneColors = new Color[]
        {
            new Color(0.35f, 0.85f, 1f),
            new Color(0.65f, 0.5f, 1f),
            new Color(1f, 0.45f, 0.85f),
            new Color(1f, 0.65f, 0.35f)
        };
    }

    public enum BackgroundTheme
    {
        NeonPurple,
        NeonOrange,
        NeonBlue,
        NeonGreen,
        NeonPink,
        NeonWhite
    }
}
