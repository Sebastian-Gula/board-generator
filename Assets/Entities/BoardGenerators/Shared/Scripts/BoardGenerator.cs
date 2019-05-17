using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardGenerator : MonoBehaviour
{
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

    [Range(5, 50)]
    public int MinimumAvailableSurfacePercent;


    private void Awake()
    {
        Initialize();
        GetResources();
        SetupScene();
    }

    private void Initialize()
    {
        Board = new BoardInfo()
        {
            BoardFields = new BoardField[BoardWidth, BoardHeight],
            BoardObstacles = new FiledStatus[BoardWidth, BoardHeight]
        };

        _dictionaryInfoHelper = new DictionaryInfoHelper();
        _outerFloorTilesPostitions = new List<Vector2>();
        _innerFloorTilesPostitions = new List<Vector2>();

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
        Board.BoardFields
            = BoardGeneratorStrategy.GenerateBoard(BoardWidth, BoardHeight, BorderSize, MinimumAvailableSurfacePercent);
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

                if (_fieldTypeByNeigbors.TryGetValue(key, out BoardField type))
                {
                    Board.BoardFields[x, y] = type;
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

                if (Board.BoardFields[x - 1, y] == BoardField.Wall
                    || Board.BoardFields[x, y + 1] == BoardField.Wall
                    || Board.BoardFields[x + 1, y] == BoardField.Wall
                    || Board.BoardFields[x, y - 1] == BoardField.Wall)
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
                if (_filedsByType.TryGetValue(Board.BoardFields[x, y], out List<GameObject> fields))
                {
                    var position = new Vector2(x, y);
                    var index = Random.Range(0, fields.Count);
                    var field = fields[index];
                    var instance = Instantiate(field, position, Quaternion.identity);
                    instance.transform.SetParent(boardHolder);
                }
            }
        }
    }

    public void SetupScene()
    {
        GenerateBoard();
        FindWalls();
        FindInnerOuterFloors();
        FindObstacles();
        DrawBoard();
    }
}