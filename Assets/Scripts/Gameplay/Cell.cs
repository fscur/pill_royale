using UnityEngine;

namespace Pills.Assets.Gameplay
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
    public struct Cell
    {
        public bool IsMainCell { get; set; }
        public Vector2Int Position { get; set; }
        public CellType Type { get; set; }
        
        public static Cell CreateCell(int x, int y, CellType type)
        {
            return new Cell
            {
                Type = type,
                Position = new Vector2Int(x, y)
            };
        }

        public static CellType GetCellColor(CellType cellType)
        {
            return cellType & (CellType.Red | CellType.Blue | CellType.Yellow);
        }

        public static bool HaveSameColor(CellType cellType0, CellType cellType1)
        {
            var c0 = cellType0 & (CellType.Red | CellType.Blue | CellType.Yellow);
            var c1 = cellType1 & (CellType.Red | CellType.Blue | CellType.Yellow);
            return c0 == c1;
        }

        public static CellType GetCellOrientation(CellType cellType)
        {
            return cellType & (CellType.Left | CellType.Right | CellType.Down | CellType.Up);
        }
        
        public static bool IsVirus(CellType cellType)
        {
            return (cellType & CellType.Virus) == CellType.Virus;
        }

        public static bool IsSingleCell(CellType cellType)
        {
            return (cellType & (CellType.Left | CellType.Right | CellType.Up | CellType.Down)) == CellType.Empty;
        } 
    }
}