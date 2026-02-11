namespace Cozy.Builder.Hexagons.Components
{
    using Cozy.Hexagons;
    using UnityEngine;

    public class HexGridDataComponent : MonoBehaviour
    {
        [SerializeField]
        private RenderTexture dataTexture;

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
        }

        private void Start()
        {
            ClearDataTexture();

            Hexagon origin = new Hexagon(0, 0);
            SetHexData(origin, true);

            foreach (var neighbor in HexagonMath.AxialNeighbors)
            {
                var offset = neighbor;
                
                int q = origin.Q + offset.x;
                int r = origin.R + offset.y;

                SetHexData(new Hexagon(q, r), true);
            }
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

        public void SetHexData(Hexagon hexagon, bool isActive)
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