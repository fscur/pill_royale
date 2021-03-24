using UnityEngine;

namespace Pills.Assets.UI
{
    public class GameSceneController : MonoBehaviour
    {
        private SceneTransitionManager _sceneTransitionManager;
        private void Start()
        {
            _sceneTransitionManager = SceneTransitionManager.Get();
        }
    }
}