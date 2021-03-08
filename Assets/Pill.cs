using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pills.Assets
{
    public enum CellOrientation
    {
        Left,
        Right,
        Up,
        Down
    }

    public class Pill
    {
        public CellOrientation Orientation;

        public Cell[] Cells { get; set; }

        public static Pill SpawnPill(int x, int y, CellOrientation orientation)
        {
            var color = UnityEngine.Random.Range(3, 6);
            CellType cellType0 = (CellType)(1 << color);
            color = UnityEngine.Random.Range(3, 6);
            CellType cellType1 = (CellType)(1 << color);

            Vector2 cell1Position = Vector2.zero;
            switch (orientation)
            {
                case CellOrientation.Left:
                    cell1Position = new Vector2(x - 1, y);
                    break;
                case CellOrientation.Right:
                    cell1Position = new Vector2(x + 1, y);
                    break;
                case CellOrientation.Up:
                    cell1Position = new Vector2(x, y + 1);
                    break;
                case CellOrientation.Down:
                    cell1Position = new Vector2(x, y - 1);
                    break;
            }

            switch (orientation)
            {
                case CellOrientation.Down:
                    cellType0 |= CellType.Down;
                    cellType1 |= CellType.Up;
                    break;
                case CellOrientation.Up:
                    cellType0 |= CellType.Up;
                    cellType1 |= CellType.Down;
                    break;
                case CellOrientation.Right:
                    cellType0 |= CellType.Left;
                    cellType1 |= CellType.Right;
                    break;
                case CellOrientation.Left:
                    cellType0 |= CellType.Right;
                    cellType1 |= CellType.Left;
                    break;
            }

            var cell0 = new Cell
            {
                Position = new Vector2(x, y),
                Type = cellType0,
                IsMainCell = true
            };

            //todo: add orientation to celltype (to render pill correctly)

            var cell1 = new Cell
            {
                Position = cell1Position,
                Type = cellType1,
                IsMainCell = false
            };

            return new Pill
            {
                Cells = new[]
                {
                    cell0,
                    cell1
                },
                Orientation = orientation
            };
        }

        public void Translate(int x, int y)
        {
            Cells[0].Position += new Vector2(x, y);
            Cells[1].Position += new Vector2(x, y);
        }

        public void MoveDown()
        {
            Cells[0].Position += Vector2.down;
            Cells[1].Position += Vector2.down;
        }

        public void MoveUp()
        {
            Cells[0].Position += Vector2.up;
            Cells[1].Position += Vector2.up;
        }

        public void MoveLeft()
        {
            Cells[0].Position += Vector2.left;
            Cells[1].Position += Vector2.left;
        }

        public void MoveRight()
        {
            Cells[0].Position += Vector2.right;
            Cells[1].Position += Vector2.right;
        }
        public void RotateCounterClockwise()
        {
            var mainCell = Cells[0].IsMainCell ? 0 : 1;
            var otherCell = Cells[0].IsMainCell ? 1 : 0;
            var x0 = Cells[mainCell].Position.x;
            var y0 = Cells[mainCell].Position.y;
            var x1 = Cells[otherCell].Position.x;
            var y1 = Cells[otherCell].Position.y;
            var otherType = Cells[otherCell].Type & (CellType.Red | CellType.Yellow | CellType.Blue);
            var mainType = Cells[mainCell].Type & (CellType.Red | CellType.Yellow | CellType.Blue);

            switch (Orientation)
            {
                case CellOrientation.Right:
                    y1 += 1;
                    x1 -= 1;
                    Orientation = CellOrientation.Up;
                    mainType |= CellType.Up;
                    otherType |= CellType.Down;
                    break;
                case CellOrientation.Up:
                    x0 += 1;
                    y1 -= 1;
                    Orientation = CellOrientation.Left;
                    mainType |= CellType.Right;
                    otherType |= CellType.Left;
                    break;
                case CellOrientation.Left:
                    x0 -= 1;
                    y0 += 1;
                    Orientation = CellOrientation.Down;
                    mainType |= CellType.Down;
                    otherType |= CellType.Up;
                    break;
                case CellOrientation.Down:
                    x1 += 1;
                    y0 -= 1;
                    Orientation = CellOrientation.Right;
                    mainType |= CellType.Left;
                    otherType |= CellType.Right;
                    break;
            }

            Cells[mainCell].Type = mainType;
            Cells[otherCell].Type = otherType;
            Cells[mainCell].Position = new Vector2(x0, y0);
            Cells[otherCell].Position = new Vector2(x1, y1);
        }

        public void RotateClockwise()
        {
            var mainCell = Cells[0].IsMainCell ? 0 : 1;
            var otherCell = Cells[0].IsMainCell ? 1 : 0;
            var x0 = Cells[mainCell].Position.x;
            var y0 = Cells[mainCell].Position.y;
            var x1 = Cells[otherCell].Position.x;
            var y1 = Cells[otherCell].Position.y;
            var otherType = Cells[otherCell].Type & (CellType.Red | CellType.Yellow | CellType.Blue);
            var mainType = Cells[mainCell].Type & (CellType.Red | CellType.Yellow | CellType.Blue);

            switch (Orientation)
            {
                case CellOrientation.Right:
                    y0 += 1;
                    x1 -= 1;
                    Orientation = CellOrientation.Down;
                    mainType |= CellType.Down;
                    otherType |= CellType.Up;
                    break;
                case CellOrientation.Down:
                    x0 += 1;
                    y0 -= 1;
                    Orientation = CellOrientation.Left;
                    mainType |= CellType.Right;
                    otherType |= CellType.Left;
                    break;
                case CellOrientation.Left:
                    x0 -= 1;
                    y1 += 1;
                    Orientation = CellOrientation.Up;
                    mainType |= CellType.Up;
                    otherType |= CellType.Down;
                    break;
                case CellOrientation.Up:
                    x1 += 1;
                    y1 -= 1;
                    Orientation = CellOrientation.Right;
                    mainType |= CellType.Left;
                    otherType |= CellType.Right;
                    break;
            }

            Cells[mainCell].Type = mainType;
            Cells[otherCell].Type = otherType;
            Cells[mainCell].Position = new Vector2(x0, y0);
            Cells[otherCell].Position = new Vector2(x1, y1);
        }

        public void SwapOrientation()
        {
            Cells[0].IsMainCell = !Cells[0].IsMainCell;
            Cells[1].IsMainCell = !Cells[1].IsMainCell;

            switch (Orientation)
            {
                case CellOrientation.Right:
                    Orientation = CellOrientation.Left;
                    break;
                case CellOrientation.Down:
                    Orientation = CellOrientation.Up;
                    break;
                case CellOrientation.Left:
                    Orientation = CellOrientation.Right;
                    break;
                case CellOrientation.Up:
                    Orientation = CellOrientation.Down;
                    break;
            }
        }
    }
}