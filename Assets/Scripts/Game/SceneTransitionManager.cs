using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pills.Assets.Managers
{
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager _instance;
        
        private static readonly int FadeOutTrigger = Animator.StringToHash("FadeOut");
        private static readonly int FadeInTrigger = Animator.StringToHash("FadeIn");
        
        [SerializeField] private Animator _animator;
        
        private SceneReference _currentScene;
        private SceneReference _nextScene;

        public static SceneTransitionManager Get()
        {
            return _instance;
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(this);
        } 

        public void SkipTransition(SceneReference nextScene)
        {
            SceneManager.LoadScene(nextScene);
        }

        public void FadeIn()
        {
            _animator.SetTrigger(FadeInTrigger);
        }
        
        public void FadeOutThenFadeIn(SceneReference nextScene, AudioClip transitionSound)
        {
            StartCoroutine(StartTransition(nextScene, transitionSound));
        }
        
        private IEnumerator StartTransition(SceneReference scene, AudioClip transitionSound)
        {
            _nextScene = scene;
            _animator.SetTrigger(FadeOutTrigger);
            SoundManager.Play(transitionSound);
            
            while (SoundManager.IsPlaying)
                yield return null;
        }

        public void OnFadeOutComplete()
        {
            StartCoroutine(LoadScene(_nextScene));
        }

        private IEnumerator LoadScene(SceneReference scene)
        {
            var asyncOp = SceneManager.LoadSceneAsync(scene.ScenePath, LoadSceneMode.Single);

            while (!asyncOp.isDone)
                yield return null;

            FadeIn();

            yield return null;
        }
    }
}