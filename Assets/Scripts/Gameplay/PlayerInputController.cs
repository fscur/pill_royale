using UnityEngine;

namespace Pills.Assets.Gameplay
{
    public class PlayerInputController
    {
        private readonly bool[] _keyIsDown = new bool[5];
        private readonly float[] _timeSinceKeyDown = new float[5];
        private readonly Board _board;
        private float _inputTime;
        
        private KeyCode _downKey;
        private KeyCode _leftKey;
        private KeyCode _rightKey;
        private KeyCode _rotateClockwiseKey;
        private KeyCode _rotateCounterClockwiseKey;

        public PlayerInputController(Board board, PlayerControlConfig playerControlConfig)
        {
            _board = board;

            InitializePlayerControls(playerControlConfig);
        }
        
        private void InitializePlayerControls(PlayerControlConfig playerControlConfig)
        {
            _downKey = playerControlConfig.DownKey;
            _leftKey = playerControlConfig.LeftKey;
            _rightKey = playerControlConfig.RightKey;
            _rotateClockwiseKey = playerControlConfig.RotateClockwiseKey;
            _rotateCounterClockwiseKey = playerControlConfig.RotateCounterClockwiseKey;
        }
        
        public void Reset()
        {
            if (Input.GetKeyUp(_downKey))
            {
                _keyIsDown[(int) MovementDirection.Down] = false;
                _timeSinceKeyDown[(int) MovementDirection.Down] = 0;
            }

            if (Input.GetKeyUp(_leftKey))
            {
                _keyIsDown[(int) MovementDirection.Left] = false;
                _timeSinceKeyDown[(int) MovementDirection.Left] = 0;
            }

            if (Input.GetKeyUp(_rightKey))
            {
                _keyIsDown[(int) MovementDirection.Right] = false;
                _timeSinceKeyDown[(int) MovementDirection.Right] = 0;
            }
        }

        public void Update(Pill pill, float inputTime)
        {
            _inputTime = inputTime;
            
            HandleMove(pill, _downKey, MovementDirection.Down);
            HandleMove(pill, _leftKey, MovementDirection.Left);
            HandleMove(pill, _rightKey, MovementDirection.Right);
            HandleRotateClockwise(pill);
            HandleRotateCounterClockwise(pill);
        }

        public bool IsKeyDown(MovementDirection direction)
        {
            return _keyIsDown[(int)direction];
        }
        
        private void HandleMove(Pill pill, KeyCode pressedKey, MovementDirection movementDirection)
        {
            if (Input.GetKeyDown(pressedKey))
            {
                _keyIsDown[(int) movementDirection] = true;

                if (_board.CanMovePill(pill, movementDirection))
                    _board.MovePill(pill, movementDirection);
            }

            if (_keyIsDown[(int) movementDirection])
                _timeSinceKeyDown[(int) movementDirection] += Time.unscaledDeltaTime;

            if (_inputTime <= GameConstants.Input.Speed)
                return;

            if (!_keyIsDown[(int) movementDirection] || !(_timeSinceKeyDown[(int) movementDirection] > GameConstants.Input.KeyDownSpeed))
                return;

            if (_board.CanMovePill(pill, movementDirection))
                _board.MovePill(pill, movementDirection);
        }

        private void HandleRotateClockwise(Pill pill)
        {
            if (!Input.GetKeyDown(_rotateClockwiseKey))
                return;

            if (_board.CanRotatePill(pill, RotationDirection.Clockwise))
            {
                _board.RotatePillClockwise(pill);
            }
            else
            {
                switch (pill.Orientation)
                {
                    case CellOrientation.Up:
                    case CellOrientation.Down:
                    {
                        if (!_board.CanMovePillLeft(pill))
                            return;

                        _board.MovePillLeft(pill);
                        _board.RotatePillClockwise(pill);
                        break;
                    }
                    case CellOrientation.Right:
                    case CellOrientation.Left:
                    {
                        if (!_board.CanMovePillDown(pill))
                            return;

                        _board.MovePillDown(pill);
                        _board.RotatePillClockwise(pill);
                        break;
                    }
                }
            }
        }

        private void HandleRotateCounterClockwise(Pill pill)
        {
            if (!Input.GetKeyDown(_rotateCounterClockwiseKey))
                return;

            if (_board.CanRotatePill(pill, RotationDirection.CounterClockwise))
            {
                _board.RotatePillCounterClockwise(pill);
            }
            else
            {
                switch (pill.Orientation)
                {
                    case CellOrientation.Up:
                    case CellOrientation.Down:
                    {
                        if (!_board.CanMovePillLeft(pill))
                            return;

                        _board.MovePillLeft(pill);
                        _board.RotatePillCounterClockwise(pill);
                        break;
                    }
                    case CellOrientation.Right:
                    case CellOrientation.Left:
                    {
                        if (!_board.CanMovePillDown(pill))
                            return;

                        _board.MovePillDown(pill);
                        _board.RotatePillCounterClockwise(pill);
                        break;
                    }
                }
            }
        }
    }
}