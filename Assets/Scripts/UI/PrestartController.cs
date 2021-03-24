using UnityEngine;

namespace Pills.Assets.UI
{
    public class PrestartController : MonoBehaviour
    {
        [SerializeField] private SceneTransitionManager _sceneTransitionManager;
        [SerializeField] private SceneReference _startMenuScene;
        
        private void Update()
        {
            _sceneTransitionManager.SkipTransition(_startMenuScene);
        }
    }
}