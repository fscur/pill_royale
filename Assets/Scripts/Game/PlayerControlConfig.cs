using UnityEngine;

namespace Pills.Assets
{
    [CreateAssetMenu(fileName = "PlayerControlConfig", menuName = "Data/Player Control Config")]
    public class PlayerControlConfig : ScriptableObject
    {
        public KeyCode DownKey;
        public KeyCode LeftKey;
        public KeyCode RightKey;
        public KeyCode RotateClockwiseKey;
        public KeyCode RotateCounterClockwiseKey;
    }
}