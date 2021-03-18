using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pills.Assets
{
    [CreateAssetMenu(fileName = "BoardConfig_", menuName = "Data/Board Config")]
    public class BoardConfig : ScriptableObject
    {
        public Tile[] PillTiles;
    }
}