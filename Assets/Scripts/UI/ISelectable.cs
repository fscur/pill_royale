using UnityEngine;

namespace Pills.Assets.UI
{
    public interface ISelectable
    {
        Vector2 Position { get; }
        void Select();
        void Reset();
    }
}