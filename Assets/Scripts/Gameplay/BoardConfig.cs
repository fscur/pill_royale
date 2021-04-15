using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pills.Assets.Gameplay
{
    [CreateAssetMenu(fileName = "BoardConfig_", menuName = "Data/Board Config")]
    public class BoardConfig : ScriptableObject
    {
        public Vector2Int[] PlayerPositions;
        public TileBase[] PillTiles;
        public Tilemap TileMap;
    }
}