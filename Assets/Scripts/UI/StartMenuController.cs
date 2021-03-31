using System;
using System.Collections;
using System.Collections.Generic;
using Pills.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pills.Assets.UI
{
    public class StartMenuController : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] _buttonTexts;
        [SerializeField] private Image[] _buttonImages;
        [SerializeField] private GraphicRaycaster _rayCaster;

        [SerializeField] private SceneReference _onlineScene;
        [SerializeField] private SceneReference _singlePlayerScene;
        [SerializeField] private SceneReference _multiPlayerScene;
        [SerializeField] private SceneReference _optionsScene;
        
        private SceneTransitionManager _sceneTransitionManager;
        
        private readonly List<RaycastResult> _results = new List<RaycastResult>(16);
        
        private int _currentSelectedButtonIndex = 0;
        private TMP_Text _currentSelectedButton = null;
        private PointerEventData _pointerEventData;
        private EventSystem _eventSystem;
        
        private void Start()
        {
            _sceneTransitionManager = SceneTransitionManager.Get();
            _eventSystem = GetComponent<EventSystem>();
            _pointerEventData = new PointerEventData(_eventSystem);
        }
        
        private void Update()
        {
            ResetOptions();
            
            var handled = HandleMouseInput();
            
            if (handled)
                return;
            
            HandleKeyBoardInput();
        }

        private bool HandleMouseInput()
        {
            _pointerEventData = new PointerEventData(_eventSystem);
            _pointerEventData.position = Input.mousePosition;
            _results.Clear();
            _rayCaster.Raycast(_pointerEventData, _results);
            
            if (_results.Count <= 0)
                return false;
            
            var resultText = _results[0].gameObject.GetComponent<TMP_Text>();
            var index = Array.IndexOf(_buttonTexts, resultText);
            
            if (index < 0) 
                return false;
            
            if (_currentSelectedButton == resultText)
                return true;
            
            _currentSelectedButton = resultText;
            _currentSelectedButtonIndex = index;
            SoundManager.Play(SoundManager.SelectClip);
            return true;
        }

        private void HandleKeyBoardInput()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                switch (_currentSelectedButtonIndex)
                {
                    case 0:
                        OnOnlineButtonClicked();
                        break;
                    case 1:
                        OnSinglePlayerButtonClicked();
                        break;
                    case 2:
                        OnMultiPlayerButtonClicked();
                        break;
                }
            }
            
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _currentSelectedButtonIndex++;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _currentSelectedButtonIndex--;
            }
            else
            {
                return;
            }
            
            if (_currentSelectedButtonIndex < 0)
            {
                _currentSelectedButtonIndex = _buttonTexts.Length - 1;
            }
            else if (_currentSelectedButtonIndex >= _buttonTexts.Length)
            {
                _currentSelectedButtonIndex = 0;
            }
            
            _currentSelectedButton = _buttonTexts[_currentSelectedButtonIndex];
            SoundManager.Play(SoundManager.SelectClip);
        }

        private void ResetOptions()
        {
            for (var i = 0; i < _buttonTexts.Length; i++)
            {
                var text = _buttonTexts[i];
                text.color = GameConstants.UI.StartMenu.OptionColor;

                var image = _buttonImages[i];
                image.enabled = false;
            }

            _buttonTexts[_currentSelectedButtonIndex].color = GameConstants.UI.StartMenu.SelectedOptionColor;
            _buttonImages[_currentSelectedButtonIndex].enabled = true;
        }
        
        public void OnOnlineButtonClicked()
        {
            SoundManager.Play(SoundManager.StartClip);
            //_sceneTransitionManager.FadeOutThenFadeIn(_onlineScene, SoundManager.StartClip);
        }
        
        public void OnSinglePlayerButtonClicked()
        {
            _sceneTransitionManager.FadeOutThenFadeIn(_singlePlayerScene, SoundManager.StartClip);
        }
        
        public void OnMultiPlayerButtonClicked()
        {
            SoundManager.Play(SoundManager.StartClip);
            //_sceneTransitionManager.FadeOutThenFadeIn(_multiPlayerScene, SoundManager.StartClip);
        }
        
        public void OnOptionsButtonClicked()
        {
            SoundManager.Play(SoundManager.StartClip);
            //_sceneTransitionManager.FadeOutThenFadeIn(_optionsPlayerScene, SoundManager.StartClip);
        }
    }
    
}
