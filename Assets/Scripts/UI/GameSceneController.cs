using Pills.Assets.Gameplay;
using Pills.Assets.Managers;
using UnityEngine;

namespace Pills.Assets.UI
{
    public class GameSceneController : MonoBehaviour
    {
        [SerializeField] private SceneReference _endGameScreenScene;
        [SerializeField] private Transform _playersTransform;
        [SerializeField] private Transform _tileMapTransform;

        private void Start()
        {
            var map = GameManager.TileMap;
            map.transform.SetParent(_tileMapTransform);
            
            for (var i = 0; i < GameManager.Settings.PlayerCount.Value; i++)
            {
                var playerController = GameManager.SpawnPlayer(i);
                playerController.transform.SetParent(_playersTransform);
                playerController.GameOver += PlayerControllerOnGameOver;
            }
        }

        private void PlayerControllerOnGameOver(GameOverInfo gameOverInfo)
        {
            SceneTransitionManager.FadeOutThenFadeIn(_endGameScreenScene, SoundManager.GameOverClip);
        }
    }
}