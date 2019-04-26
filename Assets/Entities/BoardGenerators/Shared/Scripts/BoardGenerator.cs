using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardGenerator : MonoBehaviour
{
    private int _size = 0;
    private int _minimumFloorTiles;
    private int _maximumFloorTiles;
    private DictionaryInfoHelper _dictionaryInfoHelper;
    private List<Vector2> _outerFloorTilesPostitions;
    private List<Vector2> _innerFloorTilesPostitions;
    private Dictionary<BoardField, List<GameObject>> _filedsByType;
    private Dictionary<string, BoardField> _fieldTypeByNeigbors;

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
        Initialize();
        GetResources();
        SetupScene();
    }

    private void Initialize()
    {
        var usableSpace = (BoardHeight - 2 * BorderSize) * (BoardWidth - 2 * BorderSize);

        Board = new BoardInfo()
        {
            BoardFields = new BoardField[BoardWidth, BoardHeight],
            BoardObstacles = new FiledStatus[BoardWidth, BoardHeight]
        };

        _dictionaryInfoHelper = new DictionaryInfoHelper();
        _outerFloorTilesPostitions = new List<Vector2>();
        _innerFloorTilesPostitions = new List<Vector2>();
        _minimumFloorTiles = usableSpace * MinimumAvailableSurfacePercent / 100;
        _maximumFloorTiles = usableSpace * MaximumAvailableSurfacePercent / 100;
        _filedsByType = new Dictionary<BoardField, List<GameObject>>();
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
    }

    private void GetResources()
    {
        var path = "Board/Dungeon";
        var names = _dictionaryInfoHelper.GetFolderNames("Assets/Resources/" + path);
        
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
        Board.BoardFields = Board.BoardFields = BoardGeneratorStrategy.GenerateBoard(BoardWidth, BoardHeight, BorderSize);
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

                if (Board.BoardFields[x, y] == BoardField.Wall)
                {
                    if (Random.Range(0, 25) == 1)
                    {
                        Board.BoardFields[x, y] = BoardField.Empty;
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

                if (Board.BoardFields[x, y] == BoardField.Empty)
                {
                    if (Random.Range(0, 30) == 1)
                    {
                        Board.BoardFields[x, y] = BoardField.Wall;
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
                if (Board.BoardFields[x, y] != BoardField.Wall)
                {
                    Board.BoardFields[x, y] = BoardField.Empty;
                }
            }
        }
    }

    private void RoomSize(int x, int y, int roomNumber)
    {
        if (Board.BoardFields[x, y] == BoardField.Empty)
        {
            _size++;
            Board.BoardFields[x, y] = (BoardField)roomNumber;

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
                if (Board.BoardFields[x, y] == BoardField.Wall)
                {
                    continue;
                }

                if (Board.BoardFields[x, y] == (BoardField)roomNumber)
                {
                    Board.BoardFields[x, y] = BoardField.Floor;
                }
                else
                {
                    Board.BoardFields[x, y] = BoardField.Wall;
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
                if (Board.BoardFields[x, y] != BoardField.Wall)
                {
                    continue;
                }

                var key = ((Board.BoardFields[x, y + 1] == BoardField.Floor) ? "1" : "0") +
                          ((Board.BoardFields[x + 1, y] == BoardField.Floor) ? "1" : "0") +
                          ((Board.BoardFields[x, y - 1] == BoardField.Floor) ? "1" : "0") +
                          ((Board.BoardFields[x - 1, y] == BoardField.Floor) ? "1" : "0");
                var type = BoardField.Empty;

                if (_fieldTypeByNeigbors.TryGetValue(key, out type))
                {
                    Board.BoardFields[x, y] = type;
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
                if (Board.BoardFields[x, y] == BoardField.FullWall)
                {
                    if (Random.Range(0, 2) == 1)
                    {
                        Board.BoardFields[x, y] = BoardField.Floor;
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
                if (Board.BoardFields[x, y] != BoardField.Floor)
                {
                    continue;
                }

                if (Board.BoardFields[x - 1, y] == BoardField.Wall ||
                    Board.BoardFields[x, y + 1] == BoardField.Wall ||
                    Board.BoardFields[x + 1, y] == BoardField.Wall ||
                    Board.BoardFields[x, y - 1] == BoardField.Wall)
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
                if (Board.BoardFields[x, y] == BoardField.Floor)
                {
                    Board.BoardObstacles[x, y] = FiledStatus.Empty;
                }
                else
                {
                    Board.BoardObstacles[x, y] = FiledStatus.Taken;
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

                if (_filedsByType.TryGetValue(Board.BoardFields[x, y], out fields))
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