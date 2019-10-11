using UnityEngine;

namespace Pills.Assets
{
    public enum CellType
    {
        Empty = 0,
        Wall = 1,
        Virus = 2,
        Red = 4,
        Blue = 8,
        Yellow = 16,
        Left = 32,
        Right = 64,
        Up = 128,
        Down = 256,
        RedVirus = Red | Virus,
        RedUp = Red | Up,
        RedDown = Red | Down,
        RedLeft = Red | Left,
        RedRight = Red | Right,
        BlueVirus = Blue | Virus,
        BlueUp = Blue | Up,
        BlueDown = Blue | Down,
        BlueLeft = Blue | Left,
        BlueRight = Blue | Right,
        YellowVirus = Yellow | Virus,
        YellowUp = Yellow | Up,
        YellowDown = Yellow | Down,
        YellowLeft = Yellow | Left,
        YellowRight = Yellow | Right
    }
    public class Cell
    {
        public bool IsMainCell { get; set; }
        public Vector2 Position { get; set; }
        public CellType Type { get; set; }
    }
}