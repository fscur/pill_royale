using System;
using System.Collections;
using System.Collections.Generic;
using Pills.Assets;
using Pills.Assets.Game;
using Pills.Assets.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pills.Assets.UI
{
    public class TitleScreenSceneController : UISceneController
    {
        [SerializeField] private SceneReference _onlineScene = null;
        [SerializeField] private SceneReference _singlePlayerScene = null;
        [SerializeField] private SceneReference _multiPlayerScene = null;
        [SerializeField] private SceneReference _optionsPlayerScene = null;

        public void OnOnlineButtonClicked()
        {
            GameManager.Settings.GameMode.Value = GameMode.Online;
            SoundManager.Play(SoundManager.StartClip);
            //_sceneTransitionManager.FadeOutThenFadeIn(_onlineScene, SoundManager.StartClip);
        }

        public void OnSinglePlayerButtonClicked()
        {
            GameManager.Settings.PlayerCount.Value = 2;
            GameManager.Settings.GameMode.Value = GameMode.SinglePlayer;
            SceneTransitionManager.FadeOutThenFadeIn(_singlePlayerScene, SoundManager.StartClip);
        }

        public void OnMultiPlayerButtonClicked()
        {
            GameManager.Settings.GameMode.Value = GameMode.MultiPlayer;
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