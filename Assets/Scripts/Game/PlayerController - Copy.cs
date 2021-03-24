// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.Tilemaps;
//
// namespace Pills.Assets
// {
//     public class PlayerController : MonoBehaviour
//     {
//         private enum GameState
//         {
//             WaitingStart,
//             Playing,
//             ResolvingBlows,
//             FallingCells,
//             GameOver
//         }
//         
//         private enum RoundSpeed
//         {
//             Low,
//             Medium,
//             Hard
//         }
//         
//         private static int _randomSeed;
//
//         [SerializeField, Range(0, GameConstants.MaxDifficulty)] private float _virusLevel = 0;
//         [SerializeField] private RoundSpeed _speed = RoundSpeed.Low;
//         [SerializeField] private Tilemap _tilemap;
//         [SerializeField] private BoardConfig _boardConfig;
//         [SerializeField] private int _playerId;
//         
//         private PlayerControlConfig _playerControl; 
//         
//         private KeyCode _downKey;
//         private KeyCode _leftKey;
//         private KeyCode _rightKey;
//         private KeyCode _rotateClockwiseKey;
//         private KeyCode _rotateCounterClockwiseKey;
//         
//         private float _roundSpeed;
//         private bool _isPaused;
//         private float _inputTime;
//         
//         private float _roundTime;
//         private Pill _currentPill;
//         private Pill _nextPill;
//         private bool[] _keyIsDown;
//         private float[] _timeSinceKeyDown;
//
//         private GameState _state;
//
//         private readonly Board _board = new Board();
//         private readonly Queue<List<Cell>> _toBlowQueue = new Queue<List<Cell>>();
//         private readonly Queue<List<Cell>> _blownQueue = new Queue<List<Cell>>();
//         private readonly Dictionary<Vector2Int, List<Cell>> _cellGroups = new Dictionary<Vector2Int, List<Cell>>(10 * 18);
//         
//         private Random _pillRandom;
//         private BoardDrawer _boardDrawer;
//
//         private void Awake()
//         {
//             InitializePlayerControls();
//         }
//
//         private void InitializePlayerControls()
//         {
//             var playerControlConfig = GameManager.Get().PlayerControlConfigs[_playerId];
//
//             _downKey = playerControlConfig.DownKey;
//             _leftKey = playerControlConfig.LeftKey;
//             _rightKey = playerControlConfig.RightKey;
//             _rotateClockwiseKey = playerControlConfig.RotateClockwiseKey;
//             _rotateCounterClockwiseKey = playerControlConfig.RotateCounterClockwiseKey;
//         }
//
//         private void Start()
//         {
//             if (_randomSeed == 0)
//                 _randomSeed = UnityEngine.Random.Range(1, 1024);
//             
//             UnityEngine.Random.InitState(_randomSeed);
//             
//             _boardDrawer = new BoardDrawer(_tilemap, _boardConfig.PillTiles);
//             _pillRandom = new Random();
//             _keyIsDown = new bool[5];
//             _timeSinceKeyDown = new float[5];
//
//             _isPaused = true;
//             _roundSpeed = GameConstants.RoundSpeeds[(int)_speed];
//             
//             _roundTime = 0.0f;
//             _inputTime = 0.0f;
//
//             _board.Reset();
//             _board.FillVirusesPositions((int)_virusLevel);
//             
//             _nextPill = Pill.SpawnPill(4, GameConstants.BoardHeight + 1, CellOrientation.Right, _pillRandom);
//             
//             SetState(GameState.WaitingStart);
//         }
//
//         private void SetState(GameState value)
//         {
//             _state = value;
//             
//             switch (value)
//             {
//                 case GameState.Playing:
//                     _roundSpeed = GameConstants.RoundSpeeds[(int)_speed];
//                     break;
//                 case GameState.FallingCells:
//                     _roundSpeed /= 2.0f;
//                     break;
//             }
//         }
//         
//         public void LateUpdate()
//         {
//             if (_state == GameState.GameOver)
//             {
//                 return;
//             }
//             
//             ResetInputIfNeeded();
//
//             if (Input.GetKeyDown(KeyCode.P))
//             {
//                 if (_state == GameState.WaitingStart)
//                 {
//                     SetCurrentPill(_nextPill);
//                     SetState(GameState.Playing);
//                 }
//
//                 _isPaused = !_isPaused;
//             }
//
//             if (_isPaused)
//             {
//                 return;
//             }
//
//             _inputTime += Time.unscaledDeltaTime;
//             _roundTime += Time.unscaledDeltaTime;
//
//             if (_state == GameState.Playing && _currentPill != null)
//             {
//                 HandleInput();
//             }
//
//             if (_inputTime > GameConstants.Input.Speed)
//             {
//                 _inputTime = 0.0f;
//             }
//
//             if (_roundTime <= _roundSpeed)
//             {
//                 return;
//             }
//
//             // UnityEngine.Debug.Log("input time: " + _inputTime);
//             // UnityEngine.Debug.Log("input speed: " + _inputSpeed);
//             // UnityEngine.Debug.Log("round time: " + _roundTime);
//             // UnityEngine.Debug.Log("round speed: " + _roundSpeed);
//             // UnityEngine.Debug.Log("keydown down: " + _keyIsDown[(int)InputAction.MoveDown]);
//             // UnityEngine.Debug.Log("keydown left: " + _keyIsDown[(int)InputAction.MoveLeft]);
//             // UnityEngine.Debug.Log("keydown right: " + _keyIsDown[(int)InputAction.MoveRight]);
//             // UnityEngine.Debug.Log("time keydown down: " + _timeSinceKeyDown[(int)InputAction.MoveDown]);
//             // UnityEngine.Debug.Log("time keydown left: " + _timeSinceKeyDown[(int)InputAction.MoveLeft]);
//             // UnityEngine.Debug.Log("time keydown right: " + _timeSinceKeyDown[(int)InputAction.MoveRight]);
//
//             _roundTime = 0.0f;
//
//             if (_state == GameState.WaitingStart)
//                 return;
//
//             if (_state == GameState.Playing)
//                 Play();
//
//             if (_state == GameState.ResolvingBlows)
//                 ResolveBlows();
//             
//             if (_state == GameState.FallingCells)
//                 FallCells();
//         }
//
//         private void Update()
//         {
//             _boardDrawer.Draw(_board, _nextPill);
//         }
//
//         private void ResetInputIfNeeded()
//         {
//             if (Input.GetKeyUp(_downKey))
//             {
//                 _keyIsDown[(int) MovementDirection.Down] = false;
//                 _timeSinceKeyDown[(int) MovementDirection.Down] = 0;
//             }
//
//             if (Input.GetKeyUp(_leftKey))
//             {
//                 _keyIsDown[(int) MovementDirection.Left] = false;
//                 _timeSinceKeyDown[(int) MovementDirection.Left] = 0;
//             }
//
//             if (Input.GetKeyUp(_rightKey))
//             {
//                 _keyIsDown[(int) MovementDirection.Right] = false;
//                 _timeSinceKeyDown[(int) MovementDirection.Right] = 0;
//             }
//         }
//
//         private void HandleInput()
//         {
//             HandleMove(_downKey, MovementDirection.Down);
//             HandleMove(_leftKey, MovementDirection.Left);
//             HandleMove(_rightKey, MovementDirection.Right);
//             HandleRotateClockwise();
//             HandleRotateCounterClockwise();
//         }
//         
//         private void HandleMove(KeyCode pressedKey, MovementDirection movementDirection)
//         {
//             if (Input.GetKeyDown(pressedKey))
//             {
//                 _keyIsDown[(int) movementDirection] = true;
//
//                 if (_board.CanMovePill(_currentPill, movementDirection))
//                     _board.MovePill(_currentPill, movementDirection);
//             }
//
//             if (_keyIsDown[(int) movementDirection])
//                 _timeSinceKeyDown[(int) movementDirection] += Time.unscaledDeltaTime;
//
//             if (_inputTime <= GameConstants.Input.Speed)
//                 return;
//
//             if (!_keyIsDown[(int) movementDirection] || !(_timeSinceKeyDown[(int) movementDirection] > GameConstants.Input.KeyDownSpeed))
//                 return;
//
//             if (_board.CanMovePill(_currentPill, movementDirection))
//                 _board.MovePill(_currentPill, movementDirection);
//         }
//
//         private void HandleRotateClockwise()
//         {
//             if (!Input.GetKeyDown(_rotateClockwiseKey))
//                 return;
//
//             if (_board.CanRotatePill(_currentPill, RotationDirection.Clockwise))
//             {
//                 _board.RotatePillClockwise(_currentPill);
//             }
//             else
//             {
//                 switch (_currentPill.Orientation)
//                 {
//                     case CellOrientation.Up:
//                     case CellOrientation.Down:
//                     {
//                         if (!_board.CanMovePillLeft(_currentPill))
//                             return;
//
//                         _board.MovePillLeft(_currentPill);
//                         _board.RotatePillClockwise(_currentPill);
//                         break;
//                     }
//                     case CellOrientation.Right:
//                     case CellOrientation.Left:
//                     {
//                         if (!_board.CanMovePillDown(_currentPill))
//                             return;
//
//                         _board.MovePillDown(_currentPill);
//                         _board.RotatePillClockwise(_currentPill);
//                         break;
//                     }
//                 }
//             }
//         }
//
//         private void HandleRotateCounterClockwise()
//         {
//             if (!Input.GetKeyDown(_rotateCounterClockwiseKey))
//                 return;
//
//             if (_board.CanRotatePill(_currentPill, RotationDirection.CounterClockwise))
//             {
//                 _board.RotatePillCounterClockwise(_currentPill);
//             }
//             else
//             {
//                 switch (_currentPill.Orientation)
//                 {
//                     case CellOrientation.Up:
//                     case CellOrientation.Down:
//                     {
//                         if (!_board.CanMovePillRight(_currentPill))
//                             return;
//
//                         _board.MovePillRight(_currentPill);
//                         _board.RotatePillCounterClockwise(_currentPill);
//                         break;
//                     }
//                     case CellOrientation.Right:
//                     case CellOrientation.Left:
//                     {
//                         if (!_board.CanMovePillDown(_currentPill))
//                             return;
//
//                         _board.MovePillDown(_currentPill);
//                         _board.RotatePillCounterClockwise(_currentPill);
//                         break;
//                     }
//                 }
//             }
//         }
//         
//         private void Play()
//         {
//             if (_board.HasNoVirusLeft())
//             {
//                 SetState(GameState.GameOver);
//                 return;
//             }
//             
//             if (!_board.CanMovePill(_currentPill, MovementDirection.Down))
//             {
//                 var x = (int) _currentPill.Cells[0].Position.x;
//                 var y = (int) _currentPill.Cells[0].Position.y;
//
//                 if (ShouldResolveBlows(x, y))
//                 {
//                    SetState(GameState.ResolvingBlows);
//                     return;
//                 }
//
//                 if (_state == GameState.Playing)
//                 {
//                     if (!_board.CanSpawnNextPill())
//                     {
//                         SetState(GameState.GameOver);
//                         return;
//                     }
//                     
//                     SetCurrentPill(_nextPill);
//                     return;
//                 }
//             }
//
//             if (!_keyIsDown[0])
//                 _board.MovePillDown(_currentPill);
//         }
//         
//         private void SetCurrentPill(Pill pill)
//         {
//             _currentPill = pill;
//             _currentPill.Translate(0, -3);
//             var cell0 = _currentPill.Cells[0];
//             var cell1 = _currentPill.Cells[1];
//             _board[(int) cell0.Position.x, (int) cell0.Position.y] = cell0.Type;
//             _board[(int) cell1.Position.x, (int) cell1.Position.y] = cell1.Type;
//             _nextPill = Pill.SpawnPill(4, GameConstants.BoardHeight + 1, CellOrientation.Right, _pillRandom);
//         }
//
//         private bool ShouldResolveBlows(int x, int y)
//         {
//             var cellType = _currentPill.Cells[0].Type;
//
//             var horizontalCells = GetHorizontalBlownCells(x, y, cellType);
//             if (horizontalCells.Count > 0)
//             {
//                 return true;
//             }
//
//             var verticalCells = GetVerticalBlownCells(x, y, cellType);
//             if (verticalCells.Count > 0)
//             {
//                 return true;
//             }
//
//             x = _currentPill.Cells[1].Position.x;
//             y = _currentPill.Cells[1].Position.y;
//             cellType = _currentPill.Cells[1].Type;
//
//             horizontalCells = GetHorizontalBlownCells(x, y, cellType);
//             if (horizontalCells.Count > 0)
//             {
//                 return true;
//             }
//
//             verticalCells = GetVerticalBlownCells(x, y, cellType);
//             return verticalCells.Count > 0;
//         }
//
//         private List<Cell> GetHorizontalBlownCells(int x, int y, CellType cellType)
//         {
//             var tmp = x;
//
//             var blownPositions = new List<Cell>
//             {
//                 Cell.CreateCell(x, y, cellType)
//             };
//
//             while (x >= 1 && Cell.HaveSameColor(_board[x - 1, y], cellType))
//             {
//                 blownPositions.Add(Cell.CreateCell(x - 1, y, cellType));
//                 x -= 1;
//             }
//
//             x = tmp;
//             while (x <= GameConstants.BoardWidth && Cell.HaveSameColor(_board[x + 1, y], cellType))
//             {
//                 blownPositions.Add(Cell.CreateCell(x + 1, y, cellType));
//                 x += 1;
//             }
//
//             if (blownPositions.Count < GameConstants.MinConsecutiveCells)
//                 blownPositions.Clear();
//
//             return blownPositions;
//         }
//
//         private List<Cell> GetVerticalBlownCells(int x, int y, CellType cellType)
//         {
//             var tmp = y;
//             var blownPositions = new List<Cell>
//             {
//                 Cell.CreateCell(x, y, cellType)
//             };
//
//
//             while (y >= 1 && Cell.HaveSameColor(_board[x, y - 1], cellType))
//             {
//                 blownPositions.Add(Cell.CreateCell(x, y - 1, cellType));
//                 y -= 1;
//             }
//
//             y = tmp;
//             while (y <= GameConstants.BoardHeight - 1 && Cell.HaveSameColor(_board[x, y + 1], cellType))
//             {
//                 blownPositions.Add(Cell.CreateCell(x, y + 1, cellType));
//                 y += 1;
//             }
//
//             if (blownPositions.Count < GameConstants.MinConsecutiveCells)
//                 blownPositions.Clear();
//
//             return blownPositions;
//         }
//
//         private void ResolveBlows()
//         {
//             CheckBlows(Direction.Vertical);
//             CheckBlows(Direction.Horizontal);
//
//             while (_blownQueue.Count > 0)
//             {
//                 var blownGroup = _blownQueue.Dequeue();
//
//                 foreach (var blownCell in blownGroup)
//                 {
//                     var x = blownCell.Position.x;
//                     var y = blownCell.Position.y;
//                     _board[x, y] = CellType.Empty;
//                 }
//             }
//
//             while (_toBlowQueue.Count > 0)
//             {
//                 var toBlowGroup = _toBlowQueue.Dequeue();
//                 
//                 foreach (var blownCell in toBlowGroup)
//                 {
//                     var x = blownCell.Position.x;
//                     var y = blownCell.Position.y;
//                     var blownCellType = _board[x, y];
//                     
//                     _board[x, y] = Cell.GetCellColor(blownCellType) | CellType.Blown;
//             
//                     var blownCellOrientation = Cell.GetCellOrientation(blownCellType);
//                     switch (blownCellOrientation)
//                     {
//                         case CellType.Left:
//                             _board.SplitCell(x + 1, y, blownCellType);
//                             break;
//                         case CellType.Right:
//                             _board.SplitCell(x - 1, y, blownCellType);
//                             break;
//                         case CellType.Up:
//                             _board.SplitCell(x, y + 1, blownCellType);
//                             break;
//                         case CellType.Down:
//                             _board.SplitCell(x, y - 1, blownCellType);
//                             break;
//                     }
//                 }
//                 
//                 _blownQueue.Enqueue(toBlowGroup);
//             }
//
//             if (_blownQueue.Count != 0) 
//                 return;
//             
//             SetState(GameState.FallingCells);
//             _cellGroups.Clear();
//         }
//
//         private void FallCells()
//         {
//             var fallenCells = 0;
//
//             for (var y = 2; y < GameConstants.BoardHeight - 1; ++y)
//             {
//                 for (var x = 1; x < GameConstants.BoardWidth - 1; ++x)
//                 {
//                     var cellType = _board[x, y];
//
//                     if (cellType == CellType.Empty || cellType == CellType.Wall)
//                         continue;
//
//                     var isVirus = Cell.IsVirus(cellType);
//                     if (isVirus)
//                         continue;
//
//                     if (_board[x, y - 1] != CellType.Empty)
//                         continue;
//
//                     var canFall = true;
//                     var isSingleCell = Cell.IsSingleCell(cellType);
//
//                     if (!isSingleCell)
//                     {
//                         var cellOrientation = Cell.GetCellOrientation(cellType);
//
//                         switch (cellOrientation)
//                         {
//                             case CellType.Left:
//                                 canFall = _board[x + 1, y - 1] == CellType.Empty;
//                                 if (canFall)
//                                 {
//                                     _board[x + 1, y - 1] = _board[x + 1, y];
//                                     _board[x + 1, y] = CellType.Empty;
//                                 }
//
//                                 break;
//                             case CellType.Right:
//                                 canFall = _board[x - 1, y - 1] == CellType.Empty;
//                                 if (canFall)
//                                 {
//                                     _board[x - 1, y - 1] = _board[x - 1, y];
//                                     _board[x - 1, y] = CellType.Empty;
//                                 }
//
//                                 break;
//                         }
//                     }
//
//                     if (!canFall)
//                         continue;
//
//                     fallenCells++;
//                     _board[x, y - 1] = _board[x, y];
//                     _board[x, y] = CellType.Empty;
//                 }
//             }
//
//             if (fallenCells != 0)
//             {
//                 CheckBlows(Direction.Vertical);
//                 CheckBlows(Direction.Horizontal);
//
//                 if (_toBlowQueue.Count <= 0) 
//                     return;
//                 
//                 SetState(GameState.ResolvingBlows);
//                 _cellGroups.Clear();
//
//                 return;
//             }
//
//             _cellGroups.Clear();
//             SetState(GameState.Playing);
//             SetCurrentPill(_nextPill);
//         }
//
//         private void CheckBlows(Direction direction)
//         {
//             var i = direction == Direction.Vertical ? GameConstants.BoardHeight + 1 : GameConstants.BoardWidth + 1;
//             var x = 0;
//             var y = 0;
//
//             start:
//             while (i < GameConstants.BoardWidth * GameConstants.BoardHeight)
//             {
//                 if (direction == Direction.Horizontal)
//                 {
//                     x = i % GameConstants.BoardWidth;
//                     y = i / GameConstants.BoardWidth;
//                 }
//                 else
//                 {
//                     x = i / GameConstants.BoardHeight;
//                     y = i % GameConstants.BoardHeight;
//                 }
//
//                 if (_board[x, y] != CellType.Empty &&
//                     _board[x, y] != CellType.Wall)
//                     break;
//                 i++;
//             }
//
//             var currentColor = Cell.GetCellColor(_board[x, y]);
//
//             var currentGroup = new List<Cell>();
//
//             AddCellToGroup(x, y, currentGroup);
//
//             while (++i < GameConstants.BoardWidth * GameConstants.BoardHeight)
//             {
//                 if (direction == Direction.Horizontal)
//                 {
//                     x = i % GameConstants.BoardWidth;
//                     y = i / GameConstants.BoardWidth;
//                 }
//                 else
//                 {
//                     x = i / GameConstants.BoardHeight;
//                     y = i % GameConstants.BoardHeight;
//                 }
//
//                 if (_board[x, y] == CellType.Empty || _board[x, y] == CellType.Wall ||
//                     !Cell.HaveSameColor(currentColor, Cell.GetCellColor(_board[x, y])))
//                 {
//                     if (currentGroup.Count >= GameConstants.MinConsecutiveCells)
//                     {
//                         List<Cell> foundGroup = null;
//
//                         foreach (var cell in currentGroup)
//                         {
//                             if (!_cellGroups.TryGetValue(cell.Position, out foundGroup))
//                             {
//                                 _cellGroups.Add(cell.Position, currentGroup);
//                                 continue;
//                             }
//
//                             break;
//                         }
//
//                         if (foundGroup == null)
//                             _toBlowQueue.Enqueue(currentGroup);
//                         else
//                         {
//                             foreach (var cell in currentGroup)
//                             {
//                                 if (foundGroup.Any(c => c.Position == cell.Position))
//                                 {
//                                     foundGroup.Add(cell);                                    
//                                 }
//                             }
//                         }
//                     }
//
//                     if (_board[x, y] == CellType.Empty || _board[x, y] == CellType.Wall)
//                     {
//                         goto start;
//                     }
//
//                     currentGroup = new List<Cell>();
//                     currentColor = Cell.GetCellColor(_board[x, y]);
//                 }
//
//                 AddCellToGroup(x, y, currentGroup);
//             }
//         }
//
//         private void AddCellToGroup(int x, int y, ICollection<Cell> currentGroup)
//         {
//             var cell = new Cell()
//             {
//                 Position = new Vector2Int(x, y),
//                 Type = _board[x, y]
//             };
//
//             currentGroup.Add(cell);
//         }
//         
//     }
// }