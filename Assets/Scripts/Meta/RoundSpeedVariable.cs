using Pills.Assets.Game;
using UnityEngine;

namespace Pills.Assets.Meta
{
    [CreateAssetMenu(fileName = "RoundSpeedVariable", menuName = "Data/Meta/Round Speed Variable")]
    public class RoundSpeedVariable : ScriptableObject
    {
        public RoundSpeed Value;
    }
}