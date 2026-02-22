namespace Cozy.Builder.Game
{
    using System.Collections.Generic;
    using Cozy.Builder.Hexagons;
    using Cozy.Hexagons;
    using UnityEngine;
    using GameTiles = System.Collections.Generic.HashSet<long>;

    public class GameBoard
    {
        private readonly HexagonGrid grid;
        private readonly GameTiles tiles;
        private readonly HexagonOrientation orientation;
        private readonly float hexRadius;

        public HexagonGrid Grid => grid;
        public GameTiles Tiles => tiles;
        public HexagonOrientation Orientation => orientation;
        public float HexRadius => hexRadius;

        public GameBoard(HexagonOrientation orientation, float hexRadius)
        {
            this.orientation = orientation;
            this.hexRadius = hexRadius;
            
            grid = new HexagonGrid();
            tiles = new GameTiles();
        }

        public bool IsTilePlaced(Hexagon hexagon)
        {
            var id = HexagonEncoder.Encode(hexagon);
            return tiles.Contains(id);
        }

        public bool TryPlaceTile(Hexagon hexagon)
        {
            var id = HexagonEncoder.Encode(hexagon);

            if (tiles.Contains(id))
                return false;

            tiles.Add(id);
            return true;
        }
    }
}