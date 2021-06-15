using System;
using System.Collections.Generic;
using Pills.Assets.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pills.Assets.UI
{
    public class UISceneController : MonoBehaviour
    {
        private readonly List<RaycastResult> _results = new List<RaycastResult>(16);
        
        private Selectable _currentSelection;
        private EventSystem _eventSystem;
        
        [SerializeField] private GraphicRaycaster _rayCaster = null;
        [SerializeField] private UINavigator _navigator = null;

        private void Start()
        {
            _eventSystem = GetComponent<EventSystem>(); 
            _currentSelection = _navigator.Selectables[0];
        }

        private void Update()
        {   
            var handled = HandleMouseInput();

            if (handled)
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
            var pointerEventData = new PointerEventData(_eventSystem)
            {
                position = Input.mousePosition
            };

            _results.Clear();
            _rayCaster.Raycast(pointerEventData, _results);
        
            if (_results.Count <= 0)
                return false;
        
            var selection = _results[0].gameObject.GetComponent<Selectable>();
            var index = Array.IndexOf(_navigator.Selectables, selection);
        
            if (index < 0) 
                return false;
        
            if (selection == _currentSelection)
                return true;

            selection.Select();
            _currentSelection = selection;
            _navigator.Current = selection;
            SoundManager.Play(SoundManager.SelectClip);
            return true;
        }
    }
}