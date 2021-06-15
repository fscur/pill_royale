using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pills.Assets.UI
{
    public class OptionController : Selectable
    {
        [SerializeField] protected TMP_Text _titleText;
        [SerializeField] protected TMP_Text _valueText;
        
        public event Action OnOptionLocked;
        public event Action OnOptionUnlocked;
        public event Action OnOptionBeginEdit;
        public event Action OnOptionEndEdit;
        public event Action OnOptionValueChanged;
        public event Action OnOptionError;

        private bool _selected = false;
        protected bool _editing = false;

        public bool IsLocked
        {
            get { return _editing; }
        }

        public Vector2 Position
        {
            get { return (gameObject.transform as RectTransform).position; }
        }

        public override  void Select()
        {
            //_titleText.color = GameConstants.UI.StartMenu.SelectedOptionColor;
            _selected = true;
        }

        public void Reset()
        {
            // _valueText.color = GameConstants.UI.StartMenu.OptionColor;
            // _titleText.color = GameConstants.UI.StartMenu.OptionColor;
            _selected = false;
        }

        protected virtual void Update()
        {
            if (!_selected)
                return;
            
            HandleKeyBoardInput();
        }
        
        private void HandleKeyBoardInput()
        {
            if (!_editing && Input.GetKeyDown(KeyCode.RightArrow))
            {
                _editing = true;
                OnOptionBeginEdit?.Invoke();
                
            }
            else if (_editing && Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _editing = false;
                OnOptionEndEdit?.Invoke();
            }
            else
                return;
            
            if (_editing)
            {
                OnOptionLocked?.Invoke();
            }
            else
            {
                OnOptionUnlocked?.Invoke();
            }
        }

        public virtual bool HandleMouseInput()
        {
            return false;
        }

        protected void InvokeOnErrorEvent()
        {
            OnOptionError?.Invoke();
        }
        
        protected void InvokeOnOptionValueChanged()
        {
            OnOptionValueChanged?.Invoke();
        }

        public void Unlock()
        {
            _editing = false;
            OnOptionEndEdit?.Invoke();
            OnOptionUnlocked?.Invoke();
        }
    }
}