using UnityEngine;

namespace Pills.Assets.Game
{
    public class CameraController : MonoBehaviour
    {
        private static CameraController _instance;
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
    }
}