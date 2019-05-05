using System.Linq;

public class DungeonGenerator : BoardGeneratorStrategy
{
    public Range RoomXSize;
    public Range RoomYSize;
    public Range DistanceFromBorder;
    public Range SpaceX;
    public Range SpaceY;

    private int _sum;
    private int _columns;
    private int _previousRowAverageY;

    public override BoardField[,] GenerateBoard(int width, int height, int borderSize, int minimumAvailableSurfacePercent, int maximumAvailableSurfacePercent)
    {
        _sum = 0;
        _columns = 0;
        _previousRowAverageY = DistanceFromBorder.Max;

        Initialize(width, height, borderSize, minimumAvailableSurfacePercent, maximumAvailableSurfacePercent);
        ClearBoard(width, height);
        GenerateRooms(width, height, borderSize);

        return board;
    }

    private void GenerateRooms(int width, int height, int borderSize)
    {
        var translationX = borderSize + DistanceFromBorder.GetRandomRange();

        while (true)
        {
            var startX = translationX;
            var endX = startX + RoomXSize.GetRandomRange();

            if (endX > width - borderSize - DistanceFromBorder.GetRandomRange())
            {
                translationX = borderSize + DistanceFromBorder.GetRandomRange();
                _sum = 0;
                _columns = 0;
                _previousRowAverageY = GetCurrentAverageHeight() + RoomYSize.Max + SpaceY.Max;
                continue;
            }

            var startY = GetLowerRoom(startX, endX - startX) + SpaceY.GetRandomRange();
            var endY = startY + RoomYSize.GetRandomRange();

            if (endY > height - borderSize - DistanceFromBorder.GetRandomRange())
            {
                break;
            }

            var averageY = GetCurrentAverageHeight();

            if (startY - averageY <= RoomYSize.Max + SpaceY.Max)
            {
                for (var x = startX; x < endX; x++)
                {
                    for (var y = startY; y < endY; y++)
                    {
                        board[x, y] = BoardField.Empty;
                    }
                }

                UpdateCurrentAverageHeight(startY);
            }

            translationX = endX + SpaceX.GetRandomRange();
        }
    }

    private int GetLowerRoom(int startX, int endX)
    {
        var numbers = Enumerable.Range(startX, endX).ToList();
        var maxY = borderSize + DistanceFromBorder.GetRandomRange() - SpaceY.GetRandomRange();

        foreach (var x in numbers)
        {
            if (TryGetLowerRoom(x, out int y) && y > maxY)
            {
                maxY = y;
            }
        }

        return maxY;
    }

    private bool TryGetLowerRoom(int x, out int y)
    {
        var i = 1;
        var startY = boardHeight - borderSize;

        while (true)
        {
            y = startY - i;

            if (y < borderSize + DistanceFromBorder.GetRandomRange())
            {
                return false;
            }

            var field = board[x, y];

            if (field == BoardField.Empty)
            {
                return true;
            }

            i++;
        }
    }

    private int GetCurrentAverageHeight()
    {
        if (_columns == 0)
        {
            return _previousRowAverageY;
        }

        return _sum / _columns;
    }

    private void UpdateCurrentAverageHeight(int y)
    {
        _columns++;
        _sum += y;
    }
}