using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pills.Assets
{
    public enum GameState
    {
        WaitingStart,
        Playing,
        ResolvingBlows,
        FallingCells,
        GameOver
    }

    public class GameController : MonoBehaviour
    {
        private enum InputAction
        {
            MoveDown,
            MoveLeft,
            MoveRight
        }
        
        private static int _randomSeed;

        [SerializeField, Range(0, GameConstants.MaxDifficulty)] private float _difficulty = 0;
        [SerializeField, Range(0, 2)] private int _roundStartSpeed = 0;
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private BoardConfig _boardConfig;
        
        [SerializeField] private KeyCode _downKey;
        [SerializeField] private KeyCode _leftKey;
        [SerializeField] private KeyCode _rightKey;
        [SerializeField] private KeyCode _rotateClockwiseKey;
        [SerializeField] private KeyCode _rotateCounterClockwiseKey;
        
        private Board _board;
        private float _roundSpeed;
        private bool _isPaused;
        private float _inputTime;
        private float _inputSpeed;
        private float _keyDownSpeed;
        private float _roundTime;
        private Pill _currentPill;
        private Pill _nextPill;
        private bool[] _keyIsDown;
        private float[] _timeSinceKeyDown;

        private GameState _state;

        private readonly Queue<List<Cell>> _toBlowQueue = new Queue<List<Cell>>();
        private readonly Queue<List<Cell>> _blownQueue = new Queue<List<Cell>>();
        private readonly Dictionary<Vector2Int, List<Cell>> _cellGroups = new Dictionary<Vector2Int, List<Cell>>(10 * 18);
        private static readonly float[] _roundSpeeds = new float[3];
        private Random _pillRandom;
        private BoardDrawer _boardDrawer;

        static GameController()
        {
            _roundSpeeds[0] = 1.0f;
            _roundSpeeds[1] = 0.75f;
            _roundSpeeds[2] = 0.5f;
        }
        
        private void Start()
        {
            if (_randomSeed == 0)
                _randomSeed = UnityEngine.Random.Range(1, 1024);
            
            UnityEngine.Random.InitState(_randomSeed);
            
            _boardDrawer = new BoardDrawer(_tilemap, _boardConfig.PillTiles);
            _pillRandom = new Random();
            _keyIsDown = new bool[5];
            _timeSinceKeyDown = new float[5];

            _isPaused = true;
            _roundSpeed = _roundSpeeds[_roundStartSpeed];
            
            _roundTime = 0.0f;
            _inputTime = 0.0f;
            _inputSpeed = 1.0f / 80.0f;
            _keyDownSpeed = 0.25f;

            _board = new Board();
            _board.Reset();
            _board.FillVirusesPositions((int)_difficulty);
            
            _nextPill = Pill.SpawnPill(4, GameConstants.BoardHeight + 1, CellOrientation.Right, _pillRandom);
            
            SetState(GameState.WaitingStart);
        }

        private void SetState(GameState value)
        {
            _state = value;
            
            switch (value)
            {
                case GameState.Playing:
                    _roundSpeed = _roundSpeeds[_roundStartSpeed];
                    break;
                case GameState.FallingCells:
                    _roundSpeed /= 2.0f;
                    break;
            }
        }
        
        private void SetCurrentPill(Pill pill)
        {
            _currentPill = pill;
            _currentPill.Translate(0, -3);
            var cell0 = _currentPill.Cells[0];
            var cell1 = _currentPill.Cells[1];
            _board[(int) cell0.Position.x, (int) cell0.Position.y] = cell0.Type;
            _board[(int) cell1.Position.x, (int) cell1.Position.y] = cell1.Type;
            _nextPill = Pill.SpawnPill(4, GameConstants.BoardHeight + 1, CellOrientation.Right, _pillRandom);
        } 

        public void LateUpdate()
        {
            if (_state == GameState.GameOver)
            {
                return;
            }
            
            ResetInputIfNeeded();

            if (Input.GetKeyDown(KeyCode.P))
            {
                if (_state == GameState.WaitingStart)
                {
                    SetCurrentPill(_nextPill);
                    SetState(GameState.Playing);
                }

                _isPaused = !_isPaused;
            }

            if (_isPaused)
            {
                return;
            }

            _inputTime += Time.unscaledDeltaTime;
            _roundTime += Time.unscaledDeltaTime;

            if (_state == GameState.Playing && _currentPill != null)
            {
                HandleInput();
            }

            if (_inputTime > _inputSpeed)
            {
                _inputTime = 0.0f;
            }

            if (_roundTime <= _roundSpeed)
            {
                return;
            }

            // UnityEngine.Debug.Log("input time: " + _inputTime);
            // UnityEngine.Debug.Log("input speed: " + _inputSpeed);
            // UnityEngine.Debug.Log("round time: " + _roundTime);
            // UnityEngine.Debug.Log("round speed: " + _roundSpeed);
            // UnityEngine.Debug.Log("keydown down: " + _keyIsDown[(int)InputAction.MoveDown]);
            // UnityEngine.Debug.Log("keydown left: " + _keyIsDown[(int)InputAction.MoveLeft]);
            // UnityEngine.Debug.Log("keydown right: " + _keyIsDown[(int)InputAction.MoveRight]);
            // UnityEngine.Debug.Log("time keydown down: " + _timeSinceKeyDown[(int)InputAction.MoveDown]);
            // UnityEngine.Debug.Log("time keydown left: " + _timeSinceKeyDown[(int)InputAction.MoveLeft]);
            // UnityEngine.Debug.Log("time keydown right: " + _timeSinceKeyDown[(int)InputAction.MoveRight]);

            _roundTime = 0.0f;

            if (_state == GameState.WaitingStart)
                return;

            if (_state == GameState.Playing)
                Play();

            if (_state == GameState.ResolvingBlows)
                ResolveBlows();
            
            if (_state == GameState.FallingCells)
                FallCells();
        }

        private void Update()
        {
            _boardDrawer.Draw(_board, _nextPill);
        }

        private void ResetInputIfNeeded()
        {
            if (Input.GetKeyUp(_downKey))
            {
                _keyIsDown[(int) InputAction.MoveDown] = false;
                _timeSinceKeyDown[(int) InputAction.MoveDown] = 0;
            }

            if (Input.GetKeyUp(_leftKey))
            {
                _keyIsDown[(int) InputAction.MoveLeft] = false;
                _timeSinceKeyDown[(int) InputAction.MoveLeft] = 0;
            }

            if (Input.GetKeyUp(_rightKey))
            {
                _keyIsDown[(int) InputAction.MoveRight] = false;
                _timeSinceKeyDown[(int) InputAction.MoveRight] = 0;
            }
        }

        private void HandleInput()
        {
            HandleMoveDown();
            HandleMoveLeft();
            HandleMoveRight();
            HandleRotateClockwise();
            HandleRotateCounterClockwise();
        }

        private void HandleMoveDown()
        {
            if (Input.GetKeyDown(_downKey))
            {
                _keyIsDown[(int) InputAction.MoveDown] = true;

                if (_board.CanMovePill(_currentPill, MovementDirection.Down))
                    _board.MovePillDown(_currentPill);
            }

            if (_keyIsDown[(int) InputAction.MoveDown])
                _timeSinceKeyDown[(int) InputAction.MoveDown] += Time.unscaledDeltaTime;

            if (_inputTime <= _inputSpeed)
                return;
            
            if (!_keyIsDown[(int) InputAction.MoveDown] || !(_timeSinceKeyDown[(int) InputAction.MoveDown] > _keyDownSpeed))
                return;

            if (_board.CanMovePill(_currentPill, MovementDirection.Down))
                _board.MovePillDown(_currentPill);
        }

        private void HandleMoveLeft()
        {
            if (Input.GetKeyDown(_leftKey))
            {
                _keyIsDown[(int) InputAction.MoveLeft] = true;

                if (_board.CanMovePill(_currentPill, MovementDirection.Left))
                    _board.MovePillLeft(_currentPill);
            }

            if (_keyIsDown[(int) InputAction.MoveLeft])
                _timeSinceKeyDown[(int) InputAction.MoveLeft] += Time.unscaledDeltaTime;

            if (!(_inputTime > _inputSpeed))
                return;

            if (!_keyIsDown[(int) InputAction.MoveLeft] || !(_timeSinceKeyDown[(int) InputAction.MoveLeft] > _keyDownSpeed))
                return;

            if (_board.CanMovePill(_currentPill, MovementDirection.Left))
                _board.MovePillLeft(_currentPill);
        }

        private void HandleMoveRight()
        {
            if (Input.GetKeyDown(_rightKey))
            {
                _keyIsDown[(int) InputAction.MoveRight] = true;

                if (_board.CanMovePill(_currentPill, MovementDirection.Right))
                    _board.MovePillRight(_currentPill);
            }

            if (_keyIsDown[(int) InputAction.MoveRight])
                _timeSinceKeyDown[(int) InputAction.MoveRight] += Time.unscaledDeltaTime;

            if (!(_inputTime > _inputSpeed))
                return;

            if (!_keyIsDown[(int) InputAction.MoveRight] || !(_timeSinceKeyDown[(int) InputAction.MoveRight] > _keyDownSpeed))
                return;

            if (_board.CanMovePill(_currentPill, MovementDirection.Right))
                _board.MovePillRight(_currentPill);
        }

        private void HandleRotateClockwise()
        {
            if (!Input.GetKeyDown(_rotateClockwiseKey))
                return;

            if (_board.CanRotatePill(_currentPill, RotationDirection.Clockwise))
            {
                _board.RotatePillClockwise(_currentPill);
            }
            else
            {
                switch (_currentPill.Orientation)
                {
                    case CellOrientation.Up:
                    case CellOrientation.Down:
                    {
                        if (!_board.CanMovePillLeft(_currentPill))
                            return;

                        _board.MovePillLeft(_currentPill);
                        _board.RotatePillClockwise(_currentPill);
                        break;
                    }
                    case CellOrientation.Right:
                    case CellOrientation.Left:
                    {
                        if (!_board.CanMovePillDown(_currentPill))
                            return;

                        _board.MovePillDown(_currentPill);
                        _board.RotatePillClockwise(_currentPill);
                        break;
                    }
                }
            }
        }

        private void HandleRotateCounterClockwise()
        {
            if (!Input.GetKeyDown(_rotateCounterClockwiseKey))
                return;

            if (_board.CanRotatePill(_currentPill, RotationDirection.CounterClockwise))
            {
                _board.RotatePillCounterClockwise(_currentPill);
            }
            else
            {
                switch (_currentPill.Orientation)
                {
                    case CellOrientation.Up:
                    case CellOrientation.Down:
                    {
                        if (!_board.CanMovePillRight(_currentPill))
                            return;

                        _board.MovePillRight(_currentPill);
                        _board.RotatePillCounterClockwise(_currentPill);
                        break;
                    }
                    case CellOrientation.Right:
                    case CellOrientation.Left:
                    {
                        if (!_board.CanMovePillDown(_currentPill))
                            return;

                        _board.MovePillDown(_currentPill);
                        _board.RotatePillCounterClockwise(_currentPill);
                        break;
                    }
                }
            }
        }

        private void Play()
        {
            if (_board.HasNoVirusLeft())
            {
                SetState(GameState.GameOver);
                return;
            }
            
            if (!_board.CanMovePill(_currentPill, MovementDirection.Down))
            {
                var x = (int) _currentPill.Cells[0].Position.x;
                var y = (int) _currentPill.Cells[0].Position.y;

                if (ShouldResolveBlows(x, y))
                {
                   SetState(GameState.ResolvingBlows);
                    return;
                }

                if (_state == GameState.Playing)
                {
                    if (!CanSpawnNextPill())
                    {
                        SetState(GameState.GameOver);
                        return;
                    }
                    
                    SetCurrentPill(_nextPill);
                    return;
                }
            }

            if (!_keyIsDown[0])
                _board.MovePillDown(_currentPill);
        }

        private bool CanSpawnNextPill()
        {
            var x0 = 4;
            var y0 = GameConstants.BoardHeight - 2;
            var x1 = x0 + 1;
            var y1 = y0;
            
            return _board[x0, y0] == CellType.Empty && _board[x1, y1] == CellType.Empty;
        }

        private bool ShouldResolveBlows(int x, int y)
        {
            var cellType = _currentPill.Cells[0].Type;

            var horizontalCells = GetHorizontalBlownCells(x, y, cellType);
            if (horizontalCells.Count > 0)
            {
                return true;
            }

            var verticalCells = GetVerticalBlownCells(x, y, cellType);
            if (verticalCells.Count > 0)
            {
                return true;
            }

            x = (int) _currentPill.Cells[1].Position.x;
            y = (int) _currentPill.Cells[1].Position.y;
            cellType = _currentPill.Cells[1].Type;

            horizontalCells = GetHorizontalBlownCells(x, y, cellType);
            if (horizontalCells.Count > 0)
            {
                return true;
            }

            verticalCells = GetVerticalBlownCells(x, y, cellType);
            return verticalCells.Count > 0;
        }

        private void Split(int x, int y, CellType blownCellType)
        {
            var blownCellOrientation = Cell.GetCellOrientation(blownCellType);
            var splitCellOrientation = Cell.GetCellOrientation(_board[x, y]);

            if (blownCellOrientation == CellType.Left && splitCellOrientation != CellType.Right)
                return;

            if (blownCellOrientation == CellType.Right && splitCellOrientation != CellType.Left)
                return;

            if (blownCellOrientation == CellType.Down && splitCellOrientation != CellType.Up)
                return;

            if (blownCellOrientation == CellType.Up && splitCellOrientation != CellType.Down)
                return;

            switch (_board[x, y])
            {
                case CellType.Empty:
                case CellType.Wall:
                case CellType.Virus:
                case CellType.Red:
                case CellType.Blue:
                case CellType.Yellow:
                    return;
            }

            if (_board[x, y] == CellType.Empty || _board[x, y] == CellType.Wall)
                return;

            var isVirus = (_board[x, y] & CellType.Virus) == CellType.Virus;
            if (isVirus)
                return;

            _board[x, y] = Cell.GetCellColor(_board[x, y]);
        }

        private List<Cell> GetHorizontalBlownCells(int x, int y, CellType cellType)
        {
            var tmp = x;

            var blownPositions = new List<Cell>();

            blownPositions.Add(Cell.CreateCell(x, y, cellType));

            while (x >= 1 && Cell.CellsHaveSameColor(_board[x - 1, y], cellType))
            {
                blownPositions.Add(Cell.CreateCell(x - 1, y, cellType));
                x -= 1;
            }

            x = tmp;
            while (x <= GameConstants.BoardWidth && Cell.CellsHaveSameColor(_board[x + 1, y], cellType))
            {
                blownPositions.Add(Cell.CreateCell(x + 1, y, cellType));
                x += 1;
            }

            if (blownPositions.Count < GameConstants.MinConsecutiveCells)
                blownPositions.Clear();

            return blownPositions;
        }

        private List<Cell> GetVerticalBlownCells(int x, int y, CellType cellType)
        {
            var tmp = y;
            List<Cell> blownPositions = new List<Cell>();

            blownPositions.Add(Cell.CreateCell(x, y, cellType));

            while (y >= 1 && Cell.CellsHaveSameColor(_board[x, y - 1], cellType))
            {
                blownPositions.Add(Cell.CreateCell(x, y - 1, cellType));
                y -= 1;
            }

            y = tmp;
            while (y <= GameConstants.BoardHeight - 1 && Cell.CellsHaveSameColor(_board[x, y + 1], cellType))
            {
                blownPositions.Add(Cell.CreateCell(x, y + 1, cellType));
                y += 1;
            }

            if (blownPositions.Count < GameConstants.MinConsecutiveCells)
                blownPositions.Clear();

            return blownPositions;
        }

        private void ResolveBlows()
        {
            CheckBlows(Direction.Vertical);
            CheckBlows(Direction.Horizontal);

            while (_blownQueue.Count > 0)
            {
                var blownGroup = _blownQueue.Dequeue();

                foreach (var blownCell in blownGroup)
                {
                    var x = (int) blownCell.Position.x;
                    var y = (int) blownCell.Position.y;
                    _board[x, y] = CellType.Empty;
                }
            }

            while (_toBlowQueue.Count > 0)
            {
                var toBlowGroup = _toBlowQueue.Dequeue();
                
                foreach (var blownCell in toBlowGroup)
                {
                    var x = (int) blownCell.Position.x;
                    var y = (int) blownCell.Position.y;
                    var blownCellType = _board[x, y];
                    //todo: make animated
                    
                    //_board[x, y] = CellType.Empty;
                    _board[x, y] = Cell.GetCellColor(blownCellType) | CellType.Blown;

                    var blownCellOrientation = blownCellType & (CellType.Left | CellType.Right | CellType.Down | CellType.Up);
                    switch (blownCellOrientation)
                    {
                        case CellType.Left:
                            Split(x + 1, y, blownCellType);
                            break;
                        case CellType.Right:
                            Split(x - 1, y, blownCellType);
                            break;
                        case CellType.Up:
                            Split(x, y + 1, blownCellType);
                            break;
                        case CellType.Down:
                            Split(x, y - 1, blownCellType);
                            break;
                    }
                }
                
                _blownQueue.Enqueue(toBlowGroup);
            }

            if (_blownQueue.Count == 0)
            {
                SetState(GameState.FallingCells);
                _cellGroups.Clear();
            }
        }

        private void FallCells()
        {
            var fallenCells = 0;

            for (int y = 2; y < GameConstants.BoardHeight - 1; ++y)
            {
                for (int x = 1; x < GameConstants.BoardWidth - 1; ++x)
                {
                    var cellType = _board[x, y];

                    if (cellType == CellType.Empty || cellType == CellType.Wall)
                        continue;

                    var isVirus = (cellType & CellType.Virus) == CellType.Virus;
                    if (isVirus)
                        continue;

                    if (_board[x, y - 1] != CellType.Empty)
                        continue;

                    var canFall = true;
                    var isSingleCell = (cellType & (CellType.Left | CellType.Right | CellType.Up | CellType.Down)) == CellType.Empty;

                    if (!isSingleCell)
                    {
                        var cellOrientation = cellType & (CellType.Left | CellType.Right | CellType.Down | CellType.Up);

                        switch (cellOrientation)
                        {
                            case CellType.Left:
                                canFall = _board[x + 1, y - 1] == CellType.Empty;
                                if (canFall)
                                {
                                    _board[x + 1, y - 1] = _board[x + 1, y];
                                    _board[x + 1, y] = CellType.Empty;
                                }

                                break;
                            case CellType.Right:
                                canFall = _board[x - 1, y - 1] == CellType.Empty;
                                if (canFall)
                                {
                                    _board[x - 1, y - 1] = _board[x - 1, y];
                                    _board[x - 1, y] = CellType.Empty;
                                }

                                break;
                        }
                    }

                    if (!canFall)
                        continue;

                    fallenCells++;
                    _board[x, y - 1] = _board[x, y];
                    _board[x, y] = CellType.Empty;
                }
            }

            if (fallenCells != 0)
            {
                CheckBlows(Direction.Vertical);
                CheckBlows(Direction.Horizontal);

                if (_toBlowQueue.Count <= 0) 
                    return;
                
                SetState(GameState.ResolvingBlows);
                _cellGroups.Clear();

                return;
            }

            _cellGroups.Clear();
            SetState(GameState.Playing);
            SetCurrentPill(_nextPill);
        }

        private void CheckBlows(Direction direction)
        {
            var i = direction == Direction.Vertical ? GameConstants.BoardHeight + 1 : GameConstants.BoardWidth + 1;
            var x = 0;
            var y = 0;

            start:
            while (i < GameConstants.BoardWidth * GameConstants.BoardHeight)
            {
                if (direction == Direction.Horizontal)
                {
                    x = i % GameConstants.BoardWidth;
                    y = i / GameConstants.BoardWidth;
                }
                else
                {
                    x = i / GameConstants.BoardHeight;
                    y = i % GameConstants.BoardHeight;
                }

                if (_board[x, y] != CellType.Empty &&
                    _board[x, y] != CellType.Wall)
                    break;
                i++;
            }

            var currentColor = Cell.GetCellColor(_board[x, y]);

            var currentGroup = new List<Cell>();

            AddCellToGroup(x, y, currentGroup);

            while (++i < GameConstants.BoardWidth * GameConstants.BoardHeight)
            {
                if (direction == Direction.Horizontal)
                {
                    x = i % GameConstants.BoardWidth;
                    y = i / GameConstants.BoardWidth;
                }
                else
                {
                    x = i / GameConstants.BoardHeight;
                    y = i % GameConstants.BoardHeight;
                }

                if (_board[x, y] == CellType.Empty || _board[x, y] == CellType.Wall ||
                    !Cell.CellsHaveSameColor(currentColor, Cell.GetCellColor(_board[x, y])))
                {
                    if (currentGroup.Count >= GameConstants.MinConsecutiveCells)
                    {
                        List<Cell> foundGroup = null;

                        foreach (var cell in currentGroup)
                        {
                            if (!_cellGroups.TryGetValue(cell.Position, out foundGroup))
                            {
                                _cellGroups.Add(cell.Position, currentGroup);
                                continue;
                            }

                            break;
                        }

                        if (foundGroup == null)
                            _toBlowQueue.Enqueue(currentGroup);
                        else
                        {
                            foreach (var cell in currentGroup)
                            {
                                if (foundGroup.Any(c => c.Position == cell.Position))
                                {
                                    foundGroup.Add(cell);                                    
                                }
                            }
                        }
                    }

                    if (_board[x, y] == CellType.Empty || _board[x, y] == CellType.Wall)
                    {
                        goto start;
                    }

                    currentGroup = new List<Cell>();
                    currentColor = Cell.GetCellColor(_board[x, y]);
                }

                AddCellToGroup(x, y, currentGroup);
            }
        }

        private void AddCellToGroup(int x, int y, List<Cell> currentGroup)
        {
            var cell = new Cell()
            {
                Position = new Vector2Int(x, y),
                Type = _board[x, y]
            };

            currentGroup.Add(cell);
        }
        
    }
}