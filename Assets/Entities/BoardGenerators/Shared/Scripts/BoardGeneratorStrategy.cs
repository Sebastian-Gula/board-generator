using UnityEngine;

public abstract class BoardGeneratorStrategy : MonoBehaviour
{
    protected BoardField[,] board;

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
}
