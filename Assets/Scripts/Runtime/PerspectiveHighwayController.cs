using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 27: adapta visualmente el gameplay a una pista tipo rhythm highway.
    /// No cambia el timing ni la lógica interna: solo reposiciona spawn/hit points
    /// y crea guías visuales en perspectiva para aprovechar mejor la pantalla.
    /// </summary>
    public class PerspectiveHighwayController : MonoBehaviour
    {
        [Header("Perspective Layout")]
        [SerializeField] private float topY = 4.25f;
        [SerializeField] private float hitY = -3.35f;
        [SerializeField] private float topWidth = 2.10f;
        [SerializeField] private float bottomWidth = 5.45f;
        [SerializeField] private float laneBodyAlpha = 0.13f;
        [SerializeField] private float laneEdgeAlpha = 0.72f;

        private GameObject visualRoot;

        public void Configure(LaneInput[] lanes, Transform[] spawns, Transform[] hits, Color[] laneColors)
        {
            if (spawns == null || hits == null || spawns.Length < 4 || hits.Length < 4) return;

            float[] topCenters = BuildCenters(topWidth);
            float[] bottomCenters = BuildCenters(bottomWidth);

            for (int i = 0; i < 4; i++)
            {
                if (spawns[i] != null) spawns[i].position = new Vector3(topCenters[i], topY, 0f);
                if (hits[i] != null) hits[i].position = new Vector3(bottomCenters[i], hitY, 0f);

                if (lanes != null && i < lanes.Length && lanes[i] != null)
                {
                    // Oculta sprites rectos antiguos para que no compitan con la highway.
                    foreach (SpriteRenderer sr in lanes[i].GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        if (sr != null && sr.name.ToLower().Contains("lanebody"))
                            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.03f);
                    }
                }
            }

            BuildHighwayVisuals(laneColors, topCenters, bottomCenters);
            RepositionKeyIndicators();
        }

        private float[] BuildCenters(float width)
        {
            float step = width / 4f;
            float start = -width * 0.5f + step * 0.5f;
            return new[] { start, start + step, start + step * 2f, start + step * 3f };
        }

        private void BuildHighwayVisuals(Color[] laneColors, float[] topCenters, float[] bottomCenters)
        {
            if (visualRoot != null) Destroy(visualRoot);
            visualRoot = new GameObject("PB_PerspectiveHighway");
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.position = Vector3.zero;

            // Base oscura en forma de trapezoide.
            CreateQuad("Highway_Base",
                new Vector3(-topWidth * 0.5f, topY, 0.02f),
                new Vector3(topWidth * 0.5f, topY, 0.02f),
                new Vector3(bottomWidth * 0.5f, hitY, 0.02f),
                new Vector3(-bottomWidth * 0.5f, hitY, 0.02f),
                new Color(0.02f, 0.025f, 0.035f, 0.62f), -2);

            for (int i = 0; i < 4; i++)
            {
                Color c = (laneColors != null && laneColors.Length > i) ? laneColors[i] : Color.cyan;
                float topLaneW = topWidth / 4f;
                float botLaneW = bottomWidth / 4f;

                Vector3 tl = new Vector3(topCenters[i] - topLaneW * 0.43f, topY, 0.01f);
                Vector3 tr = new Vector3(topCenters[i] + topLaneW * 0.43f, topY, 0.01f);
                Vector3 br = new Vector3(bottomCenters[i] + botLaneW * 0.43f, hitY, 0.01f);
                Vector3 bl = new Vector3(bottomCenters[i] - botLaneW * 0.43f, hitY, 0.01f);
                CreateQuad("Lane_Surface_" + i, tl, tr, br, bl,
                    new Color(c.r, c.g, c.b, laneBodyAlpha), -1);

                CreateLine("Lane_CenterGlow_" + i,
                    new Vector3(topCenters[i], topY, 0f),
                    new Vector3(bottomCenters[i], hitY, 0f),
                    new Color(c.r, c.g, c.b, 0.18f), new Color(c.r, c.g, c.b, 0.62f), 0.045f, 1);
            }

            // Líneas laterales y separadores convergentes.
            for (int s = 0; s <= 4; s++)
            {
                float topX = -topWidth * 0.5f + (topWidth / 4f) * s;
                float botX = -bottomWidth * 0.5f + (bottomWidth / 4f) * s;
                CreateLine("Highway_Edge_" + s,
                    new Vector3(topX, topY, 0f), new Vector3(botX, hitY, 0f),
                    new Color(0.55f, 0.90f, 1f, 0.16f), new Color(0.95f, 1f, 1f, laneEdgeAlpha), 0.035f, 2);
            }

            // Línea de golpe tipo Festival/Guitar Hero.
            CreateLine("Festival_HitLine",
                new Vector3(-bottomWidth * 0.56f, hitY, 0f), new Vector3(bottomWidth * 0.56f, hitY, 0f),
                new Color(1f, 0.92f, 0.45f, 0.90f), new Color(0.55f, 0.95f, 1f, 0.90f), 0.10f, 5);
        }

        private void CreateLine(string name, Vector3 a, Vector3 b, Color ca, Color cb, float width, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(visualRoot.transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.positionCount = 2;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
            lr.startColor = ca;
            lr.endColor = cb;
            lr.widthMultiplier = width;
            lr.numCapVertices = 8;
            lr.sortingOrder = order;
            lr.useWorldSpace = false;
        }

        private void CreateQuad(string name, Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl, Color color, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(visualRoot.transform, false);
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            mesh.vertices = new[] { tl, tr, br, bl };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            mf.sharedMesh = mesh;
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            mr.sharedMaterial = mat;
            mr.sortingOrder = order;
        }

        private void RepositionKeyIndicators()
        {
            // Mantiene los botones D/F/J/K fuera del carril, justo debajo de la línea de golpe.
            GameObject root = GameObject.Find("PB_GameplayKeyIndicators");
            if (root == null) return;
            RectTransform rt = root.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchoredPosition = new Vector2(0f, 42f);
            rt.sizeDelta = new Vector2(620f, 70f);
        }
    }
}
