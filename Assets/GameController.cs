using System.Collections.Generic;
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
        enum InputAction
        {
            MoveDown,
            MoveLeft,
            MoveRight
        }

        [SerializeField, Range(1, 4)] private int _difficulty = 1;
        [SerializeField, Range(1, 3)] private int _roundStartSpeed = 1;
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Tile[] _pillTiles;

        private CellType[,] _board;
        private int _boardWidth = 10;
        private int _boardHeight = 18;
        private int _minConsecutiveCells = 4;
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
        private readonly Dictionary<Vector2, List<Cell>> _cellGroups = new Dictionary<Vector2, List<Cell>>(10 * 18);
        
        private void Start()
        {
            _keyIsDown = new bool[5];
            _timeSinceKeyDown = new float[5];

            _isPaused = true;
            _roundSpeed = 1.0f / (Mathf.Pow(2.0f, (float) _roundStartSpeed - 1.0f));
            _roundTime = 0.0f;
            _inputTime = 0.0f;
            _inputSpeed = 1.0f / 50.0f;
            _keyDownSpeed = 0.25f;

            ResetBoard();
            //InitializeVirusesPositions();
            _nextPill = Pill.SpawnPill(4, _boardHeight + 1, CellOrientation.Right);
            _state = GameState.WaitingStart;
        }

        private void SetCurrentPill(Pill pill)
        {
            _currentPill = pill;
            _currentPill.Translate(0, -3);
            var cell0 = _currentPill.Cells[0];
            var cell1 = _currentPill.Cells[1];
            _board[(int) cell0.Position.x, (int) cell0.Position.y] = cell0.Type;
            _board[(int) cell1.Position.x, (int) cell1.Position.y] = cell1.Type;
            _nextPill = Pill.SpawnPill(4, _boardHeight + 1, CellOrientation.Right);
        }

        private void ResetBoard()
        {
            _board = new CellType[_boardWidth, _boardHeight];

            int x, y;
            for (x = 0; x < _boardWidth; ++x)
            {
                for (y = 0; y < _boardHeight; ++y)
                {
                    if (IsWall(x, y))
                    {
                        _board[x, y] = CellType.Wall;
                        continue;
                    }

                    _board[x, y] = CellType.Empty;
                }
            }

            _board[2, 1] = CellType.Red;
            _board[2, 2] = CellType.Red;
            _board[2, 3] = CellType.Red;
            _board[2, 4] = CellType.Blue;
            _board[2, 5] = CellType.Blue;
            _board[2, 6] = CellType.Blue;
            //_board[3, 5] = CellType.Blue;
            //_board[3, 6] = CellType.Blue;
            //_board[3, 7] = CellType.Red;
            //_board[2, 7] = CellType.Red;
        }

        private bool IsWall(int x, int y)
        {
            return x == 0 || x == _boardWidth - 1 || y == 0 || y == _boardHeight - 1;
        }

        private void InitializeVirusesPositions()
        {
            int x, y;
            int maxViruses = 1 << _difficulty + 2;

            while (maxViruses-- > 0)
            {
                try_set_color:
                x = UnityEngine.Random.Range(0, _boardWidth);
                y = UnityEngine.Random.Range(0, _boardHeight - _minConsecutiveCells);

                if (_board[x, y] != 0)
                    goto try_set_color;

                var color = UnityEngine.Random.Range(2, 5);
                CellType cellType = (CellType) (1 << color) | CellType.Virus;

                if (!CanPlaceVirus(x, y, cellType))
                    goto try_set_color;

                _board[x, y] = cellType;
            }
        }

        private bool CanPlaceVirus(int x, int y, CellType cellType)
        {
            int blow = _minConsecutiveCells - 1;
            int i0 = x < blow ? 0 : x - blow;
            int i1 = x >= _boardWidth - blow ? _boardWidth - 1 : x + blow;
            int count = 0;

            for (int i = x - 1; i >= i0; --i)
            {
                if (_board[i, y] == cellType)
                    count++;
            }

            if (count >= blow)
                return false;

            for (int i = x + 1; i <= i1; ++i)
            {
                if (_board[i, y] == cellType)
                    count++;
            }

            if (count >= blow)
                return false;

            i0 = y < blow ? 0 : y - blow;
            i1 = y >= _boardHeight - blow ? _boardHeight - 1 : y + blow;
            count = 0;

            for (int i = y - 1; i >= i0; --i)
            {
                if (_board[x, i] == cellType)
                    count++;
            }

            if (count >= blow)
                return false;

            for (int i = y + 1; i <= i1; ++i)
            {
                if (_board[x, i] == cellType)
                    count++;
            }

            if (count >= blow)
                return false;

            return true;
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
                    _state = GameState.Playing;
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
            DrawBoard();
        }

        private void ResetInputIfNeeded()
        {
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                _keyIsDown[(int) InputAction.MoveDown] = false;
                _timeSinceKeyDown[(int) InputAction.MoveDown] = 0;
            }

            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                _keyIsDown[(int) InputAction.MoveLeft] = false;
                _timeSinceKeyDown[(int) InputAction.MoveLeft] = 0;
            }

            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                _keyIsDown[(int) InputAction.MoveRight] = false;
                _timeSinceKeyDown[(int) InputAction.MoveRight] = 0;
            }
        }

        private void DrawBoard()
        {
            if (_nextPill != null)
                DrawPill(_nextPill);

            for (int x = 1; x < _boardWidth - 1; ++x)
            {
                for (int y = 1; y < _boardHeight; ++y)
                {
                    if (_board[x, y] == CellType.Wall)
                        continue;

                    Tile tile = GetTile(_board[x, y]);

                    _tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        private void DrawPill(Pill pill)
        {
            var x0 = (int) pill.Cells[0].Position.x;
            var y0 = (int) pill.Cells[0].Position.y;
            var x1 = (int) pill.Cells[1].Position.x;
            var y1 = (int) pill.Cells[1].Position.y;

            CellType cellType0 = pill.Cells[0].Type;
            CellType cellType1 = pill.Cells[1].Type;

            _tilemap.SetTile(new Vector3Int(x0, y0, 0), GetTile(cellType0));
            _tilemap.SetTile(new Vector3Int(x1, y1, 0), GetTile(cellType1));
        }

        private Tile GetTile(CellType cellType)
        {
            Tile tile;

            var tileRotation = Quaternion.identity;

            var orientation = cellType & (CellType.Down | CellType.Up | CellType.Left | CellType.Right);

            switch (orientation)
            {
                case CellType.Right:
                    tileRotation.eulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                    break;
                case CellType.Left:
                    tileRotation.eulerAngles = new Vector3(0.0f, 0.0f, 270.0f);
                    break;
                case CellType.Down:
                    tileRotation.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                    break;
                case CellType.Up:
                    tileRotation.eulerAngles = new Vector3(0.0f, 0.0f, 180.0f);
                    break;
            }

            switch (cellType)
            {
                case CellType.RedDown:
                    tile = _pillTiles[0];
                    break;
                case CellType.YellowDown:
                    tile = _pillTiles[1];
                    break;
                case CellType.BlueDown:
                    tile = _pillTiles[2];
                    break;
                case CellType.RedUp:
                    tile = _pillTiles[3];
                    break;
                case CellType.YellowUp:
                    tile = _pillTiles[4];
                    break;
                case CellType.BlueUp:
                    tile = _pillTiles[5];
                    break;
                case CellType.RedLeft:
                    tile = _pillTiles[6];
                    break;
                case CellType.YellowLeft:
                    tile = _pillTiles[7];
                    break;
                case CellType.BlueLeft:
                    tile = _pillTiles[8];
                    break;
                case CellType.RedRight:
                    tile = _pillTiles[9];
                    break;
                case CellType.YellowRight:
                    tile = _pillTiles[10];
                    break;
                case CellType.BlueRight:
                    tile = _pillTiles[11];
                    break;
                case CellType.Red:
                    tile = _pillTiles[12];
                    break;
                case CellType.Yellow:
                    tile = _pillTiles[13];
                    break;
                case CellType.Blue:
                    tile = _pillTiles[14];
                    break;
                case CellType.RedVirus:
                    tile = _pillTiles[15];
                    break;
                case CellType.YellowVirus:
                    tile = _pillTiles[16];
                    break;
                case CellType.BlueVirus:
                    tile = _pillTiles[17];
                    break;
                case CellType.RedBlown:
                    tile = _pillTiles[18];
                    break;
                case CellType.YellowBlown:
                    tile = _pillTiles[19];
                    break;
                case CellType.BlueBlown:
                    tile = _pillTiles[20];
                    break;
                case CellType.Empty:
                    tile = _pillTiles[21];
                    break;
                default:
                    tile = _pillTiles[21];
                    break;
            }

            return tile;
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
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _keyIsDown[(int) InputAction.MoveDown] = true;

                if (CanMovePill(_currentPill, MovementDirection.Down))
                    MovePillDown();
            }

            if (_keyIsDown[(int) InputAction.MoveDown])
                _timeSinceKeyDown[(int) InputAction.MoveDown] += Time.unscaledDeltaTime;

            if (!(_inputTime > _inputSpeed))
                return;

            if (!_keyIsDown[(int) InputAction.MoveDown] ||
                !(_timeSinceKeyDown[(int) InputAction.MoveDown] > _keyDownSpeed))
                return;

            if (CanMovePill(_currentPill, MovementDirection.Down))
                MovePillDown();
        }

        private void HandleMoveLeft()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _keyIsDown[(int) InputAction.MoveLeft] = true;

                if (CanMovePill(_currentPill, MovementDirection.Left))
                    MovePillLeft();
            }

            if (_keyIsDown[(int) InputAction.MoveLeft])
                _timeSinceKeyDown[(int) InputAction.MoveLeft] += Time.unscaledDeltaTime;

            if (!(_inputTime > _inputSpeed))
                return;

            if (!_keyIsDown[(int) InputAction.MoveLeft] || !(_timeSinceKeyDown[(int) InputAction.MoveLeft] > _keyDownSpeed))
                return;

            if (CanMovePill(_currentPill, MovementDirection.Left))
                MovePillLeft();
        }

        private void HandleMoveRight()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _keyIsDown[(int) InputAction.MoveRight] = true;

                if (CanMovePill(_currentPill, MovementDirection.Right))
                    MovePillRight();
            }

            if (_keyIsDown[(int) InputAction.MoveRight])
                _timeSinceKeyDown[(int) InputAction.MoveRight] += Time.unscaledDeltaTime;

            if (!(_inputTime > _inputSpeed))
                return;

            if (!_keyIsDown[(int) InputAction.MoveRight] || !(_timeSinceKeyDown[(int) InputAction.MoveRight] > _keyDownSpeed))
                return;

            if (CanMovePill(_currentPill, MovementDirection.Right))
                MovePillRight();
        }

        private void HandleRotateClockwise()
        {
            if (!Input.GetKeyDown(KeyCode.S))
                return;

            if (CanRotatePill(RotationDirection.Clockwise))
            {
                RotatePillClockwise();
            }
            else
            {
                switch (_currentPill.Orientation)
                {
                    case CellOrientation.Up:
                    case CellOrientation.Down:
                    {
                        if (!CanMovePillLeft(_currentPill))
                            return;

                        MovePillLeft();
                        RotatePillClockwise();
                        break;
                    }
                    case CellOrientation.Right:
                    case CellOrientation.Left:
                    {
                        if (!CanMovePillDown(_currentPill))
                            return;

                        MovePillDown();
                        RotatePillClockwise();
                        break;
                    }
                }
            }
        }

        private void HandleRotateCounterClockwise()
        {
            if (!Input.GetKeyDown(KeyCode.A))
                return;

            if (CanRotatePill(RotationDirection.CounterClockwise))
            {
                RotatePillCounterClockwise();
            }
            else
            {
                switch (_currentPill.Orientation)
                {
                    case CellOrientation.Up:
                    case CellOrientation.Down:
                    {
                        if (!CanMovePillRight(_currentPill))
                            return;

                        MovePillRight();
                        RotatePillCounterClockwise();
                        break;
                    }
                    case CellOrientation.Right:
                    case CellOrientation.Left:
                    {
                        if (!CanMovePillDown(_currentPill))
                            return;

                        MovePillDown();
                        RotatePillCounterClockwise();
                        break;
                    }
                }
            }
        }

        private void MovePillDown()
        {
            Vector2 posCell0 = _currentPill.Cells[0].Position;
            Vector2 posCell1 = _currentPill.Cells[1].Position;
            _board[(int) posCell0.x, (int) posCell0.y] = CellType.Empty;
            _board[(int) posCell1.x, (int) posCell1.y] = CellType.Empty;

            _currentPill.MoveDown();

            posCell0 = _currentPill.Cells[0].Position;
            posCell1 = _currentPill.Cells[1].Position;
            _board[(int) posCell0.x, (int) posCell0.y] = _currentPill.Cells[0].Type;
            _board[(int) posCell1.x, (int) posCell1.y] = _currentPill.Cells[1].Type;
        }

        private void MovePillLeft()
        {
            Vector2 posCell0 = _currentPill.Cells[0].Position;
            Vector2 posCell1 = _currentPill.Cells[1].Position;
            _board[(int) posCell0.x, (int) posCell0.y] = CellType.Empty;
            _board[(int) posCell1.x, (int) posCell1.y] = CellType.Empty;

            _currentPill.MoveLeft();

            posCell0 = _currentPill.Cells[0].Position;
            posCell1 = _currentPill.Cells[1].Position;
            _board[(int) posCell0.x, (int) posCell0.y] = _currentPill.Cells[0].Type;
            _board[(int) posCell1.x, (int) posCell1.y] = _currentPill.Cells[1].Type;
        }

        private void MovePillRight()
        {
            Vector2 posCell0 = _currentPill.Cells[0].Position;
            Vector2 posCell1 = _currentPill.Cells[1].Position;
            _board[(int) posCell0.x, (int) posCell0.y] = CellType.Empty;
            _board[(int) posCell1.x, (int) posCell1.y] = CellType.Empty;

            _currentPill.MoveRight();

            posCell0 = _currentPill.Cells[0].Position;
            posCell1 = _currentPill.Cells[1].Position;
            _board[(int) posCell0.x, (int) posCell0.y] = _currentPill.Cells[0].Type;
            _board[(int) posCell1.x, (int) posCell1.y] = _currentPill.Cells[1].Type;
        }

        private bool CanMovePill(Pill pill, MovementDirection direction)
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

        private bool CanMovePill(Pill pill)
        {
            return CanMovePillDown(pill) ||
                   CanMovePillLeft(pill) ||
                   CanMovePillRight(pill);

        }

        private bool CanMovePillDown(Pill pill)
        {
            int x0 = 0;
            int y0 = 0;
            int x1 = 0;
            int y1 = 0;

            int mainCell = pill.Cells[0].IsMainCell ? 0 : 1;
            int otherCell = pill.Cells[1].IsMainCell ? 0 : 1;

            if (pill.Orientation == CellOrientation.Up)
            {
                x0 = (int) pill.Cells[mainCell].Position.x;
                y0 = (int) pill.Cells[mainCell].Position.y - 1;
            }
            else if (pill.Orientation == CellOrientation.Down)
            {
                x0 = (int) pill.Cells[otherCell].Position.x;
                y0 = (int) pill.Cells[otherCell].Position.y - 1;
            }
            else
            {
                x0 = (int) pill.Cells[mainCell].Position.x;
                y0 = (int) pill.Cells[mainCell].Position.y - 1;
                x1 = (int) pill.Cells[otherCell].Position.x;
                y1 = (int) pill.Cells[otherCell].Position.y - 1;

                if (y1 < 1)
                    return false;

                if (_board[x1, y1] != CellType.Empty)
                    return false;
            }

            if (y0 < 1)
                return false;

            return _board[x0, y0] == CellType.Empty;
        }

        private bool CanMovePillLeft(Pill pill)
        {
            int x0 = 0;
            int y0 = 0;
            int x1 = 0;
            int y1 = 0;

            int mainCell = pill.Cells[0].IsMainCell ? 0 : 1;
            int otherCell = pill.Cells[1].IsMainCell ? 0 : 1;

            if (pill.Orientation == CellOrientation.Left)
            {
                x0 = (int) pill.Cells[otherCell].Position.x - 1;
                y0 = (int) pill.Cells[otherCell].Position.y;
            }
            else if (pill.Orientation == CellOrientation.Right)
            {
                x0 = (int) pill.Cells[mainCell].Position.x - 1;
                y0 = (int) pill.Cells[mainCell].Position.y;
            }
            else
            {
                x0 = (int) pill.Cells[mainCell].Position.x - 1;
                y0 = (int) pill.Cells[mainCell].Position.y;
                x1 = (int) pill.Cells[otherCell].Position.x - 1;
                y1 = (int) pill.Cells[otherCell].Position.y;

                if (_board[x1, y1] != CellType.Empty)
                    return false;
            }

            return _board[x0, y0] == CellType.Empty;
        }

        private bool CanMovePillRight(Pill pill)
        {
            int x0 = 0;
            int y0 = 0;
            int x1 = 0;
            int y1 = 0;

            int mainCell = pill.Cells[0].IsMainCell ? 0 : 1;
            int otherCell = pill.Cells[1].IsMainCell ? 0 : 1;

            if (pill.Orientation == CellOrientation.Left)
            {
                x0 = (int) pill.Cells[mainCell].Position.x + 1;
                y0 = (int) pill.Cells[mainCell].Position.y;
            }
            else if (pill.Orientation == CellOrientation.Right)
            {
                x0 = (int) pill.Cells[otherCell].Position.x + 1;
                y0 = (int) pill.Cells[otherCell].Position.y;
            }
            else
            {
                x0 = (int) pill.Cells[mainCell].Position.x + 1;
                y0 = (int) pill.Cells[mainCell].Position.y;
                x1 = (int) pill.Cells[otherCell].Position.x + 1;
                y1 = (int) pill.Cells[otherCell].Position.y;

                if (_board[x1, y1] != CellType.Empty)
                    return false;
            }

            return _board[x0, y0] == CellType.Empty;
        }

        private bool CanRotatePill(RotationDirection direction)
        {
            if (direction == RotationDirection.CounterClockwise)
            {
                return CanRotatePillCounterClockwise();
            }

            return CanRotatePillClockwise();
        }

        private bool CanRotatePillCounterClockwise()
        {
            int x0 = 0;
            int y0 = 0;
            int mainCell = _currentPill.Cells[0].IsMainCell ? 0 : 1;
            int otherCell = _currentPill.Cells[1].IsMainCell ? 0 : 1;

            switch (_currentPill.Orientation)
            {
                case CellOrientation.Right:
                    x0 = (int) _currentPill.Cells[otherCell].Position.x - 1;
                    y0 = (int) _currentPill.Cells[otherCell].Position.y + 1;
                    break;
                case CellOrientation.Up:
                    x0 = (int) _currentPill.Cells[mainCell].Position.x + 1;
                    y0 = (int) _currentPill.Cells[mainCell].Position.y;
                    break;
                case CellOrientation.Left:
                    x0 = (int) _currentPill.Cells[mainCell].Position.x - 1;
                    y0 = (int) _currentPill.Cells[mainCell].Position.y + 1;
                    break;
                case CellOrientation.Down:
                    x0 = (int) _currentPill.Cells[otherCell].Position.x + 1;
                    y0 = (int) _currentPill.Cells[otherCell].Position.y;
                    break;
            }

            return _board[x0, y0] == CellType.Empty;
        }

        private bool CanRotatePillClockwise()
        {
            int x0 = 0;
            int y0 = 0;

            int mainCell = _currentPill.Cells[0].IsMainCell ? 0 : 1;
            int otherCell = _currentPill.Cells[1].IsMainCell ? 0 : 1;

            switch (_currentPill.Orientation)
            {
                case CellOrientation.Right:
                    x0 = (int) _currentPill.Cells[mainCell].Position.x;
                    y0 = (int) _currentPill.Cells[mainCell].Position.y + 1;
                    break;
                case CellOrientation.Down:
                    x0 = (int) _currentPill.Cells[mainCell].Position.x + 1;
                    y0 = (int) _currentPill.Cells[mainCell].Position.y - 1;
                    break;
                case CellOrientation.Left:
                    x0 = (int) _currentPill.Cells[otherCell].Position.x;
                    y0 = (int) _currentPill.Cells[otherCell].Position.y + 1;
                    break;
                case CellOrientation.Up:
                    x0 = (int) _currentPill.Cells[otherCell].Position.x + 1;
                    y0 = (int) _currentPill.Cells[otherCell].Position.y - 1;
                    break;
            }

            if (_board[x0, y0] != CellType.Empty)
                return false;

            return true;
        }

        private void RotatePillClockwise()
        {
            Vector2 posCell0 = _currentPill.Cells[0].Position;
            Vector2 posCell1 = _currentPill.Cells[1].Position;
            _board[(int) posCell0.x, (int) posCell0.y] = CellType.Empty;
            _board[(int) posCell1.x, (int) posCell1.y] = CellType.Empty;

            _currentPill.RotateClockwise();

            posCell0 = _currentPill.Cells[0].Position;
            posCell1 = _currentPill.Cells[1].Position;
            _board[(int) posCell0.x, (int) posCell0.y] = _currentPill.Cells[0].Type;
            _board[(int) posCell1.x, (int) posCell1.y] = _currentPill.Cells[1].Type;
        }

        private void RotatePillCounterClockwise()
        {
            Vector2 posCell0 = _currentPill.Cells[0].Position;
            Vector2 posCell1 = _currentPill.Cells[1].Position;
            _board[(int) posCell0.x, (int) posCell0.y] = CellType.Empty;
            _board[(int) posCell1.x, (int) posCell1.y] = CellType.Empty;

            _currentPill.RotateCounterClockwise();

            posCell0 = _currentPill.Cells[0].Position;
            posCell1 = _currentPill.Cells[1].Position;
            _board[(int) posCell0.x, (int) posCell0.y] = _currentPill.Cells[0].Type;
            _board[(int) posCell1.x, (int) posCell1.y] = _currentPill.Cells[1].Type;
        }

        private void Play()
        {
            // if (NoVirusLeft())
            // {
            //     _state = GameState.GameOver;
            //     return;
            // }
            
            if (!CanMovePill(_currentPill, MovementDirection.Down))
            {
                var x = (int) _currentPill.Cells[0].Position.x;
                var y = (int) _currentPill.Cells[0].Position.y;

                if (ShouldResolveBlows(x, y))
                {
                   _state = GameState.ResolvingBlows;
                    return;
                }

                if (_state == GameState.Playing)
                {
                    if (!CanSpawnNextPill())
                    {
                        _state = GameState.GameOver;
                        return;
                    }
                    
                    SetCurrentPill(_nextPill);
                    return;
                }
            }

            if (!_keyIsDown[0])
                MovePillDown();
        }

        private bool NoVirusLeft()
        {
            for (int y = 2; y < _boardHeight - 1; ++y)
            {
                for (int x = 1; x < _boardWidth - 1; ++x)
                {
                    var cellType = _board[x, y];
                    if (IsVirus(cellType))
                        return false;
                }
            }

            return true;
        }

        private bool CanSpawnNextPill()
        {
            var x0 = 4;
            var y0 = _boardHeight - 2;
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
            var blownCellOrientation = GetCellOrientation(blownCellType);
            var splitCellOrientation = GetCellOrientation(_board[x, y]);

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

            _board[x, y] = GetCellColor(_board[x, y]);
        }

        private List<Cell> GetHorizontalBlownCells(int x, int y, CellType cellType)
        {
            var tmp = x;

            var blownPositions = new List<Cell>();

            blownPositions.Add(CreateCell(x, y, cellType));

            while (x >= 1 && CellsHaveSameColor(_board[x - 1, y], cellType))
            {
                blownPositions.Add(CreateCell(x - 1, y, cellType));
                x -= 1;
            }

            x = tmp;
            while (x <= _boardWidth && CellsHaveSameColor(_board[x + 1, y], cellType))
            {
                blownPositions.Add(CreateCell(x + 1, y, cellType));
                x += 1;
            }

            if (blownPositions.Count < _minConsecutiveCells)
                blownPositions.Clear();

            return blownPositions;
        }

        private List<Cell> GetVerticalBlownCells(int x, int y, CellType cellType)
        {
            var tmp = y;
            List<Cell> blownPositions = new List<Cell>();

            blownPositions.Add(CreateCell(x, y, cellType));

            while (y >= 1 && CellsHaveSameColor(_board[x, y - 1], cellType))
            {
                blownPositions.Add(CreateCell(x, y - 1, cellType));
                y -= 1;
            }

            y = tmp;
            while (y <= _boardHeight - 1 && CellsHaveSameColor(_board[x, y + 1], cellType))
            {
                blownPositions.Add(CreateCell(x, y + 1, cellType));
                y += 1;
            }

            if (blownPositions.Count < _minConsecutiveCells)
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
                    _board[x, y] = GetCellColor(blownCellType) | CellType.Blown;

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
                _state = GameState.FallingCells;
                _cellGroups.Clear();
            }
        }

        private void FallCells()
        {
            var fallenCells = 0;

            for (int y = 2; y < _boardHeight - 1; ++y)
            {
                for (int x = 1; x < _boardWidth - 1; ++x)
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
                
                _state = GameState.ResolvingBlows;
                _cellGroups.Clear();

                return;
            }

            _cellGroups.Clear();
            _state = GameState.Playing;
            SetCurrentPill(_nextPill);
        }

        private void CheckBlows(Direction direction)
        {
            var i = direction == Direction.Vertical ? _boardHeight + 1 : _boardWidth + 1;
            var x = 0;
            var y = 0;

            start:
            while (i < _boardWidth * _boardHeight)
            {
                if (direction == Direction.Horizontal)
                {
                    x = i % _boardWidth;
                    y = i / _boardWidth;
                }
                else
                {
                    x = i / _boardHeight;
                    y = i % _boardHeight;
                }

                if (_board[x, y] != CellType.Empty &&
                    _board[x, y] != CellType.Wall)
                    break;
                i++;
            }

            var currentColor = GetCellColor(_board[x, y]);

            var currentGroup = new List<Cell>();

            AddCellToGroup(x, y, currentGroup);

            while (++i < _boardWidth * _boardHeight)
            {
                if (direction == Direction.Horizontal)
                {
                    x = i % _boardWidth;
                    y = i / _boardWidth;
                }
                else
                {
                    x = i / _boardHeight;
                    y = i % _boardHeight;
                }

                if (_board[x, y] == CellType.Empty || _board[x, y] == CellType.Wall ||
                    !CellsHaveSameColor(currentColor, GetCellColor(_board[x, y])))
                {
                    if (currentGroup.Count >= _minConsecutiveCells)
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
                                if (foundGroup.Find(c => c.Position == cell.Position) == null)
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
                    currentColor = GetCellColor(_board[x, y]);
                }

                AddCellToGroup(x, y, currentGroup);
            }
        }

        private void AddCellToGroup(int x, int y, List<Cell> currentGroup)
        {
            var cell = new Cell()
            {
                Position = new Vector2(x, y),
                Type = _board[x, y]
            };

            currentGroup.Add(cell);
        }

        private static Cell CreateCell(int x, int y, CellType type)
        {
            return new Cell
            {
                Type = type,
                Position = new Vector2(x, y)
            };
        }

        private static CellType GetCellColor(CellType cellType)
        {
            return cellType & (CellType.Red | CellType.Blue | CellType.Yellow);
        }

        private static bool CellsHaveSameColor(CellType cellType0, CellType cellType1)
        {
            var c0 = cellType0 & (CellType.Red | CellType.Blue | CellType.Yellow);
            var c1 = cellType1 & (CellType.Red | CellType.Blue | CellType.Yellow);
            return c0 == c1;
        }

        private static CellType GetCellOrientation(CellType cellType)
        {
            return cellType & (CellType.Left | CellType.Right | CellType.Down | CellType.Up);
        }
        
        private static bool IsVirus(CellType cellType)
        {
            return (cellType & CellType.Virus) == CellType.Virus;
        }
    }
}