using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pills.Assets.UI
{
    public class HoverBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text _text;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            Select();
        }

        private void Select()
        {
            _text.color = GameConstants.UI.StartMenu.SelectedOptionColor;
            SoundManager.Play(SoundManager.SelectClip);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Reset();
        }

        private void Reset()
        {
            _text.color = GameConstants.UI.StartMenu.OptionColor;
        }
    }
}