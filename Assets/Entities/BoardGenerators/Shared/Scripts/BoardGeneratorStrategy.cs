using UnityEngine;

public abstract class BoardGeneratorStrategy : MonoBehaviour
{
    protected BoardField[,] board;
    protected int minimumFloorTiles;
    protected int maximumFloorTiles;
    protected int boardWidth;
    protected int boardHeight;
    protected int borderSize;


    public abstract BoardField[,] GenerateBoard(int width, int height, int borderSize, int minimumAvailableSurfacePercent, int maximumAvailableSurfacePercent);


    protected void Initialize(int width, int height, int borderSize, int minimumAvailableSurfacePercent, int maximumAvailableSurfacePercent)
    {
        var usableSpace = (height - (2 * borderSize)) * (width - (2 * borderSize));

        boardWidth = width;
        boardHeight = height;
        this.borderSize = borderSize;
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

    private void ConnectRooms()
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (x < borderSize
                    || x > boardWidth - borderSize
                    || y < borderSize
                    || y > boardHeight - borderSize)
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

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (x < borderSize
                    || x > boardWidth - borderSize
                    || y < borderSize
                    || y > boardHeight - borderSize)
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
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
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
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
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
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
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
