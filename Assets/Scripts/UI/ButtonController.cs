using TMPro;
using UnityEngine;

namespace Pills.Assets.UI
{
    public class ButtonController : MonoBehaviour, ISelectable
    {
        [SerializeField] protected TMP_Text _titleText;
        
        public Vector2 Position
        {
            get { return (gameObject.transform as RectTransform).position; }
        }
        
        public void Select()
        {
            _titleText.color = GameConstants.UI.StartMenu.SelectedOptionColor;
        }

        public void Reset()
        {
            _titleText.color = GameConstants.UI.StartMenu.OptionColor;
        }
        
    }
}