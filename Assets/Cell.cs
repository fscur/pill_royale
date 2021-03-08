using UnityEngine;

namespace Pills.Assets
{
    public enum CellType
    {
        Empty = 0,
        Wall = 1,
        Virus = 2,
        Blown = 4,
        Red = 8,
        Blue = 16,
        Yellow = 32,
        Left = 64,
        Right = 128,
        Up = 256,
        Down = 512,
        RedVirus = Red | Virus,
        RedUp = Red | Up,
        RedDown = Red | Down,
        RedLeft = Red | Left,
        RedRight = Red | Right,
        RedBlown = Red | Blown,
        BlueVirus = Blue | Virus,
        BlueUp = Blue | Up,
        BlueDown = Blue | Down,
        BlueLeft = Blue | Left,
        BlueRight = Blue | Right,
        BlueBlown = Blue | Blown,
        YellowVirus = Yellow | Virus,
        YellowUp = Yellow | Up,
        YellowDown = Yellow | Down,
        YellowLeft = Yellow | Left,
        YellowRight = Yellow | Right,
        YellowBlown = Yellow | Blown,
    }
    public class Cell
    {
        public bool IsMainCell { get; set; }
        public Vector2 Position { get; set; }
        public CellType Type { get; set; }
    }
}