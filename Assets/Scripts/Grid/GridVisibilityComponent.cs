using Cozy.Hexagons;
using UnityEngine;
using UnityEngine.UI;

namespace Cozy.Builder.GridVisibilityComponent
{
    [RequireComponent(typeof(MeshRenderer))]
    public class GridVisibilityComponent : MonoBehaviour
    {
        [SerializeField]
        private RenderTexture visibilityTexture;

        private Texture2D visible;

        private Texture2D hidden;

        private void Awake()
        {
            var gridMeshRenderer = GetComponent<MeshRenderer>();
            gridMeshRenderer.material.SetTexture("_visibilityTex", visibilityTexture);

            Vector2 gridSize = visibilityTexture != null
                ? new Vector2(visibilityTexture.width, visibilityTexture.height)
                : Vector2.one;
            gridMeshRenderer.material.SetVector("_gridSize", gridSize);

            Fill(Color.black);

            visible = new Texture2D(1, 1, TextureFormat.R8, false);
            visible.SetPixel(0, 0, Color.white);
            visible.Apply();

            hidden = new Texture2D(1, 1, TextureFormat.R8, false);
            hidden.SetPixel(0, 0, Color.black);
            hidden.Apply();
        }

        private void OnDestroy()
        {
            if (visibilityTexture != null)
            {
                Fill(Color.black);
            }
        }

        private void Fill(Color color)
        {
            if (visibilityTexture == null)
            {
                return;
            }

            var previousRT = RenderTexture.active;
            RenderTexture.active = visibilityTexture;

            GL.Clear(true, true, color);

            RenderTexture.active = previousRT;
        }

        public void AddVisibility(Hexagon hexagon)
        {
            SetPixel(hexagon.Q, hexagon.R, true);
        }

        public void RemoveVisibility(Hexagon hexagon)
        {
            SetPixel(hexagon.Q, hexagon.R, false);
        }

        private void SetPixel(int x, int y, bool isVisible)
        {
            if (visibilityTexture == null)
            {
                return;
            }

            x += visibilityTexture.width / 2;
            y += visibilityTexture.height / 2;

            Texture2D tex = isVisible ? visible : hidden;
            Graphics.CopyTexture(tex, 0, 0, 0, 0, 1, 1, visibilityTexture, 0, 0, x, y);
        }
    }
}
