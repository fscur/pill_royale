using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pills.Assets
{
    public class Board
    {
        private static readonly float[] _spawnChances = new float[21];
        private readonly CellType[,] _board = new CellType[GameConstants.BoardWidth, GameConstants.BoardHeight];

        public CellType this[int x, int y]
        {
            get { return _board[x, y]; }
            set { _board[x, y] = value; }
        }

        static Board()
        {
            PrecomputeVirusSpawnChance();
        }

        private static void PrecomputeVirusSpawnChance()
        {
            for (var d = 0; d <= GameConstants.MaxDifficulty; d++)
            {
                _spawnChances[d] = (d - GameConstants.MinVirusSpawnChance * d + GameConstants.MinVirusSpawnChance * (GameConstants.MaxDifficulty - d)) / GameConstants.MaxDifficulty;    
            }
        }
        
        private static bool IsCellWall(int x, int y)
        {
            return x == 0 || x == GameConstants.BoardWidth - 1 || y == 0 || y == GameConstants.BoardHeight - 1;
        }
        
        public void Reset()
        {
            for (var x = 0; x < GameConstants.BoardWidth; ++x)
            {
                for (var y = 0; y < GameConstants.BoardHeight; ++y)
                {
                    if (IsCellWall(x, y))
                    {
                        _board[x, y] = CellType.Wall;
                        continue;
                    }

                    _board[x, y] = CellType.Empty;
                }
            }
        }
        
        public bool HasNoVirusLeft()
        {   
            for (var y = 1; y < GameConstants.BoardHeight - 1; ++y)
            {
                for (var x = 1; x < GameConstants.BoardWidth - 1; ++x)
                {
                    var cellType = _board[x, y];
                    if (Cell.IsVirus(cellType))
                        return false;
                }
            }

            return true;
        }
        
        public void FillVirusesPositions(int difficulty)
        {
            var maxHeight = GameConstants.MinVirusPlacementHeight + difficulty / 2;
            
            for (var y = 1; y < maxHeight; y++)
            {
                for (var x = 1; x < GameConstants.BoardWidth -1; x++)
                {
                    var shouldSpawn = UnityEngine.Random.Range(0.0f, 1.0f) < _spawnChances[difficulty];
                    if (!shouldSpawn)
                        continue;
                    
                    var color = UnityEngine.Random.Range(3, 6);
                    var cellType = (CellType) (1 << color) | CellType.Virus;
                    _board[x, y] = cellType;
                }
            }
        }
        
        public void MovePillDown(Pill pill)
        {
            var posCell0 = pill.Cells[0].Position;
            var posCell1 = pill.Cells[1].Position;
            _board[posCell0.x, posCell0.y] = CellType.Empty;
            _board[posCell1.x, posCell1.y] = CellType.Empty;

            pill.MoveDown();

            posCell0 = pill.Cells[0].Position;
            posCell1 = pill.Cells[1].Position;
            _board[posCell0.x, posCell0.y] = pill.Cells[0].Type;
            _board[posCell1.x, posCell1.y] = pill.Cells[1].Type;
        }

        public void MovePillLeft(Pill pill)
        {
            var posCell0 = pill.Cells[0].Position;
            var posCell1 = pill.Cells[1].Position;
            _board[posCell0.x, posCell0.y] = CellType.Empty;
            _board[posCell1.x, posCell1.y] = CellType.Empty;

            pill.MoveLeft();

            posCell0 = pill.Cells[0].Position;
            posCell1 = pill.Cells[1].Position;
            _board[posCell0.x, posCell0.y] = pill.Cells[0].Type;
            _board[posCell1.x, posCell1.y] = pill.Cells[1].Type;
        }

        public void MovePillRight(Pill pill)
        {
            var posCell0 = pill.Cells[0].Position;
            var posCell1 = pill.Cells[1].Position;
            _board[posCell0.x, posCell0.y] = CellType.Empty;
            _board[posCell1.x, posCell1.y] = CellType.Empty;

            pill.MoveRight();

            posCell0 = pill.Cells[0].Position;
            posCell1 = pill.Cells[1].Position;
            _board[posCell0.x, posCell0.y] = pill.Cells[0].Type;
            _board[posCell1.x, posCell1.y] = pill.Cells[1].Type;
        }

        public bool CanMovePill(Pill pill, MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.Down:
                    return CanMovePillDown(pill);
                case MovementDirection.Left:
                    return CanMovePillLeft(pill);
                case MovementDirection.Right:
                    return CanMovePillRight(pill);
            }

            return true;
        }

        public bool CanMovePillDown(Pill pill)
        {
            int x0 = 0;
            int y0 = 0;
            int x1 = 0;
            int y1 = 0;

            int mainCell = pill.Cells[0].IsMainCell ? 0 : 1;
            int otherCell = pill.Cells[1].IsMainCell ? 0 : 1;

            if (pill.Orientation == CellOrientation.Up)
            {
                x0 = pill.Cells[mainCell].Position.x;
                y0 = pill.Cells[mainCell].Position.y - 1;
            }
            else if (pill.Orientation == CellOrientation.Down)
            {
                x0 = pill.Cells[otherCell].Position.x;
                y0 = pill.Cells[otherCell].Position.y - 1;
            }
            else
            {
                x0 = pill.Cells[mainCell].Position.x;
                y0 = pill.Cells[mainCell].Position.y - 1;
                x1 = pill.Cells[otherCell].Position.x;
                y1 = pill.Cells[otherCell].Position.y - 1;

                if (y1 < 1)
                    return false;

                if (_board[x1, y1] != CellType.Empty)
                    return false;
            }

            if (y0 < 1)
                return false;

            return _board[x0, y0] == CellType.Empty;
        }

        public bool CanMovePillLeft(Pill pill)
        {
            int x0 = 0;
            int y0 = 0;
            int x1 = 0;
            int y1 = 0;

            int mainCell = pill.Cells[0].IsMainCell ? 0 : 1;
            int otherCell = pill.Cells[1].IsMainCell ? 0 : 1;

            if (pill.Orientation == CellOrientation.Left)
            {
                x0 = pill.Cells[otherCell].Position.x - 1;
                y0 = pill.Cells[otherCell].Position.y;
            }
            else if (pill.Orientation == CellOrientation.Right)
            {
                x0 = pill.Cells[mainCell].Position.x - 1;
                y0 = pill.Cells[mainCell].Position.y;
            }
            else
            {
                x0 = pill.Cells[mainCell].Position.x - 1;
                y0 = pill.Cells[mainCell].Position.y;
                x1 = pill.Cells[otherCell].Position.x - 1;
                y1 = pill.Cells[otherCell].Position.y;

                if (_board[x1, y1] != CellType.Empty)
                    return false;
            }

            return _board[x0, y0] == CellType.Empty;
        }

        public bool CanMovePillRight(Pill pill)
        {
            int x0 = 0;
            int y0 = 0;
            int x1 = 0;
            int y1 = 0;

            int mainCell = pill.Cells[0].IsMainCell ? 0 : 1;
            int otherCell = pill.Cells[1].IsMainCell ? 0 : 1;

            if (pill.Orientation == CellOrientation.Left)
            {
                x0 = pill.Cells[mainCell].Position.x + 1;
                y0 = pill.Cells[mainCell].Position.y;
            }
            else if (pill.Orientation == CellOrientation.Right)
            {
                x0 = pill.Cells[otherCell].Position.x + 1;
                y0 = pill.Cells[otherCell].Position.y;
            }
            else
            {
                x0 = pill.Cells[mainCell].Position.x + 1;
                y0 = pill.Cells[mainCell].Position.y;
                x1 = pill.Cells[otherCell].Position.x + 1;
                y1 = pill.Cells[otherCell].Position.y;

                if (_board[x1, y1] != CellType.Empty)
                    return false;
            }

            return _board[x0, y0] == CellType.Empty;
        }

        public bool CanRotatePill(Pill pill, RotationDirection direction)
        {
            if (direction == RotationDirection.CounterClockwise)
            {
                return CanRotatePillCounterClockwise(pill);
            }

            return CanRotatePillClockwise(pill);
        }

        private bool CanRotatePillCounterClockwise(Pill pill)
        {
            var x0 = 0;
            var y0 = 0;
            var mainCell = pill.Cells[0].IsMainCell ? 0 : 1;
            var otherCell = pill.Cells[1].IsMainCell ? 0 : 1;

            switch (pill.Orientation)
            {
                case CellOrientation.Right:
                    x0 = pill.Cells[otherCell].Position.x - 1;
                    y0 = pill.Cells[otherCell].Position.y + 1;
                    break;
                case CellOrientation.Up:
                    x0 = pill.Cells[mainCell].Position.x + 1;
                    y0 = pill.Cells[mainCell].Position.y;
                    break;
                case CellOrientation.Left:
                    x0 = pill.Cells[mainCell].Position.x - 1;
                    y0 = pill.Cells[mainCell].Position.y + 1;
                    break;
                case CellOrientation.Down:
                    x0 = pill.Cells[otherCell].Position.x + 1;
                    y0 = pill.Cells[otherCell].Position.y;
                    break;
            }

            return _board[x0, y0] == CellType.Empty;
        }
        
        private bool CanRotatePillClockwise(Pill pill)
        {
            var x0 = 0;
            var y0 = 0;

            var mainCell = pill.Cells[0].IsMainCell ? 0 : 1;
            var otherCell = pill.Cells[1].IsMainCell ? 0 : 1;

            switch (pill.Orientation)
            {
                case CellOrientation.Right:
                    x0 = pill.Cells[mainCell].Position.x;
                    y0 = pill.Cells[mainCell].Position.y + 1;
                    break;
                case CellOrientation.Down:
                    x0 = pill.Cells[mainCell].Position.x + 1;
                    y0 = pill.Cells[mainCell].Position.y - 1;
                    break;
                case CellOrientation.Left:
                    x0 = pill.Cells[otherCell].Position.x;
                    y0 = pill.Cells[otherCell].Position.y + 1;
                    break;
                case CellOrientation.Up:
                    x0 = pill.Cells[otherCell].Position.x + 1;
                    y0 = pill.Cells[otherCell].Position.y - 1;
                    break;
            }

            return _board[x0, y0] == CellType.Empty;
        }

        public void RotatePillClockwise(Pill pill)
        {
            Vector2Int posCell0 = pill.Cells[0].Position;
            Vector2Int posCell1 = pill.Cells[1].Position;
            _board[posCell0.x, posCell0.y] = CellType.Empty;
            _board[posCell1.x, posCell1.y] = CellType.Empty;

            pill.RotateClockwise();

            posCell0 = pill.Cells[0].Position;
            posCell1 = pill.Cells[1].Position;
            _board[posCell0.x, posCell0.y] = pill.Cells[0].Type;
            _board[posCell1.x, posCell1.y] = pill.Cells[1].Type;
        }

        public void RotatePillCounterClockwise(Pill pill)
        {
            Vector2Int posCell0 = pill.Cells[0].Position;
            Vector2Int posCell1 = pill.Cells[1].Position;
            _board[posCell0.x, posCell0.y] = CellType.Empty;
            _board[posCell1.x, posCell1.y] = CellType.Empty;

            pill.RotateCounterClockwise();

            posCell0 = pill.Cells[0].Position;
            posCell1 = pill.Cells[1].Position;
            _board[posCell0.x, posCell0.y] = pill.Cells[0].Type;
            _board[posCell1.x, posCell1.y] = pill.Cells[1].Type;
        }
    }
}