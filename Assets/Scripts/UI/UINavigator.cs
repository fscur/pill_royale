using System.Collections.Generic;
using System.Linq;
using Pills.Assets.Managers;
using UnityEngine;

namespace Pills.Assets.UI
{
    public class UINavigator : MonoBehaviour
    {
        [SerializeField] private GameObject[] _selectableObjects;
        private ISelectable[] _selectables;

        public ISelectable Current { get; set; }

        private void Start()
        {
            _selectables = new ISelectable[_selectableObjects.Length];
            
            for (int i = 0; i < _selectableObjects.Length; i++)
            {
                _selectables[i] = _selectableObjects[i].GetComponent<ISelectable>();
            }

            if (_selectables.Length <= 0) 
                return;
            
            Current = _selectables[0];
            Current.Select();
        }
        
        public void NavigateNext(Vector2 direction)
        {
            if (_selectables.Length == 0)
                return;
            
            var next = _selectables
                .Where(s => Vector2.Dot(s.Position - Current.Position, direction) > 0)
                .OrderBy(s => Vector2.Dot(s.Position - Current.Position, direction))
                .OrderBy(s => Vector2.Distance(Current.Position, s.Position))
                .FirstOrDefault();

            if (next == null)
                return;

            for (int i = 0; i < _selectables.Length; i++)
            {
                _selectables[i].Reset();
            }

            next.Select();
            Current = next;
            SoundManager.Play(SoundManager.SelectClip);
        }
    }
}