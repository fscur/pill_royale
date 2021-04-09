
using System;
using Pills.Assets.Meta;

namespace Pills.Assets.Game
{
    [Serializable]
    public class GameSettings
    {
        public IntVariable PlayerCount;
        public IntVariable RoundSpeed;
        public IntVariable VirusLevel;
        public GameModeVariable GameMode;
    }
}