﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pills.Assets
{
    //Covid Fighters
    //Up to four players (local/remote multi-player)
    //The objective is to eradicate the corona virus and its variants.
    //The player who eradicates first, wins.
    //If the virus dominates, player loses.
    //Every x seconds y person dies. (stats)
    //Graphs showing the contamination/death curves (stats)
    //Char special ability! - Player can select between four (maybe more) characters (world leaders)
    //                        with a special ability and a negative passive effect.
        // Dr. Bozo Mario (starts with Cloroquina! card from the start) / viruses spread faster 
        // Coronald Tromb (freezes all other players input for x seconds) / viruses start speed is faster
        // Valdomiro Sputnik (think about)
        // Tedros Adhanom (think about)
    //Event! - all players are affected by same negative/positive event (random)
        // Examples
        // - Fake news (death ratio increases)
        // - New variant (increase consecutive viruses to blow)
        // - New vaccine (decrease consecutive viruses to blow)
        // - Carnival! (doubles viruses speed creation)
        // - Lockdown! (halves viruses speed creation)
    //Goal card! - whenever a player eliminates x viruses, it gain a card (strategy).
    //             when it reaches y number of cards he is obliged to use one of them.
        // Examples
        // - Train doctors (death ratio is decreased)
        // - Cloroquina! (50% hospital beds are cleared, some may instantly die)
        // - Use graduates! (doctors feel rested for some time)
        // - Expatriate! (send patients abroad to other players)
    
    
    public class PlayerController : MonoBehaviour
    {
        private enum GameState
        {
            WaitingStart,
            Playing,
            ResolvingBlows,
            FallingCells,
            GameOver
        }
        
        private enum RoundSpeed
        {
            Low,
            Medium,
            Hard
        }
        
        private static int _randomSeed;

        [SerializeField, Range(0, GameConstants.MaxDifficulty)] private float _virusLevel = 0;
        [SerializeField] private RoundSpeed _speed = RoundSpeed.Low;
        [SerializeField] private Tilemap _tilemap = null;
        [SerializeField] private BoardConfig _boardConfig = null;
        [SerializeField] private int _playerId = 0;
        
        private PlayerControlConfig _playerControl;
        
        private float _roundSpeed;
        private float _inputTime;
        private float _roundTime;
        
        private Pill _currentPill;
        private Pill _nextPill;

        private GameState _state;

        private readonly Random _pillRandom = new Random();
        private readonly Board _board = new Board();
        private readonly Queue<List<Cell>> _toBlowQueue = new Queue<List<Cell>>(GameConstants.BlownQueueCapacity);
        private readonly Queue<List<Cell>> _blownQueue = new Queue<List<Cell>>(GameConstants.BlownQueueCapacity);
        private readonly Dictionary<Vector2Int, List<Cell>> _cellGroups =
            new Dictionary<Vector2Int, List<Cell>>(GameConstants.BoardWidth * GameConstants.BoardWidth);
        
        private PlayerInputController _input;
        private BoardDrawer _boardDrawer;
        private float _startTime;

        private void Start()
        {
            if (_randomSeed == 0)
                _randomSeed = UnityEngine.Random.Range(GameConstants.MinSeedValue, GameConstants.MaxSeedValue);
            
            UnityEngine.Random.InitState(_randomSeed);
            
            _boardDrawer = new BoardDrawer(_tilemap, _boardConfig.PillTiles);
            _roundSpeed = GameConstants.RoundSpeeds[(int)_speed];

            _board.Reset();
            _board.FillVirusesPositions((int)_virusLevel);
            
            _input = new PlayerInputController(_board, GameManager.Get().PlayerControlConfigs[_playerId]);
            
            _nextPill = SpawnNextPill();
            
            SetState(GameState.WaitingStart);
            _startTime = Time.realtimeSinceStartup;
        }

        private Pill SpawnNextPill()
        {
            return Pill.SpawnPill(GameConstants.SpawnPosition.x, GameConstants.BoardHeight + 1, CellOrientation.Right, _pillRandom);
        }

        private void SetState(GameState value)
        {
            _state = value;
            
            switch (value)
            {
                case GameState.Playing:
                    _roundSpeed = GameConstants.RoundSpeeds[(int)_speed];
                    break;
                case GameState.FallingCells:
                    _roundSpeed /= 2.0f;
                    break;
            }
        }
        
        private void Update()
        {
            if (GameManager.Get().IsPaused)
            {
                return;
            }
            
            if (_state == GameState.GameOver)
            {
                //todo: show game over 
                return;
            }
            
            _input.Reset();

            if (_state == GameState.WaitingStart && Time.realtimeSinceStartup - _startTime > 3.0f)
            {
                SetCurrentPill(_nextPill);
                SetState(GameState.Playing);
            }

            _inputTime += Time.unscaledDeltaTime;
            _roundTime += Time.unscaledDeltaTime;

            if (_state == GameState.Playing && _currentPill != null)
            {
                _input.Update(_currentPill, _inputTime);
            }

            if (_inputTime > GameConstants.Input.Speed)
            {
                _inputTime = 0.0f;
            }

            if (_roundTime <= _roundSpeed)
            {
                return;
            }

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

        private void LateUpdate()
        {
            _boardDrawer.Draw(_board, _nextPill);
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
                    if (!_board.CanSpawnNextPill())
                    {
                        SetState(GameState.GameOver);
                        return;
                    }
                    
                    SetCurrentPill(_nextPill);
                    return;
                }
            }

            if (!_input.IsKeyDown(MovementDirection.Down))
                _board.MovePillDown(_currentPill);
        }
        
        private void SetCurrentPill(Pill pill)
        {
            _currentPill = pill;
            _currentPill.Translate(0, -3);
            var cell0 = _currentPill.Cells[0];
            var cell1 = _currentPill.Cells[1];
            _board[cell0.Position.x, cell0.Position.y] = cell0.Type;
            _board[cell1.Position.x, cell1.Position.y] = cell1.Type;
            _nextPill = SpawnNextPill();
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

            x = _currentPill.Cells[1].Position.x;
            y = _currentPill.Cells[1].Position.y;
            cellType = _currentPill.Cells[1].Type;

            horizontalCells = GetHorizontalBlownCells(x, y, cellType);
            if (horizontalCells.Count > 0)
            {
                return true;
            }

            verticalCells = GetVerticalBlownCells(x, y, cellType);
            return verticalCells.Count > 0;
        }

        private List<Cell> GetHorizontalBlownCells(int x, int y, CellType cellType)
        {
            var tmp = x;

            var blownPositions = new List<Cell>
            {
                Cell.CreateCell(x, y, cellType)
            };

            while (x >= 1 && Cell.HaveSameColor(_board[x - 1, y], cellType))
            {
                blownPositions.Add(Cell.CreateCell(x - 1, y, cellType));
                x -= 1;
            }

            x = tmp;
            while (x <= GameConstants.BoardWidth && Cell.HaveSameColor(_board[x + 1, y], cellType))
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
            var blownPositions = new List<Cell>
            {
                Cell.CreateCell(x, y, cellType)
            };


            while (y >= 1 && Cell.HaveSameColor(_board[x, y - 1], cellType))
            {
                blownPositions.Add(Cell.CreateCell(x, y - 1, cellType));
                y -= 1;
            }

            y = tmp;
            while (y <= GameConstants.BoardHeight - 1 && Cell.HaveSameColor(_board[x, y + 1], cellType))
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
                    var x = blownCell.Position.x;
                    var y = blownCell.Position.y;
                    _board[x, y] = CellType.Empty;
                }
            }

            while (_toBlowQueue.Count > 0)
            {
                var toBlowGroup = _toBlowQueue.Dequeue();
                
                foreach (var blownCell in toBlowGroup)
                {
                    var x = blownCell.Position.x;
                    var y = blownCell.Position.y;
                    var blownCellType = _board[x, y];
                    
                    _board[x, y] = Cell.GetCellColor(blownCellType) | CellType.Blown;
            
                    var blownCellOrientation = Cell.GetCellOrientation(blownCellType);
                    switch (blownCellOrientation)
                    {
                        case CellType.Left:
                            _board.SplitCell(x + 1, y, blownCellType);
                            break;
                        case CellType.Right:
                            _board.SplitCell(x - 1, y, blownCellType);
                            break;
                        case CellType.Up:
                            _board.SplitCell(x, y + 1, blownCellType);
                            break;
                        case CellType.Down:
                            _board.SplitCell(x, y - 1, blownCellType);
                            break;
                    }
                }
                
                _blownQueue.Enqueue(toBlowGroup);
            }

            if (_blownQueue.Count != 0) 
                return;
            
            SetState(GameState.FallingCells);
            _cellGroups.Clear();
        }

        private void FallCells()
        {
            var fallenCells = 0;

            for (var y = 2; y < GameConstants.BoardHeight - 1; ++y)
            {
                for (var x = 1; x < GameConstants.BoardWidth - 1; ++x)
                {
                    var cellType = _board[x, y];

                    if (cellType == CellType.Empty || cellType == CellType.Wall)
                        continue;

                    var isVirus = Cell.IsVirus(cellType);
                    if (isVirus)
                        continue;

                    if (_board[x, y - 1] != CellType.Empty)
                        continue;

                    var canFall = true;
                    var isSingleCell = Cell.IsSingleCell(cellType);

                    if (!isSingleCell)
                    {
                        var cellOrientation = Cell.GetCellOrientation(cellType);

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
                    !Cell.HaveSameColor(currentColor, Cell.GetCellColor(_board[x, y])))
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

        private void AddCellToGroup(int x, int y, ICollection<Cell> currentGroup)
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