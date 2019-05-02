using UnityEngine;

public abstract class BoardGeneratorStrategy : MonoBehaviour
{
    protected BoardField[,] board;

    public int BoardWidth { get; set; }
    public int BoardHeight { get; set; }
    public int BorderSize { get; set; }

    public abstract BoardField[,] GenerateBoard(int width, int height, int borderSize);

    protected void ClearBoard(int width, int height)
    {
        board = new BoardField[width, height];

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
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                if (x < BorderSize
                    || x > BoardWidth - BorderSize
                    || y < BorderSize
                    || y > BoardHeight - BorderSize)
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

        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                if (x < BorderSize
                    || x > BoardWidth - BorderSize
                    || y < BorderSize
                    || y > BoardHeight - BorderSize)
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
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
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
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
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
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
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
