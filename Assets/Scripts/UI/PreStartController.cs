using Pills.Assets.Managers;
using UnityEngine;

namespace Pills.Assets.UI
{
    public class PreStartController : MonoBehaviour
    {
        [SerializeField] private SceneReference _startMenuScene;
        
        private void Update()
        {
            SceneTransitionManager.SkipTransition(_startMenuScene);
        }
    }
}