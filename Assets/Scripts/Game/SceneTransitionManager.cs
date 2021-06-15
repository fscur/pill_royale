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

        public static void SkipTransition(SceneReference nextScene)
        {
            SceneManager.LoadScene(nextScene);
        }

        public static void FadeIn()
        {
            _instance._animator.SetTrigger(FadeInTrigger);
        }
        
        public static void FadeOutThenFadeIn(SceneReference nextScene, AudioClip transitionSound)
        {
            _instance.StartCoroutine(StartTransition(nextScene, transitionSound));
        }
        
        private static IEnumerator StartTransition(SceneReference scene, AudioClip transitionSound)
        {
            _instance._nextScene = scene;
            _instance._animator.SetTrigger(FadeOutTrigger);
            SoundManager.Play(transitionSound);
            
            while (SoundManager.IsPlaying)
                yield return null;
        }

        public void OnFadeOutComplete()
        {
            StartCoroutine(LoadScene(_instance._nextScene));
        }

        private static IEnumerator LoadScene(SceneReference scene)
        {
            var asyncOp = SceneManager.LoadSceneAsync(scene.ScenePath, LoadSceneMode.Single);

            while (!asyncOp.isDone)
                yield return null;

            FadeIn();

            yield return null;
        }
    }
}