using UnityEngine;

public abstract class BoardGeneratorStrategy : MonoBehaviour
{
    protected BoardField[,] board;
    protected int minimumFloorTiles;
    protected int maximumFloorTiles;

    private int _boardWidth;
    private int _boardHeight;
    private int _borderSize;


    public abstract BoardField[,] GenerateBoard(int width, int height, int borderSize, int minimumAvailableSurfacePercent, int maximumAvailableSurfacePercent);


    protected void Initialize(int width, int height, int borderSize, int minimumAvailableSurfacePercent, int maximumAvailableSurfacePercent)
    {
        var usableSpace = (height - (2 * borderSize)) * (width - (2 * borderSize));

        _boardWidth = width;
        _boardHeight = height;
        _borderSize = borderSize;
        minimumFloorTiles = usableSpace * minimumAvailableSurfacePercent / 100;
        maximumFloorTiles = usableSpace * maximumAvailableSurfacePercent / 100;

        board = new BoardField[width, height];
    }

    protected void ClearBoard(int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                board[x, y] = BoardField.Wall;
            }
        }
    }

    private int RoomSize(int x, int y, int roomNumber, int size)
    {
        if (board[x, y] == BoardField.Empty)
        {
            size++;
            board[x, y] = (BoardField)roomNumber;

            size = RoomSize(x - 1, y, roomNumber, size);
            size = RoomSize(x, y + 1, roomNumber, size);
            size = RoomSize(x + 1, y, roomNumber, size);
            size = RoomSize(x, y - 1, roomNumber, size);
        }

        return size;
    }

    protected Room FindBiggestRoom()
    {
        int roomNumber = 3;
        var bigestRoom = new Room
        {
            RoomNumber = 0,
            Size = 0
        };

        for (var x = 0; x < board.GetLength(0); x++)
        {
            for (var y = 0; y < board.GetLength(1); y++)
            {
                var size = RoomSize(x, y, roomNumber, 0);

                if (size != 0)
                {
                    if (size > bigestRoom.Size)
                    {
                        bigestRoom.RoomNumber = roomNumber;
                        bigestRoom.Size = size;
                    }

                    roomNumber++;
                }
            }
        }

        return bigestRoom;
    }

    private void ConnectRooms()
    {
        for (int x = 0; x < _boardWidth; x++)
        {
            for (int y = 0; y < _boardHeight; y++)
            {
                if (x < _borderSize
                    || x > _boardWidth - _borderSize
                    || y < _borderSize
                    || y > _boardHeight - _borderSize)
                {
                    continue;
                }

                if (board[x, y] == BoardField.Wall
                    && Random.Range(0, 25) == 1)
                {
                    board[x, y] = BoardField.Empty;
                }
            }
        }

        RemoveRooms();
    }

    private void DisconnectRooms()
    {
        RemoveRooms();

        for (int x = 0; x < _boardWidth; x++)
        {
            for (int y = 0; y < _boardHeight; y++)
            {
                if (x < _borderSize
                    || x > _boardWidth - _borderSize
                    || y < _borderSize
                    || y > _boardHeight - _borderSize)
                {
                    continue;
                }

                if (board[x, y] == BoardField.Empty
                    && Random.Range(0, 30) == 1)
                {
                    board[x, y] = BoardField.Wall;
                }
            }
        }
    }

    private void RemoveRooms()
    {
        for (int x = 0; x < _boardWidth; x++)
        {
            for (int y = 0; y < _boardHeight; y++)
            {
                if (board[x, y] != BoardField.Wall)
                {
                    board[x, y] = BoardField.Empty;
                }
            }
        }
    }

    private void ClearOtherRooms(int roomNumber)
    {
        for (int x = 0; x < _boardWidth; x++)
        {
            for (int y = 0; y < _boardHeight; y++)
            {
                if (board[x, y] == (BoardField)roomNumber)
                {
                    board[x, y] = BoardField.Floor;
                }
                else
                {
                    board[x, y] = BoardField.Wall;
                }
            }
        }
    }

    private void RemoveClutter()
    {
        for (int x = 0; x < _boardWidth; x++)
        {
            for (int y = 0; y < _boardHeight; y++)
            {
                if (board[x, y] == BoardField.FullWall
                    && Random.Range(0, 2) == 1)
                {
                    board[x, y] = BoardField.Floor;
                }
            }
        }
    }
}
