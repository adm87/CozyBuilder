using Cozy.Hexagons;
using Cozy.Hexagons.Components;
using UnityEngine;

namespace Cozy.Builder.GridVisibilityComponent
{
    public class GridComponent : HexagonGridComponent
    {
        [SerializeField]
        private GridVisibilityComponent visibilityComponent;

        private void Start()
        {
            UnlockTile(new Hexagon(0, 0));
        }

        private void UnlockTile(Hexagon hexagon)
        {
            Grid.AddHexagon(hexagon);

            foreach (var (x, y) in HexagonMath.AxialNeighbors)
            {
                var neighbor = new Hexagon(hexagon.Q + x, hexagon.R + y);
                Grid.AddHexagon(neighbor);

                visibilityComponent.AddVisibility(neighbor);
            }

            visibilityComponent.AddVisibility(hexagon);
        }
    }
}