using System;
using System.Collections.Generic;
using Pills.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pills.Assets.UI
{
    public class UpDownOptionController : OptionController
    {
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;
        [SerializeField] private int _minValue;
        [SerializeField] private int _maxValue;
        [SerializeField] private GraphicRaycaster _rayCaster;
        
        private readonly List<RaycastResult> _results = new List<RaycastResult>(16);
        private PointerEventData _pointerEventData;
        private EventSystem _eventSystem;
        
        private int _currentValue;
        private bool _isDirty = true;

        private void Start()
        {
            _eventSystem = GetComponent<EventSystem>();
            _pointerEventData = new PointerEventData(_eventSystem);
        }
        
        protected override void Update()
        {
            Reset();
            base.Update();
            HandleKeyBoardInput();
            
            var handled = HandleMouseInput();

            if (!_isDirty)
                return;

            _valueText.text = _currentValue.ToString();
            
            if (handled)
                return;
            
            if (_editing)
            {
                _upButton.image.color = GameConstants.UI.StartMenu.SelectedOptionColor;
                _downButton.image.color = GameConstants.UI.StartMenu.SelectedOptionColor;
            }
            else
            {
                _upButton.image.color = GameConstants.UI.StartMenu.OptionColor;
                _downButton.image.color = GameConstants.UI.StartMenu.OptionColor;
            }
        }

        private void Reset()
        {
            _upButton.image.color = GameConstants.UI.StartMenu.OptionColor;
            _downButton.image.color = GameConstants.UI.StartMenu.OptionColor;
        }
        
        private void ValidateCurrentValue()
        {
            if (_currentValue < _minValue)
            {
                _currentValue = _minValue;
                InvokeOnErrorEvent();
            }
            else if (_currentValue > _maxValue)
            {
                _currentValue = _maxValue;
                InvokeOnErrorEvent();
            }
            else
            {
                InvokeOnOptionValueChanged();
            }
        }

        private void HandleKeyBoardInput()
        {
            if (!_editing)
                return;
        
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _currentValue--;
                _isDirty = true;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _currentValue++;
                _isDirty = true;
            }
            else
            {
                return;
            }
            
            ValidateCurrentValue();
        }
        
        public override bool HandleMouseInput()
        {
            _pointerEventData = new PointerEventData(_eventSystem);
            _pointerEventData.position = Input.mousePosition;
            _results.Clear();
            _rayCaster.Raycast(_pointerEventData, _results);
        
            if (_results.Count <= 0)
                return false;
        
            var button = _results[0].gameObject.GetComponent<Button>();
            if (button == null)
                return false;

            button.image.color = GameConstants.UI.StartMenu.SelectedOptionColor;
            
            return true;
        }
        
        public void OnUpButtonClicked()
        {
            if (_currentValue == _maxValue)
            {
                InvokeOnErrorEvent();
                return;
            }
            
            _currentValue++;
            _isDirty = true;
            
            ValidateCurrentValue();
        }
        
        public void OnDownButtonClicked()
        {
            if (_currentValue == _minValue)
            {
                InvokeOnErrorEvent();
                return;
            }

            _currentValue--;
            _isDirty = true;
            
            ValidateCurrentValue();
        }
    }
}