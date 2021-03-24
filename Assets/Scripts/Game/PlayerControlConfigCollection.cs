using UnityEngine;

namespace Pills.Assets
{
    [CreateAssetMenu(fileName = "PlayerControlConfigCollection", menuName = "Data/Player Control Config Collection")]
    public class PlayerControlConfigCollection : ScriptableObject
    {
        public PlayerControlConfig[] PlayerControlConfigs;
    }
}