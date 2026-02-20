namespace Cozy.Builder.Hexagons.Components
{
    using Cozy.Hexagons;
    using UnityEngine;

    public class HexGridViewComponent : MonoBehaviour
    {
        [SerializeField]
        private RenderTexture dataTexture;

        [SerializeField]
        private MeshRenderer gridRenderer;

        private Texture2D whitePixel;

        private Texture2D blackPixel;

        private void Awake()
        {
            whitePixel = new Texture2D(1, 1);
            whitePixel.SetPixel(0, 0, Color.white);
            whitePixel.Apply();

            blackPixel = new Texture2D(1, 1);
            blackPixel.SetPixel(0, 0, Color.black);
            blackPixel.Apply();

            ClearDataTexture();
        }

        private void OnDestroy()
        {
            ClearDataTexture();

            Destroy(whitePixel);
            Destroy(blackPixel);
        }

        private void ClearDataTexture()
        {
            if (dataTexture == null) return;

            RenderTexture.active = dataTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }

        public void SetOrientation(HexagonOrientation orientation)
        {
                if (gridRenderer == null) return;

                float orientationValue = orientation switch
                {
                    HexagonOrientation.PointyTop => 0f,
                    HexagonOrientation.FlatTop => 1f,
                    _ => 0f
                };
    
                var block = new MaterialPropertyBlock();
                gridRenderer.GetPropertyBlock(block);
                block.SetFloat("_Orientation", orientationValue);
                gridRenderer.SetPropertyBlock(block);
        }

        public void SetHexRadius(float radius)
        {
            if (gridRenderer == null) return;

            var block = new MaterialPropertyBlock();
            gridRenderer.GetPropertyBlock(block);
            block.SetFloat("_HexRadius", radius);
            gridRenderer.SetPropertyBlock(block);
        }

        public void ToggleHexagon(Hexagon hexagon, bool isActive)
        {
            if (dataTexture == null) return;

            int x = hexagon.Q + (dataTexture.width / 2);
            int y = hexagon.R + (dataTexture.height / 2);

            RenderTexture.active = dataTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, dataTexture.width, 0, dataTexture.height);

            Texture2D pixel = isActive ? whitePixel : blackPixel;
            Graphics.DrawTexture(new Rect(x, y, 1, 1), pixel);

            GL.PopMatrix();
            GL.Flush();
            RenderTexture.active = null;
        }
    }
}