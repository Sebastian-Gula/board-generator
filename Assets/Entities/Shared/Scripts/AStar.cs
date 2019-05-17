using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStar
{
    public MatrixNode FindShortestPath(Vector2 start, Vector2 end)
    {
        var explored = new Dictionary<string, MatrixNode>();
        var open = new Dictionary<string, MatrixNode>();
        var neighbors = new List<Vector2>
        {
            new Vector2(-1, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1)
        };
        var startNode = new MatrixNode(start, end);
        var key = start.x.ToString() + start.y.ToString();

        open.Add(key, startNode);

        while (open.Count > 0)
        {
            var smallest = SmallestNode(open);

            foreach (var neighbor in neighbors)
            {
                var position = smallest.Value.Position + neighbor;
                var current = new MatrixNode(position, end, smallest.Value);

                if (position == end)
                {
                    return current;
                }

                key = current.Position.x.ToString() + current.Position.y.ToString();

                if (!open.ContainsKey(key) && !explored.ContainsKey(key))
                {
                    open.Add(key, current);
                }
            }

            open.Remove(smallest.Key);
            explored.Add(smallest.Key, smallest.Value);
        }

        return null;
    }

    public List<Vector2> FindShortestPathAsList(Vector2 start, Vector2 end)
    {
        return GetAsList(FindShortestPath(start, end));
    }

    private KeyValuePair<string, MatrixNode> SmallestNode(Dictionary<string, MatrixNode> open)
    {
        return open.Aggregate((p, n) => p.Value.FunctionF < n.Value.FunctionF ? p : n);
    }

    private List<Vector2> GetAsList(MatrixNode end)
    {
        var next = new List<Vector2>();

        while (end != null)
        {
            next.Add(end.Position);
            end = end.Parent;
        }

        return next.GetRange(1, next.Count - 2);
    }
}