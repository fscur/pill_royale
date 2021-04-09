using Pills.Assets.Managers;
using UnityEngine;

namespace Pills.Assets.UI
{
    public class PreStartController : MonoBehaviour
    {
        //note: this scene is loaded first so the DontDestroyOnLoad gameObjects can be created
        
        [SerializeField] private SceneTransitionManager _sceneTransitionManager;
        [SerializeField] private SceneReference _startMenuScene;
        
        private void Update()
        {
            _sceneTransitionManager.SkipTransition(_startMenuScene);
        }
    }
}