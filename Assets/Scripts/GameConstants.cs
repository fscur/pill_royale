namespace Pills.Assets
{
    public class GameConstants
    {
        public const float MaxDifficulty = 20.0f;
        public const float MinVirusSpawnChance = 0.2f;
        
        public const int BoardWidth = 10;
        public const int BoardHeight = 18;
        public const int MinConsecutiveCells = 4;
        public const int MinVirusPlacementHeight = 3;

        public static readonly float[] RoundSpeeds = new float[3];

        public class Input
        {
            public const float Speed = 1.0f/50.0f;
            public const float KeyDownSpeed = 0.25f;    
        }
        
        static GameConstants()
        {
            RoundSpeeds[0] = 1.0f;
            RoundSpeeds[1] = 0.75f;
            RoundSpeeds[2] = 0.5f;
        }
    }
}