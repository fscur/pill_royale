using System.Linq;
using UnityEngine;

namespace Pills.Assets.UI
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _selectClip;
        [SerializeField] private AudioClip _backClip;
        [SerializeField] private AudioClip _startClip;
        [SerializeField] private AudioClip _errorClip;

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
        
        public static AudioClip SelectClip
        {
            get
            {
                return _instance._selectClip;
            }
        }

        public static AudioClip BackClip
        {
            get
            {
                return _instance._backClip;
            }
        }
        
        public static AudioClip StartClip
        {
            get
            {
                return _instance._startClip;
            }
        }
        
        public static AudioClip ErrorClip
        {
            get
            {
                return _instance._errorClip;
            }
        }

        public static void Play(AudioClip clip)
        {
            _instance._audioSource.PlayOneShot(clip);
        }
    }
}