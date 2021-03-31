// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
//
// namespace Pills.Assets.UI
// {
//     public class OptionsMenuController : MonoBehaviour
//     {
//         [SerializeField] private OptionController[] _options;
//         
//         private int _currentOptionIndex = 0;
//         private GameObject _currentOption = null;
//         private bool _inputLocked;
//         
//         [SerializeField] private GraphicRaycaster _rayCaster;
//         [SerializeField] private SceneReference _gameScene;
//         [SerializeField] private SceneReference _startMenuScene;
//         
//         private readonly List<RaycastResult> _results = new List<RaycastResult>(16);
//         private PointerEventData _pointerEventData;
//         private EventSystem _eventSystem;
//         private SceneTransitionManager _sceneTransitionManager;
//         
//         private void Start()
//         {
//             _eventSystem = GetComponent<EventSystem>();
//             _pointerEventData = new PointerEventData(_eventSystem);
//             _sceneTransitionManager = SceneTransitionManager.Get();
//         
//             for (int i = 0; i < _options.Length; i++)
//             {
//                 _options[i].OnOptionLocked += LockInput;
//                 _options[i].OnOptionUnlocked += UnlockInput;
//                 _options[i].OnOptionError += OnError;
//                 _options[i].OnOptionValueChanged += OnValueChanged;
//                 _options[i].OnOptionBeginEdit += OnBeginEdit;
//                 _options[i].OnOptionEndEdit += OnEndEdit;
//             }
//         }
//
//         private void OnDestroy()
//         {
//             for (int i = 0; i < _options.Length; i++)
//             {
//                 _options[i].OnOptionLocked += LockInput;
//                 _options[i].OnOptionUnlocked += UnlockInput;
//                 _options[i].OnOptionError += OnError;
//                 _options[i].OnOptionValueChanged += OnValueChanged;
//                 _options[i].OnOptionBeginEdit += OnBeginEdit;
//                 _options[i].OnOptionEndEdit += OnEndEdit;
//             }
//         }
//
//         private void OnEndEdit()
//         {
//             SoundManager.Play(SoundManager.SelectClip);
//         }
//
//         private void OnBeginEdit()
//         {
//             SoundManager.Play(SoundManager.SelectClip);
//         }
//
//         private void OnValueChanged()
//         {
//             SoundManager.Play(SoundManager.SelectClip);
//         }
//
//         private void OnError()
//         {
//             SoundManager.Play(SoundManager.ErrorClip);
//         }
//
//         private void LockInput()
//         {
//             _inputLocked = true;
//         }
//         
//         private void UnlockInput()
//         {
//             _inputLocked = false;
//         }
//
//         private void Update()
//         {   
//             ResetOptions();
//             var handled = HandleMouseInput();
//
//             if (handled)
//                 return;
//             
//             if (_inputLocked)
//                 return;
//             
//             HandleKeyBoardInput();
//         }
//         
//         private void HandleKeyBoardInput()
//         {
//             if (Input.GetKeyDown(KeyCode.DownArrow))
//             {
//                 _currentOptionIndex++;
//             }
//             else if (Input.GetKeyDown(KeyCode.UpArrow))
//             {
//                 _currentOptionIndex--;
//             }
//             else
//             {
//                 return;
//             }
//         
//             if (_currentOptionIndex < 0)
//             {
//                 _currentOptionIndex = _options.Length - 1;
//             }
//             else if (_currentOptionIndex >= _options.Length)
//             {
//                 _currentOptionIndex = 0;
//             }
//             
//             SoundManager.Play(SoundManager.SelectClip);
//         }
//         
//         private bool HandleMouseInput()
//         {
//             _pointerEventData = new PointerEventData(_eventSystem);
//             _pointerEventData.position = Input.mousePosition;
//             _results.Clear();
//             _rayCaster.Raycast(_pointerEventData, _results);
//         
//             if (_results.Count <= 0)
//                 return false;
//         
//             var resultOption = _results[0].gameObject.GetComponent<OptionController>();
//
//             var index = Array.IndexOf(_options, resultOption);
//         
//             if (index < 0) 
//                 return false;
//         
//             if (index == _currentOptionIndex)
//                 return true;
//
//             _options[_currentOptionIndex].Unlock();
//             _currentOptionIndex = index;
//             SoundManager.Play(SoundManager.SelectClip);
//             return true;
//         }
//         
//         private void ResetOptions()
//         {
//             for (var i = 0; i < _options.Length; i++)
//             {
//                 _options[i].Reset();
//             }
//             
//             _options[_currentOptionIndex].Select();
//         }
//
//         public void OnBackButtonClicked()
//         {
//             _sceneTransitionManager.FadeOutThenFadeIn(_startMenuScene, SoundManager.BackClip);
//         }
//         
//         public void OnPlayButtonClicked()
//         {
//             _sceneTransitionManager.FadeOutThenFadeIn(_gameScene, SoundManager.StartClip);
//         }
//     }
// }