using System.Collections.Generic;
using System.Linq;
using Pills.Assets.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Pills.Assets.UI
{
    public class UINavigator : MonoBehaviour
    {
        [SerializeField] private Selectable[] _selectables = null;
        
        public Selectable Current { get; set; }

        public Selectable[] Selectables
        {
            get { return _selectables; }
        }
        
        private void Start()
        {   
            for (var i = 0; i < _selectables.Length; i++)
            {
                _selectables[i] = _selectables[i].GetComponent<Selectable>();
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
                .Where(s => DotProduct(s, direction) > 0)
                .OrderBy(s=> DotProduct(s, direction))
                .OrderBy(Distance)
                .FirstOrDefault();

            if (next == null)
                return;

            next.Select();
            Current = next;
            SoundManager.Play(SoundManager.SelectClip);
        }

        private float Distance(Selectable selectable) => Vector2.Distance(((RectTransform) Current.gameObject.transform).position, ((RectTransform) selectable.gameObject.transform).position);
        private float DotProduct(Selectable selectable, Vector3 direction) => Vector2.Dot(((RectTransform) selectable.gameObject.transform).position - ((RectTransform) Current.gameObject.transform).position, direction);
    }
}