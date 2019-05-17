using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public override BoardField[,] GenerateBoard(int width, int height, int borderSize, int minimumAvailableSurfacePercent)
    {
        _sum = 0;
        _columns = 0;
        _previousRowAverageY = DistanceFromBorder.Max;

        Initialize(width, height, borderSize, minimumAvailableSurfacePercent);
        ClearBoard(width, height);
        GenerateRooms(width, height, borderSize);

        var rooms = FilterRooms(FindRooms());
        var list = FindClosestPointsBeetwanRooms(rooms);

        foreach (var points in list.GroupBy(x => x.RoomNumber).Select(g => g.Aggregate((p, n) => p.Distance < n.Distance ? p : n)).ToList())
        {
            ConnectTwoPoins(points.PointA, points.PointB);
        }

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
                        board[x, y] = BoardField.Floor;
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

            if (field == BoardField.Floor)
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

    private List<Room> FindRooms()
    {
        var rooms = new List<Room>();
        int roomNumber = 3;

        for (var x = 0; x < boardWidth; x++)
        {
            for (var y = 0; y < boardHeight; y++)
            {
                var roomFields = GetRoom(x, y, roomNumber, new List<Vector2>());

                if (roomFields.Count > 0)
                {
                    rooms.Add(new Room(roomNumber, roomFields));
                    roomNumber++;
                }
            }
        }

        return rooms.OrderByDescending(room => room.RoomFields.Count).ToList();
    }

    private List<Room> FilterRooms(List<Room> rooms)
    {
        var filteredRooms = new List<Room>();
        var sum = 0;
        var i = 0;

        while (sum < minimumFloorTiles && i < rooms.Count)
        {
            var room = rooms[i];
            sum += room.RoomFields.Count;
            filteredRooms.Add(room);
            i++;
        }

        ClearOtherRooms(filteredRooms.Select(r => r.RoomNumber).ToList());

        return filteredRooms;
    }

    private List<RoomConnectionPoints> FindClosestPointsBeetwanRooms(List<Room> rooms)
    {
        var list = new List<RoomConnectionPoints>();

        for (var i = 0; i < rooms.Count; i++)
        {
            var roomA = rooms[i];

            for (var j = i + 1; j < rooms.Count; j++)
            {
                list.Add(FindClosestPointsBeetweanTwoRooms(roomA, rooms[j]));
            }
        }

        return list;
    }

    private RoomConnectionPoints FindClosestPointsBeetweanTwoRooms(Room roomA, Room roomB)
    {
        var list = new List<RoomConnectionPoints>();
        var tree = new KdTree();
        tree.AddAll(roomB.RoomFields);

        foreach (var field in roomA.RoomFields)
        {
            var closestPoint = tree.FindClosest(field);
            var distance = Vector2.Distance(field, closestPoint);

            list.Add(new RoomConnectionPoints
            {
                RoomNumber = roomA.RoomNumber,
                PointA = field,
                PointB = closestPoint,
                Distance = distance
            });
        }

        return list.Aggregate((p, n) => p.Distance < n.Distance ? p : n);
    }

    private void ConnectTwoPoins(Vector2 pointA, Vector2 pointB)
    {
        var aStar = new AStar();

        foreach (var point in aStar.FindShortestPathAsList(pointA, pointB))
        {
            board[(int)point.x, (int)point.y] = BoardField.Floor;
        }
    }
}