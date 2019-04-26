using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardGenerator : MonoBehaviour
{
    int _size = 0;
    int _minimumFloorTiles;
    int _maximumFloorTiles;
    FiledStatus[,] _obstacles;
    BoardField[,] _board;  
    List<Vector2> _outerFloorTilesPostitions;
    List<Vector2> _innerFloorTilesPostitions;
    Dictionary<BoardField, List<GameObject>> _filedsByType;
    Dictionary<string, BoardField> _fieldTypeByNeigbors;

    public BoardInfo Board { get; set; }

    public BoardGeneratorStrategy BoardGeneratorStrategy;
    public int BoardWidth;
    public int BoardHeight;
    public int BorderSize;

    [Range(10, 30)]
    public int MinimumAvailableSurfacePercent;

    [Range(35, 60)]
    public int MaximumAvailableSurfacePercent;


    private void Awake()
    {
        var usableSpace = (BoardHeight - 2 * BorderSize) * (BoardWidth - 2 * BorderSize);

        Board = new BoardInfo()
        {
            BoardFields = new BoardField[BoardWidth, BoardHeight],
            BoardObstacles = new FiledStatus[BoardWidth, BoardHeight]
        };
        _board = Board.BoardFields;
        _obstacles = Board.BoardObstacles;
        _outerFloorTilesPostitions = new List<Vector2>();
        _innerFloorTilesPostitions = new List<Vector2>();
        _minimumFloorTiles = usableSpace * MinimumAvailableSurfacePercent / 100;
        _maximumFloorTiles = usableSpace * MaximumAvailableSurfacePercent / 100;
        _fieldTypeByNeigbors = new Dictionary<string, BoardField>
        {
            // 1
            { "1000", BoardField.TopWall },
            { "0100", BoardField.RightWall },
            { "0010", BoardField.BottomWall },
            { "0001", BoardField.LeftWall },
            // 2
            { "1100", BoardField.TopRightWall },
            { "1010", BoardField.TopBottomWall },
            { "1001", BoardField.TopLeftWall },
            { "0110", BoardField.RightBottomWall },
            { "0101", BoardField.RightLeftWall },
            { "0011", BoardField.BottomLeftWall },
            // 3
            { "1110", BoardField.TopRightBottomWall },
            { "1101", BoardField.TopRightLeftWall },
            { "1011", BoardField.TopBottomLeftWall },
            { "0111", BoardField.RightBottomLeftWall },
            // 4
            { "1111", BoardField.FullWall }
        };

        GetResources();
        SetupScene();
    }

    private IEnumerable<string> GetFolderNames(string path)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        DirectoryInfo[] info = dir.GetDirectories();

        return info.Select(i => i.Name);
    }

    private void GetResources()
    {
        var path = "Board/Dungeon";
        var names = GetFolderNames("Assets/Resources/" + path).ToList();
        _filedsByType = new Dictionary<BoardField, List<GameObject>>();

        foreach (var name in names)
        {
            var fieldType = BoardField.Empty;

            if (Enum.TryParse(name, out fieldType))
            {
                var fields = Resources.LoadAll<GameObject>(path + "/" + name).ToList();
                _filedsByType.Add(fieldType, fields);
            }
        }
    }

    private void GenerateBoard()
    {
        Board.BoardFields = _board = BoardGeneratorStrategy.GenerateBoard(BoardWidth, BoardHeight, BorderSize);
    }

    private void ImproveBoard()
    {
        Room bigestRoom = FindBiggestRoom();

        if (bigestRoom.Size < _minimumFloorTiles)
        {
            ConnectRooms();
            return;
        }

        if (bigestRoom.Size > _maximumFloorTiles)
        {
            DisconnectRooms();
            return;
        }

        ClearOtherRooms(bigestRoom.RoomNumber);
        FindWalls();
        RemoveClutter();
    }

    private Room FindBiggestRoom()
    {
        int roomNumber = 3;
        var bigestRoom = new Room
        {
            RoomNumber = 0,
            Size = 0
        };

        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                RoomSize(x, y, roomNumber);

                if (_size != 0)
                {
                    if (_size > bigestRoom.Size)
                    {
                        bigestRoom.RoomNumber = roomNumber;
                        bigestRoom.Size = _size;
                    }

                    _size = 0;
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
                if (x < BorderSize || 
                    x > BoardWidth - BorderSize ||
                    y < BorderSize || 
                    y > BoardHeight - BorderSize)
                {
                    continue;
                }

                if (_board[x, y] == BoardField.Wall)
                {
                    if (Random.Range(0, 25) == 1)
                    {
                        _board[x, y] = BoardField.Empty;
                    }
                }
            }
        }

        RemoveRooms();
        ImproveBoard();
    }

    private void DisconnectRooms()
    {
        RemoveRooms();

        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                if (x < BorderSize ||
                    x > BoardWidth - BorderSize ||
                    y < BorderSize ||
                    y > BoardHeight - BorderSize)
                {
                    continue;
                }

                if (_board[x, y] == BoardField.Empty)
                {
                    if (Random.Range(0, 30) == 1)
                    {
                        _board[x, y] = BoardField.Wall;
                    }
                }
            }
        }
   
        ImproveBoard();
    }

    private void RemoveRooms()
    {
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                if (_board[x, y] != BoardField.Wall)
                {
                    _board[x, y] = BoardField.Empty;
                }
            }
        }
    }

    private void RoomSize(int x, int y, int roomNumber)
    {
        if (_board[x, y] == BoardField.Empty)
        {
            _size++;
            _board[x, y] = (BoardField)roomNumber;

            RoomSize(x - 1, y, roomNumber);
            RoomSize(x, y + 1, roomNumber);
            RoomSize(x + 1, y, roomNumber);
            RoomSize(x, y - 1, roomNumber);
        }
    }

    private void ClearOtherRooms(int roomNumber)
    {
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                if (_board[x, y] == BoardField.Wall)
                {
                    continue;
                }

                if (_board[x, y] == (BoardField)roomNumber)
                {
                    _board[x, y] = BoardField.Floor;
                }
                else
                {
                    _board[x, y] = BoardField.Wall;
                }
            }
        }
    }

    private void FindWalls()
    {
        for (int x = 1; x < BoardWidth - 1; x++)
        {
            for (int y = 1; y < BoardHeight - 1; y++)
            {
                if (_board[x, y] != BoardField.Wall)
                {
                    continue;
                }

                var key = ((_board[x, y + 1] == BoardField.Floor) ? "1" : "0") +
                          ((_board[x + 1, y] == BoardField.Floor) ? "1" : "0") +
                          ((_board[x, y - 1] == BoardField.Floor) ? "1" : "0") +
                          ((_board[x - 1, y] == BoardField.Floor) ? "1" : "0");
                var type = BoardField.Empty;

                if (_fieldTypeByNeigbors.TryGetValue(key, out type))
                {
                    _board[x, y] = type;
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
                if (_board[x, y] == BoardField.FullWall)
                {
                    if (Random.Range(0, 2) == 1)
                    {
                        _board[x, y] = BoardField.Floor;
                    }
                }
            }
        }
    }

    private void FindInnerOuterFloors()
    {
        for (int x = 1; x < BoardWidth - 1; x++)
        {
            for (int y = 1; y < BoardHeight - 1; y++)
            {
                if (_board[x, y] != BoardField.Floor)
                {
                    continue;
                }

                if (_board[x - 1, y] == BoardField.Wall ||
                    _board[x, y + 1] == BoardField.Wall ||
                    _board[x + 1, y] == BoardField.Wall ||
                    _board[x, y - 1] == BoardField.Wall)
                {
                    _outerFloorTilesPostitions.Add(new Vector2(x, y));
                }
                else
                {
                    _innerFloorTilesPostitions.Add(new Vector2(x, y));
                }
            }
        }
    }

    private void FindObstacles()
    {
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                if (_board[x, y] == BoardField.Floor)
                {
                    _obstacles[x, y] = FiledStatus.Empty;
                }
                else
                {
                    _obstacles[x, y] = FiledStatus.Taken;
                }
            }
        }
    }
 
    private void DrawBoard()
    {
        Transform boardHolder = new GameObject("Board").transform;

        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                List<GameObject> fields = null;

                if (_filedsByType.TryGetValue(_board[x, y], out fields))
                {
                    var position = new Vector2(x, y);
                    var index = Random.Range(0, fields.Count);
                    var field = fields[index];
                    var instance = Instantiate(field, position, Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                }
            }
        }
    }


    public void SetupScene()
    {
        GenerateBoard();

        ImproveBoard();

        FindInnerOuterFloors();

        FindObstacles();

        DrawBoard();
    }
}