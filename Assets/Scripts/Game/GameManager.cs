using UnityEngine;

namespace Pills.Assets
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        private bool _isPaused;

        [SerializeField] private PlayerControlConfigCollection _playerControlConfigCollection = null;

        public PlayerControlConfig[] PlayerControlConfigs
        {
            get { return _playerControlConfigCollection.PlayerControlConfigs; }
        }
        
        public static GameManager Get()
        {
            if (_instance == null)
                _instance = GameObject.Find("GameManager").GetComponent<GameManager>();
            
            return _instance;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                _isPaused = !_isPaused;
            }
        }

        public bool IsPaused
        {
            get { return _isPaused; }
        }
    }
}