using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pills.Assets.Gameplay
{
    public class BoardDrawer
    {
        private readonly Tilemap _tileMap;
        private readonly TileBase[] _pillTiles;
        private Vector2Int _offset;

        public Vector2Int Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        public BoardDrawer(Tilemap tileMap, TileBase[] pillTiles, Vector2Int offset)
        {
            _tileMap = tileMap;
            _pillTiles = pillTiles;
            _offset = offset;
        }
        
        public void Draw(Board _board, Pill nextPill)
        {
            if (nextPill != null)
                DrawPill(nextPill);

            for (var x = 0; x < GameConstants.BoardWidth; ++x)
            {
                for (var y = 0; y < GameConstants.BoardHeight; ++y)
                {
                    if (_board[x, y] == CellType.Wall)
                        continue;

                    var tile = GetTile(_board[x, y]);

                    SetTile(x, y, tile);
                }
            }
        }

        private void DrawPill(Pill pill)
        {
            var x0 = pill.Cells[0].Position.x;
            var y0 = pill.Cells[0].Position.y;
            var x1 = pill.Cells[1].Position.x;
            var y1 = pill.Cells[1].Position.y;

            var cellType0 = pill.Cells[0].Type;
            var cellType1 = pill.Cells[1].Type;

            SetTile(x0, y0, GetTile(cellType0));
            SetTile(x1, y1, GetTile(cellType1));
        }
        
        private void SetTile(int x, int y, TileBase tile)
        {
            _tileMap.SetTile(new Vector3Int(x + _offset.x, y + _offset.y, 0), tile);
        }

        private TileBase GetTile(CellType cellType)
        {
            TileBase tile;

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
    }
}