using System.Collections.Generic;
using UnityEngine;

public abstract class BoardGeneratorStrategy : MonoBehaviour
{
    protected BoardField[,] board;
    protected int minimumFloorTiles;
    protected int boardWidth;
    protected int boardHeight;
    protected int borderSize;

    public abstract BoardField[,] GenerateBoard(int width, int height, int borderSize, int minimumAvailableSurfacePercent);

    protected void Initialize(int width, int height, int borderSize, int minimumAvailableSurfacePercent)
    {
        var usableSpace = (height - (2 * borderSize)) * (width - (2 * borderSize));

        boardWidth = width;
        boardHeight = height;
        this.borderSize = borderSize;
        minimumFloorTiles = usableSpace * minimumAvailableSurfacePercent / 100;

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

    protected List<Vector2> GetRoom(int x, int y, int roomNumber, List<Vector2> roomFields)
    {
        if (board[x, y] == BoardField.Floor)
        {
            roomFields.Add(new Vector2(x, y));

            board[x, y] = (BoardField)roomNumber;

            roomFields = GetRoom(x - 1, y, roomNumber, roomFields);
            roomFields = GetRoom(x, y + 1, roomNumber, roomFields);
            roomFields = GetRoom(x + 1, y, roomNumber, roomFields);
            roomFields = GetRoom(x, y - 1, roomNumber, roomFields);
        }

        return roomFields;
    }

    protected void ClearOtherRooms(List<int> roomNumbers)
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                var number = (int)board[x, y];
                if (roomNumbers.Contains(number))
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
}
