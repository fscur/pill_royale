using Pills.Assets.Game;
using Pills.Assets.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pills.Assets.Managers
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        private bool _isPaused;
        private int _currentPlayerId; 
            
        [SerializeField] private GameSettings _settings;
        [SerializeField] private PlayerControlConfigCollection _playerControlConfigCollection = null;
        [SerializeField] private PlayerController _playerControllerTemplate;
        [SerializeField] private BoardConfig _boardConfig = null;

        private Tilemap _tileMap;
        public static Tilemap TileMap
        {
            get
            {
                if (_instance._tileMap == null)
                {
                    _instance._tileMap = GameObject.Instantiate(_instance._boardConfig.TileMap);    
                }
                
                return _instance._tileMap;
            }
        }
        
        public static Tile[] PillTiles
        {
            get
            {
                return _instance._boardConfig.PillTiles;
            }
        }
        
        public static GameSettings Settings
        {
            get
            {
                return _instance._settings;
            }
        }
        
        public static PlayerControlConfig[] PlayerControlConfigs
        {
            get { return _instance._playerControlConfigCollection.PlayerControlConfigs; }
        }   

        public static bool IsPaused
        {
            get { return _instance._isPaused; }
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

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                _isPaused = !_isPaused;
            }
        }

        public static PlayerController SpawnPlayer(int playerId)
        {
            _instance._playerControllerTemplate.gameObject.SetActive(false);
            
            var playerController = GameObject.Instantiate(_instance._playerControllerTemplate);
            var player = new Player
            {
                Id = playerId
            };
            
            playerController.Player = player;
            playerController.Position = _instance._boardConfig.PlayerPositions[playerId];
            playerController.gameObject.SetActive(true);
            return playerController;
        }
    }
}