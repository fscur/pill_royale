using UnityEngine;

namespace Pills.Assets.Meta
{
    [CreateAssetMenu(fileName = "IntVariable", menuName = "Data/Meta/Int Variable")]
    public class IntVariable : ScriptableObject
    {
        public int Value;
    }
}