public class DungeonGenerator : BoardGeneratorStrategy
{
    public Size RoomXSize;
    public Size RoomYSize;
    public Size StartSize;
    public Size SpaceX;
    public Size SpaceY;

    public override BoardField[,] GenerateBoard(int width, int height, int borderSize)
    {
        ClearBoard(width, height);

        var column = 0;
        var row = 0;
        var sumY = 0;
        var translationX = borderSize;
        var translationY = borderSize;

        while (true)
        {
            var roomXSize = RoomXSize.GetRandomSize();
            var roomYSize = RoomYSize.GetRandomSize();
            var beginningOfX = translationX + StartSize.GetRandomSize();
            var endOfX = beginningOfX + roomXSize;
            var beginningOfY = translationY + StartSize.GetRandomSize();
            var endOfY = beginningOfY + roomYSize;

            sumY += roomYSize;

            if (endOfY > height - borderSize - StartSize.GetRandomSize())
            {
                break;
            }
                
            if (endOfX > width - borderSize - StartSize.GetRandomSize())
            {
                translationX = borderSize;
                translationY += (sumY / column) + SpaceY.GetRandomSize();
                sumY = 0;
                column = 0;
                row++;
                continue;
            }

            for (var x = beginningOfX; x < endOfX; x++)
            {
                for (var y = beginningOfY; y < endOfY; y++)
                {
                    board[x, y] = BoardField.Empty;
                }
            }

            translationX = endOfX + SpaceX.GetRandomSize();         
            column++;
        }

        return board;
    }
}