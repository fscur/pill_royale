using System;
using System.Collections.Generic;
using Pills.Assets.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pills.Assets.UI
{
    public class GameSettingsSceneController : MonoBehaviour
    {
        [SerializeField] private OptionController[] _options;
        [SerializeField] private GraphicRaycaster _rayCaster;
        [SerializeField] private SceneReference _gameScene;
        [SerializeField] private SceneReference _titleScreenScene;
        [SerializeField] private UINavigator _navigator;
        
        private Selectable _currentSelection;
        private bool _inputLocked;
        private readonly List<RaycastResult> _results = new List<RaycastResult>(16);
        private PointerEventData _pointerEventData;
        private EventSystem _eventSystem;

        private void Start()
        {
            _eventSystem = GetComponent<EventSystem>();
            _pointerEventData = new PointerEventData(_eventSystem);

            _currentSelection = _navigator.Selectables[0];
            
            for (int i = 0; i < _options.Length; i++)
            {
                _options[i].OnOptionLocked += LockInput;
                _options[i].OnOptionUnlocked += UnlockInput;
                _options[i].OnOptionError += OnError;
                _options[i].OnOptionValueChanged += OnValueChanged;
                _options[i].OnOptionBeginEdit += OnBeginEdit;
                _options[i].OnOptionEndEdit += OnEndEdit;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _options.Length; i++)
            {
                _options[i].OnOptionLocked += LockInput;
                _options[i].OnOptionUnlocked += UnlockInput;
                _options[i].OnOptionError += OnError;
                _options[i].OnOptionValueChanged += OnValueChanged;
                _options[i].OnOptionBeginEdit += OnBeginEdit;
                _options[i].OnOptionEndEdit += OnEndEdit;
            }
        }

        private void OnEndEdit()
        {
           SoundManager.Play(SoundManager.SelectClip);
        }

        private void OnBeginEdit()
        {
            SoundManager.Play(SoundManager.SelectClip);
        }

        private void OnValueChanged()
        {
            SoundManager.Play(SoundManager.SelectClip);
        }

        private void OnError()
        {
            SoundManager.Play(SoundManager.ErrorClip);
        }

        private void LockInput()
        {
            _inputLocked = true;
        }
        
        private void UnlockInput()
        {
            _inputLocked = false;
        }

        private void Update()
        {   
            var handled = HandleMouseInput();

            if (handled)
                return;
            
            if (_inputLocked)
                return;
            
            HandleKeyBoardInput();
        }
        
        private void HandleKeyBoardInput()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _navigator.NavigateNext(new Vector2(0, -1));
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _navigator.NavigateNext(new Vector2(0, 1));
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _navigator.NavigateNext(new Vector2(-1, 0));
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _navigator.NavigateNext(new Vector2(1, 0));
            }
        }
        
        private bool HandleMouseInput()
        {
            _pointerEventData = new PointerEventData(_eventSystem);
            _pointerEventData.position = Input.mousePosition;
            _results.Clear();
            _rayCaster.Raycast(_pointerEventData, _results);
        
            if (_results.Count <= 0)
                return false;
        
            var selection = _results[0].gameObject.GetComponent<Selectable>();

            var index = Array.IndexOf(_navigator.Selectables, selection);
        
            if (index < 0) 
                return false;
        
            if (selection == _currentSelection)
                return true;

            if (_currentSelection is OptionController option)
            {
                if (option.IsLocked)
                {
                    option.Unlock();
                }
            }

            selection.Select();
            _currentSelection = selection;
            _navigator.Current = selection;
            SoundManager.Play(SoundManager.SelectClip);
            return true;
        }

        public void OnBackButtonClicked()
        {
            SceneTransitionManager.FadeOutThenFadeIn(_titleScreenScene, SoundManager.BackClip);
        }
        
        public void OnPlayButtonClicked()
        {
            SceneTransitionManager.FadeOutThenFadeIn(_gameScene, SoundManager.StartClip);
        }
    }
}