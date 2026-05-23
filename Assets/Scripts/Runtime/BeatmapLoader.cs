using UnityEngine;

namespace ProjectBeat.Runtime
{
    public static class BeatmapLoader
    {
        public static BeatmapData LoadFromJson(TextAsset jsonAsset)
        {
            if (jsonAsset == null)
            {
                Debug.LogError("No se asignó el beatmap JSON.");
                return null;
            }

            BeatmapData data = JsonUtility.FromJson<BeatmapData>(jsonAsset.text);
            if (data == null || data.notes == null || data.notes.Length == 0)
            {
                Debug.LogError("El beatmap no pudo cargarse o no contiene notas.");
                return null;
            }

            return data;
        }
    }
}
