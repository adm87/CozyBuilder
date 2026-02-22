namespace Cozy.Builder.Game.Components
{
    using Cozy.Builder.Hexagons.Components;
    using Cozy.Builder.Utility;
    using Cozy.Hexagons;
    using UnityEngine;

    public class GameBoardComponent : MonoBehaviour
    {
        [SerializeField]
        private HexagonOrientation orientation;

        [SerializeField]
        private float hexRadius = 1f;
        
        [SerializeField]
        private HexGridViewComponent hexView;

        [SerializeField]
        private VolumeComponent bounds;

        private GameBoard gameBoard;

        private void Awake()
        {
            gameBoard = new GameBoard(orientation, hexRadius);

            hexView.SetOrientation(orientation);
            hexView.SetHexRadius(hexRadius);
        }

        private void Start()
        {
            var zero = new Hexagon(0, 0);

            ActivateCell(zero);
            PlaceTile(zero);
            AdjustBounds();
        }

        /// <summary>
        /// ActivateCell is responsible for adding a hexagon coordinate to the grid and updating the grid view to reflect that change.
        /// Noop if the cell is already active.
        /// </summary>
        /// <param name="hexagon">Hexagon coordinate to activate.</param>
        private void ActivateCell(Hexagon hexagon)
        {
            if (gameBoard.Grid.TryGetHexagon(hexagon, out _))
                return; // Cell is already active.

            gameBoard.Grid.AddHexagon(hexagon);
            hexView.ToggleHexagon(hexagon, true);
        }

        /// <summary>
        /// PlaceTile is responsible for placing a tile on a given hexagon coordinate, which includes adding the tile to the game 
        /// board's tile set and activating all neighboring cells.
        /// </summary>
        /// <param name="hexagon">Hexagon coordinate to place a tile on.</param>
        private void PlaceTile(Hexagon hexagon)
        {
            if (!gameBoard.Grid.TryGetHexagon(hexagon, out _))
                return; // Cell is not active, so we can't place a tile on it.

            var id = HexagonEncoder.Encode(hexagon);

            if (gameBoard.Tiles.Contains(id))
                return; // Tile is already placed on this cell.

            gameBoard.Tiles.Add(id);

            for (int i = 0; i < HexagonMath.AxialNeighbors.Length; i++)
            {
                var (q, r) = HexagonMath.AxialNeighbors[i];
                var neighbor = new Hexagon(hexagon.Q + q, hexagon.R + r);

                ActivateCell(neighbor);
            }
        }

        private void AdjustBounds()
        {
            var min = new Vector3(float.MaxValue, 0f, float.MaxValue);
            var max = new Vector3(float.MinValue, 0f, float.MinValue);

            gameBoard.Grid.ForEach(hexagon =>
            {
                var (x, y) = HexagonMath.FromHex(hexagon.Q, hexagon.R, gameBoard.HexRadius, gameBoard.Orientation);
                var position = bounds.transform.InverseTransformPoint(new Vector3(x, 0f, y));

                min = Vector3.Min(min, position);
                max = Vector3.Max(max, position);

                return true;
            });

            var size = max - min;
            var center = bounds.transform.TransformPoint((max + min) / 2f);

            bounds.SetSize(size);
            bounds.SetCenter(center);
        }
    }
}