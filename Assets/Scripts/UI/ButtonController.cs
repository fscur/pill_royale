
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pills.Assets.UI
{
    public class ButtonController : MonoBehaviour, ISelectable
    {
        [SerializeField] protected TMP_Text _titleText;
        [SerializeField] protected Image _image;
        
        public Vector2 Position
        {
            get { return (gameObject.transform as RectTransform).position; }
        }
        
        public void Select()
        {
            _titleText.color = GameConstants.UI.StartMenu.SelectedOptionColor;
            _image.color = Color.clear;   
        }

        public void Reset()
        {
            _image.color = Color.clear;
            _titleText.color = GameConstants.UI.StartMenu.OptionColor;
        }
        
    }
}