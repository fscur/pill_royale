using Pills.Assets.Game;
using UnityEngine;

namespace Pills.Assets.Meta
{
    [CreateAssetMenu(fileName = "GameModeVariable", menuName = "Data/Meta/Game Mode Variable")]
    public class GameModeVariable : ScriptableObject
    {
        public GameMode Value;
    }
}