using UnityEngine;

namespace Pills.Assets
{
    public class GameConstants
    {
        public class Input
        {
            public const float Speed = 1.0f/50.0f;
            public const float KeyDownSpeed = 0.25f;    
        }

        public class UI
        {
            public class StartMenu
            {
                //public static Color SelectedOptionColor = new Color(1, 0.3598299f, 0, 1);
                //public static Color OptionColor = Color.white;
            }
        }

        public const int MinSeedValue = 1;
        public const int MaxSeedValue = 1024;
        public const int BlownQueueCapacity = 16;
        public const float MaxDifficulty = 20.0f;
        public const float MinVirusSpawnChance = 0.2f;
        
        public const int BoardWidth = 10;
        public const int BoardHeight = 18;
        public static Vector2Int SpawnPosition = new Vector2Int(BoardWidth / 2 - 1, BoardHeight - 2);
        public const int MinConsecutiveCells = 4;
        public const int MinVirusPlacementHeight = 3;

        public static readonly float[] RoundSpeeds = new float[3];

        
        static GameConstants()
        {
            //RoundSpeeds[0] = 5.0f;
            RoundSpeeds[0] = 1.0f;
            RoundSpeeds[1] = 0.75f;
            RoundSpeeds[2] = 0.5f;
        }
    }
}