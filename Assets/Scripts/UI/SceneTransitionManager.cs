using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pills.Assets.UI
{
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager _instance;
        
        private static readonly int FadeOut = Animator.StringToHash("FadeOut");
        
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _audioSource;
        private SceneReference _currentScene;
        private SceneReference _nextScene;

        public static SceneTransitionManager Get()
        {
            return _instance;
        }
        
        public void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
                Destroy(gameObject);
            
            DontDestroyOnLoad(gameObject);
        }

        public void SkipTransition(SceneReference nextScene)
        {
            SceneManager.LoadScene(nextScene);
        }

        public void FadeIn()
        {
            _animator.SetTrigger("FadeIn");
        }
        
        public void FadeOutThenFadeIn(SceneReference nextScene, AudioClip transitionSound)
        {
            StartCoroutine(StartTransition(nextScene, transitionSound));
        }
        
        private IEnumerator StartTransition(SceneReference scene, AudioClip transitionSound)
        {
            _nextScene = scene;
            _animator.SetTrigger(FadeOut);
            _audioSource.PlayOneShot(transitionSound);

            while (_audioSource.isPlaying)
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